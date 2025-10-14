#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using SpaceTrader.Core;
using SpaceTrader.Core.Data;

public static class ValidateDefs
{
    [MenuItem("Tools/Data/Validate All Defs")]
    public static void Run()
    {
        var db = Object.FindFirstObjectByType<DefDatabase>();
        if (!db)
        {
            EditorUtility.DisplayDialog(
                "Validate All Defs",
                "DefDatabase sahnede bulunamadı. Core sahnesini açın.",
                "Tamam"
            );
            return;
        }

        // DefDatabase indeksini kullanarak hataları tarayalım:
        db.BuildIndex();

        var catalogs =
            db.GetType()
                .GetField(
                    "catalogs",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
                )
                ?.GetValue(db) as List<CatalogBase>;

        var all = new List<BaseDef>();
        foreach (var c in catalogs.Where(c => c != null))
            all.AddRange(c.Enumerate());

        int n = all.Count;
        int warn = 0,
            err = 0;
        var sb = new StringBuilder();

        // Boş id
        var emptyId = all.Where(d => string.IsNullOrEmpty(d.Id)).ToList();
        if (emptyId.Count > 0)
        {
            warn++;
            sb.AppendLine($"- Boş id: {emptyId.Count}");
        }

        // Regex
        var badRegex = all.Where(d =>
                !string.IsNullOrEmpty(d.Id)
                && !System.Text.RegularExpressions.Regex.IsMatch(d.Id, "^[a-z0-9_]+$")
            )
            .ToList();
        if (badRegex.Count > 0)
        {
            warn++;
            sb.AppendLine($"- id regex uyumsuz: {string.Join(", ", badRegex.Select(d => d.Id))}");
        }

        // Duplicate'lar tipe göre
        var dups = all.Where(d => !string.IsNullOrEmpty(d.Id))
            .GroupBy(d => (d.GetType(), d.Id))
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key.GetType().Name}.{g.Key.Id} (x{g.Count()})")
            .ToList();
        if (dups.Count > 0)
        {
            err++;
            sb.AppendLine($"- Duplicate id (type+id): {string.Join(", ", dups)}");
        }

        // Eksik ikon/prefab
        var noIcon = all.Where(d => d.Icon == null).ToList();
        if (noIcon.Count > 0)
        {
            warn++;
            sb.AppendLine($"- Icon eksik: {string.Join(", ", noIcon.Select(d => d.Key))}");
        }

        var noPrefab = all.Where(d => d.Prefab == null).ToList();
        if (noPrefab.Count > 0)
        {
            warn++;
            sb.AppendLine($"- Prefab eksik: {string.Join(", ", noPrefab.Select(d => d.Key))}");
        }

        if (err == 0 && warn == 0)
            EditorUtility.DisplayDialog("Validate All Defs", $"OK ({n} defs)", "Tamam");
        else
            EditorUtility.DisplayDialog(
                "Validate All Defs",
                $"Kontrol tamamlandı ({n} defs)\nHata: {err}  Uyarı: {warn}\n\n{sb}",
                "Tamam"
            );
    }
}
#endif
