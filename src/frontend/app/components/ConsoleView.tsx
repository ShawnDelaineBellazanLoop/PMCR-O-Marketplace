// src/frontend/app/components/ConsoleView.tsx
//
// ARCH-CONSOLE-TRAILPLAYER-001 (2026-07-20): extracted from app/page.tsx so
// the page itself can become an async Server Component (matching the
// app/directory/page.tsx pattern) that reads real trails via
// lib/trails.ts's loadTrailsByDomain() and hands them down as a plain prop.
// This is what finally wires the Trails section's TrailView -- previously
// hardcoded to `trail={null}` with a comment saying "no read endpoint
// exists yet" (that endpoint, loadTrailsByDomain, has existed since
// ARCH-AGENT-DIRECTORY-002 but was never plumbed into this page).
//
// ARCH-VISUAL-BRIDGE-002 (2026-07-20): A2UI renderer integrated to display
// TrailCard and AgentDomainCard components when the LLM calls render_a2ui.
"use client";

import { Fragment, useMemo, useState } from "react";
import { z } from "zod";
import { useRouter } from "next/navigation";
import { useAgent, useCopilotKit, useFrontendTool } from "@copilotkit/react-core/v2";
import DomainSelector, { DOMAINS } from "./DomainSelector";
import RoundTable from "./RoundTable";
import TrailView, { type Trail } from "./TrailView";
import A2UIRenderer from "./A2UIRenderer";

// ARCH-AGUI-STATE-001 (2026-07-13): mirrors ProjectName.OrchestratorService's
// Services/PmcroStateBroadcast.cs PmcroCycleStateSnapshot record field-for-field.
// No shared schema between the .NET and TS sides yet -- if that record's shape
// changes, update this type by hand (JsonSerializerOptions.PropertyNamingPolicy
// = CamelCase on the .NET side is what makes these field names line up).
type PmcroPhase =
  | "Planning"
  | "Checking"
  | "Reflecting"
  | "CycleComplete"
  | "Sealed"
  | "Error";

type PmcroCycleState = {
  trailId: string;
  cycle: number;
  phase: PmcroPhase;
  lastAction?: string | null;
  disposition?: string | null;
  allPassed?: boolean | null;
};

// ── Phase rail ───────────────────────────────────────────────────────────
// ARCH-FRONTEND-REDESIGN-001: only these four steps ever get a live node --
// Plan / Check / Reflect / Seal -- because those are the only phases
// PmcroLoop.cs's PmcroStateBroadcast.Publish calls actually emit (verified
// against Loop/PmcroLoop.cs 2026-07-13: Planning at cycle top, Checking at
// Turn B start, Reflecting after the Checker, Sealed after trailWriter.SealAsync).
// "Make" runs silently inside Turn A between Planning and Checking with no
// snapshot of its own, so it's rendered as a label on the connector rather
// than a fifth node that would falsely claim live visibility into it.
const RAIL_STEPS = [
  { key: "Planning", label: "Plan" },
  { key: "Checking", label: "Check" },
  { key: "Reflecting", label: "Reflect" },
  { key: "Sealed", label: "Seal" },
] as const;

function railIndex(phase: PmcroPhase): number {
  switch (phase) {
    case "Planning":
      return 0;
    case "Checking":
      return 1;
    case "Reflecting":
    case "CycleComplete":
      return 2;
    case "Sealed":
      return 3;
    default:
      return 0;
  }
}

function dispositionTone(disposition?: string | null): "pass" | "retry" | "halt" | null {
  switch (disposition) {
    case "Accept":
      return "pass";
    case "Retry":
      return "retry";
    case "Halt":
      return "halt";
    default:
      return null;
  }
}

