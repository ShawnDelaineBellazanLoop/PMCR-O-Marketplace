# Field-Service Job-Kit Layout

Generic layout for ANY field-service business instance. `<business_name>`
and `<trade>` come from that business's profile (see
`business-profile-template.md`) — never hardcoded here.

## Layout

Scaffolded into `<target_folder>/<job_name>/`:

```
<job_name>/
  photos/
    before/        (empty, ready for uploads)
    after/         (empty, ready for uploads)
  checklist.md
  invoice.md
  notes.md
```

## checklist.md template
```
# Job Checklist — <job_name>
Business: <business_name> (<trade>)

- [ ] <trade-specific tasks — pulled from business profile's task list>
- [ ] Before photos taken
- [ ] After photos taken
- [ ] Client walkthrough / sign-off
```

## invoice.md template
```
# Invoice — <job_name>
Business: <business_name>
Date:
Client:
Address:

| Line Item | Qty | Rate | Total |
|---|---|---|---|
|   |   |   |   |

Subtotal:
Total Due:
```

## notes.md template
```
# Field Notes — <job_name>

## Observed conditions

## Work performed

## Follow-up needed
```

## Provenance
Every field created here is logged by the dispatching cycle's trail
(`.pmcro/trails/field-service-job-kit/<uuid>/`) — the trail records the
exact `business_name` and `target_folder` scaffolded, not just that
scaffolding happened.
