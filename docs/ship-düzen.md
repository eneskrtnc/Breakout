1) Hedef boyutu seç (PPU 64 → 1 unit = 64 px)

Önerilen sınıflar (ekranda görünen piksel boyu):

Fighter (S): 64×64

Corvette (M): 96×96

Frigate (L): 128×128

Cruiser (XL): 192×128 veya 192×192

Master’ı ister 128/256 px çiz, ama Unity’ye hedef pikselde (örn. 96×96) koyacağız.

2) Aseprite çalışma biçimi

Master çalışma: 2× ya da 4× büyük çizebilirsin (örn. 192×192 → hedef 96×96).

Grid/kurallar (2× güvenli):

Çizgi kalınlıklarını çift piksel tut (2 px, 4 px).

Dither/pattern’i 2×2 veya 4×4 periyotla çiz.

Kritik minik highlight’ları 2×2 blok olarak planla (tek piksellik parıltı 50% küçülmede kaybolur).

Export: Sprite → Sprite Size → 50% (Interpolation: None/Nearest) → hedef piksel.
Gerekirse küçük rötuş yap (outline’ı 1 px’e indir, kaybolan detayları yeniden koy).

Katman önerisi: outline / base panels / shadows / highlights / decals / emissives (engine, windows)

3) Unity import (World sprites)

Her gemi sprite’ında:

Texture Type: Sprite (2D and UI)

Mesh Type: Full Rect

Filter: Point

Compression: None

MipMaps: OFF

Pixels Per Unit: 64 (sabit)

Pivot: Center (0.5, 0.5) – döndürme/nişan için ideal

Scale = (1,1,1) bırak. Büyütme/küçültme gerekiyorsa Aseprite’te yeni export al.

4) Prefab düzeni (öneri)
Ship_X (Prefab, scale=1)
 ├─ Sprite (SpriteRenderer)   ← gemi görseli
 ├─ Hardpoints (empty)
 │   ├─ GunL (empty)          ← world units: piksel/64
 │   └─ GunR (empty)
 ├─ Thrusters (empty)
 │   ├─ Main (empty)
 │   └─ Side (empty)
 ├─ Colliders (empty)
 │   ├─ BoxCollider2D (gövde)
 │   └─ PolygonCollider2D (isteğe bağlı ince işler)
 └─ VFX (empty)               ← motor alevi, patlama vs. child objeler


Hardpoint/Thruster boş objelerini piksele hizala: konumları (n / 64) unit (örn. 10 px sağ → 0.15625 unit).

Collider’ları görseli hafif “içinde” bırak (kenardan 1–2 px içeri), performans için 1–2 kutu + küçük poligon makul.

5) Kamera & pixel-perfect

URP 2D kullanıyorsan Pixel Perfect Camera:

Assets PPU = 64

Upscale Render Texture = ON, Crop Frame X/Y = ON

Game View’de referans çözünürlük ve katları (1080p/1440p) ile test et.