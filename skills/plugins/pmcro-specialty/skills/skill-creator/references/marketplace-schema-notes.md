# Claude Code marketplace.json Schema Notes

Current fields worth setting on new plugin entries:

- **`version`** -- per-plugin version pinning. Without it, every commit is
  effectively treated as a new version for every plugin in the
  marketplace. Set this on new entries going forward.
- **`renames`** -- maps an old plugin name to its new name (or `null` to
  retire it outright). Relevant the moment any plugin here is ever
  renamed -- without it, anyone with the old name still referenced gets a
  plugin-not-found error instead of a clean redirect.
- **`hooks`**, **`mcpServers`**, **`lspServers`** -- first-class fields on
  a marketplace entry now, not just plugin-internal config. Only relevant
  if a package actually needs one of these; none of the current PMCR-O
  engine packages do (they're pure Agent Skills, no MCP server or LSP of
  their own).

## The Real Validator

`claude plugin validate .` is the authoritative CLI check for this repo's
`.claude-plugin/marketplace.json` shape.

## Not A Twin: .agents/plugins/marketplace.json

This repo also has `.agents/plugins/marketplace.json` at repo root -- do
not confuse it with the file above or try to keep them in lockstep. It is
a separate, much simpler convention: a single generic pointer entry
(`pmcro-subject-agents`, `source: "."`) consumed only by the .NET
runtime's `MarketplaceSkillsMaterializer.cs`, unrelated to Claude Code's
own plugin-marketplace mechanism. Adding per-skill entries to it would
conflate two different consumers of two differently-shaped files; leave
it as its own convention unless a CTO-scoped cycle decides otherwise.
