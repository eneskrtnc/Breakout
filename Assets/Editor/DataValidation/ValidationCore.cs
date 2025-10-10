// Assets/Editor/DataValidation/ValidationCore.cs
// Space Trader — Editor Validation Core (genel çekirdek)

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SpaceTrader.EditorValidation
{
    [Serializable]
    public class DomainConfig
    {
        public string DomainName; // "Ships"
        public string Label; // "ships"
        public string DefAddressPrefix; // "ships/defs/"
        public string IconAddressPrefix; // "ships/icons/"  (null => kontrol etme)
        public string PrefabAddressPrefix; // "ships/prefabs/"(null => kontrol etme)
        public string TypeNameOverride; // t: filtrelemesi için (boşsa generic T)
    }

    public class ValidationContext
    {
        public int Errors;
        public int Warnings;
        public UnityEngine.Object FirstToPing;
        public readonly Dictionary<string, List<ScriptableObject>> IdMap = new(
            StringComparer.Ordinal
        );

        public void Error(UnityEngine.Object ctx, string msg)
        {
            Errors++;
            if (ctx)
                Debug.LogError($"[Validate] {msg}", ctx);
            else
                Debug.LogError($"[Validate] {msg}");
            if (FirstToPing == null && ctx != null)
                FirstToPing = ctx;
        }

        public void Warn(UnityEngine.Object ctx, string msg)
        {
            Warnings++;
            if (ctx)
                Debug.LogWarning($"[Validate] {msg}", ctx);
            else
                Debug.LogWarning($"[Validate] {msg}");
        }
    }

    public static class ValidationCore
    {
        public static (
            int errors,
            int warnings,
            int checkedCount,
            UnityEngine.Object firstToPing
        ) ValidateDefs<TDef>(DomainConfig cfg, Action<TDef, ValidationContext> perItemRules = null)
            where TDef : ScriptableObject
        {
            var ctx = new ValidationContext();

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                ctx.Error(
                    null,
                    "Addressables Settings bulunamadı. Window → Asset Management → Addressables ile oluşturun."
                );
                EditorUtility.DisplayDialog(
                    $"Validate {cfg.DomainName}",
                    "Addressables Settings yok.",
                    "Kapat"
                );
                return (ctx.Errors, ctx.Warnings, 0, ctx.FirstToPing);
            }

            string typeName = string.IsNullOrEmpty(cfg.TypeNameOverride)
                ? typeof(TDef).Name
                : cfg.TypeNameOverride;
            string[] guids = AssetDatabase.FindAssets($"t:{typeName}");
            int checkedCount = guids.Length;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var def = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (!def)
                {
                    ctx.Error(null, $"{path}: asset yüklenemedi.");
                    continue;
                }

                // ---- id alanı
                string id = GetStringField(def, "id");
                if (string.IsNullOrWhiteSpace(id))
                {
                    ctx.Error(def, "id boş.");
                }
                else
                {
                    if (!Regex.IsMatch(id, @"^[a-z0-9_]+$"))
                        ctx.Warn(def, $"id formatı lower_snake olmalı: '{id}'");

                    if (!ctx.IdMap.TryGetValue(id, out var list))
                    {
                        list = new List<ScriptableObject>();
                        ctx.IdMap[id] = list;
                    }
                    list.Add(def);
                }

                // ---- ShipDef kaydının kendisi için addressables kontrolü (defPrefix varsa)
                if (!string.IsNullOrEmpty(cfg.DefAddressPrefix))
                {
                    ValidateAddressablesEntry(
                        settings,
                        guid,
                        cfg.Label,
                        cfg.DefAddressPrefix + id,
                        def,
                        true,
                        ctx
                    );
                }

                // ---- iconRef/prefabRef (opsiyonel config)
                if (!string.IsNullOrEmpty(cfg.IconAddressPrefix))
                {
                    var iconRef = GetAssetReference(def, "iconRef");
                    ValidateAssetReference(
                        settings,
                        iconRef,
                        cfg.Label,
                        cfg.IconAddressPrefix + id,
                        def,
                        "iconRef",
                        ctx
                    );
                }

                if (!string.IsNullOrEmpty(cfg.PrefabAddressPrefix))
                {
                    var prefabRef = GetAssetReference(def, "prefabRef");
                    ValidateAssetReference(
                        settings,
                        prefabRef,
                        cfg.Label,
                        cfg.PrefabAddressPrefix + id,
                        def,
                        "prefabRef",
                        ctx
                    );
                }

                // ---- Domain'e özgü kurallar
                if (perItemRules != null)
                {
                    try
                    {
                        perItemRules((TDef)def, ctx);
                    }
                    catch (Exception e)
                    {
                        ctx.Error(def, $"perItemRules hata: {e.Message}");
                    }
                }
            }

            // duplicate id
            foreach (var kv in ctx.IdMap)
            {
                if (kv.Value.Count > 1)
                    foreach (var d in kv.Value)
                        ctx.Error(d, $"duplicate id '{kv.Key}' — {kv.Value.Count} adet.");
            }

            // özet
            if (ctx.Errors == 0)
            {
                EditorUtility.DisplayDialog(
                    $"Validate {cfg.DomainName}",
                    $"OK ✓  ({checkedCount} asset kontrol edildi)\nUyarı: {ctx.Warnings}",
                    "Kapat"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    $"Validate {cfg.DomainName}",
                    $"Hatalar bulundu! ({ctx.Errors} hata, {ctx.Warnings} uyarı)\nDetaylar Console'da.",
                    "Tamam"
                );
                if (ctx.FirstToPing)
                    EditorGUIUtility.PingObject(ctx.FirstToPing);
            }

            return (ctx.Errors, ctx.Warnings, checkedCount, ctx.FirstToPing);
        }

        // ---------- yardımcılar ----------
        private static string GetStringField(object obj, string field)
        {
            var f = obj.GetType().GetField(field);
            return f?.GetValue(obj) as string ?? string.Empty;
        }

        private static AssetReference GetAssetReference(object obj, string field)
        {
            var f = obj.GetType().GetField(field);
            return f?.GetValue(obj) as AssetReference;
        }

        private static void ValidateAddressablesEntry(
            AddressableAssetSettings settings,
            string guid,
            string requiredLabel,
            string expectedAddress,
            UnityEngine.Object ctxObj,
            bool isDef,
            ValidationContext ctx
        )
        {
            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                ctx.Error(ctxObj, $"{(isDef ? "Def" : "Asset")} Addressables'a ekli değil.");
                return;
            }
            if (!entry.labels.Contains(requiredLabel))
                ctx.Error(ctxObj, $"Label eksik: '{requiredLabel}'.");

            if (!string.Equals(entry.address, expectedAddress, StringComparison.Ordinal))
                ctx.Warn(
                    ctxObj,
                    $"Adres beklenen değil. Şu an: '{entry.address}', Beklenen: '{expectedAddress}'."
                );
        }

        private static void ValidateAssetReference(
            AddressableAssetSettings settings,
            AssetReference reference,
            string requiredLabel,
            string expectedAddress,
            UnityEngine.Object ctxObj,
            string fieldName,
            ValidationContext ctx
        )
        {
            if (reference == null || string.IsNullOrEmpty(reference.AssetGUID))
            {
                ctx.Warn(ctxObj, $"{fieldName} atanmadı.");
                return;
            }

            var entry = settings.FindAssetEntry(reference.AssetGUID);
            if (entry == null)
            {
                ctx.Error(ctxObj, $"{fieldName}: Addressables kaydı bulunamadı.");
                return;
            }
            if (!entry.labels.Contains(requiredLabel))
                ctx.Error(ctxObj, $"{fieldName}: Label '{requiredLabel}' eksik.");

            if (!string.Equals(entry.address, expectedAddress, StringComparison.Ordinal))
                ctx.Warn(
                    ctxObj,
                    $"{fieldName}: Adres beklenen değil. Şu an: '{entry.address}', Beklenen: '{expectedAddress}'."
                );
        }
    }
}
#endif
