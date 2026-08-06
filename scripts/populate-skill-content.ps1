# Populate agents/, commands/, and skills/ with minimal content for each skill plugin
$skillDirs = @(
    "plugins/pmcro-csuite/skills/ceo",
    "plugins/pmcro-csuite/skills/cfo",
    "plugins/pmcro-csuite/skills/cto",
    "plugins/pmcro-csuite/skills/coo",
    "plugins/pmcro-csuite/skills/cmo",
    "plugins/pmcro-csuite/skills/cro",
    "plugins/pmcro-csuite/skills/clo",
    "plugins/pmcro-csuite/skills/chro",
    "plugins/pmcro-csuite/skills/chief-of-staff",
    "plugins/pmcro-engine/skills/orchestrator",
    "plugins/pmcro-engine/skills/planner",
    "plugins/pmcro-engine/skills/maker",
    "plugins/pmcro-engine/skills/checker",
    "plugins/pmcro-engine/skills/reflector",
    "plugins/pmcro-legacy/skills/checker-agent",
    "plugins/pmcro-legacy/skills/codeact-agent",
    "plugins/pmcro-legacy/skills/cognitive-trails",
    "plugins/pmcro-legacy/skills/dependency-resolver",
    "plugins/pmcro-legacy/skills/desktop-commander",
    "plugins/pmcro-legacy/skills/domain-specialist",
    "plugins/pmcro-legacy/skills/filesystem-agent",
    "plugins/pmcro-legacy/skills/filesystem-mcp",
    "plugins/pmcro-legacy/skills/framework-evolution",
    "plugins/pmcro-legacy/skills/maker-agent",
    "plugins/pmcro-legacy/skills/mcp-server-template",
    "plugins/pmcro-legacy/skills/orchestrator-agent",
    "plugins/pmcro-legacy/skills/pattern-learner",
    "plugins/pmcro-legacy/skills/planner-agent",
    "plugins/pmcro-legacy/skills/playwright-agent",
    "plugins/pmcro-legacy/skills/playwright-mcp",
    "plugins/pmcro-legacy/skills/pmcro-framework",
    "plugins/pmcro-legacy/skills/reflector-agent",
    "plugins/pmcro-legacy/skills/source-dump-generator",
    "plugins/pmcro-legacy/skills/terminal-agent",
    "plugins/pmcro-legacy/skills/terminal-mcp",
    "plugins/pmcro-legacy/skills/trail-indexer",
    "plugins/pmcro-specialty/skills/property-preservation",
    "plugins/pmcro-specialty/skills/career-evidence",
    "plugins/pmcro-specialty/skills/skill-creator",
    "plugins/pmcro-specialty/skills/plugin-creator",
    "plugins/pmcro-specialty/skills/powershell-expert",
    "plugins/pmcro-specialty/skills/git"
)

foreach ($skillDir in $skillDirs) {
    $name = Split-Path $skillDir -Leaf
    
    # Create agent file
    $agentFile = Join-Path $skillDir "agents/$name.md"
    if (!(Test-Path $agentFile)) {
        @"
# $name Agent

This is the $name agent for the PMCR-O Colony.
"@ | Set-Content -Path $agentFile -Encoding UTF8
    }
    
    # Create command file
    $commandFile = Join-Path $skillDir "commands/$name.md"
    if (!(Test-Path $commandFile)) {
        @"
---
description: "$name command"
---

# /$name

This is the $name command.
"@ | Set-Content -Path $commandFile -Encoding UTF8
    }
    
    # Create skill file in skills/ directory
    $skillFile = Join-Path $skillDir "skills/domain-scope.md"
    if (!(Test-Path $skillFile)) {
        @"
# $name Domain Skill

This is the $name domain skill.
"@ | Set-Content -Path $skillFile -Encoding UTF8
    }
    
    Write-Host "Populated: $skillDir"
}

Write-Host "`nDone! Populated $($skillDirs.Count) skills."