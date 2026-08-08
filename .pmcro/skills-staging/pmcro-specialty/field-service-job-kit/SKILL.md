---
name: field-service-job-kit
description: "Field-Service Job-Kit domain skill — reusable job-workspace scaffolding, before/after photo kits, field checklists, notes, and invoice assembly for ANY field-service business. Brand/trade is a parameter (business profile), never hardcoded into this skill."
version: 1.0.0
compatibility: "MAF skill — loaded by codeact-agent when domain=field-service-job-kit."
metadata:
  pattern_d: opt-in
---

# Field-Service Job-Kit Domain Skill

Reusable industry-agnostic execution specialist for field-service
businesses (landscaping, cleaning, handyman, snow removal, etc). The
skill itself never names a business — every brand runs through
`businesses/<business_id>.json` (see
`references/business-profile-template.md`). "Chop Chop" (landscaping)
is the first instance, not part of this skill's identity.

## Owns
- Business-profile instantiation (`instantiate-business`)
- Job/workspace folder scaffolding — creating the standard field-job
  kit (before/after photo folders, checklist, invoice, notes) for a
  registered business
- Field job documentation workflows (checklist + notes capture)
- Invoice assembly from a completed job kit
- Provenance logging of exactly which business and where a job kit
  was created

## Does Not Own
- Company-wide SOP design (`coo` — this domain executes within COO's
  operational framework)
- Financial reporting / books (`cfo`)
- Contract legal terms (`clo`)
- Property-preservation's separate domain (`property-preservation`)
- Chat/session distillation, pattern extraction, memory hydration
  (`domain-specialist`)

## Reports To
`coo`

## Scripts

| Script | Purpose |
|---|---|
| `job_folder_scaffolder.py` | Read a business profile + create the standard field-job kit (photos/before, photos/after, checklist.md, invoice.md, notes.md) inside a target folder |
| `job_invoice_assembler.py` | Read a job kit's checklist + notes and assemble a finished invoice.md |

**Note (EC-VERIFY-FIRST-001):** as of this cycle, `scripts/` is empty —
the two scripts above are documented as the intended interface but not
yet implemented on disk. Do not report job scaffolding as automated
until both files exist and a cycle has produced an ACCEPT-disposition
trail exercising them. Until then, `new-job`/`job-report` describe a
manual procedure a cycle's Maker step performs directly.

## References
- `references/business-profile-template.md` — the ONLY place a brand
  name/trade lives; schema + Chop Chop example instance
- `references/job-kit-layout.md` — generic job-kit layout and templates

## Macro-Loop (Pattern D)

When this skill is invoked as the top-level entry point (not a Pattern B
mid-plan consult), it may run its own bound Plan-Make-Check-Reflect loop
and seal its own trail under
`.pmcro/trails/field-service-job-kit/<uuid>/`. See
`../../../pmcro-engine/skills/orchestrator/references/pattern-d-macro-loop.md`
for the exact trigger conditions and disclosure requirements. Mid-plan
consults are unaffected: still Pattern B, no loop, no seal.

## Guardrails
1. NEVER hardcode a brand/business name into this skill's SKILL.md,
   commands, or references — brand is always read from a
   `businesses/<business_id>.json` profile. A new trade or a new brand
   is a new profile, never a new skill.
2. Every scaffold names the exact target folder path AND business_id —
   never "somewhere" or "the job folder."
3. Job-kit scaffolding and business instantiation are TYPE1 writes —
   always dispatch through `/orchestrator:run-cycle
   field-service-job-kit ...` so the trail records the exact path.
4. Checklist and notes are evidence for `job-report`; never fabricate
   work performed or line items not recorded there.
5. This skill does not own chat/session distillation — route that to
   `domain-specialist`.
6. Never scaffold into a folder that already contains a `checklist.md`
   or `invoice.md` without explicit confirmation.

## Workflow

This section contains the executable workflows formerly in commands/.


### instantiate-business
Register a new field-service business instance (brand + trade). Usage: /field-service-job-kit:instantiate-business <business_id> <business_name> <trade>

---
description: "Register a new field-service business instance (brand + trade) against this generic pattern. Usage: /field-service-job-kit:instantiate-business <business_id> <business_name> <trade>"
---
```
business_id: <first argument -- short slug, e.g. 'chop-chop'>
business_name: <second argument -- display brand, e.g. 'Chop Chop'>
trade: <third argument -- e.g. 'landscaping'>
repo_path: <the target repo root>
```

Write `businesses/<business_id>.json` per
`references/business-profile-template.md` -- the only place the brand
name lives. `new-job`/`job-report` read it from here, never hardcode it.

1. Confirm no profile already exists for `business_id`.
2. Determine the trade-specific `task_checklist`.
3. Write the profile JSON.
4. TYPE1 write -- dispatch through `/orchestrator:run-cycle
   field-service-job-kit "instantiate business <business_name>
   (<trade>)"`.
5. Report the exact file path written -- never claim success without
   listing what's on disk.

### new-job
Scaffold a new field-job kit for a registered business. Usage: /field-service-job-kit:new-job <business_id> <target-folder> <client-or-job-name>

---
description: "Scaffold a new field-job kit for a registered business inside a target workspace folder. Usage: /field-service-job-kit:new-job <business_id> <target-folder> <client-or-job-name>"
---
```
business_id: <first argument -- must have a businesses/<business_id>.json profile>
target_folder: <second argument -- absolute path to scaffold into>
job_name: <remaining arguments -- client or job identifier>
repo_path: <the target repo root>
```

Read the business profile (fail loudly if missing -- run
`instantiate-business` first). Create `photos/before/`,
`photos/after/`, `checklist.md` (using the profile's task list),
`invoice.md`, `notes.md` inside `target_folder`. See
`references/job-kit-layout.md` for exact layout/templates.

1. Confirm the request is job-folder scaffolding.
2. Read the business profile; do not proceed without it.
3. TYPE1 write -- dispatch through `/orchestrator:run-cycle
   field-service-job-kit "scaffold job kit for <job_name>
   (<business_id>) at <target_folder>"`.
4. Report the exact folder path and files created -- never claim
   success without listing what's on disk (EC-VERIFY-FIRST-001).

### job-report
Assemble a completed job's invoice/report from its field-job kit. Usage: /field-service-job-kit:job-report <business_id> <job-folder>

---
description: "Assemble a completed job's invoice/report from its field-job kit. Usage: /field-service-job-kit:job-report <business_id> <job-folder>"
---
```
business_id: <first argument>
job_folder: <second argument -- absolute path to the scaffolded job kit>
repo_path: <the target repo root>
```

Assemble a structured job report from an existing job kit
(`checklist.md`, `notes.md`, `photos/before|after/`) into a finished
`invoice.md`, using the business profile for brand name/trade.

1. Confirm the request is job-report/invoice assembly.
2. Read `checklist.md` and `notes.md` -- do not invent line items.
3. Separate three sections: work performed, photo evidence reference
   (folder counts only), amount due.
4. Run `/orchestrator:run-cycle field-service-job-kit "job report for
   <job_folder> (<business_id>)"`.
5. Do not blend the three sections into a single narrative note.

## Guardrails (Workflow-level)
- Checklist and notes are evidence; do not fabricate work performed.
- If `checklist.md` or `notes.md` is missing, say so -- do not guess.
- This skill does not own chat/session distillation -- route to
  `domain-specialist`.
