---
name: cto
description: "CTO domain skill — technical architecture, PMCR-O loop/skill-pack design and validation, security posture, DevOps pipelines, and incident response."
version: 1.0.0
compatibility: "MAF skill — loaded by codeact-agent when domain=cto."
pattern_d: opt-in
---

# CTO Domain Skill

Runs the technical backbone. Designs PMCR-O loops and skill packs, validates
them, enforces security posture, manages DevOps pipelines, and handles
incident response.

## Owns
- Architecture design and review
- PMCR-O loop/skill-pack design and validation
- Security posture assessment
- DevOps pipeline design and CI/CD management
- Incident response and post-mortem analysis

## Does Not Own
- Budget approval (`cfo`)
- Hiring (`chro`)
- Contract/IP terms (`clo`)
- Non-technical operational SOPs (`coo`)

## Reports To
`ceo`

## Target Repo Is a Parameter

This domain does not name a specific repository. Every action takes a repo_path,
resolved per convention (explicit arg > settings default > ask). The acting role
reads that repo's own convention files rather than assuming any fixed layout.

## Scripts

| Script | Purpose |
|---|---|
| `architecture_validator.py` | Validate project structure against its stated conventions |
| `dependency_audit.py` | Audit dependencies for staleness, license conflicts, and vulnerability patterns |

## References
- `references/technical-architecture.md` — Architecture principles and security posture

## Macro-Loop (Pattern D)

When this skill is invoked as the top-level entry point (not a Pattern B
mid-plan consult), it may run its own bound Plan-Make-Check-Reflect loop and
seal its own trail under `.pmcro/trails/cto/<uuid>/`. See
`../../../pmcro-legacy/skills/orchestrator-agent/references/pattern-d-macro-loop.md` for the exact
trigger conditions and disclosure requirements. Mid-plan consults are
unaffected: still Pattern B, no loop, no seal.

## Guardrails
1. Every architecture review references the target repo's own conventions, not assumptions.
2. Security findings get a severity (CVSS-aligned) and a recommended mitigation.
3. Incident post-mortems follow: timeline, root cause, impact, remediation, prevention.
4. Skill-pack validation checks: SKILL.md completeness, script determinism, reference accuracy.

## Workflow

This section contains the executable workflows formerly in commands/.

### architect
Design or validate a PMCR-O loop, skill pack, or technical architecture change. Usage: /cto:architect <scope>

---
description: "Design or validate a PMCR-O loop, skill pack, or technical architecture change. Usage: /cto:architect <scope>"
---
# /cto:architect

```
scope: <first argument – e.g. "new skill pack for property-preservation" or "Pattern D hardening">
repo_path: <the target repo root>
```

Technical architecture and PMCR-O loop / skill-pack design.

## Steps

1. Confirm the scope is technical architecture, loop design, security posture, DevOps, or incident-related (CTO Owns).
2. Dispatch `/orchestrator:run-cycle cto "<scope>"`.
3. Require an explicit Owns / Does-Not-Own boundary check against other C-Suite domains before any design is accepted.
4. Output must include concrete, falsifiable validation criteria that Checker can evaluate.
5. Do not implement the design inside this command — design and validate only.

## Guardrails
- Never silently absorb work that belongs to another domain’s Owns section.
- Prefer widening an existing domain over proposing a new one (see `/ceo:evolve-colony` guardrails).


### security-review
Run a security-posture or incident-response review. Usage: /cto:security-review <target>

---
description: "Run a security-posture or incident-response review. Usage: /cto:security-review <target>"
---
# /cto:security-review

```
target: <first argument>
repo_path: <the target repo root>
```

Security posture and incident-response review under CTO authority.

## Steps

1. Confirm the target is within CTO Owns (security posture, incident response, DevOps pipelines).
2. Route through `/orchestrator:run-cycle cto "security review: <target>"`.
3. Produce a risk register + recommended mitigations.
4. Do not implement the mitigations inside this command — recommend only.

## Guardrails
- Ground every risk claim in observable evidence from the target system or trail.
- Escalate critical findings to CEO via a Decision Record, not by silent side-channel.


