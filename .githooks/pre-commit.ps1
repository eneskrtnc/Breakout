$ErrorActionPreference = "Stop"

# ÇALIŞMA KLASÖRÜNÜ REPO KÖKÜNE SABİTLE
Set-Location -LiteralPath (git rev-parse --show-toplevel)

# 1) CSharpier: sadece check (yazmaz)
Write-Host "🔎 CSharpier (check)"
dotnet csharpier check .

# 2) dotnet-format: .sln varsa onu kullan, yoksa folder mode
$solution = (Get-ChildItem -LiteralPath . -Filter *.sln -File | Select-Object -First 1)

if ($solution) {
    Write-Host "🧹 dotnet-format (verify-no-changes) on $($solution.FullName)"
    dotnet format $solution.FullName --verify-no-changes
}
else {
    Write-Host "ℹ️ .sln bulunamadı, folder mode kullanılıyor."
    dotnet format --folder . --verify-no-changes
}


Write-Host "✅ Lint/format OK"
