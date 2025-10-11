# Space Trader — Palette & Pixel/VFX Manifesto (v2 @ 64×64)

**Hedef:** Sprite ve VFX’te tutarlı, okunaklı ve telegraph’ı güçlü bir dil.

## Ölçek / Grid
- **Tile grid:** 64×64 px  → Unity’de **Pixels Per Unit = 64**.
- **Import:** Sprite Mode=Single/Multiple, Filter **Point (no filter)**, Compression **None**.
- **UI/ekran çözünürlüğü:** 64 tabanını 2×–3× ölçekle büyüt; kaynak görsel her zaman 64 grid’e otursun.

## Palet (v2)
- **Neutrals (10):** N0→N9, geniş değer aralığı (siyah→beyaz).
- **Hue ramps (36):** 6 renk ailesi × 6 adım → **Blues, Teals, Greens, Oranges, Reds, Purples**.  
  *Kural:* Gölgede **soğuk/purpur**a, parlakta **sıcak/az satüre**ye kay.
- **Accents/Emissive (10):** UI & telegraph için ayrılmış.
  - Enemy Telegraph **Amber** `#FFC83D`
  - Danger/Impact **Red** `#FF5555`
  - Ally/Safe **Cyan** `#00E5FF`
  - Selection `#38F8B7`, Interact `#6CE1FF`, Critical UI `#FF3D7D`
  - Emissive Blue/Green/Orange, White

> **Telegraph rezervi:** Amber/Red/Cyan yalnızca **anticipation/uyarı** efektlerinde. Gövde sprite/ikonlarda **kullanma**.

## Gölgelendirme Reçetesi (A→M→L→H)
- **A (Ambient):** N3–N5 (veya ramp’ın 2./3. değeri).  
- **M (Mid):** Malzemenin ana tonu (ramp’ın ortası, satüre).  
- **L (Light):** Ramp’ta daha **açık + biraz sıcak**.  
- **H (Spec):** Az satüre/açık bir dokunuş (tek tük piksel).  
**Kullanım:** Bir malzemede **en fazla 3–4 değer**; gradient için 50% dither (checker) tercih et.

## İpuçları
- Silüeti birincil kontrastla kur (N0/N1 ↔ N8/N9).  
- Gölge doygunluğu hafif **arttır**, highlight’ta **azalt** (pixart klasik hue-shift).  
- Emissive için 2 ton + beyaz pırıltı; additive/soft-add blend kullan.

## Örnekler (repoya ekli)
- **Palette**: `Assets/Art/Palette/palette_v2_56.png`
- **Tile (64×64)**: `Assets/Art/Tiles/tile_floor_64.png`
- **VFX Telegraph (8f, 64×64)**: `Assets/Art/VFX/vfx_telegraph_sheet_64x64_8f.png`
