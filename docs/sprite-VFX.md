# VFX Havuzu (Pooling) + Yeni Input System Rehberi

> **Amaç:** VFX efektlerini *Instantiate/Destroy* yerine **havuzdan** üretip performansı artırmak; efektler **bittiğinde otomatik iade** etmek. Bu doküman, *Yeni Input System* ile tam uyumludur.

---

## Hızlı Başlangıç (TL;DR)

1. **Klasör yapısı**

   ```text
   Assets/
     Scripts/
       Core/Pooling/
         IPoolable.cs
         PooledObject.cs
         SimplePool.cs
       Gameplay/VFX/
         VfxOneShotPoolReturner.cs
         VfxPoolSpawner.cs  (opsiyonel test)
   ```
2. **Prefab ayarları:** VFX prefab’ına `VfxOneShotPoolReturner` ekle. **Play On Awake = OFF**.
3. **Sahne:** `SimplePool` içeren bir **VFX Pool** GameObject’i oluştur, `prefab` alanına VFX prefab’ını ver, `initialSize` ~20.
4. **Tetikleme:** Spawner’dan `vfxPool.Get(position, rotation)` çağır.
5. **İade:**

   * Tercihen **aliveParticleCount stratejisi** (önerilen, event gerekmez)
   * (Opsiyonel) VFX Graph’ta **Output Event** *Finished* tanımla.
   * (Opsiyonel) `fallbackLifetime` ile emniyet süresi tanımla.

---

## Ön Koşullar

* **Rendering:** URP/HDRP (VFX Graph, Built-in pipeline’da desteklenmez).
* **Package sürümleri:** *Visual Effect Graph* sürümü ile URP/HDRP sürümü **aynı major/minor** olmalı (örn. ikisi de 14.x).
* **Yeni Input System:** *Project Settings → Player → Active Input Handling* → **Input System Package (New)**.

> **Not:** 2D Renderer ile VFX görünmeyebilir. Gerekirse Universal Renderer (Forward) veya kamera stack kullanın.

---

## Kod Dosyaları ve Roller

* **`IPoolable.cs`** — Havuz kancaları: `OnTakenFromPool`, `OnReturnedToPool`.
* **`PooledObject.cs`** — Objenin hangi havuza ait olduğunu tutar, `ReturnToPool()` sağlar.
* **`SimplePool.cs`** — Tek prefab için kuyruk tabanlı havuz; `initialSize`, `maxSize`, `Get/Return`.
* **`VfxOneShotPoolReturner.cs`** — VFX **bittiğinde** (event veya alive=0 stratejisi) havuza otomatik iade.
* **`VfxPoolSpawner.cs`** — Yeni Input System ile **InputAction** üzerinden örnek tetikleyici (opsiyonel).

> **Namespace’ler:** Alt yapı `SpaceTrader.Core.Pooling`, oyun tarafı `SpaceTrader.Gameplay.VFX`.

---

## Kurulum Adımları

### 1) Havuzu Hazırla

1. **Hierarchy → Create Empty** → ad: **VFX Pool**
2. `SimplePool` component ekle.
3. `prefab` alanına **VFX prefab**’ını ver.
4. `initialSize = 20` (projeye göre artar/azalır), `maxSize = 0` (sınırsız) veya bir üst sınır.

### 2) VFX Prefab’ı

* **VisualEffect (Component):**

  * **Play On Awake = OFF**  → çifte tetik ve üst üste binme (stacking) engellenir.
  * Testte *Culling Flags* → **Always Simulate** seçilebilir; sonra normale dönün.
* **VfxOneShotPoolReturner (Component):**

  * **Return Stratejisi:**

    * `returnWhenNoAlive = true` (**önerilen**) → event gerekmeden iade.
    * `noAliveFramesToReturn` = **3–7** → erken iade engeli.
    * `startGrace = 0.25–0.4s` → başlangıçta erken kontrol yapma.
    * `postAliveCooldown = 0.05–0.1s` → son alive’tan hemen sonra iade etme.
  * (Opsiyonel) `finishEventName = "Finished"` + VFX Graph’ta Output Event ekle.
  * (Opsiyonel) `fallbackLifetime = 1.5–3.0s` emniyet.

