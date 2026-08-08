# Field-Service Job-Kit Domain Skill

Documentation-only specialty scope for the PMCR-O Colony. Reusable
across any field-service business (landscaping, cleaning, handyman,
snow removal, etc.) — the business itself is a **parameter**
(`business_name`), never hardcoded into this skill. Consulted by
pmcro-loop when a cycle's true_intent falls under it. Never
reimplements the PMCR-O loop itself.

## Owns
- Job/workspace folder scaffolding for any field-service business
- Field job documentation (before/after photos, checklist, notes)
- Invoice assembly from completed job data
- Business-profile instantiation (registering a new brand/instance —
  e.g. "Chop Chop" for landscaping — against this generic pattern)

## Does Not Own
- Strategic direction (`ceo`)
- Budgeting/cash flow (`cfo`)
- Company-wide SOP design (`coo`)
- Legal/contract terms (`clo`)
- Property-preservation's separate domain (`property-preservation`)
- Chat/session distillation or memory hydration (`domain-specialist`)

## Domain Consulting Pattern

Each domain is documentation-only scope. When a cycle's `true_intent`
falls under field-service work, pmcro-loop consults this skill. It
**decides and routes** — it never performs the specialist work itself.

## Commands
- `instantiate-business` — register a new business instance (brand
  name + trade) against this generic pattern
- `new-job` — scaffold a new field-job kit in a target folder
- `job-report` — assemble a completed job's report/invoice

## Guardrails
1. Never bake a specific brand/business name into this skill's own
   identity — brand is always a parameter, read from a business
   profile, never hardcoded in SKILL.md, commands, or references.
2. Every scaffold names the exact target folder path — never "somewhere."
3. Approve job-kit scope; delegate execution. Do not do the field work.
4. "No action needed" is a valid decision — not every input requires a new kit.
