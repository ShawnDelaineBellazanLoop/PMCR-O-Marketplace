# Move misplaced command files from plugin-level commands/ into skill-level commands/
$moves = @(
    # pmcro-csuite commands
    @{ From = "plugins/pmcro-csuite/commands/ceo/approve-initiative.md"; To = "plugins/pmcro-csuite/skills/ceo/commands/approve-initiative.md" },
    @{ From = "plugins/pmcro-csuite/commands/ceo/evolve-colony.md"; To = "plugins/pmcro-csuite/skills/ceo/commands/evolve-colony.md" },
    @{ From = "plugins/pmcro-csuite/commands/ceo/set-direction.md"; To = "plugins/pmcro-csuite/skills/ceo/commands/set-direction.md" },
    @{ From = "plugins/pmcro-csuite/commands/cfo/cashflow.md"; To = "plugins/pmcro-csuite/skills/cfo/commands/cashflow.md" },
    @{ From = "plugins/pmcro-csuite/commands/cfo/forecast.md"; To = "plugins/pmcro-csuite/skills/cfo/commands/forecast.md" },
    @{ From = "plugins/pmcro-csuite/commands/cfo/variance.md"; To = "plugins/pmcro-csuite/skills/cfo/commands/variance.md" },
    @{ From = "plugins/pmcro-csuite/commands/chief-of-staff/coordinate.md"; To = "plugins/pmcro-csuite/skills/chief-of-staff/commands/coordinate.md" },
    @{ From = "plugins/pmcro-csuite/commands/chro/people-ops.md"; To = "plugins/pmcro-csuite/skills/chro/commands/people-ops.md" },
    @{ From = "plugins/pmcro-csuite/commands/clo/legal-review.md"; To = "plugins/pmcro-csuite/skills/clo/commands/legal-review.md" },
    @{ From = "plugins/pmcro-csuite/commands/cmo/campaign.md"; To = "plugins/pmcro-csuite/skills/cmo/commands/campaign.md" },
    @{ From = "plugins/pmcro-csuite/commands/coo/define-workflow.md"; To = "plugins/pmcro-csuite/skills/coo/commands/define-workflow.md" },
    @{ From = "plugins/pmcro-csuite/commands/coo/track-work.md"; To = "plugins/pmcro-csuite/skills/coo/commands/track-work.md" },
    @{ From = "plugins/pmcro-csuite/commands/cro/pipeline.md"; To = "plugins/pmcro-csuite/skills/cro/commands/pipeline.md" },
    @{ From = "plugins/pmcro-csuite/commands/cto/architect.md"; To = "plugins/pmcro-csuite/skills/cto/commands/architect.md" },
    @{ From = "plugins/pmcro-csuite/commands/cto/security-review.md"; To = "plugins/pmcro-csuite/skills/cto/commands/security-review.md" },
    # pmcro-specialty commands
    @{ From = "plugins/pmcro-specialty/commands/career-evidence/timeline.md"; To = "plugins/pmcro-specialty/skills/career-evidence/commands/timeline.md" },
    @{ From = "plugins/pmcro-specialty/commands/property-preservation/inspection-report.md"; To = "plugins/pmcro-specialty/skills/property-preservation/commands/inspection-report.md" },
    @{ From = "plugins/pmcro-specialty/commands/property-preservation/score-candidates.md"; To = "plugins/pmcro-specialty/skills/property-preservation/commands/score-candidates.md" },
    @{ From = "plugins/pmcro-specialty/commands/skill-creator/create-skill.md"; To = "plugins/pmcro-specialty/skills/skill-creator/commands/create-skill.md" },
    @{ From = "plugins/pmcro-specialty/commands/skill-creator/update-catalog.md"; To = "plugins/pmcro-specialty/skills/skill-creator/commands/update-catalog.md" },
    @{ From = "plugins/pmcro-specialty/commands/skill-creator/validate-skill.md"; To = "plugins/pmcro-specialty/skills/skill-creator/commands/validate-skill.md" }
)

foreach ($move in $moves) {
    if (Test-Path $move.From) {
        $toDir = Split-Path $move.To -Parent
        if (!(Test-Path $toDir)) {
            New-Item -ItemType Directory -Path $toDir -Force | Out-Null
        }
        Move-Item $move.From $move.To -Force
        Write-Host "Moved: $($move.From) -> $($move.To)"
    } else {
        Write-Host "SKIP (not found): $($move.From)"
    }
}

# Remove empty command directories
$emptyDirs = @(
    "plugins/pmcro-csuite/commands/ceo",
    "plugins/pmcro-csuite/commands/cfo",
    "plugins/pmcro-csuite/commands/chief-of-staff",
    "plugins/pmcro-csuite/commands/chro",
    "plugins/pmcro-csuite/commands/clo",
    "plugins/pmcro-csuite/commands/cmo",
    "plugins/pmcro-csuite/commands/coo",
    "plugins/pmcro-csuite/commands/cro",
    "plugins/pmcro-csuite/commands/cto",
    "plugins/pmcro-specialty/commands/career-evidence",
    "plugins/pmcro-specialty/commands/property-preservation",
    "plugins/pmcro-specialty/commands/skill-creator"
)

foreach ($dir in $emptyDirs) {
    if (Test-Path $dir) {
        $remaining = Get-ChildItem $dir -Recurse -File
        if ($remaining.Count -eq 0) {
            Remove-Item $dir -Recurse -Force
            Write-Host "Removed empty dir: $dir"
        }
    }
}

# Remove top-level commands dirs if empty
foreach ($dir in @("plugins/pmcro-csuite/commands", "plugins/pmcro-specialty/commands")) {
    if (Test-Path $dir) {
        $remaining = Get-ChildItem $dir -Recurse -File
        if ($remaining.Count -eq 0) {
            Remove-Item $dir -Recurse -Force
            Write-Host "Removed empty dir: $dir"
        }
    }
}

Write-Host "`nDone!"