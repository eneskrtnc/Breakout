Çizilecekler — UI/ICON Base Set v1
A) Barlar (HUD)

Klasör: Assets/Art/Sprites/UI/Bars/
PPU: 64

Frame (9-slice) — 12px & 6px

UI.Bar.Frame.12px.9slice.png (köşe 1px ya da 3px)

UI.Bar.Frame.6px.9slice.png

Nötr renk (koyu gri), üst 1px highlight, alt 1px shadow.

Fill (tile) — Enerji / Isı / Kalkan (12px yükseklik)

Ölçü: 8×10 ya da 4×10 (iç boşluk 10px ise) — dikişsiz tekrar.

UI.Bar.Fill.Energy.8x10.tile.png (cyan ramp + diagonal dither)

UI.Bar.Fill.Heat.8x10.tile.png (turuncu ramp + checker)

UI.Bar.Fill.Shield.8x10.tile.png (mor ramp + dot)

(Ops.) Critical varyant: Heat’e yakın ton + hafif blink

UI.Bar.Fill.<Stat>.Critical.8x10.tile.png

Fill (tile) — Mini barlar (6px yükseklik)

Ölçü: 8×4 ya da 4×4

UI.Bar.Fill.<Stat>.8x4.tile.png (Energy/Heat/Shield)

Overlay (opsiyonel)

%10 tik çizgileri (sabit tile): UI.Bar.Overlay.Ticks.12px.tile.png, UI.Bar.Overlay.Ticks.6px.tile.png

HP barı tamam: aynı mantıkla Enerji/Isı/Kalkan’ı çiziyorsun.

B) Global Sayaç (Counter)

Klasör: Assets/Art/Sprites/UI/Counters/

Monospace rakam sheet (5×7 px)

Karakterler: 0–9, x, +, -

Sprite sheet tek satır, 1px padding

UI.Counter.Font.5x7.sheet.png (+ .aseprite dosyan)

Sayaç ikonları

16×16 ve 32×32 “kredi/çip” simgesi

UI.Icon.Counter.16.png, UI.Icon.Counter.32.png

C) 16 px ikon seti (HUD)

Klasör: Assets/Art/Sprites/UI/Icons16/

Stat ikonları (16×16)

Energy (⚡/bolt), Heat (🔥/flame), Shield (⬡/shield)

UI.Icon.Energy.16.png

UI.Icon.Heat.16.png

UI.Icon.Shield.16.png

UI aksiyon ikonları (16×16)

Pause, Settings, Inventory, Map

UI.Icon.Pause.16.png

UI.Icon.Settings.16.png

UI.Icon.Inventory.16.png

UI.Icon.Map.16.png

D) 32 px ikonlar (panel/envanter)

Klasör: Assets/Art/Sprites/UI/Icons32/

Stat ikonları (32×32)

UI.Icon.Energy.32.png

UI.Icon.Heat.32.png

UI.Icon.Shield.32.png

Global Sayaç 32×32

UI.Icon.Counter.32.png

E) Atlaslar (Sprite Atlas v2)

Klasör: Assets/Art/SpriteAtlases/

UI_16.atlas (16px grubun tamamı)

UI_32.atlas (32px grubun tamamı)
Ayarlar: Tight OFF, Padding 2–4 px, Rotation OFF

Dosya/İsimlendirme kuralları (özet)

UI.Bar.Frame.<height>px.9slice.png

UI.Bar.Fill.<Stat>.<w>x<h>.tile.png

UI.Bar.Overlay.Ticks.<height>px[.tile].png

UI.Icon.<Name>.16.png / 32.png

UI.Counter.Font.5x7.sheet.png

Kabul kriterleri (AC) kısaca

Tile’lar dikişsiz ve stretch yok (Tiled kullanımına uygun).

Filter=Point, Compression=None, MipMaps=OFF, Mesh=Full Rect, PPU=64.

Color-blind simülasyonda stat renkleri + doku ile ayırt edilebilir.

16/32 ikonlar küçük ölçekte okunaklı (outline ve şekil net).

Atlas’ta padding ≥2 px, tight/rotation kapalı → bleeding yok.

Çizim sırası (öneri)

Enerji/Isı/Kalkan fill tile’ları (12px)

16 px stat ikonları (Energy/Heat/Shield)

Global sayaç: 5×7 font + 16/32 sayaç ikonu

16 px UI ikonları (Pause/Settings/Inventory/Map)

32 px stat & sayaç ikonları

Mini bar 6px fill’ler

Atlaslar