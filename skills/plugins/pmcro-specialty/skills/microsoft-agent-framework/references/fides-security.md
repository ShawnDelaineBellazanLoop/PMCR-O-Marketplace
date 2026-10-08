# FIDES Security Framework

A Python-native information-flow-control layer (`agent_framework.security`)
for gating tool usage in agent workflows.

## What It Does

FIDES tracks the provenance of data flowing through an agent -- in
particular, whether a given piece of context originated from an
untrusted source (e.g. a user-supplied document, an external API
response, scraped web content). It treats tool invocation as a
capability with a risk level, and lets you define policies that gate
high-risk tools (database writes, external API calls, file mutations)
behind those provenance checks.

## Typical Flow

1. Untrusted data enters the agent's context (e.g. a vendor-submitted
   document).
2. That data, or anything derived from it, is tagged as untrusted by
   FIDES's flow tracking.
3. If the agent's next step is a downstream tool call whose policy
   requires trusted input for that risk tier, FIDES blocks the call and
   raises a Human-in-the-Loop approval request instead of executing it
   silently.

## Why This Matters For PMCR-O

FIDES is MAF's native analog to the TYPE1/TYPE2 tool boundary --
both exist to stop an agent from taking a high-risk, hard-to-reverse
action on the basis of unverified/untrusted input without a human in
the loop. The difference: FIDES's gating is driven by data provenance
(where did this input come from) where PMCR-O's TYPE1/TYPE2 split is
driven by action class (is this action world-changing). The two are
complementary, not substitutes -- provenance-based gating catches
"trustworthy-looking action fed by untrustworthy data" cases PMCR-O's
purely action-class-based gate wouldn't.
