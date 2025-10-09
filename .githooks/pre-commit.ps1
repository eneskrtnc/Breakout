$ErrorActionPreference = "Stop"

# ÇALIŞMA KLASÖRÜNÜ REPO KÖKÜNE SABİTLE
Set-Location -LiteralPath (git rev-parse --show-toplevel)

# 1) CSharpier: sadece check (yazmaz)
Write-Host "🔎 CSharpier (check)"
dotnet csharpier check .

# 2) dotnet-format: tek .sln'e kilitli
$solution = ".\Space Trader.sln"   # sln adını kendi adına göre değiştir
if (-not (Test-Path -LiteralPath $solution)) {
    throw "Çözüm dosyası bulunamadı: $solution"
}

Write-Host "🧹 dotnet-format (verify-no-changes) on $solution"
dotnet format $solution --verify-no-changes

Write-Host "✅ Lint/format OK"