### 3) Tetikleme (Spawner)

* `VfxPoolSpawner` (örnek) ya da kendi spawner’ında **yalnızca** `vfxPool.Get(...)` kullan.
* **Instantiate/Destroy** çağrıları **olmamalı**.

### 4) VFX Graph (Opsiyonel Event Yolu)

* Efektin doğal bittiği akışta **Output Event** bloğu ekle → **Name = `Finished`**.
* Kodda `finishEventName` ile eşleşmeli.
* Event hiç gelmese bile alive stratejisi devrede kalabilir.

---

## Ayar/Tuning Rehberi

* **Erken yok olma** → `noAliveFramesToReturn`’ı **yükselt** (örn. 5), `startGrace` ekle, **Bounds** büyüt, kısa süreli 0’ları filtrelemek için `postAliveCooldown` kullan.
* **Geç iade/ekranda takılı kalma** → `noAliveFramesToReturn`’ı **düşür** (örn. 2–3), `fallbackLifetime` ekle.
* **Üst üste animasyon eklenmesi (stacking)** → Prefab **Play On Awake = OFF** + `OnTakenFromPool` içinde **Stop→Reinit→Play**.
* **Havuz tükenmesi** → `initialSize`’ı artır, `maxSize` sınır koy veya spawner frekansını düşür.

---

## Test / Doğrulama Listesi

* [ ] **Console temiz** (Play sırasında uyarı/hata yok)
* [ ] **Profiler** → *GC Alloc* düşük, **Instantiate/Destroy yok** (yalnızca preload’da olabilir)
* [ ] **Peak senaryo** (spam) testinde iade mantığı stabil (birikme yok)
* [ ] **Input System** ile tetikleme çalışıyor (InputAction veya event)
* [ ] **Bounds** yeterli; efekt kamerada görünmese bile yanlış culling’e düşmüyor

---

## Sık Karşılaşılan Sorunlar (FAQ)

**S:** Efekt **hemen yok oluyor**.

* **C:** `noAliveFramesToReturn`’ı 5–7 yap, `startGrace` ekle; Graph **Bounds**’u büyüt; test için **Always Simulate** dene.

**S:** Efekt **bitmiyor/ekranda asılı kalıyor**.

* **C:** `noAliveFramesToReturn`’ı 2–3’e indir, `fallbackLifetime` ekle; Output Event yolunu etkinleştir.

**S:** Inspector’da **NullReference** (VFX Editor) hatası.

* **C:** Visual Effect **Asset** referansını kaldırıp yeniden ata; `Library/PackageCache` temizle; URP/HDRP ve VFX Graph sürümlerini hizala.

**S:** Event hiç gelmiyor.

* **C:** Output Event **Name** eşleşmiyor olabilir; ama zaten **alive stratejisi** ile eventsiz çalışabilir.

---

## Örnek Kullanım (Pseudo)

```csharp
// Spawner içinden
var pos = firePoint.position;
var rot = firePoint.rotation;
var go = vfxPool.Get(pos, rot);
// go üzerinde VfxOneShotPoolReturner var → bitince otomatik iade
```

---

## Definition of Done (DoD)

* [ ] Yeni Input System aktif; eski `UnityEngine.Input` kullanılmıyor
* [ ] `SimplePool` ile tetikleme; sahnede **Instantiate/Destroy** yok
* [ ] VFX Prefab: **Play On Awake = OFF**
* [ ] İade stratejisi: **alive-based** + (opsiyonel) event + (opsiyonel) fallback
* [ ] Profiler/Console temiz, peak test stabil

---

## Sürüm/Pipeline Notları

