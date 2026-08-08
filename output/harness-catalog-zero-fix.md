# Harness Catalog Zero-Count Fix

## Symptom

Harness `get_skill_catalog` returned `count: 0` and `plugins: 0` even though the Skills page showed the marketplace catalog.

## Cause

The backend catalog reader depended only on `OrchestratorConfig.FileSystemRoot`. In the Aspire Harness process, that configured root was not resolving to the repository, while the native MAF staging process could still start.

## Fix

`SkillCatalogService` now resolves `.agents/plugins/marketplace.json` in this order:

1. configured `Orchestrator:FileSystemRoot`
2. parent directories from `AppContext.BaseDirectory`
3. parent directories from the current working directory

The marketplace registry remains the canonical catalog source. `.pmcro/skills-staging` remains the native MAF execution source.

## Validation

Added a regression test for configured marketplace resolution. Rebuild the service and start a new Harness conversation after restart.
