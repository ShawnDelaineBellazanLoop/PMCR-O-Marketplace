# PMCR-O Autonomous AI Agent Company: 100 QA items

Status words: VERIFIED (checked in this session), FAIL (checked and wrong), MISSING (referenced but absent), NOT BUILT (backlog says no), OPEN (needs a founder decision), NOT VERIFIED (claimed, not checked).

Important: these statuses are the maker's own assessment. The checker and the Auditor must confirm them independently before any row is trusted. The approval gates (spend, external send, installs, deletes, git push) are in scope for every pass.

## Pass 1: identity, laws, trail (1-25)

| # | Check | Status | Evidence |
|---|---|---|---|
| 1 | Founder raw intent recorded verbatim | NOT VERIFIED | transcript only |
| 2 | Five laws written in one file | NOT VERIFIED | not located in repo |
| 3 | Plan, make, check, reflect are separate passes | NOT VERIFIED | design only |
| 4 | Checker is independent of maker | NOT VERIFIED | no test |
| 5 | Agent cannot certify its own work | NOT BUILT | no check |
| 6 | No absolute paths in trails | FAIL | 111 of 310 files |
| 7 | No first-person "I" in trail text | FAIL | 16 files |
| 8 | Frames are append-only | NOT VERIFIED | not measured |
| 9 | Frame schema defined | NOT BUILT | no speaker, addressee, act |
| 10 | Each trail has a trail_id | VERIFIED | 00-frame.json |
| 11 | Cycle number recorded | NOT VERIFIED | not measured |
| 12 | Seed intent recorded | VERIFIED | 00-frame.json seed_intent |
| 13 | Start time recorded | VERIFIED | 00-frame.json started_utc |
| 14 | Replay result (MATCH or MISMATCH) on frames | NOT BUILT | 0 files |
| 15 | Score block on trails | NOT BUILT | none |
| 16 | Simulated trails labelled as non-evidence | FAIL | ADR 0013 missing |
| 17 | Sellable trail needs checker PASS, auditor pass, replay MATCH | NOT BUILT | backlog #5 |
| 18 | Redaction before any export | NOT BUILT | backlog #68 |
| 19 | Export manifest and hash | NOT BUILT | backlog #68 |
| 20 | Ownership and licensing reviewed by CLO | NOT BUILT | needs a lawyer |
| 21 | Round-table transcript page reviewed for disclosure | NOT VERIFIED | in pmcro-round-table |
| 22 | Canonical repo decided | OPEN | backlog #6 |
| 23 | Glossary: replay, execute, simulation | VERIFIED | qa/PMCRO-QA.md |
| 24 | Philosophy separated from application docs | NOT BUILT | backlog #23 |
| 25 | docs/ folder with ADRs exists | MISSING | no docs/ here |

## Pass 2: orchestration, queue, approval (26-50)

| # | Check | Status | Evidence |
|---|---|---|---|
| 26 | Orchestrator runs as a gRPC service | VERIFIED | ProjectName.OrchestratorService |
| 27 | RunCycle endpoint returns four role outputs | VERIFIED | ran; state complete |
| 28 | Planner, maker, checker, reflector run as separate agents | VERIFIED | orchestrator logs |
| 29 | Checker gets maker output in its own turn | VERIFIED | PmcroCycle.cs turn B |
| 30 | Reflector emits a structured next seed | NOT BUILT | text only |
| 31 | Next seed placed in the message queue | NOT BUILT | no queue |
| 32 | Queue schema | NOT BUILT | none |
| 33 | HIGH priority drained first | NOT BUILT | none |
| 34 | Queue is durable off this machine | NOT BUILT | backlog #16 |
| 35 | Queue never stalls on unprioritized seeds | NOT BUILT | backlog #115 risk |
| 36 | Loop has a cap | NOT BUILT | v1 runs one cycle |
| 37 | Standing approval policy | NOT BUILT | backlog #58 |
| 38 | Approval edge: spend | NOT BUILT | none |
| 39 | Approval edge: external send | NOT BUILT | none |
| 40 | Approval edge: installs | NOT BUILT | none |
| 41 | Approval edge: deletes | NOT BUILT | none |
| 42 | Approval edge: git push | NOT BUILT | none |
| 43 | Secrets never written to trails | NOT BUILT | backlog #67 |
| 44 | Error-lesson step | NOT BUILT | backlog #67 |
| 45 | Halt on a repeated defect | NOT BUILT | backlog #67 |
| 46 | Cascade depth cap | NOT BUILT | backlog #41 |
| 47 | Chief file calls the orchestrator service | NOT BUILT | founder design |
| 48 | Overnight wake-up report | NOT BUILT | backlog #74 |
| 49 | Accountability alarm with due times | NOT BUILT | backlog #59 |
| 50 | Delegation with due times | NOT BUILT | backlog #61 |

