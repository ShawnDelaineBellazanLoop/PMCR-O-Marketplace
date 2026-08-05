# Batch-create .claude-plugin/plugin.json for all skills
$skills = @(
    # pmcro-csuite skills
    "plugins/pmcro-csuite/skills/ceo",
    "plugins/pmcro-csuite/skills/cfo",
    "plugins/pmcro-csuite/skills/cto",
    "plugins/pmcro-csuite/skills/coo",
    "plugins/pmcro-csuite/skills/cmo",
    "plugins/pmcro-csuite/skills/cro",
    "plugins/pmcro-csuite/skills/clo",
    "plugins/pmcro-csuite/skills/chro",
    "plugins/pmcro-csuite/skills/chief-of-staff",
    # pmcro-engine skills
    "plugins/pmcro-engine/skills/orchestrator",
    "plugins/pmcro-engine/skills/planner",
    "plugins/pmcro-engine/skills/maker",
    "plugins/pmcro-engine/skills/checker",
    "plugins/pmcro-engine/skills/reflector",
    # pmcro-legacy skills
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
    # pmcro-specialty skills
    "plugins/pmcro-specialty/skills/property-preservation",
    "plugins/pmcro-specialty/skills/career-evidence",
    "plugins/pmcro-specialty/skills/skill-creator",
    "plugins/pmcro-specialty/skills/plugin-creator",
    "plugins/pmcro-specialty/skills/powershell-expert",
    "plugins/pmcro-specialty/skills/git"
)

foreach ($skill in $skills) {
    $pluginJson = Join-Path $skill ".claude-plugin/plugin.json"
    $name = Split-Path $skill -Leaf
    
    # Create directory if needed
    $dir = Split-Path $pluginJson -Parent
    if (!(Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    
    # Create plugin.json
    $content = @{
        name = $name
        version = "1.0.0"
        description = "$name skill"
    } | ConvertTo-Json
    
    Set-Content -Path $pluginJson -Value $content -Encoding UTF8
    Write-Host "Created: $pluginJson"
}

Write-Host "`nDone! Created $($skills.Count) plugin manifests."