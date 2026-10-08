---
description: "Scaffold a new field-job kit for a registered business inside a target workspace folder. Usage: /field-service-job-kit:new-job <business_id> <target-folder> <client-or-job-name>"
---
# /field-service-job-kit:new-job

```
business_id: <first argument -- e.g. 'chop-chop', must have a businesses/<business_id>.json profile>
target_folder: <second argument -- absolute path to the workspace folder to scaffold into>
job_name: <remaining arguments -- client or job identifier>
repo_path: <the target repo root>
```

Read `businesses/<business_id>.json` (fail loudly if missing --
run `instantiate-business` first). Create the standard field-job kit
inside `target_folder`: `photos/before/`, `photos/after/`,
`checklist.md` (using the profile's `task_checklist`), `invoice.md`,
and `notes.md`. See `../references/job-kit-layout.md` for the exact
layout and file templates -- this command does not restate them.

## Steps

1. Confirm the request is job-folder scaffolding (this skill's Owns).
2. Read the business profile; do not proceed without it.
3. Run `scripts/job_folder_scaffolder.py <business_id> <target_folder> <job_name>`
   (not yet implemented on disk -- see SKILL.md Scripts note).
4. TYPE1 write -- dispatch through
   `/orchestrator:run-cycle field-service-job-kit "scaffold job kit for
   <job_name> (<business_id>) at <target_folder>"` so the trail records
   the exact business and path.
5. Report back the exact folder path and files created -- never claim
   success without listing what's actually on disk (EC-VERIFY-FIRST-001).

## Guardrails
- Never scaffold into a folder that already contains a `checklist.md`
  or `invoice.md` without explicit confirmation -- it may belong to a
  different job.
- The trail is the provenance record for *which business* and *where*
  a job kit was created; do not scaffold silently outside a dispatched
  cycle.
