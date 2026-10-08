---
description: "Register a new field-service business instance (brand + trade) against this generic pattern. Usage: /field-service-job-kit:instantiate-business <business_id> <business_name> <trade>"
---
# /field-service-job-kit:instantiate-business

```
business_id: <first argument -- short slug, e.g. 'chop-chop'>
business_name: <second argument -- display brand, e.g. 'Chop Chop'>
trade: <third argument -- e.g. 'landscaping'>
repo_path: <the target repo root>
```

Create `businesses/<business_id>.json` at `repo_path` per
`../references/business-profile-template.md`. This is the ONLY place
the brand name is written — `new-job` and `job-report` read it from
here, they never hardcode it.

## Steps

1. Confirm no `businesses/<business_id>.json` already exists — do not
   overwrite an existing business without explicit confirmation.
2. Ask (or infer from context) the trade-specific `task_checklist`.
3. Write the profile JSON.
4. This is a TYPE1 write -- dispatch through
   `/orchestrator:run-cycle field-service-job-kit "instantiate business
   <business_name> (<trade>)"` so the trail records exactly what
   business was created and where.
5. Report the exact file path written -- never claim the business was
   registered without listing what's actually on disk.

## Guardrails
- Never hardcode `business_name` anywhere outside the profile JSON.
- One profile per business_id; distinct businesses never share a file.
