---
description: "Assemble a completed job's invoice/report from its field-job kit. Usage: /field-service-job-kit:job-report <business_id> <job-folder>"
---
# /field-service-job-kit:job-report

```
business_id: <first argument -- e.g. 'chop-chop'>
job_folder: <second argument -- absolute path to the scaffolded job kit>
repo_path: <the target repo root>
```

Assemble a structured job report from an existing job kit
(`checklist.md`, `notes.md`, `photos/before|after/`) into a finished
`invoice.md`, using the `business_id`'s profile for brand name/trade.

## Steps

1. Confirm the request is job-report/invoice assembly (this skill's Owns).
2. Read `checklist.md` and `notes.md` from `job_folder` -- do not invent
   line items or work not recorded there.
3. Separate three sections: work performed, photo evidence reference
   (folder counts only, not contents), amount due.
4. Run `/orchestrator:run-cycle field-service-job-kit "job report for
   <job_folder> (<business_id>)"`.
5. Do not blend the three sections into a single narrative note.

## Guardrails
- Checklist and notes are evidence; do not fabricate work performed.
- If `checklist.md` or `notes.md` is missing, say so -- do not guess.
- This skill does not own chat/session distillation -- route that to
  `domain-specialist`.
