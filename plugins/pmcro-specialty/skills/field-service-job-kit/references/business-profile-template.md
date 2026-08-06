# Business Profile Template

A business profile is a small JSON config — the *only* place a brand
name lives. This skill reads from it; it never contains a brand name
itself.

## Location
`businesses/<business_id>.json` (relative to the repo root running
this skill, e.g. `W:\pmcro-ai-company\businesses\chop-chop.json`)

## Schema
```json
{
  "business_id": "chop-chop",
  "business_name": "Chop Chop",
  "trade": "landscaping",
  "reports_to": "coo",
  "task_checklist": [
    "Mow / trim",
    "Edge walkways/beds",
    "Blow debris",
    "Haul away clippings/debris"
  ]
}
```

## Example instance — Chop Chop (landscaping)
```json
{
  "business_id": "chop-chop",
  "business_name": "Chop Chop",
  "trade": "landscaping",
  "reports_to": "coo",
  "task_checklist": [
    "Mow / trim",
    "Edge walkways/beds",
    "Blow debris",
    "Haul away clippings/debris"
  ]
}
```

Any other trade (cleaning, handyman, snow removal) is just a new
`businesses/<business_id>.json` file with a different `task_checklist`
— never a new skill.
