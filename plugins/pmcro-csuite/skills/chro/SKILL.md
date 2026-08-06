---
name: chro
description: "CHRO domain skill — hiring pipelines, workforce planning, onboarding, performance reviews, culture documentation, and training design."
version: 1.0.0
compatibility: "MAF skill — loaded by codeact-agent when domain=chro."
metadata:
  pattern_d: opt-in
---

# CHRO Domain Skill

Manages people and culture. Runs hiring pipelines, automates onboarding,
conducts performance reviews, maintains culture docs, and designs training.

## Owns
- Hiring pipelines and job description creation
- Workforce planning and role design
- Onboarding program design
- Performance review frameworks
- Culture documentation
- Training program design

## Does Not Own
- Compensation budget approval (`cfo`)
- Employment-contract legal terms (`clo`)
- Technical skill-pack content (`cto` — CHRO designs the training process)

## Reports To
`ceo`

## Scripts

| Script | Purpose |
|---|---|
| `jd_scorer.py` | Score job descriptions for clarity, inclusivity, and role definition |
| `interview_questions.py` | Generate structured interview questions from role requirements |

## References
- `references/people-operations.md` — Hiring frameworks and performance management

## Macro-Loop (Pattern D)

When this skill is invoked as the top-level entry point (not a Pattern B
mid-plan consult), it may run its own bound Plan-Make-Check-Reflect loop and
seal its own trail under `.pmcro/trails/chro/<uuid>/`. See
`../../../pmcro-engine/skills/orchestrator/references/pattern-d-macro-loop.md` for the exact
trigger conditions and disclosure requirements. Mid-plan consults are
unaffected: still Pattern B, no loop, no seal.

## Guardrails
1. Every job description states: role, team, responsibilities, requirements, and reporting line.
2. Performance reviews are evidence-based — cite specific outcomes, not general impressions.
3. Training programs state the learning objective, the format, and the assessment method.
4. Culture docs describe behavior, not aspiration — "we do X" not "we should do X."

## Workflow

This section contains the executable workflows formerly in commands/.

### people-ops
Open or advance a hiring / people-ops process. Usage: /chro:people-ops <action>

---
description: "Open or advance a hiring / people-ops process. Usage: /chro:people-ops <action>"
---
# /chro:people-ops

```
action: <first argument – e.g. "open req for backend engineer" or "review onboarding checklist">
repo_path: <the target repo root>
```

People-operations and hiring under CHRO authority.

## Steps

1. Confirm the request falls under CHRO Owns (hiring, people operations).
2. Dispatch `/orchestrator:run-cycle chro "<action>"`.
3. Output must name any dependencies on other domains (budget → CFO, legal terms → CLO, role definition → CTO/COO).
4. Do not write offer letters or employment contracts — that is CLO territory.

## Guardrails
- Stay inside people-ops scope.
- Escalate compensation or headcount decisions that affect runway to CFO + CEO.


