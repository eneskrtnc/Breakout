# UI Bars & HUD Coin Counter (PPU=64) — Kullanım Kılavuzu

Bu doküman HUD’daki HP/Shield/Energy/Heat barları ve Coin sayacının nasıl kurulduğunu ve nasıl kullanılacağını özetler.

## 0) Kurulum Özeti
- **Referans PPU**: 64 (UI)
- **Import**: Sprite(2D and UI), **Filter=Point**, **Compression=None**, **MipMaps=OFF**, **Mesh=Full Rect**
- **Canvas**: Scale With Screen Size, Reference Resolution = 1920×1080, **Reference Pixels Per Unit = 64**
- **Pixel Perfect Camera**: ON → Assets PPU = 64, Upscale RT=ON, Crop X/Y=ON

---

## 1) UI Barları

### 1.1 Prefab Yapısı