* **URP/HDRP ve VFX Graph** sürümlerinizi birlikte güncelleyin (örn. 12.x, 14.x). Mismatch, Inspector’da NRE’lere yol açabilir.
* 2D projelerde VFX’i görmek için kamera/renderer ayarlarını kontrol edin; gerekirse kamera stack veya ikinci kamera.

---

## Sprite Ayarları (2D / Pixel Art)

> Bu bölüm, bu issue kapsamında yaptığımız **sprite import/ölçek/atlas** ayarlarının özetidir. Hızlı geri dönüş için kısa reçete ve nedenleri aşağıdadır.

### 1) Sprite Import Settings — Önerilen Şablon

* **Texture Type:** Sprite (2D and UI)
* **Sprite Mode:** Single (tek görsel) / Multiple (sprite sheet)
* **Pixels Per Unit (PPU):**

  * **Dünya** (tile/taban): *seç bir standard* → örn. **32 PPU** (32px = 1 Unity unit)
  * **UI ikonları:** genelde **100 PPU** (Canvas ölçeğiyle yönetilir)
* **Filter Mode:** **Point (no filter)** *(pixel‑art için keskinlik)*
* **Compression:** **None (Truecolor)** *(UI ve keskin piksel için)*
* **Generate Mip Maps:** **OFF** *(2D/pixel‑art’ta bulanıklık yapabilir)*
* **Wrap Mode:** **Clamp** (çoğu sprite). **Repeat** sadece tiling dokular için.
* **Mesh Type:** **Full Rect** *(pixel‑art’ta kenar bleeding’i azaltır)*
* **sRGB (Color Texture):** **ON** (renkli sprite). Mask/LUT için OFF olabilir.
* **Pivot:** İçeriğe göre (Center/Custom). UI’de genelde **Center**.
* **Extrude Edges:** **1–2 px** *(kanama/bleeding’i azaltır)*

> **Not:** Daha önce kalp ikonunun sahnede küçük/büyük görünmesi sorununa karşı: **PPU standardını** belirledikten sonra **transform scale = (1,1,1)** tutmayı hedefle. Örn. 32×32 ikon için **PPU=32** seçersen, dünyada 1 birlik ölçüye tam oturur.

### 2) Sprite Editor — Slice/Grid

* **Grid** ile dilimleme: tileset için **Cell Size = 16×16** veya **32×32** (projeye göre).
* **Padding**: 2–4 px tavsiye (atlas’ta bleeding riskini azaltır).

### 3) UI — Canvas Scaler

* **Canvas Scaler** → **Scale With Screen Size**

  * **Reference Resolution:** 1920×1080 (veya 1280×720)
  * **Match:** 0.5 (genel)
  * **Reference Pixels Per Unit:** **100** (UI için sektör standardı)
* UI ikonlarını **Sprite (UI)** olarak içeri alabilir veya normal Sprite ile kullanabilirsin; amaç, UI tarafında **PPU=100** varsayımı ile stabil boyut.

### 4) Sprite Atlas (v2)

* **Create → Sprite Atlas**

  * **Include In Build:** ON
  * **Tight Packing:** **OFF** *(pixel‑art için tavsiye; kenar bozulmalarını azaltır)*
  * **Padding:** **2–4 px**
  * **Allow Rotation:** OFF *(pixel‑art’ta çevrim artefakt riski)*
* Packables’a ilgili sprite klasörlerini ekle.
* URP/HDRP kullanıyorsan versiyon uyumunu koru. 2D Renderer’da atlas kullanımı **draw call** sayısını düşürür.

### 5) Pixel‑Perfect (opsiyonel)

* **Pixel Perfect Camera** ekleyip **Assets Pixels Per Unit** değerini **dünya PPU** ile eşleştir (örn. 32).
* **Upscale Render Texture:** ON, **Crop Frame X/Y:** ON → sabit görsel tutarlılık.
