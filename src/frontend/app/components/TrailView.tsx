// src/frontend/app/components/TrailView.tsx
//
// ARCH-VISUAL-BRIDGE-001 (2026-07-20): extracted from PMCRO_UI's
// src/app/App.tsx, where `TrailView` exists only as an inline function
// component in the same file as everything else -- no standalone
// TrailView.tsx in that source repo to port 1:1.
//
// Two corrections made against the PMCRO_UI source during the port:
//
//   1. Disposition schema. PMCRO_UI's mock used a 4-value TrailDisposition
//      ("accept" | "needs-approval" | "needs-revision" | "reject") --
//      turns out that wasn't fictional. UPDATE 2026-07-20: it's the real
//      schema the session-level PMCR-O flow (/orchestrator:run-cycle etc.)
//      writes to disk, confirmed against trail-schema.md and real sealed
//      trails. The compiled C# runtime's LoopDisposition enum (Accept |
//      Retry | Halt, from LoopFrame.cs) is ALSO real, just not yet
//      exercised on disk. See ARCH-VISUAL-BRIDGE-002 below -- this file
//      now accepts both rather than assuming only one is real.
//
//   2. Data source. page.tsx's own Trails section originally said "No
//      trail-listing endpoint exists yet." UPDATE 2026-07-20: sealed
//      trails now exist on disk (.pmcro/trails/ceo/*, .pmcro/trails/cto/*)
//      -- a real read endpoint to populate this prop from them is the
//      Agent Directory feature currently being built, not done in this file.
"use client";

import { useState } from "react";

export type TrailRoleEntry = {
  seq: number | string;
  content: string;
  result?: "pass" | "fail" | "n/a" | "note" | "pass-with-finding";
};

export type TrailCycle = {
  number: string;
  plan: TrailRoleEntry[];
  make: TrailRoleEntry[];
  check: TrailRoleEntry[];
  reflect: TrailRoleEntry[];
};

// ARCH-VISUAL-BRIDGE-002 (2026-07-20): there are two real, independently
// verified trail producers writing into the same .pmcro/trails/<domain>/
// <uuid>/ convention, each with its OWN real disposition schema -- neither
// is a typo or a stale assumption:
//
//   1. The compiled C# runtime (PmcroLoop.cs / Loop/LoopFrame.cs,
//      ProjectName.OrchestratorService) -- confirmed by direct source
//      read: `public enum LoopDisposition { Accept, Retry, Halt }`.
//      3-value, PascalCase. What this type was originally built against.
//   2. The session-level PMCR-O flow (/orchestrator:run-cycle,
//      /ceo:approve-initiative, etc., run by Claude Code/Cowork/claude.ai
//      per the pmcro-loop skill) -- confirmed by direct read of
//      catalog/Platform/PMCR-O/skills/orchestrator/references/trail-schema.md
//      and real sealed trails on disk: 4-value, lowercase-hyphenated --
//      "accept" | "needs-approval" | "reject" | "needs-revision".
//
// As of 2026-07-20 every trail actually on disk is kind 2 (the compiled
// runtime's MakerAsync rewrite has compiled but never run a real cycle).
// This type accepts both real schemas rather than coercing one into the
// other.
export type TrailDisposition =
  | "Accept" | "Retry" | "Halt"
  | "accept" | "needs-approval" | "reject" | "needs-revision"
  | null;

export type Trail = {
  id: string;
  domain: string;
  trueIntent: string;
  requestedBy?: string;
  createdAt?: string;
  disposition: TrailDisposition;
  reason?: string;
  cycles: TrailCycle[];
};

// Reuses the real tone vocabulary already defined in globals.css
// (--colony-pass/--colony-retry/--colony-halt, .phase-rail-badge[data-tone])
// instead of inventing a parallel color set. Both real schemas map onto
// the same three tones.
export function dispositionTone(d: TrailDisposition): "pass" | "retry" | "halt" | null {
  switch (d) {
    case "Accept":
    case "accept":
      return "pass";
    case "Retry":
    case "needs-approval":
    case "needs-revision":
      return "retry";
    case "Halt":
    case "reject":
      return "halt";
    default:
      return null;
  }
}

