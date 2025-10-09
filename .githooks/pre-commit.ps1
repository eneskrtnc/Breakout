$ErrorActionPreference = "Stop"

# ÇALIŞMA KLASÖRÜNÜ REPO KÖKÜNE SABİTLE
Set-Location -LiteralPath (git rev-parse --show-toplevel)

# 1) CSharpier: sadece check (yazmaz)
Write-Host "🔎 CSharpier (check)"
dotnet csharpier check .

# .sln ya da .csproj bul
$solution = Get-ChildItem -Filter *.sln -File | Select-Object -First 1
$project  = if (-not $solution) { Get-ChildItem -Filter *.csproj -File | Select-Object -First 1 } else { $null }

if ($solution) {
    dotnet format $solution.FullName --verify-no-changes
} elseif ($project) {
    dotnet format $project.FullName --verify-no-changes
} else {
    Write-Host "ℹ️ .sln/.csproj yok; dotnet-format atlandı (CSharpier check yapıldı)."
}


Write-Host "✅ Lint/format OK"
