<#
.SYNOPSIS
    Migrates PMCR-O from legacy catalog/ structure to official dotnet-agent-skills plugin architecture.
.DESCRIPTION
    This script:
    1. Creates the new plugins/ directory structure
    2. Moves skills from catalog/Platform/*/skills/* to plugins/*/skills/*
    3. Merges commands/*.md into SKILL.md Workflow sections
    4. Deletes obsolete pmcro/ config folders
    5. Flattens nested skills/ directories
    6. Generates plugin.json manifests
    7. Generates root marketplace.json
    8. Cleans up old catalog/ structure
.NOTES
    File:      Migrate-PmcroStructure.ps1
    Author:    PMCR-O Migration Script
    Requires:  PowerShell 5.1 or later
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$SourceRoot = ".",
    
    [Parameter(Mandatory=$false)]
    [switch]$DryRun = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBackup = $false
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

#region Configuration
$script:SourceRoot = Resolve-Path $SourceRoot
$script:CatalogPath = Join-Path $script:SourceRoot "catalog"
$script:PluginsPath = Join-Path $script:SourceRoot "plugins"
$script:MarketplacePath = Join-Path $script:SourceRoot ".claude-plugin"
$script:MarketplaceFile = Join-Path $script:MarketplacePath "marketplace.json"

# Plugin mapping: old path pattern -> new plugin name
$script:PluginMap = @{
    "C-Suite" = "pmcro-csuite"
    "PMCR-O"  = "pmcro-engine"
}

# Skills that belong to specialty/agents plugins (if they exist)
$script:SpecialtySkills = @(
    "property-preservation",
    "career-evidence",
    "skill-creator",
    "filesystem-agent",
    "terminal-agent",
    "playwright-agent"
)
#endregion

#region Logging
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-DryRun {
    param([string]$Message)
    Write-Host "[DRY-RUN] $Message" -ForegroundColor Magenta
}
#endregion

#region Backup
function New-Backup {
    if ($script:SkipBackup) {
        Write-Warning "Skipping backup as requested"
        return
    }
    
    $backupPath = Join-Path $script:SourceRoot "backup-pre-migration"
    if (Test-Path $backupPath) {
        Write-Warning "Backup already exists at $backupPath"
        return
    }
    
    Write-Info "Creating backup of catalog/ to $backupPath..."
    Copy-Item -Path $script:CatalogPath -Destination $backupPath -Recurse -Force
    Write-Success "Backup created"
}

#region Skill Migration
function Get-SkillDirectories {
    param([string]$PlatformPath)
    
    $skills = @()
    $skillDirs = Get-ChildItem -Path $PlatformPath -Directory | Where-Object { $_.Name -eq "skills" }
    
    foreach ($skillDir in $skillDirs) {
        $subSkills = Get-ChildItem -Path $skillDir.FullName -Directory
        foreach ($subSkill in $subSkills) {
            $skills += $subSkill.FullName
        }
    }
    
    return $skills
}

function Get-SkillName {
    param([string]$SkillPath)
    return Split-Path $SkillPath -Leaf
}

function Get-PluginName {
    param(
        [string]$SkillPath,
        [string]$PlatformName
    )
    
    # Check if it's a known specialty skill
    $skillName = Get-SkillName -SkillPath $SkillPath
    if ($script:SpecialtySkills -contains $skillName) {
        return "pmcro-specialty"
    }
    
    # Map platform to plugin
    if ($script:PluginMap.ContainsKey($PlatformName)) {
        return $script:PluginMap[$PlatformName]
    }
    
    return "pmcro-unknown"
}

function Merge-CommandsIntoSkill {
    param(
        [string]$SkillPath,
        [string]$SkillName
    )
    
    $skillMdPath = Join-Path $SkillPath "SKILL.md"
    if (-not (Test-Path $skillMdPath)) {
        Write-Warning "No SKILL.md found in $SkillPath"
        return $null
    }
    
    $skillContent = Get-Content -Path $skillMdPath -Raw
    $commandsDir = Join-Path $SkillPath "commands"
    
    if (-not (Test-Path $commandsDir)) {
        Write-Info "  No commands/ folder found for $SkillName"
        return $skillContent
    }
    
    $commandFiles = @(Get-ChildItem -Path $commandsDir -Filter "*.md" | Sort-Object Name)
    if ($commandFiles.Count -eq 0) {
        Write-Info "  No command files found in $SkillName/commands/"
        return $skillContent
    }
    
    Write-Info "  Merging $($commandFiles.Count) command files into $SkillName/SKILL.md"
    
    # Build Workflow section from commands
    $workflowContent = @"
## Workflow

This section contains the executable workflows formerly in commands/.

"@
    
    foreach ($cmdFile in $commandFiles) {
        $cmdContent = Get-Content -Path $cmdFile.FullName -Raw
        $cmdName = [System.IO.Path]::GetFileNameWithoutExtension($cmdFile.Name)
        
        # Extract description from frontmatter if present
        $description = ""
        if ($cmdContent -match '(?s)^---\s*\n.*?description:\s*"([^"]+)"') {
            $description = $Matches[1]
        }
        
        $workflowContent += @"

### $cmdName
$description

$cmdContent

"@
    }
    
    # Append Workflow section to skill content
    $updatedSkillContent = $skillContent + "`n" + $workflowContent
    
    return $updatedSkillContent
}

function Copy-SkillToPlugin {
    param(
        [string]$SourceSkillPath,
        [string]$DestSkillPath,
        [string]$SkillName
    )
    
    Write-Info "  Migrating skill: $SkillName"
    
    # Create destination directory
    if (-not $script:DryRun) {
        New-Item -Path $DestSkillPath -ItemType Directory -Force | Out-Null
    }
    
    # Process SKILL.md
    $skillMdPath = Join-Path $SourceSkillPath "SKILL.md"
    if (Test-Path $skillMdPath) {
        $updatedContent = Merge-CommandsIntoSkill -SkillPath $SourceSkillPath -SkillName $SkillName
        
        if (-not $script:DryRun) {
            $updatedContent | Out-File -FilePath (Join-Path $DestSkillPath "SKILL.md") -Encoding utf8
            Write-Success "    SKILL.md migrated and commands merged"
        } else {
            Write-DryRun "    Would migrate SKILL.md with merged commands"
        }
    }
    
    # Copy references/ folder
    $sourceRefs = Join-Path $SourceSkillPath "references"
    $destRefs = Join-Path $DestSkillPath "references"
    if (Test-Path $sourceRefs) {
        if (-not $script:DryRun) {
            Copy-Item -Path $sourceRefs -Destination $destRefs -Recurse -Force
            Write-Success "    references/ copied"
        } else {
            Write-DryRun "    Would copy references/"
        }
    }
    
    # Copy scripts/ folder
    $sourceScripts = Join-Path $SourceSkillPath "scripts"
    $destScripts = Join-Path $DestSkillPath "scripts"
    if (Test-Path $sourceScripts) {
        if (-not $script:DryRun) {
            Copy-Item -Path $sourceScripts -Destination $destScripts -Recurse -Force
            Write-Success "    scripts/ copied"
        } else {
            Write-DryRun "    Would copy scripts/"
        }
    }
    
    # Copy assets/ folder (if exists)
    $sourceAssets = Join-Path $SourceSkillPath "assets"
    $destAssets = Join-Path $DestSkillPath "assets"
    if (Test-Path $sourceAssets) {
        if (-not $script:DryRun) {
            Copy-Item -Path $sourceAssets -Destination $destAssets -Recurse -Force
            Write-Success "    assets/ copied"
        } else {
            Write-DryRun "    Would copy assets/"
        }
    }
    
    # Delete obsolete folders
    $obsoleteFolders = @("commands", "pmcro", "skills")
    foreach ($folder in $obsoleteFolders) {
        $sourceFolder = Join-Path $SourceSkillPath $folder
        if (Test-Path $sourceFolder) {
            Write-Info "    Deleting obsolete $folder/ folder"
            if (-not $script:DryRun) {
                Remove-Item -Path $sourceFolder -Recurse -Force
            }
        }
    }
}

function New-PluginManifest {
    param(
        [string]$PluginPath,
        [string]$PluginName,
        [array]$Skills
    )
    
    $manifest = @{
        name = $PluginName
        version = "1.0.0"
        description = "PMCR-O $PluginName plugin"
        skills = @()
    }
    
    foreach ($skill in $Skills) {
        $skillName = Split-Path $skill -Leaf
        $manifest.skills += @{
            name = $skillName
            path = "skills/$skillName/SKILL.md"
        }
    }
    
    $manifestDir = Join-Path $PluginPath ".claude-plugin"
    $manifestFile = Join-Path $manifestDir "plugin.json"
    
    if (-not $script:DryRun) {
        New-Item -Path $manifestDir -ItemType Directory -Force | Out-Null
        $manifest | ConvertTo-Json -Depth 5 | Out-File -FilePath $manifestFile -Encoding utf8
        Write-Success "  Created plugin.json with $($Skills.Count) skills"
    } else {
        Write-DryRun "  Would create plugin.json with $($Skills.Count) skills"
    }
}

function New-MarketplaceManifest {
    param([array]$Plugins)
    
    $marketplace = @{
        name = "pmcro-colony"
        version = "1.0.0"
        description = "PMCR-O Colony - Agent Skills Marketplace"
        plugins = @()
    }
    
    foreach ($plugin in $Plugins) {
        $marketplace.plugins += @{
            name = $plugin
            path = "plugins/$plugin"
            source = "./"
        }
    }
    
    if (-not $script:DryRun) {
        New-Item -Path $script:MarketplacePath -ItemType Directory -Force | Out-Null
        $marketplace | ConvertTo-Json -Depth 3 | Out-File -FilePath $script:MarketplaceFile -Encoding utf8
        Write-Success "Created root marketplace.json"
    } else {
        Write-DryRun "Would create root marketplace.json"
    }
}

#region Main Migration Logic
function Start-Migration {
    Write-Info "Starting PMCR-O structure migration..."
    Write-Info "Source: $script:SourceRoot"
    Write-Info "Target: $script:PluginsPath"
    
    if ($script:DryRun) {
        Write-Warning "DRY RUN MODE - No changes will be made"
    }
    
    # Create backup
    if (-not $script:DryRun) {
        New-Backup
    }
    
    # Discover all skills
    Write-Info "`nDiscovering skills in catalog/Platform/..."
    $allSkills = @{}
    $pluginSkills = @{}
    
    $platformDirs = Get-ChildItem -Path $script:CatalogPath -Directory | Where-Object { $_.Name -eq "Platform" }
    if ($platformDirs) {
        $platformPath = $platformDirs.FullName
        $categoryDirs = Get-ChildItem -Path $platformPath -Directory
        
        foreach ($category in $categoryDirs) {
            $categoryName = $category.Name
            Write-Info "`nProcessing category: $categoryName"
            
            $skillDirs = Get-ChildItem -Path $category.FullName -Directory | Where-Object { $_.Name -eq "skills" }
            if (-not $skillDirs) { continue }
            
            $skillsPath = $skillDirs.FullName
            $skills = Get-ChildItem -Path $skillsPath -Directory
            
            foreach ($skill in $skills) {
                $skillName = $skill.Name
                $pluginName = Get-PluginName -SkillPath $skill.FullName -PlatformName $categoryName
                
                Write-Info "  Found: $skillName -> $pluginName"
                
                if (-not $pluginSkills.ContainsKey($pluginName)) {
                    $pluginSkills[$pluginName] = @()
                }
                $pluginSkills[$pluginName] += $skill.FullName
                $allSkills[$skillName] = $skill.FullName
            }
        }
    }
    
    Write-Info "`nDiscovered $($allSkills.Count) skills across $($pluginSkills.Count) plugins"
    
    # Create plugin structure
    Write-Info "`nCreating plugin structure..."
    foreach ($pluginName in $pluginSkills.Keys) {
        $pluginPath = Join-Path $script:PluginsPath $pluginName
        $skillsPath = Join-Path $pluginPath "skills"
        
        if (-not $script:DryRun) {
            New-Item -Path $skillsPath -ItemType Directory -Force | Out-Null
        }
        
        Write-Info "  Plugin: $pluginName"
        
        # Migrate each skill
        foreach ($skillPath in $pluginSkills[$pluginName]) {
            $skillName = Split-Path $skillPath -Leaf
            $destSkillPath = Join-Path $skillsPath $skillName
            
            Copy-SkillToPlugin -SourceSkillPath $skillPath -DestSkillPath $destSkillPath -SkillName $skillName
        }
        
        # Create plugin manifest
        $skillPaths = $pluginSkills[$pluginName] | ForEach-Object { Split-Path $_ -Leaf }
        New-PluginManifest -PluginPath $pluginPath -PluginName $pluginName -Skills $skillPaths
    }
    
    # Create marketplace manifest
    Write-Info "`nCreating marketplace manifest..."
    New-MarketplaceManifest -Plugins $pluginSkills.Keys
    
    # Cleanup old structure
    if (-not $script:DryRun) {
        Write-Info "`nCleaning up old catalog/ structure..."
        Remove-Item -Path $script:CatalogPath -Recurse -Force
        Write-Success "Removed catalog/ directory"
        
        # Remove old schema files
        $oldFiles = @("skills.json", "skills.schema.json", "manifest.json")
        foreach ($file in $oldFiles) {
            $filePath = Join-Path $script:SourceRoot $file
            if (Test-Path $filePath) {
                Remove-Item -Path $filePath -Force
                Write-Success "Removed $file"
            }
        }
    } else {
        Write-DryRun "`nWould remove catalog/ and old manifest files"
    }
    
    Write-Success "`nMigration complete!"
    Write-Info "Next steps:"
    Write-Info "  1. Review the new plugins/ structure"
    Write-Info "  2. Verify all SKILL.md files have correct frontmatter"
    Write-Info "  3. Update any cross-references in markdown files"
    Write-Info "  4. Test skill loading with Claude Code"
    Write-Info "  5. Delete backup when satisfied: Remove-Item backup-pre-migration -Recurse"
}

# Run migration
try {
    Start-Migration
    exit 0
} catch {
    Write-Error "Migration failed: $_"
    exit 1
}
#>

<#
.SYNOPSIS
    Validates the migrated PMCR-O structure.
.DESCRIPTION
    Checks that all plugins, skills, and manifests are correctly formatted.
.EXAMPLE
    .\Validate-PmcroStructure.ps1
#>
function Test-PmcroStructure {
    [CmdletBinding()]
    param(
        [string]$RootPath = "."
    )
    
    $root = Resolve-Path $RootPath
    $pluginsPath = Join-Path $root "plugins"
    $marketplaceFile = Join-Path $root ".claude-plugin/marketplace.json"
    
    Write-Info "Validating PMCR-O structure at $root"
    
    # Check root structure
    if (-not (Test-Path $pluginsPath)) {
        Write-Error "plugins/ directory not found"
        return $false
    }
    Write-Success "plugins/ directory exists"
    
    if (-not (Test-Path $marketplaceFile)) {
        Write-Error ".claude-plugin/marketplace.json not found"
        return $false
    }
    Write-Success ".claude-plugin/marketplace.json exists"
    
    # Validate marketplace.json
    try {
        $marketplace = Get-Content $marketplaceFile | ConvertFrom-Json
        Write-Success "marketplace.json is valid JSON"
        
        if (-not $marketplace.plugins) {
            Write-Warning "No plugins registered in marketplace.json"
        } else {
            Write-Success "Found $($marketplace.plugins.Count) plugins in marketplace"
        }
    } catch {
        Write-Error "marketplace.json is invalid JSON: $_"
        return $false
    }
    
    # Validate each plugin
    $plugins = Get-ChildItem -Path $pluginsPath -Directory
    Write-Info "`nValidating $($plugins.Count) plugins..."
    
    foreach ($plugin in $plugins) {
        Write-Info "`nPlugin: $($plugin.Name)"
        
        $pluginManifest = Join-Path $plugin.FullName ".claude-plugin/plugin.json"
        if (-not (Test-Path $pluginManifest)) {
            Write-Error "  Missing .claude-plugin/plugin.json"
            continue
        }
        Write-Success "  Has plugin.json"
        
        $skillsPath = Join-Path $plugin.FullName "skills"
        if (-not (Test-Path $skillsPath)) {
            Write-Error "  Missing skills/ directory"
            continue
        }
        
        $skills = Get-ChildItem -Path $skillsPath -Directory
        Write-Success "  Found $($skills.Count) skills"
        
        foreach ($skill in $skills) {
            $skillMd = Join-Path $skill.FullName "SKILL.md"
            if (-not (Test-Path $skillMd)) {
                Write-Error "    Missing SKILL.md in $($skill.Name)"
                continue
            }
            Write-Success "    $($skill.Name) has SKILL.md"
            
            # Check for obsolete folders
            $obsolete = @("commands", "pmcro")
            foreach ($folder in $obsolete) {
                $folderPath = Join-Path $skill.FullName $folder
                if (Test-Path $folderPath) {
                    Write-Warning "    Found obsolete $folder/ in $($skill.Name)"
                }
            }
        }
    }
    
    Write-Success "`nValidation complete!"
    return $true
}

Export-ModuleMember -Function Test-PmcroStructure
#>

<#
.SYNOPSIS
    Generates a migration report showing what was changed.
.DESCRIPTION
    Compares old and new structures and generates a detailed report.
#>
function New-MigrationReport {
    [CmdletBinding()]
    param(
        [string]$RootPath = "."
    )
    
    $root = Resolve-Path $RootPath
    $backupPath = Join-Path $root "backup-pre-migration"
    
    if (-not (Test-Path $backupPath)) {
        Write-Error "Backup not found. Cannot generate report."
        return
    }
    
    $report = @"
# PMCR-O Migration Report
Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')

## Summary

This report details the migration from the legacy catalog/ structure to the
official dotnet-agent-skills plugin architecture.

## Old Structure (catalog/)

"@
    
    # Analyze old structure
    $oldSkills = Get-ChildItem -Path $backupPath -Recurse -Filter "SKILL.md" | 
        Select-Object -ExpandProperty DirectoryName
    
    $report += "Found $($oldSkills.Count) skills in old structure:`n`n"
    
    foreach ($skill in $oldSkills) {
        $relativePath = $skill.Replace($backupPath, "").TrimStart("\")
        $report += "- $relativePath`n"
    }
    
    # Analyze new structure
    $report += @"

## New Structure (plugins/)

"@
    
    $newSkills = Get-ChildItem -Path (Join-Path $root "plugins") -Recurse -Filter "SKILL.md" | 
        Select-Object -ExpandProperty DirectoryName
    
    $report += "Found $($newSkills.Count) skills in new structure:`n`n"
    
    foreach ($skill in $newSkills) {
        $relativePath = $skill.Replace($root, "plugins").TrimStart("\")
        $report += "- $relativePath`n"
    }
    
    $report += @"

## Changes Made

1. Created plugins/ directory with plugin subdirectories
2. Migrated all skills from catalog/Platform/*/skills/* to plugins/*/skills/*
3. Merged commands/*.md files into SKILL.md Workflow sections
4. Deleted obsolete pmcro/ configuration folders
5. Flattened nested skills/ directories
6. Generated plugin.json manifests for each plugin
7. Generated root .claude-plugin/marketplace.json
8. Removed catalog/ directory and old manifest files

## Files Removed

- catalog/ (entire directory)
- catalog/skills.json
- catalog/skills.schema.json
- All commands/*.md files (merged into SKILL.md)
- All pmcro/ directories

## Next Steps

1. Review migrated SKILL.md files for accuracy
2. Update any internal cross-references
3. Test skill loading in Claude Code
4. Delete backup when satisfied: Remove-Item backup-pre-migration -Recurse

"@
    
    $reportPath = Join-Path $root "MIGRATION_REPORT.md"
    $report | Out-File -FilePath $reportPath -Encoding utf8
    Write-Success "Migration report generated: $reportPath"
}

Export-ModuleMember -Function New-MigrationReport, Test-PmcroStructure