// ARCH-AGUI-STATE-001: renders PmcroLoop's live phase transitions. Each
// PmcroCycleStateSnapshot the backend publishes is a full STATE_SNAPSHOT (not
// a delta), so agent.state is simply the latest one -- no client-side
// merging needed.
function PhaseRail() {
  // "Orchestrator" matches both the keyed AIAgent name in Program.cs and the
  // agent="Orchestrator" default already set on the <CopilotKit> provider in
  // layout.tsx -- passed explicitly here so this panel doesn't silently break
  // if that default ever changes.
  const { agent } = useAgent({ agentId: "Orchestrator" });
  const state = agent.state as PmcroCycleState | undefined;

  // FIX (2026-07-13): during Next.js static prerendering of "/" there's no
  // live AG-UI connection, so agent.state comes back as {} (truthy, but with
  // no fields) rather than undefined -- the earlier `if (!state)` guard alone
  // let `state.trailId.slice(...)` through as undefined.slice() and crashed
  // the build (confirmed via `npm run build`: TypeError reading 'slice').
  // Guard on the one field every real snapshot always carries (phase) instead.
  if (!state?.phase) {
    return (
      <div className="phase-rail">
        <div className="phase-rail-track" aria-hidden="true">
          {RAIL_STEPS.map((step, i) => (
            <Fragment key={step.key}>
              <div className="phase-rail-step" data-status="pending">
                <div className="phase-rail-node" />
                <span className="phase-rail-label">{step.label}</span>
              </div>
              {i < RAIL_STEPS.length - 1 && (
                <div className="phase-rail-connector">
                  {i === 0 && <span className="phase-rail-connector-tag">make</span>}
                </div>
              )}
            </Fragment>
          ))}
        </div>
        <p className="phase-rail-idle">No cycle running yet.</p>
      </div>
    );
  }

  const isError = state.phase === "Error";
  const activeIndex = railIndex(state.phase);
  const tone = dispositionTone(state.disposition);

  return (
    <div className="phase-rail">
      <div className="phase-rail-track">
        {RAIL_STEPS.map((step, i) => {
          const status = isError && i === activeIndex
            ? "halt"
            : i < activeIndex || (i === activeIndex && state.phase === "Sealed")
              ? "done"
              : i === activeIndex
                ? "active"
                : "pending";
          return (
            <Fragment key={step.key}>
              <div className="phase-rail-step" data-status={status}>
                <div className="phase-rail-node" />
                <span className="phase-rail-label">{step.label}</span>
              </div>
              {i < RAIL_STEPS.length - 1 && (
                <div
                  className="phase-rail-connector"
                  data-filled={i < activeIndex ? "true" : "false"}
                >
                  {i === 0 && <span className="phase-rail-connector-tag">make</span>}
                </div>
              )}
            </Fragment>
          );
        })}
      </div>

      <div className="phase-rail-meta">
        <span>trail <strong>{state.trailId ? state.trailId.slice(0, 8) : "—"}</strong></span>
        <span>cycle <strong>{state.cycle}</strong></span>
        {state.lastAction && (
          <span>last action <strong>{state.lastAction}</strong></span>
        )}
        {typeof state.allPassed === "boolean" && (
          <span className={`phase-rail-badge`} data-tone={state.allPassed ? "pass" : "halt"}>
            checker {state.allPassed ? "pass" : "fail"}
          </span>
        )}
        {tone && (
          <span className="phase-rail-badge" data-tone={tone}>
            {state.disposition}
          </span>
        )}
      </div>
    </div>
  );
}