// Title-cases whichever real value arrived ("needs-approval" ->
// "Needs Approval", "Accept" -> "Accept") instead of maintaining two
// separate label maps for the two schemas.
export function dispositionLabel(d: NonNullable<TrailDisposition>): string {
  return d
    .split("-")
    .map((w) => w[0].toUpperCase() + w.slice(1))
    .join(" ");
}

const ROLE_TABS = ["plan", "make", "check", "reflect"] as const;
type RoleTab = (typeof ROLE_TABS)[number];

function ResultTag({ result }: { result?: TrailRoleEntry["result"] }) {
  if (!result) return null;
  const tone =
    result === "pass" ? "pass" : result === "fail" ? "halt" : result === "note" || result === "pass-with-finding" ? "retry" : null;
  return (
    <span className="trail-entry-result" data-tone={tone ?? "neutral"}>
      {result}
    </span>
  );
}

function TrailEntryRow({ entry }: { entry: TrailRoleEntry }) {
  return (
    <div className="trail-entry-row">
      <span className="trail-entry-seq">{entry.seq}</span>
      <p className="trail-entry-content">{entry.content}</p>
      <ResultTag result={entry.result} />
    </div>
  );
}

export default function TrailView({ trail }: { trail: Trail | null }) {
  const [activeCycle, setActiveCycle] = useState(0);
  const [activeRole, setActiveRole] = useState<RoleTab>("plan");

  if (!trail) {
    return (
      <div className="trail-view trail-view--empty">
        <p className="phase-rail-idle">
          No trail selected -- ITrailWriter has no read endpoint yet, so this view has no live data
          source to load from.
        </p>
      </div>
    );
  }

  const tone = dispositionTone(trail.disposition);
  const cycle = trail.cycles[activeCycle];
  const entries: Record<RoleTab, TrailRoleEntry[]> = {
    plan: cycle?.plan ?? [],
    make: cycle?.make ?? [],
    check: cycle?.check ?? [],
    reflect: cycle?.reflect ?? [],
  };

  return (
    <div className="trail-view">
      <div className="trail-view-header">
        <div className="trail-view-header-line">
          <span className="trail-view-domain">{trail.domain}</span>
          <span className="trail-view-id">{trail.id.slice(0, 8)}…</span>
          {tone && (
            <span className="phase-rail-badge" data-tone={tone}>
              {dispositionLabel(trail.disposition as NonNullable<TrailDisposition>)}
            </span>
          )}
        </div>
        <p className="trail-view-intent">{trail.trueIntent}</p>
        {(trail.requestedBy || trail.createdAt) && (
          <div className="trail-view-header-line trail-view-header-line--muted">
            {trail.requestedBy && <span>{trail.requestedBy}</span>}
            {trail.createdAt && <span>{trail.createdAt}</span>}
          </div>
        )}
      </div>

      {trail.reason && (
        <div className="trail-view-reason">
          <p className="trail-view-reason-label">Disposition reason</p>
          <p className="trail-view-reason-text">{trail.reason}</p>
        </div>
      )}

      <div className="trail-view-tabs">
        <div className="trail-view-cycle-tabs">
          {trail.cycles.map((c, i) => (
            <button
              key={c.number}
              type="button"
              className="trail-view-tab"
              data-active={activeCycle === i}
              onClick={() => setActiveCycle(i)}
            >
              Cycle {c.number}
            </button>
          ))}
        </div>
        <div className="trail-view-role-tabs">
          {ROLE_TABS.map((r) => (
            <button
              key={r}
              type="button"
              className="trail-view-tab trail-view-tab--role"
              data-active={activeRole === r}
              onClick={() => setActiveRole(r)}
            >
              {r}
            </button>
          ))}
        </div>
      </div>

      <div className="trail-view-entries">
        {entries[activeRole].length === 0 ? (
          <p className="phase-rail-idle">No {activeRole} entries for this cycle.</p>
        ) : (
          entries[activeRole].map((entry, i) => <TrailEntryRow key={i} entry={entry} />)
        )}
      </div>
    </div>
  );
}
