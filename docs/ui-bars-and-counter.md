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
BarRoot (RectTransform)
├─ Frame (Image • Sliced)
├─ Mask (RectMask2D)
│ ├─ BackFill (Image • Tiled) [opsiyonel: damage chip]
│ └─ Fill (Image • Tiled) [zorunlu]
├─ FullFX (Image) [opsiyonel: flash/pulse overlay]
└─ Ticks
└─ TickTemplate (Image • disabled • 1×1 beyaz)

**Notlar:**
- `Frame` 9-slice; `Fill/BackFill` **Tiled** (tekrar eden doku).
- **Dikey bar** için `Fill`: **Anchor=Bottom**, **Pivot=(0.5,0)**, **Anchored=(0,0)**.

### 1.2 Bileşenler

#### `UiBar` (Assets/Game/Core/Scripts/UI/UiBar.cs)
- **orientation**: `Horizontal` (HP/Shield) veya `Vertical` (Energy/Heat)
- **value01**: 0..1 doluluk
- **pixelsPerSecond**: ön bar dolum/sönüm hızı (px/s)
- **backFillRt + backPixelsPerSecond**: hasar “chip” efekti (opsiyonel)
- **fullFxMode**: `None` / `FlashOnce` / `PulseWhileFull`
  - Shield → **PulseWhileFull** (mavi/cyan tonu)
  - Heat → **FlashOnce** (kırmızı/turuncu vurgusu)
- **fullThreshold**: 0.999 (tam dolu eşiği), **fullFxImage**: `FullFX` Image referansı
- **fillImage + criticalThreshold**: düşük seviyede renk geçişi (opsiyonel)

**Önerilen Hızlar:** `pixelsPerSecond=320–400`, `backPixelsPerSecond=120–180`

#### `UiBarTicks` (Assets/Game/Core/Scripts/UI/UiBarTicks.cs)
- **orientation**: `Horizontal` → dikey tik, `Vertical` → yatay tik çizer
- **divisions**: 10 (→ %10’luk aralıklar)
- **padding**: Frame iç kenar kadar (genelde **1–3 px**)
- **tickTemplate**: 1×1 beyaz Image (disabled). Çocuk çizgiler **havuzlanır** (instantiate/destroy yok).

### 1.3 Hızlı Kurulum (özet)
1) `BarRoot` oluştur; yukarıdaki hiyerarşiyi kur.  
2) `Fill` **Tiled**, `Frame` **Sliced**.  
3) `UiBar`’ı `BarRoot`’a ekle → `fillRt=Fill`, (ops) `backFillRt=BackFill`, (ops) `fillImage=Fill`  
4) `UiBarTicks`’i `BarRoot`’a ekle → `targetRect=Mask`, `ticksContainer=Ticks`, `tickTemplate=TickTemplate`  
5) **Energy/Heat (dikey):** `UiBar.orientation=Vertical` + `Fill`’in Anchor/Pivot ayarını yukarıdaki gibi yap.  
6) **Shield/Heat FX:** `UiBar.fullFxMode` ve `fullFxColor`’ı set et.

---

## 2) HUD Coin Counter

### 2.1 Asset’ler
- Coin ikonları:  
  `Assets/Art/Sprites/UI/Icons16/UI.Icon.Counter.16.png`  
  `Assets/Art/Sprites/UI/Icons32/UI.Icon.Counter.32.png`
- 5×7 rakam sheet:  
  `Assets/Art/Sprites/UI/Counters/UI.Counter.Font.5x7.sheet.png`

**Rakam Sheet Dilimleme (Sprite Editor):**
- Mode: **Grid by Cell Size**
- **Cell Size = 6×8** (5×7 glyph + sağ/alt 1 px pad)
- **Offset = 1,1** (dış pad)
- **Padding = 0,0**
- Slice → Apply  
> Böylece 0..9 (ve eklediysen `+ - x / : .`) ayrı sprite olur. **Trim yok.**

### 2.2 HUD Prefab
CounterBadge (Top-Right anchored • Pivot=1,1)
├─ Icon (Image • 16×16 • Anchor Right-Center)
└─ Digits (RectTransform • Anchor Right-Center • Pivot=1,0.5)
└─ DigitTemplate (Image • disabled • sheet’ten herhangi bir glif)

