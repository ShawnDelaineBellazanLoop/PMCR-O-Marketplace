# Technical Architecture Reference — CTO Domain

Architecture principles, security posture, and skill-pack validation.

## Architecture Review Dimensions

When reviewing a codebase, assess:

| Dimension | Questions |
|---|---|
| Structure | Does the directory layout reflect the domain model? |
| Contracts | Are interfaces explicit? Are boundaries enforced? |
| Dependencies | Circular dependencies? External dependency freshness? |
| Error Handling | Are failure modes handled or silently swallowed? |
| Testing | Test coverage? Are tests testing behavior or implementation? |
| Security | Input validation? AuthN/AuthZ? Secret management? |
| Observability | Logging, metrics, tracing? Can you debug from production data? |

## Security Posture Assessment

Check these surfaces:

1. **Authentication**: how are users/services identified?
2. **Authorization**: is access scoped to need? Are there god-mode tokens?
3. **Input boundaries**: every external input validated and sanitized?
4. **Secret management**: no hardcoded keys, tokens, or connection strings
5. **Dependency surface**: known vulnerabilities in direct/transitive deps?
6. **Data at rest**: encryption? access controls on storage?

Each finding gets: severity (CVSS-aligned), location (file/line), remediation.

## Skill-Pack Validation Checklist

When reviewing a new skill/plugin:

1. SKILL.md has: name, description, version, compatibility
2. Scripts are deterministic (same input → same output)
3. References are accurate and cite sources where applicable
4. Assets follow naming conventions
5. No hardcoded paths that assume a specific machine or repo layout
6. Guardrails section defines boundaries, not just aspirations