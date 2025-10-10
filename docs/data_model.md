1) Klasör Düzeni (öneri)
Assets/
  Data/
    Ships/
      starter_ship.asset
  Scripts/
    Data/
      Ships/
        ShipDef.cs
      GameDatabase/
        GameDatabase.cs
  Art/
    Ships/
      Icons/
      Prefabs/

2) Kimlik & Adlandırma

id: küçük harf + underscore, boşluk yok.
Örn: starter_ship, cargo_freighter_mk1

displayName: oyuncuya görünen ad.

desc: 1–2 cümlelik açıklama.

Do: starter_ship, mining_drone, pirate_raider_mk2
Don’t: Starter Ship, starter-ship, StarterShip, starter ship

3) ShipDef Şeması (Hızlı Referans)
// Assets/Scripts/Data/Ships/ShipDef.cs
[CreateAssetMenu(menuName = "SpaceTrader/ShipDef")]
public class ShipDef : ScriptableObject
{
    public string id;                  // unique (lower_snake)
    public string displayName;         // UI name
    [TextArea] public string desc;
    public List<string> tags;          // ["starter","civilian",...]
    public int hp;                     // > 0
    public int cargo;                  // >= 0
    public float speed;                // > 0
    public AssetReferenceSprite iconRef;
    public AssetReferenceGameObject prefabRef;
}


Sağduyu sınırları: hp > 0, cargo ≥ 0, speed > 0.

4) Addressables Sözleşmesi

Label: ships

Adresler:

ShipDef: ships/defs/{id}

Sprite: ships/icons/{id}

Prefab: ships/prefabs/{id}

Play Mode & Build

Geliştirme: Use Asset Database (Fast)

Player build öncesi: Build → New Build → Default Build Script (Addressables grupları)

5) Yeni Gemi Ekleme (Checklist)

 Prefab ve Icon hazır.

 ShipDef oluştur → Assets/Data/Ships/{id}.asset

 id, displayName, desc, tags, hp, cargo, speed, iconRef, prefabRef doldur.

 Üçünü de Addressable yap (Label = ships):

 ShipDef → ships/defs/{id}

 Icon → ships/icons/{id}

 Prefab → ships/prefabs/{id}

 Data → Validate Ships → OK

 Smoke: Prefab sahneye instantiate ediliyor mu?

 Commit & push.

6) Yükleme Akışı (Bootstrap → GameDatabase)

Açılışta:

await GameDatabase.InitAsync(); // ships dahil tüm domainler


Erişim Örnekleri:

// def çekme
if (GameDatabase.Ships.TryGet("starter_ship", out var def))
{
    // prefab yükle & spawn
    var shipGo = await GameDatabase.Ships.LoadPrefabAsync(def.id);
    Instantiate(shipGo, spawnPos, Quaternion.identity);

    // ikon yükle & UI'da kullan
    var icon = await GameDatabase.Ships.LoadIconAsync(def.id);
    uiImage.sprite = icon;
}


Not: Load*Async çağrılarını sık tekrarlama; gerekirse önbellekle.

7) Editor Validate (Gemiler)

Menü: Data → Validate Ships

Kontroller:

 id boş değil

 Duplicate id yok

 prefabRef atanmış

 iconRef atanmış

 Label = ships

 Adres sözleşmesi doğru:

ShipDef: ships/defs/{id}

Icon: ships/icons/{id}

Prefab: ships/prefabs/{id}

 Alan sınırları: hp > 0, cargo ≥ 0, speed > 0

Çıktılar:

Dialog: OK veya Issues found

Console: Kural bazlı detaylı log (asset’lere ping)

8) Smoke Test (Kısa Rehber)

EditMode/PlayMode kısa senaryo:

LoadPrefabAsync("starter_ship") → Instantiate → 1 kare → Destroy

Console: Loaded ShipDefs: {N} / Spawned: Starter Shuttle

CI’da bu testi koşturmak önerilir.

9) Sık Hatalar & Çözüm

Boş/yanlış id: lower_snake; boşluk yok.

Duplicate id: Validate ile yakala; benzersiz yap.

Yanlış label/adres: Üç nesne de Label=ships; adresleri şablona göre düzelt.

Icon/Prefab atanmamış: ShipDef’te referansları doldur.

Play Mode yanlış: Geliştirme için Use Asset Database (Fast).

Build sonrası eksik asset: Addressables Build al.

10) Diğer “Def” Tiplerine Yayılım (Şablon)

XDef : ScriptableObject (id + domain alanları + asset refs)

Label: x

Adresler: x/defs/{id}, x/icons/{id}, x/prefabs/{id}

GameDatabase.X.InitAsync(label: "x")

GameDatabase.X.TryGet / LoadIconAsync / LoadPrefabAsync

Menü: Data → Validate X (X için aynı kurallar)