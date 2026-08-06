# Create agents/, commands/, and skills/ subdirectories for each skill plugin
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
    # Create agents directory
    $agentsDir = Join-Path $skillDir "agents"
    if (!(Test-Path $agentsDir)) {
        New-Item -ItemType Directory -Path $agentsDir -Force | Out-Null
    }
    
    # Create commands directory
    $commandsDir = Join-Path $skillDir "commands"
    if (!(Test-Path $commandsDir)) {
        New-Item -ItemType Directory -Path $commandsDir -Force | Out-Null
    }
    
    # Create skills directory
    $skillsDir = Join-Path $skillDir "skills"
    if (!(Test-Path $skillsDir)) {
        New-Item -ItemType Directory -Path $skillsDir -Force | Out-Null
    }
    
    # Move SKILL.md to skills/domain-scope.md if it exists
    $skillMd = Join-Path $skillDir "SKILL.md"
    $domainSkillMd = Join-Path $skillsDir "domain-scope.md"
    if (Test-Path $skillMd -and !(Test-Path $domainSkillMd)) {
        Move-Item $skillMd $domainSkillMd -Force
        Write-Host "Moved: $skillMd -> $domainSkillMd"
    }
    
    Write-Host "Created structure for: $skillDir"
}

Write-Host "`nDone! Structure created for $($skillDirs.Count) skills."