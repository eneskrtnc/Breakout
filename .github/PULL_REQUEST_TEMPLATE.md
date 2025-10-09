## Öz / Kapsam

Bu PR neyi çözüyor? Kısa özet.

## İlgili Issue

Closes #<issue-id>

## Değişiklikler

- [ ] CI: push/PR tetiklerinde Debug build üretiliyor
- [ ] Telemetri: Build süresi ve artefact boyutu Step Summary’de
- [ ] Dizin yapısı: /src, /assets, /data, /docs, /tools, /exports
- [ ] Dokümantasyon: README / docs güncellendi (gerekliyse)

## Test Adımları

1. Boş bir PR aç / bu PR’ı aç.
2. Actions → son run → **dev artefact** indir.
3. Zip’i aç, exe’yi çalıştır, “Development Build” watermark’ını doğrula.

## Ekran Görüntüsü / Kanıt

- [ ] Actions run ekran görüntüsü (artefact listesi)
- [ ] Step Summary ekran görüntüsü (süre + boyut)
- [ ] Oyun çalışıyor ekran görüntüsü (isteğe bağlı)

## Geriye Dönük Uyum / Risk

- [ ] Breaking change yok
- [ ] Secrets: UNITY_EMAIL / UNITY_PASSWORD / UNITY_SERIAL tanımlı

## Kontrol Listesi (DoD)

- [ ] CI < 10 dk (cache açık)
- [ ] Hata durumunda anlamlı log (secret kontrolü vs.)
- [ ] Artefact üretildi ve indirilebilir
