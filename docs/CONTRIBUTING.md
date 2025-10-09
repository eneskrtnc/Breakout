# Katkı Rehberi

## Branch

- `feature/<kısa-ad>` / `bugfix/<kısa-ad>` / `chore/<kısa-ad>`

## PR

- PR açmadan önce ilgili **Issue**’yu bağla (Closes #id).
- Şablondaki **Test Adımları** ve **Ekran Görüntüsü** alanlarını doldur.
- Actions → artefact’ı indir ve çalışabilirliğini doğrula.

## CI

- Her push/PR’da **Development build** ve artefact üretimi.
- Build süresi ve artefact boyutu Step Summary’de.
- Secrets: UNITY_EMAIL / UNITY_PASSWORD / UNITY_SERIAL (repo Settings → Secrets).

## Dizinler

`/src`, `/assets`, `/data`, `/docs`, `/tools`, `/exports`.

## Kod

- `BuildScript.BuildWindowsDevelopment()` günlük geliştirmenin hedefi.
- `BuildScript.BuildWindowsRelease()` milestone/oyuncu yapıları için.
