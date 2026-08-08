# PMCR-O Round Table — Live C-Suite Multi-Agent Session

STATUS: draft, corrected 2026-08-06. Written directly against the live
tree (W:\PMCR_O\PMCR-O-Marketplace — confirmed as the active project;
an earlier pass of this doc was mistakenly written against the stale
W:\PMCR-O\PMCR-O tree, last touched 2026-07-22, and has been removed
from there). Two corrections from the first draft (based on an uploaded
transcript only, before any files were read) are load-bearing and are
called out inline below (search "CORRECTION").

## 0. What "Round Table" currently means on disk (verified 2026-08-06)

There are TWO unrelated things both called "Round Table" in this repo,
and the original spec conflated them:

1. **`ChatPanel.tsx`'s `CopilotSidebar`** — `AGENT_LABELS.Orchestrator
   .modalHeaderTitle` is literally the string `"PMCR-O Round Table"`.
   This is the panel in the screenshot (PMCR-O / Harness pill toggle at
   top). It is ONE `AIAgent` ("Orchestrator") streaming ONE completion.
2. **`RoundTable.tsx`** — an unrelated component on the Console page (`/`)
   that replays `.pmcro/SESSION-BRIEF.md` turn-by-turn as scripted
   narration (`playBriefing()` frontend tool). Static, file-driven, not a
   chat surface at all.

The screenshot is (1), not (2). The live-session work below targets (1).

## 1. CORRECTION — the chiefs are not role-play

The first draft of this spec described chiefs "each running their own
autonomous PMCR-O loop" without specifying what a chief's output actually
*is*. Shawn's correction (2026-08-06): a chief is not the model told
"pretend to be the CTO." A chief is **trail-grounded self-simulation** —
the model is given a chief's own accumulated trail history
(`.pmcro/trails/{chiefId}/**`: seed intents, plans, checks, reflections,
dispositions) and embodies that role *from its own record*, self-
referentially, not from a persona instruction layered on top. This is
the same "loop that knows itself" framing that underlies the rest of
PMCR-O — the persona is derived from the trail, not performed.

Consequence for the data model (§3): `RoundTableEntry.cycleRef` is not
optional metadata. It is the actual input the simulation is grounded on.
A chief's turn generator must read real trail files for that chief before
producing an entry — there is no "just prompt it as the CTO" shortcut.

## 2. CORRECTION — audience-dependent register

Same underlying trail, different rendering depending on who is watching
the session: a technical audience (Shawn, inside the architecture) can
get dense, system-level language from a chief's self-simulation. A
non-technical business stakeholder watching the *same* trail-grounded
session should get plainer language, same facts, less internal jargon.
This is a rendering-time concern, not a trail-content concern — the
trail itself doesn't change, only how a chief's entry gets phrased for
the audience currently attached to the session.

Consequence: `RoundTableSession` needs an `audience: "technical" |
"business"` field (default `"technical"`), threaded into whatever prompt
generates each chief's entry text alongside that chief's trail context.

## 3. Data model

```
RoundTableSession
  id
  status: "open" | "sealed"
  audience: "technical" | "business"      // CORRECTION §2 — default "technical"
  createdAt
  sealedAt?
  participants: string[]                  // chief agent IDs active in this session
  sessionTrailId                          // this session's own trail dir under .pmcro/trails/round-table/{id}/

RoundTableEntry
  id
  sessionId
  authorType: "orchestrator" | "chief"
  authorId: string                        // "orchestrator" | "cto" | "cfo" | etc.
  kind: "message" | "plan" | "make" | "check" | "reflect" | "disposition"
  content: string                         // rendered for session.audience at generation time
  createdAt
  cycleRef: string                        // CORRECTION §1 — required, not optional. Points at the
                                           // chief's own trail cycle this entry was grounded on:
                                           // .pmcro/trails/{authorId}/{cycleRef}/
```

Close to the existing per-agent trail schema (`ITrailWriter`) on purpose —
`FileTrailWriter` gets extended, not replaced. A session is a live view
that reads across N already-existing per-chief trail directories, plus
writes its own thin session trail (message log + participant list) so the
session itself is durable and replayable like everything else in `.pmcro/trails/`.

## 4. Backend wiring

- **Session start** — `POST /roundtable/sessions {participants, audience}`
  → for each participant, start (or attach to) that chief's own PMCR-O
  macro loop (Pattern D). Each chief loop continues writing its normal
  trail at `.pmcro/trails/{chiefId}/{trailId}/`, unchanged.
- **Turn generation** — on each chief loop boundary (start of plan or
  reflect phase), read that chief's latest trail cycle, generate a
  `RoundTableEntry` grounded on it (CORRECTION §1), rendered for
  `session.audience` (CORRECTION §2), append to the session's shared
  timeline, and emit an AG-UI event tagged by `authorId`.
- **Orchestrator injection** — `POST /roundtable/sessions/{id}/messages`
  appends a `RoundTableEntry` with `authorType: "orchestrator"`. Writes
  only — does not call any chief directly. A chief's next loop boundary
  reads unconsumed orchestrator entries as extra context alongside its own
  trail, same way HIL input already folds into a cycle.
- **Sealing** — per-chief trail sealing (`SealAsync`) is unchanged and
  independent. The session itself seals when Orchestrator calls seal or
  all participants reach a terminal disposition.


## 5. Frontend — live session surface

Where it lives: a new `RoundTableSession.tsx` component — NOT a rework of
either existing "Round Table" thing from §0. `ChatPanel.tsx`'s
`CopilotSidebar` stays what it is: a single-agent completion stream
(Orchestrator/Harness pill toggle). A live multi-chief session doesn't fit
inside a sidebar's bubble list — it's a multi-party timeline, not a 1:1
chat. `RoundTable.tsx` also stays untouched — it replays
`.pmcro/SESSION-BRIEF.md`, an unrelated static data source. New mount
point: a `/roundtable` route, peer to the existing `/harness`, `/directory`,
`/platform` routes under `app/`, with a nav entry added to `Sidebar.tsx`.

Visual language: extend the system that already exists here, don't invent
a new one.
- **Per-chief color** — reuse `AgentCard.tsx`'s `LOOP_ROLE_COLOR` pattern:
  each chief gets the same `agent.color` already assigned in
  `DomainSelector.tsx`'s `DOMAINS` list, so a chief's Round Table chip is
  the same color as its card everywhere else in the app. No new palette.
- **Timeline, not chat bubbles** — entries append as `.colony-card`-styled
  rows (same border/radius/`--colony-panel-raised` treatment
  `.round-table-turn` already uses), stacked vertically. This is a session
  log across N chiefs, not a two-party conversation.
- **Streaming state** — a chief's `RoundTableEntry.content` fills in as AG-UI
  partial-text events arrive; the in-progress row gets the same
  `data-active="true"` pulse/border-color treatment
  `round-table-turn[data-active="true"]` already uses for playback
  highlighting, not a new spinner component.
- **Audience toggle** — the `technical`/`business` field from §2 renders as
  a pill toggle using the *exact* existing `agent-mode-pills`/
  `agent-mode-pill` classes and `AgentModeToggle` structure from
  `ChatPanel.tsx`, not a second bespoke toggle.

## 6. "Production-ready, enterprise-grade" — translated into requirements

"Enterprise grade / bleeding edge / state of the art" isn't itself a spec
line — it's translated below into things that can pass or fail review, all
scoped to what this repo already has, not new dependencies:

- **Streaming resilience** — an AG-UI SSE drop mid-session must resume from
  the last consumed `RoundTableEntry`, not leave the timeline stuck on a
  spinner. Session has an explicit resumable cursor (last entry id/seq),
  same shape as reconnect logic AG-UI's `@ag-ui/client` (already a
  dependency) is built for.
- **Virtualized timeline** — long sessions (many chiefs × many cycles)
  render through `@tanstack/react-virtual` (already in `package.json`),
  not an unbounded `.map()`. No new list-virtualization dependency needed.
- **Per-chief failure isolation** — if one chief's loop errors mid-session,
  only that chief's lane shows a halt state (`agent-card-status-dot`'s
  existing `data-tone="halt"` treatment) — the rest of the session keeps
  streaming. One chief erroring must never blank the whole view.
- **Accessibility** — each appended entry lands in an `aria-live="polite"`
  region (screen readers get session updates without interrupting);
  keyboard nav moves focus chief-to-chief; the streaming pulse/shimmer
  respects `prefers-reduced-motion`.
- **Empty/loading/sealed states** — no entries yet, entries loading, and a
  sealed (read-only, replayable) session are three distinct, explicit UI
  states — not the same "loading spinner forever" for all three.
- **Responsive** — mobile stacks role label above entry text, same
  breakpoint pattern `.round-table-turn` already uses at 640px, not a
  separate mobile layout.
- **No new dependencies** — `@copilotkit/react-core` v2, `@ag-ui/client`,
  `@tanstack/react-virtual`, and Radix primitives are already installed;
  the live session ships from what's already in `package.json`.

## 7. Phased implementation plan

1. **Backend data + wiring** (§3, §4) — extend `FileTrailWriter`/
   `ITrailWriter` for session trails, add `RoundTableSession`/
   `RoundTableEntry` persistence under `.pmcro/trails/round-table/{id}/`,
   stand up `POST /roundtable/sessions`, `POST /roundtable/sessions/{id}/messages`,
   seal path.
2. **Frontend surface** — `RoundTableSession.tsx` + `/roundtable` route,
   AG-UI event subscription, reusing `AgentCard`-style chief chips,
   `agent-mode-pills` audience toggle, `.colony-card`/`.round-table-turn`
   timeline styling. No new visual system.
3. **Production hardening** (§6) — reconnect/resume cursor, virtualized
   list, per-chief failure isolation, a11y pass (aria-live, keyboard nav,
   reduced-motion), responsive pass, empty/loading/sealed states.
4. **Seal + replay** — a sealed session should render through the same
   read path `TrailView.tsx`/`app/trails/TrailsPageView.tsx` already use
   for sealed trails, so a Round Table session becomes just another
   trail artifact once sealed rather than a special-cased view.

Next action: start Phase 1 — extend `ITrailWriter`/`FileTrailWriter` and
add the `RoundTable*` types to `ProjectName.Core`, then wire the two
endpoints in `ProjectName.OrchestratorApi`. Say the word and I'll re-verify
`PmcroCycleWorkflow.cs`/`FileTrailWriter.cs`/`Loop/` against this live tree
(not the stale one) before writing any C#.