## Pass 3: skills, marketplace, evals (51-75)

| # | Check | Status | Evidence |
|---|---|---|---|
| 51 | Marketplace plugin per capability | OPEN | canonical repo undecided |
| 52 | Skills have scripts, references, assets | NOT VERIFIED | not checked |
| 53 | Skill size guard (150 lines) | NOT VERIFIED | backlog #54 claims it |
| 54 | New-skill scaffold | NOT VERIFIED | backlog #54 claims it |
| 55 | Accept or deny gate on skill calls | NOT VERIFIED | backlog #55 claims it |
| 56 | Skill is called only if its plugin exists | NOT BUILT | founder law, unenforced |
| 57 | Eval quality gate | MISSING | eng/eval-quality absent |
| 58 | Eval file per skill | NOT BUILT | AGENTS.md says pending |
| 59 | Evals run in CI without model credits | NOT VERIFIED | backlog #3 claims it |
| 60 | LLM eval job | NOT BUILT | backlog #3 |
| 61 | Eval across model families | NOT BUILT | backlog #52 |
| 62 | With and without skill comparison | NOT BUILT | backlog #52 |
| 63 | Templated skills tested on a small model | NOT BUILT | backlog #54 |
| 64 | Dated model capability profiles | NOT BUILT | backlog #50 |
| 65 | Follows dotnet/skills structure | NOT VERIFIED | backlog #9 says partly |
| 66 | Skill sync step | OPEN | backlog #56 |
| 67 | Generators write only outside plugins/ | OPEN | backlog #60 |
| 68 | MCP servers exist | NOT VERIFIED | backlog #29 |
| 69 | Live MCP run | NOT BUILT | backlog #7 |
| 70 | Cloudflare connected to a real account | NOT BUILT | backlog #14 |
| 71 | Self-hosted runner | NOT BUILT | needs outbound access |
| 72 | Figma plugin run in Figma | NOT BUILT | backlog #19 |
| 73 | OpenAPI to MCP generator | NOT VERIFIED | backlog #49 claims it |
| 74 | Platform connectors reviewed by CLO | NOT BUILT | none |
| 75 | Money tools ask first | NOT BUILT | backlog #57 |

## Pass 4: economics, identity, operations (76-100)

| # | Check | Status | Evidence |
|---|---|---|---|
| 76 | Spend ceiling set by founder | NOT BUILT | CFO seat |
| 77 | Cost per cycle recorded | NOT BUILT | none |
| 78 | Cost per accepted cycle | NOT BUILT | none |
| 79 | Revenue per piece recorded | NOT BUILT | backlog #31 |
| 80 | Account registry with dated limits | NOT BUILT | backlog #35 |
| 81 | No workload split to evade a limit | NOT BUILT | backlog #35 rule |
| 82 | Channel registry with red-flag check | NOT BUILT | backlog #40 |
| 83 | Approval before first public post | NOT BUILT | backlog #40 |
| 84 | Brand search limited to own name | NOT BUILT | backlog #36 |
| 85 | Identity profile per person | NOT BUILT | backlog #75 |
| 86 | Agent discloses it is AI in communities | NOT BUILT | backlog #43 |
| 87 | Secrets attached at call time only | NOT BUILT | backlog #34 |
| 88 | Training data from human-validated trails only | NOT BUILT | none |
| 89 | Replay-generated frames tagged separately | NOT BUILT | none |
| 90 | Export with redaction | NOT BUILT | backlog #68 |
| 91 | Score block fields (replays, passes, dates, checker id) | NOT BUILT | none |
| 92 | Auditor samples passed trails | NOT BUILT | csuite:auditor seat, not in repo |
| 93 | CLO reviews provider terms | NOT BUILT | none |
| 94 | Kill switch for a runaway loop | NOT BUILT | none |
| 95 | Queue keeps running; only irreversible steps wait | NOT BUILT | founder design |
| 96 | Trail backup off this machine | NOT BUILT | backlog #16 |
| 97 | Replay from trail after a crash | NOT BUILT | replay not built |
| 98 | Newcomer README | NOT BUILT | backlog #23 |
| 99 | ADRs for application rules | MISSING | no docs/ here |
| 100 | Weekly QA by a separate Auditor sitting | NOT BUILT | none |

## Tally (maker's count, unconfirmed)

Counts are taken from the status column above.

- VERIFIED: 8
- FAIL: 3
- MISSING: 3
- NOT BUILT: 67
- OPEN: 4
- NOT VERIFIED: 15
- Total: 100
