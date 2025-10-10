// Assets/Editor/DataValidation/ValidateShips.cs
// Space Trader — Editor Validate (Ships) — tip güvenli (ShipDef)

#if UNITY_EDITOR
using UnityEditor;
using SpaceTrader.EditorValidation;
using SpaceTrader.Data; // <-- ShipDef'in gerçek namespace'i buysa bırak; farklıysa kendi namespace'inle değiştir.

public static class ValidateShips
{
    [MenuItem("Data/Validate Ships", false, 10)]
    public static void Validate()
    {
        var cfg = new SpaceTrader.EditorValidation.DomainConfig
        {
            DomainName = "Ships",
            Label = "ships",
            DefAddressPrefix = "ships/defs/",
            IconAddressPrefix = "ships/icons/",
            PrefabAddressPrefix = "ships/prefabs/",
            // TypeNameOverride = nameof(ShipDef) // Gerek değil; generic tipten okunuyor
        };

        // Domain'e özgü kurallar: hp>0, cargo>=0, speed>0
        ValidationCore.ValidateDefs<ShipDef>(
            cfg,
            (ship, ctx) =>
            {
                if (ship.hp <= 0)
                    ctx.Warn(ship, $"hp > 0 olmalı (şu an: {ship.hp})");
                if (ship.cargo < 0)
                    ctx.Warn(ship, $"cargo ≥ 0 olmalı (şu an: {ship.cargo})");
                if (ship.speed <= 0f)
                    ctx.Warn(ship, $"speed > 0 olmalı (şu an: {ship.speed})");
            }
        );
    }

    // (Opsiyonel) Menü sadece ShipDef varsa aktif olsun:
    [MenuItem("Data/Validate Ships", true, 10)]
    public static bool Validate_MenuEnabled()
    {
        return AssetDatabase.FindAssets("t:ShipDef").Length > 0;
    }
}
#endif
