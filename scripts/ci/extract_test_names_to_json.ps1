# 1️⃣ List all tests (quiet console output + list-tests)
$raw = dotnet test dotPerfStatTest/dotPerfStatTest.csproj --list-tests 

# 2️⃣ Split into lines, then skip everything up to the header
$lines   = $raw -split "`r?`n"
$started = $false
$tests   = @()
foreach ($line in $lines) {
if (-not $started) {
    if ($line -match '^The following Tests are available:') {
    $started = $true
    continue
    }
}
elseif ($line.Trim()) {
    $tests += $line.Trim()
}
}

# 3️⃣ Pack into JSON array
$json = $tests | ConvertTo-Json -Compress

# 4️⃣ Emit for GitHub Actions (modern syntax)
"test-matrix=$json" | Out-File -FilePath $env:GITHUB_OUTPUT -Encoding utf8 -Append

Write-Host $json