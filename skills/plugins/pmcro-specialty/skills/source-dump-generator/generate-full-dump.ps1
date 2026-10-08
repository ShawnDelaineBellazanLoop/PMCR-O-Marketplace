# Source Dump Script - PMCR-O Project
# Excludes: bin, obj, node_modules, .git, .pmcro, zip files, temp files
# Includes: .cs, .csproj, .json, .md, .ts, .tsx, .props, .slnx, .xml, .yaml, .yml, .ps1

$outputFile = "pmcro-source-dump.txt"
$excludePatterns = @('\\bin\\', '\\obj\\', 'node_modules', '\\.git\\', '\\.pmcro\\', '\.zip$', 'tmp_', 'check-harness')

# Get all matching files
$files = Get-ChildItem -Recurse -File . | Where-Object {
    $file = $_
    $ext = $_.Extension.ToLower()
    $match = $ext -match '\.(cs|csproj|json|md|ts|tsx|props|slnx|xml|yaml|yml|ps1)$'
    $exclude = $false
    foreach ($pattern in $excludePatterns) {
        if ($file.FullName -match $pattern) {
            $exclude = $true
            break
        }
    }
    $match -and -not $exclude
} | Sort-Object FullName

Write-Host "Found $($files.Count) files to include in source dump..."

$sb = [System.Text.StringBuilder]::new()
$null = $sb.AppendLine("=" * 80)
$null = $sb.AppendLine("PMCR-O PROJECT SOURCE DUMP")
$null = $sb.AppendLine("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
$null = $sb.AppendLine("Total Files: $($files.Count)")
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
Write-Host "Source dump created: $outputFile"
Write-Host "Total size: $((Get-Item $outputFile).Length / 1KB) KB"