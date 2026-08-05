# Compact Source Dump Script - PMCR-O Project
# Focuses on essential code, excludes heavy documentation

$outputFile = "pmcro-compact-source-dump.txt"
$excludePatterns = @('\\bin\\', '\\obj\\', 'node_modules', '\\.git\\', '\\.pmcro\\', '\.zip$', 'tmp_', 'check-harness', 'AI-Knowledge-Corpus', 'docs\\', 'repos\\')

# Collect files from essential directories separately
$allFiles = @()
foreach ($dir in @('src', 'mcp', 'tests', 'skills')) {
    if (Test-Path $dir) {
        $files = Get-ChildItem -Recurse -File $dir | Where-Object {
            $file = $_
            $ext = $_.Extension.ToLower()
            $match = $ext -match '\.(cs|csproj|json|props|slnx|xml|ps1|md|txt|yaml|yml)$'
            $exclude = $false
            foreach ($pattern in $excludePatterns) {
                if ($file.FullName -match $pattern) {
                    $exclude = $true
                    break
                }
            }
            $match -and -not $exclude
        }
        $allFiles += $files
    }
}

$files = $allFiles | Sort-Object FullName

Write-Host "Found $($files.Count) files to include in compact source dump..."

$sb = [System.Text.StringBuilder]::new()
$null = $sb.AppendLine("=" * 80)
$null = $sb.AppendLine("PMCR-O PROJECT COMPACT SOURCE DUMP")
$null = $sb.AppendLine("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
$null = $sb.AppendLine("Total Files: $($files.Count)")
$null = $sb.AppendLine("Included Directories: src, mcp, tests, skills")
$null = $sb.AppendLine("Excluded: bin, obj, node_modules, .git, .pmcro, zip, temp files, docs, repos, AI-Knowledge-Corpus")
$null = $sb.AppendLine("=" * 80)
$null = $sb.AppendLine("")

foreach ($file in $files) {
    $null = $sb.AppendLine("-" * 80)
    $null = $sb.AppendLine("FILE: $($file.FullName)")
    $null = $sb.AppendLine("-" * 80)
    try {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction Stop
        $null = $sb.AppendLine($content)
    } catch {
        $null = $sb.AppendLine("[ERROR: Could not read file - $($_.Exception.Message)]")
    }
    $null = $sb.AppendLine("")
    $null = $sb.AppendLine("")
}

[System.IO.File]::WriteAllText($outputFile, $sb.ToString(), [System.Text.Encoding]::UTF8)
$sizeMB = [math]::Round((Get-Item $outputFile).Length / 1MB, 2)
Write-Host "Compact source dump created: $outputFile"
Write-Host "Total size: $sizeMB MB"