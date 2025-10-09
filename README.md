## CI / Artefact

Her push/PR’da GitHub Actions otomatik **Development build** üretir ve artefact olarak yükler.

- Actions sekmesine gir → run’ı aç → **Artifacts**’tan `dev-<sha>.zip` indir.
- Release build, `main` push’larında veya `Run workflow` ile manuel tetiklenir (`release-<sha>.zip`).