export default function ConsoleView({
  trailsByDomain,
}: {
  trailsByDomain: Record<string, Trail[]>;
}) {
  const [prompt, setPrompt] = useState("");
  const [sending, setSending] = useState(false);
  // ARCH-DOMAIN-SELECT-001: null = untagged (today's default, resolves to
  // filesystem-agent). A chosen domain id gets prefixed onto the outgoing
  // message as an explicit routing tag the Orchestrator's instructions parse
  // (see Program.cs) -- this is what makes FileTrailWriter name the trail
  // directory after the domain instead of "filesystem-agent", even before any
  // domain-specific skill-loading is wired in.
  const [domain, setDomain] = useState<string | null>(null);

  // ARCH-NEURAL-ACTION-001 (2026-07-20): LLM-addressable UI state.
  // briefingPlayTrigger is bumped (never read for its value, only its
  // identity-changing) to (re)start RoundTable's turn-by-turn playback.
  // NOTE (ARCH-IA-SPLIT-001, 2026-07-20): selectedAgentId / the on-page
  // C-Suite grid it used to highlight are gone -- that grid now lives only
  // in /directory (see plan point 3: Directory is the canonical place for
  // it, the Console copy was a redundant duplicate). selectAgent's handler
  // below navigates there instead of scrolling a local element.
  const [briefingPlayTrigger, setBriefingPlayTrigger] = useState(0);

  // ARCH-NEURAL-ACTION-002 (2026-07-20): pins the Trail player to one
  // specific sealed trail by id, independent of the domain tag. Takes
  // priority over the domain-based fallback below when set (playTrail's
  // handler is the only setter -- there's no UI control for this yet, by
  // design; picking a specific trail by id is an LLM-addressable action,
  // not a manual one).
  const [selectedTrailId, setSelectedTrailId] = useState<string | null>(null);

  // ARCH-CONSOLE-TRAILPLAYER-001 (2026-07-20): the trail the Trails section's
  // TrailView renders. If a domain is tagged, show that domain's most
  // recent trail (trailsByDomain entries already come back sorted newest
  // first from loadTrailsByDomain); untagged, fall back to the single most
  // recent trail across every domain. Real data end to end -- no fabricated
  // "current" trail state.
  const latestTrail = useMemo<Trail | null>(() => {
    // ARCH-NEURAL-ACTION-002: an explicit playTrail(uuid) pin wins over
    // both the domain tag and the cross-domain fallback -- it's a direct
    // request for one specific trail, searched across every domain since
    // the id alone doesn't say which domain it sealed under.
    if (selectedTrailId) {
      for (const trails of Object.values(trailsByDomain)) {
        const match = trails.find((t) => t.id === selectedTrailId);
        if (match) return match;
      }
      // Stale/unknown id (e.g. trail pruned since the model last saw it) --
      // fall through to the normal domain-based behavior rather than
      // rendering nothing.
    }
    if (domain) return trailsByDomain[domain]?.[0] ?? null;
    let best: Trail | null = null;
    for (const trails of Object.values(trailsByDomain)) {
      const candidate = trails[0];
      if (!candidate) continue;
      if (!best || (candidate.createdAt ?? "") > (best.createdAt ?? "")) best = candidate;
    }
    return best;
  }, [trailsByDomain, domain, selectedTrailId]);

  // ARCH-FRONTEND-SIDEBAR-004 (2026-07-15): the previous handler only
  // focused the CopilotKit sidebar's input and copied text into it via a
  // native setter -- it never actually dispatched anything, so "Send to
  // Orchestrator" silently did nothing until the person pressed Enter
  // themselves. Fixed using the documented v2 pattern (verified against
  // CopilotKit's own docs/blog examples, 2026-07-15): add a user message to
  // the agent directly, then run it via useCopilotKit()'s copilotkit.runAgent
  // -- the same call CopilotChat's own send button makes internally.
  const { agent } = useAgent({ agentId: "Orchestrator" });
  const { copilotkit } = useCopilotKit();
  const router = useRouter();

  async function handleHeroSubmit(e: React.FormEvent) {
    e.preventDefault();
    const text = prompt.trim();
    if (!text || sending) return;
    setSending(true);
    try {
      // ARCH-DOMAIN-SELECT-001: the tag is plain text in the message body, not
      // a structured field -- AG-UI's message shape has no side-channel for it.
      // Program.cs's Orchestrator instructions look for this exact
      // "[domain: x]" prefix and strip it (same pattern EC-INTENT-001 already
      // uses to strip stray routing params the LLM echoes into seedIntent).
      const content = domain ? `[domain: ${domain}] ${text}` : text;
      agent.addMessage({
        id: crypto.randomUUID(),
        role: "user",
        content,
      });
      await copilotkit.runAgent({ agent });
      setPrompt("");
    } finally {
      setSending(false);
    }
  }

  // ARCH-NEURAL-ACTION-001 (2026-07-20): registered via useFrontendTool, the
  // real v2 API -- there is no useCopilotAction in @copilotkit/react-core/v2
  // (that's the v1 hook name; confirmed against this package's own
  // dist/v2/index.d.mts export list, which has no such export). Both tools
  // are unscoped (no agentId), so they're callable from any agent turn,
  // matching how DomainSelector's routing tag already works untagged.
  useFrontendTool<{ agentId: string }>({
    name: "selectAgent",
    description:
      "Opens the Colony Directory with one of the C-Suite domains " +
      "(ceo, chief-of-staff, cto, coo, cfo, cro, cmo, clo, chro, " +
      "domain-specialist) pre-selected. Use this to draw the user's " +
      "attention to whichever domain the conversation is currently about.",
    parameters: z.object({
      agentId: z
        .enum(DOMAINS.map((d) => d.id) as [string, ...string[]])
        .describe("The domain id to select, e.g. 'cfo' or 'cto'."),
    }),
    handler: async ({ agentId }) => {
      const match = DOMAINS.find((d) => d.id === agentId);
      if (!match) {
        return `No C-Suite domain with id "${agentId}". Valid ids: ${DOMAINS.map((d) => d.id).join(", ")}.`;
      }
      // ARCH-IA-SPLIT-001 (2026-07-20): the C-Suite grid this used to
      // scroll-and-highlight on the Console page has moved to /directory
      // (see AgentDirectory's initialDomainId prop, read from this exact
      // query param by app/directory/page.tsx).
      router.push(`/directory?agent=${match.id}`);
      return `Opened the Directory with ${match.label} (${match.id}) selected.`;
    },
  });

  useFrontendTool({
    name: "playBriefing",
    description:
      "Plays back the Colony's Round Table briefing (from " +
      ".pmcro/SESSION-BRIEF.md) turn by turn, scrolling to it and " +
      "highlighting each speaker in sequence. Use this when the user asks " +
      "to hear, replay, or walk through the current session brief.",
    handler: async () => {
      // Bumping the trigger (rather than toggling a boolean) is what lets a
      // second playBriefing() call restart playback from turn 0 even if one
      // was already mid-run -- RoundTable's effect keys off this value
      // changing, not its truthiness.
      setBriefingPlayTrigger((n) => n + 1);
      return "Playing the Round Table briefing.";
    },
  });

  // ARCH-NEURAL-ACTION-002 (2026-07-20): 'playTrail' is the second Action
  // Bridge tool. There is deliberately no 'focusAgent' here -- naming-
  // discipline check found that's the exact same capability as the
  // 'selectAgent' tool above (select a C-Suite domain, scroll it into
  // view), just requested under a different name. Adding a second tool
  // for one intent would give the model two names to choose between for
  // the same action, which is worse than adding nothing.
  useFrontendTool<{ uuid: string }>({
    name: "playTrail",
    description:
      "Pins the Trail player (bottom of the Console) to one specific " +
      "sealed or in-progress PMCR-O trail by its id, and scrolls it into " +
      "view. Use this when the user asks to see, open, replay, or pull up " +
      "a specific trail by its id or uuid.",
    parameters: z.object({
      uuid: z.string().describe("The trail's id, e.g. 'd5f17cc3-61f1-4ac9-b73d-09c102d8147e'."),
    }),
    handler: async ({ uuid }) => {
      const found = Object.values(trailsByDomain).some((trails) =>
        trails.some((t) => t.id === uuid),
      );
      if (!found) {
        return `No trail with id "${uuid}" found on disk.`;
      }
      setSelectedTrailId(uuid);
      document.getElementById("trails")?.scrollIntoView({ behavior: "smooth", block: "start" });
      return `Playing trail ${uuid}.`;
    },
  });

  return (
    <>
      <section id="console" className="colony-shell">
        <span className="colony-eyebrow">
          <span className="dot" />
          PMCR-O AI Agent Company
        </span>

        <h1 className="hero-title">What should the Colony work on?</h1>
        <p className="hero-subtitle">
          Every request runs the full Plan → Make → Check → Reflect cycle
          against the filesystem, terminal, and browser subject agents,
          with human-in-the-loop approval on file-write and
          command-execution steps.
        </p>

        <form className="hero-bar" onSubmit={handleHeroSubmit}>
          <input
            className="hero-input"
            type="text"
            placeholder="e.g. list the files in src/services…"
            value={prompt}
            onChange={(e) => setPrompt(e.target.value)}
          />
          <button type="submit" className="hero-submit" disabled={sending || !prompt.trim()}>
            {sending ? "Sending…" : "Send to Orchestrator"}
          </button>
        </form>

        {/* ARCH-DOMAIN-SELECT-001: optional C-Suite routing tag. Untagged
            (default) behaves exactly as before this change -- filesystem-agent,
            no domain in the trail path. */}
        <DomainSelector value={domain} onChange={setDomain} />
        {domain && (
          <p className="domain-pill-hint">
            Trail will be tagged <strong>{DOMAINS.find((d) => d.id === domain)?.label}</strong> —
            runs as filesystem-agent until that domain's skill is wired in.
          </p>
        )}

        {/* ARCH-ROUND-TABLE-001: live Round Table rendered from
            .pmcro/SESSION-BRIEF.md (served as /SESSION-BRIEF.md via the
            public/ copy, refreshed whenever brief-session regenerates it).
            Replaces the static placeholder Trails section below. */}
        <RoundTable playTrigger={briefingPlayTrigger} />

        <PhaseRail />
      </section>

      {/* ARCH-CONSOLE-TRAILPLAYER-001 (2026-07-20): now wired to real
          trail data via lib/trails.ts's loadTrailsByDomain(), read
          server-side in page.tsx and passed down as trailsByDomain --
          replaces the old hardcoded `trail={null}` placeholder. Shows the
          most recent trail for the tagged domain, or the most recent
          trail overall when untagged. Full history for every domain is
          still one click away in the Directory.
          ARCH-IA-SPLIT-001 (2026-07-20): this now sits directly beneath
          Console instead of at the bottom of a five-section scroll -- the
          static Subject agents / C-Suite / Harness / Skills sections that
          used to separate it from the hero moved out to /platform and
          /directory. */}
      <section id="trails" className="colony-section">
        <h2 className="colony-section-title">Trail player</h2>
        {!latestTrail && (
          <p className="colony-hint" style={{ marginTop: 0, marginBottom: 16 }}>
            No sealed or in-progress trails on disk yet — this fills in as soon as a cycle runs.
          </p>
        )}
        <TrailView trail={latestTrail} />
      </section>

      <p className="colony-hint" style={{ maxWidth: 960, margin: "56px auto 0", padding: "0 24px" }}>
        Open the assistant in the corner to give the Orchestrator a task —
        e.g. &ldquo;list the files in src/services&rdquo; or &ldquo;check the
        status of the terminal agent&rdquo;.
      </p>

      {/* ARCH-VISUAL-BRIDGE-002 (2026-07-20): mounts the A2UI renderer so the LLM
          can call render_a2ui with TrailCard or AgentDomainCard and see real
          components in the CopilotKit chat surface. */}
      <A2UIRenderer />
    </>
  );
}