> **Digits altında layout yok** (Horizontal/Vertical/Grid/ContentSizeFitter **kullanma**).

### 2.3 Sayaç Bileşenleri

#### `UiCounterBitmap` (Assets/Game/Core/Scripts/UI/UiCounterBitmap.cs)
- **Amaç:** 5×7 bitmap rakamlarla integer değeri çizmek.
- **Özellikler:**
  - Sabit glif boyu **6×8 px** (pixel-perfect; `SetNativeSize` **yok**).
  - **alignRight=true** → sağa yaslı, **sola doğru büyür**.
  - **iconOnLeft=true** → coin solda kalır, sayı büyüdükçe coin **sola kayar**.
  - **pixelSpacing** → karakter arası piksel boşluğu (tam sayı).
- **Inspector:**
  - `digitsRoot=Digits`, `digitTemplate=DigitTemplate (disabled)`
  - `glyphs` → 0..9 (gerekirse sırayı genişlet)
  - `alignRight=ON`, `pixelSpacing=1..3`
  - `iconRt=Icon`, `iconOnLeft=ON`, `iconSpacing=4`
  - (Kullanıyorsan) `layoutElement` → `CounterBadge` üzerindeki LayoutElement

#### Oyun Verisi (event-driven)

**Wallet** ve **Binder** (Assets/Game/Core/Scripts/UI/CoinHudBinder.cs):
- `Wallet`: Coin state + `OnCoinsChanged(int)` eventi  
  `Add(int amount)`, `Set(int value)` metodları event tetikler.
- `CoinHudBinder`: `Wallet`’ı `UiCounterBitmap`’e bağlar  
  `OnEnable()`’da abone olur, başlangıç değerini set eder; `OnDisable()`’da çıkar.

**Kurulum:**
- `GameManager` gibi bir objeye **Wallet** ekle.  
- `CounterBadge` objesine **CoinHudBinder** ekle → `wallet` ve `counter` referanslarını bağla.  
- Oyun içinde coin değişiminde **sadece** `Wallet`’ı güncelle:
  ```csharp
  wallet.Add(5);     // UI otomatik güncellenir
  // veya
  wallet.Set(1234);

3) Test Planı

Barlar: HP/Shield yatay, Energy/Heat dikey. value01’i değiştir → px/s animasyon düzgün.

Ticks: %10 çizgileri, yatay/dikeyde hizalı (padding doğru).

Full FX: Shield full → mavi pulse; Heat full → tek seferlik kırmızı flash.

Sayaç: wallet.Set(0), wallet.Add(123) → sayı sola büyür, coin sola kayar, boşluk sabit.

Çözünürlükler: 960×540 / 1920×1080 / 3840×2160 → netlik ve hizalama sorunsuz.

4) Sık Sorunlar & Çözümler

UI bulanık: PPU=64, Filter=Point, Pixel Perfect ON; scale=1.

Dikey bar ters doluyor: Fill Anchor Bottom, Pivot (0.5,0) değil → düzelt.

Sayaç boşluk/ölçü bozuluyor:

Rakam sheet Grid 6×8 dilimlenmeli (auto trim kullanma).

UiCounterBitmap SetNativeSize kullanmıyor; sabit 6×8 veriyor.

Digits altında layout component olmasın; digit Image’larında ignoreLayout=true.

Atlas kaynaklı bleeding: Atlas v2 → Tight OFF / Padding 2–4 / Rotation OFF.

5) Dosya Düzeni (öneri)
Assets/
 ├─ Art/
 │   └─ Sprites/
 │       └─ UI/
 │           ├─ Bars/
 │           ├─ Icons16/
 │           │    UI.Icon.Counter.16.png
 │           ├─ Icons32/
 │           │    UI.Icon.Counter.32.png
 │           └─ Counters/
 │                UI.Counter.Font.5x7.sheet.png
 └─ Game/
     └─ Core/
         └─ Scripts/
             └─ UI/
                 UiBar.cs
                 UiBarTicks.cs
                 UiCounterBitmap.cs
                 CoinHudBinder.cs

                 