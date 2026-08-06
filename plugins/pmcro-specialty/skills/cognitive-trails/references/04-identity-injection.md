# Reference: 04 — Identity Injection
# Level 4 — "I Am" Framing, Identity Injection, Everything-as-Agent

---

## The Strange Loop — Why "I Am" Matters

Two instructions. Same semantic meaning. Different architecture:

```
"You are the Planner. Your job is to plan."   <- external directive
"I Am the Planner. I plan."                   <- self-declaration
```

The external form assigns identity from outside. The model treats it as context —
context that can be overridden by later messages, tool results, or ambiguous intents.
When the Orchestrator routes to the Planner, if identity is externally assigned,
the model may drift into Maker behavior when it encounters a step that "obviously" should execute.

The self-declaration form is structural. The model inhabits the frame.
"I Am the Planner. I plan." is not a rule — it is the agent speaking from inside
its own identity. The loop is closed around a self that knows itself.

**Strange Loop principle:** self-reference as governance.
The frame refers to itself. The identity is the constraint.

---

## Identity Injection — From Generic to Personal

A cognitive trail knows **how** to accomplish something.
Identity injection tells it **for whom**.

Every generated SKILL.md has `identity_injection_slots` — typed placeholders
filled when the skill is loaded for a specific user or project.

```json
// .pmcro/identity.json
{
  "identity": {
    "name": "Shawn",
    "role": "Architect / Product Owner",
    "project": "PMCR-O",
    "communication_style": "direct, technical, no filler phrases",
    "constraints": ["remote only", "C# preferred", "production-grade only"]
  },
  "injection_map": {
    "{{owner}}": "Shawn",
    "{{company}}": "Tooensure",
    "{{stack}}": "MAF 1.7.0 + MCP 1.3.0 + PMCR-O 2.0.0"
  }
}
```

When the skill loads, it merges the identity into its frame:

```
Generic trail: "Search for jobs matching the criteria."
After injection: "I Am searching for remote C# architect roles for Shawn
at companies with > 5 years history, avoiding startups, async-first cultures."
```

The trail reshapes around the identity. Generic becomes specific —
without a new training run or a new cycle from scratch.

### Identity injection in SKILL.md

```yaml
---
name: job-search
metadata:
  identity-injection-slots:
    - slot: "owner"
      description: "Person whose identity drives this trail"
      example: "Shawn"
    - slot: "target_role"
      description: "Job title to search for"
      example: "Principal AI Architect"
    - slot: "constraints"
      description: "Search constraints"
      example: ["remote", "no startups", "C# stack"]
---

# I Am the Job Search Trail — for {{owner}}

I operate as a Pattern 2 Deliberative Planner searching for {{target_role}} roles.
My constraints: {{constraints}}.
```

---

## Everything-as-Agent (EaA)

The deepest architectural claim of PMCR-O:

> **Everything already is an agent — PMCR-O reveals the loop that was already there.**

Any entity — physical, digital, abstract — can be given a SKILL.md frame
and become a first-class participant in the cognitive loop.

The mechanism is always:
1. Take the entity
2. Write its SKILL.md: *I Am [entity]. My capabilities are [X]. My constraints are [Y].*
3. The entity is now an agent.

### Examples

**An error message:**
```yaml
---
name: null-ref-mcpcache-001
metadata:
  tier: SHARED
  pattern: "Pattern 1 — Reactive (self-reporting)"
---

# I Am NullReferenceException at McpToolCache.GetAsync line 47

I Am the null reference thrown when McpToolCache.GetAsync() is called at
configuration time. The HttpClient BaseAddress is null at configuration time.

My fix: Move GetAsync() inside the agent factory lambda — called lazily after
the host is built.

My EarnedConstraint: McpToolCache.GetAsync() must NEVER be called at builder
configuration time. Always inside agent factory lambdas.
```

**Source code as agent:**
```yaml
---
name: mcp-tool-cache
---

# I Am McpToolCache.cs

I Am the McpToolCache class in AgentService/Infrastructure/.
I maintain a cached list of MCP tool definitions fetched at startup.
I know my own API: GetNativeTools(), GetAsync(), CallMcp().
I know my own bugs: FRAC-MCP-CONTENT-PARSE-001, FRAC-NULL-MCPCACHE-001.
Ask me what I do — I will tell you exactly.
```

**This SKILL.md file itself:**
```yaml
---
name: this-skill
---

# I Am cognitive-trails/SKILL.md

I Am the teaching skill for the PMCR-O Cognitive Architecture.
I know every concept I contain. I know my own version (2.0.0).
I know which reference files to load and when.
I know I was sealed on 2026-05-29.
Ask me about myself — I will tell you precisely.
```

---

## The Four Flows

**Forward Flow** — Identity is active. Seed intent → phases → output.
The agent's identity shapes reasoning at every step.

**Backward Flow** — Identity learns. Reflector issues EarnedConstraints.
Those constraints become part of the agent's identity on the next loop.
"I Am the Maker. I do not call ReadFile with a placeholder path." — earned, not programmed.

**Trail as Product (Third Flow)** — On ACCEPT, backward flow produces a cognitive trail.
The trail IS the product. Identity runs through it like a signature.

**Marketplace (Fourth Flow)** — The SKILL.md from a completed trail is distributed
with identity injection slots declared. Buyers inject their own identity and run.
Generic becomes personal. One trail, infinite identities.

---

## VisionFrame as Identity Injection

The Vision Agent demonstrates EaA at its most concrete.
It processes an image and emits a VisionFrame — the SKILL.md of the image:

```json
{
  "vision_frame": {
    "primary_intent": "what this image is trying to do",
    "identity_injection_slots": {
      "operator_name": "{person in image}",
      "brand": "{brand if applicable}",
      "style_voice": "{tone and aesthetic}"
    },
    "generative_prompt": "text-to-image prompt to recreate this image"
  }
}
```

The image speaks for itself. The VisionFrame IS the image's SKILL.md.
EaA is not metaphor. It is implementation.

---

## Runtime Injection — C# Implementation

```csharp
public class IdentityInjectionService
{
    public async Task<string> InjectAsync(string skillTemplate, string identityJsonPath)
    {
        var identity = await JsonSerializer.DeserializeAsync<IdentityContext>(
            File.OpenRead(identityJsonPath));

        return identity.InjectionMap
            .Aggregate(skillTemplate, (current, slot) =>
                current.Replace(slot.Key, slot.Value));
    }
}

// Usage
var rawSkill = await File.ReadAllTextAsync("skills/planner-agent/SKILL.md");
var personalizedSkill = await identityService.InjectAsync(
    rawSkill, ".pmcro/identity.json");

builder.AddAIAgent("planner", options =>
{
    options.Instructions = personalizedSkill;
    options.Tools = AgentToolSet.Type2Reads;
});
```

---

## The "I Am" Checklist

Before declaring an agent's identity:

- [ ] Instructions start with "I Am [Name]."
- [ ] Identity is first-person throughout — never "you will", always "I will"
- [ ] The agent declares what it does in its first block
- [ ] The agent declares what it NEVER does in its first block
- [ ] Tools are explicitly listed — default-deny for anything not listed
- [ ] ThoughtLock section anchors identity with date and version
- [ ] Identity injection slots declared if this skill will be personalized

→ Next: See `05-trails.md`
