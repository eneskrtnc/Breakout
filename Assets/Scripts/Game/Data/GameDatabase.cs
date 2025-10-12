using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SpaceTrader.Game.Data
{
    [CreateAssetMenu(menuName = "Game/Core/GameDatabase", fileName = "GameDatabase")]
    public sealed class GameDatabase : ScriptableObject
    {
        [Header("Catalogs (optional, id→GameDef için)")]
        public List<GameCatalog> catalogs = new();

        // GameDef hızlı erişim (id → GameDef)
        [NonSerialized]
        Dictionary<string, GameDef> _byId;

        // BaseDef set’leri (type → DefSet<T>)
        [NonSerialized]
        readonly Dictionary<Type, object> _sets = new();

        public DatabaseHealth Health { get; private set; } =
            new DatabaseHealth { State = DatabaseState.NotStarted };

        public bool IsInitialized { get; private set; }

        void OnEnable()
        {
            RebuildCatalogIndex();
            if (Health == null)
                Health = new DatabaseHealth { State = DatabaseState.NotStarted };
        }

        /// <summary>GameCatalog listesinden id→GameDef sözlüğünü yeniden kur.</summary>
        public void RebuildCatalogIndex()
        {
            _byId = new Dictionary<string, GameDef>(catalogs?.Count ?? 0);
            if (catalogs == null)
                return;

            foreach (var c in catalogs)
            {
                if (!c)
                    continue;
                foreach (var d in c.All)
                {
                    if (!d || string.IsNullOrWhiteSpace(d.id))
                        continue;
                    _byId[d.id] = d;
                }
            }
        }

        /// <summary>id→GameDef (Catalog tabanlı kısayol)</summary>
        public bool TryGet(string id, out GameDef def)
        {
            if (_byId == null)
            {
                def = null;
                return false;
            }
            return _byId.TryGetValue(id, out def);
        }

        /// <summary>Addressables’tan tüm BaseDef türevlerini etikete göre yükleyip set’leri kurar.</summary>
        public async Task InitAsync()
        {
            if (IsInitialized)
                return;

            Health.State = DatabaseState.Initializing;
            await Addressables.InitializeAsync().Task;

            var defTypes = AppDomain
                .CurrentDomain.GetAssemblies()
                .SelectMany(GetTypesSafe)
                .Where(t => t != null && !t.IsAbstract && typeof(BaseDef).IsAssignableFrom(t))
                .ToArray();

            foreach (var t in defTypes)
            {
                var label =
                    t.GetCustomAttribute<DataLabelAttribute>()?.Label ?? DefaultLabelForType(t);
                await LoadTypeIntoSet(t, label);
            }

            IsInitialized = true;
            Health.State = DatabaseState.Ready;
            Debug.Log($"[GameDB] Initialized with {_sets.Count} def sets.");
        }

        /// <summary>T türü için set al (yoksa boş set döner).</summary>
        public DefSet<T> Set<T>()
            where T : BaseDef =>
            _sets.TryGetValue(typeof(T), out var boxed) ? (DefSet<T>)boxed : new DefSet<T>();

        /// <summary>Id ile T tipindeki def’i getir (set içinde).</summary>
        public T Get<T>(string id)
            where T : BaseDef => Set<T>().Get(id);

        // ----------------- Helpers -----------------

        static IEnumerable<Type> GetTypesSafe(Assembly a)
        {
            try
            {
                return a.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null)!;
            }
        }

        static string DefaultLabelForType(Type t)
        {
            var n = t.Name;
            if (n.EndsWith("Def", StringComparison.OrdinalIgnoreCase))
                n = n[..^3];
            return n.ToLowerInvariant() + "s"; // ShipDef -> ships
        }

        async Task LoadTypeIntoSet(Type t, string label)
        {
            // 1) Action<T> callback: list.Add(x)
            var listType = typeof(List<>).MakeGenericType(t);
            var listObj = Activator.CreateInstance(listType);
            var addMeth = listType.GetMethod("Add", new[] { t });

            var p = Expression.Parameter(t, "x");
            var addCall = Expression.Call(Expression.Constant(listObj), addMeth!, p);
            var actionTp = typeof(Action<>).MakeGenericType(t);
            var callback = Expression.Lambda(actionTp, addCall, p).Compile(); // Action<T>

            // 2) Label + type ile lokasyonları bul
            var locHandle = Addressables.LoadResourceLocationsAsync(label, t);
            await locHandle.Task;
            var locations = locHandle.Result; // IList<IResourceLocation>

            if (locations == null || locations.Count == 0)
            {
                Debug.LogWarning(
                    $"[GameDB] No assets for label='{label}' and type='{t.Name}'. (Label verdin mi?)"
                );

                var emptySetType = typeof(DefSet<>).MakeGenericType(t);
                var emptySet = Activator.CreateInstance(emptySetType);
                emptySetType
                    .GetMethod("Replace")!
                    .Invoke(emptySet, new object[] { Array.CreateInstance(t, 0) });
                _sets[t] = emptySet!;
                return;
            }

            // 3) Addressables.LoadAssetsAsync<T>(IEnumerable<IResourceLocation>, Action<T>)
            var loadAssetsGeneric = typeof(Addressables)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "LoadAssetsAsync" && m.IsGenericMethodDefinition)
                .FirstOrDefault(m =>
                {
                    var ps = m.GetParameters();
                    if (ps.Length != 2)
                        return false;
                    var p0 = ps[0].ParameterType;
                    if (!p0.IsGenericType)
                        return false;
                    var gtd = p0.GetGenericTypeDefinition();
                    return gtd == typeof(IEnumerable<>) || gtd == typeof(IList<>);
                });

            // Fallback: object key overload’ı kullan (label ile)
            bool useObjectKey = false;
            if (loadAssetsGeneric == null)
            {
                loadAssetsGeneric = typeof(Addressables)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m =>
                    {
                        if (m.Name != "LoadAssetsAsync" || !m.IsGenericMethodDefinition)
                            return false;
                        var ps = m.GetParameters();
                        return ps.Length == 2 && ps[0].ParameterType == typeof(object);
                    });
                useObjectKey = true;
            }

            if (loadAssetsGeneric == null)
                throw new InvalidOperationException(
                    "Addressables.LoadAssetsAsync için uygun overload bulunamadı."
                );

            var loadAssetsT = loadAssetsGeneric.MakeGenericMethod(t);

            object handleObj;
            if (useObjectKey)
            {
                // object key overload: key = label
                handleObj = loadAssetsT.Invoke(null, new object[] { label, callback })!;
            }
            else
            {
                handleObj = loadAssetsT.Invoke(null, new object[] { locations, callback })!;
            }

            // 4) Handle.Task bekle
            var taskProp = handleObj.GetType().GetProperty("Task");
            var task = (Task)taskProp!.GetValue(handleObj)!;
            await task;

            // 5) DefSet<T>.Replace(list)
            var setType = typeof(DefSet<>).MakeGenericType(t);
            var set = Activator.CreateInstance(setType);
            setType.GetMethod("Replace")!.Invoke(set, new object[] { (IEnumerable)listObj });

            _sets[t] = set!;
        }

        // -------------- (Opsiyonel) Global erişim kolaylığı --------------

        static GameDatabase _instance;

        /// <summary>
        /// Editor’de: projedeki ilk GameDatabase asset’ini bulur.
        /// Runtime’da: Resources/GameDatabase arar (istersen asset’i Resources altına koy).
        /// </summary>
        public static GameDatabase Instance
        {
            get
            {
                if (_instance)
                    return _instance;
#if UNITY_EDITOR
                var guids = UnityEditor.AssetDatabase.FindAssets("t:GameDatabase");
                if (guids.Length > 0)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<GameDatabase>(path);
                    if (_instance)
                        return _instance;
                }
#endif
                _instance = Resources.Load<GameDatabase>("GameDatabase");
                return _instance;
            }
            set { _instance = value; }
        }
    }
}
