using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions; // önemli
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace SpaceTrader.Data
{
    public static class GameDatabase
    {
        public static DatabaseHealth Health { get; private set; } =
            new DatabaseHealth { State = DatabaseState.NotStarted };

        private static readonly Dictionary<Type, object> _sets = new();

        public static bool IsInitialized { get; private set; }

        public static async Task InitAsync()
        {
            if (IsInitialized)
                return;

            await Addressables.InitializeAsync().Task;

            var defTypes = AppDomain
                .CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        return e.Types.Where(t => t != null);
                    }
                })
                .Where(t => t != null && !t.IsAbstract && typeof(BaseDef).IsAssignableFrom(t))
                .ToArray();

            foreach (var t in defTypes)
            {
                var label =
                    t.GetCustomAttribute<DataLabelAttribute>()?.Label ?? DefaultLabelForType(t);
                await LoadTypeIntoSet(t, label);
            }

            IsInitialized = true;
            Debug.Log($"[GameDB] Initialized with {_sets.Count} def sets.");
        }

        public static DefSet<T> Set<T>()
            where T : BaseDef =>
            _sets.TryGetValue(typeof(T), out var boxed) ? (DefSet<T>)boxed : new DefSet<T>();

        public static T Get<T>(string id)
            where T : BaseDef => Set<T>().Get(id);

        private static string DefaultLabelForType(Type t)
        {
            var n = t.Name;
            if (n.EndsWith("Def", StringComparison.OrdinalIgnoreCase))
                n = n[..^3];
            return n.ToLowerInvariant() + "s"; // ShipDef -> ships
        }

        private static async Task LoadTypeIntoSet(System.Type t, string label)
        {
            // list<T> ve callback: (T x) => list.Add(x)
            var listType = typeof(List<>).MakeGenericType(t);
            var listObj = System.Activator.CreateInstance(listType);
            var addMeth = listType.GetMethod("Add", new[] { t });

            var p = Expression.Parameter(t, "x");
            var addCall = Expression.Call(Expression.Constant(listObj), addMeth!, p);
            var actionTp = typeof(System.Action<>).MakeGenericType(t);
            var callback = Expression.Lambda(actionTp, addCall, p).Compile(); // Action<T>

            // 1) Önce label + tip için lokasyonları bul
            var locHandle = Addressables.LoadResourceLocationsAsync(label, t);
            await locHandle.Task;
            var locations = locHandle.Result; // IList<IResourceLocation>

            if (locations == null || locations.Count == 0)
            {
                UnityEngine.Debug.LogWarning(
                    $"[GameDB] No assets for label='{label}' and type='{t.Name}'. "
                        + $"(Asset'lere '{label}' label'ını verdin mi?)"
                );

                var setType = typeof(DefSet<>).MakeGenericType(t);
                var emptySet = System.Activator.CreateInstance(setType);
                setType
                    .GetMethod("Replace")!
                    .Invoke(emptySet, new object[] { System.Array.CreateInstance(t, 0) });
                _sets[t] = emptySet!;
                return;
            }

            // 2) Uygun LoadAssetsAsync<T> overload’ını seç: IList<>/IEnumerable<> ya da object
            var methods = typeof(Addressables)
                .GetMethods(
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
                )
                .Where(m => m.Name == "LoadAssetsAsync" && m.IsGenericMethodDefinition);

            var ilistResLoc = typeof(IList<>).MakeGenericType(typeof(IResourceLocation));
            var ienumResLoc = typeof(IEnumerable<>).MakeGenericType(typeof(IResourceLocation));

            System.Reflection.MethodInfo selected = null;
            foreach (var m in methods)
            {
                var ps = m.GetParameters();
                if (ps.Length != 2)
                    continue;
                var p0 = ps[0].ParameterType;
                if (p0.IsGenericType)
                {
                    var gdef = p0.GetGenericTypeDefinition();
                    if (p0.IsAssignableFrom(ilistResLoc) || p0.IsAssignableFrom(ienumResLoc))
                    {
                        selected = m;
                        break; // locations alan overload bulundu
                    }
                }
            }

            bool useObjectKey = false;
            if (selected == null)
            {
                // Fallback: (object key, Action<T>) overload
                selected = methods.FirstOrDefault(m =>
                {
                    var ps = m.GetParameters();
                    return ps.Length == 2 && ps[0].ParameterType == typeof(object);
                });
                if (selected == null)
                    throw new System.InvalidOperationException(
                        "Addressables.LoadAssetsAsync uygun overload bulunamadı."
                    );
                useObjectKey = true;
            }

            selected = selected.MakeGenericMethod(t);

            object handleObj;
            if (useObjectKey)
            {
                // object key overload: key = label
                handleObj = selected.Invoke(null, new object[] { label, callback })!;
            }
            else
            {
                // locations overload
                handleObj = selected.Invoke(null, new object[] { locations, callback })!;
            }

            // Generic handle’ın Task’ını al ve await et
            var taskProp = handleObj.GetType().GetProperty("Task");
            var task = (Task)taskProp!.GetValue(handleObj)!;
            await task;

            // 3) DefSet<T>.Replace(list)
            var setType2 = typeof(DefSet<>).MakeGenericType(t);
            var set = System.Activator.CreateInstance(setType2);
            setType2.GetMethod("Replace")!.Invoke(set, new object[] { (IEnumerable)listObj });

            _sets[t] = set!;
        }
    }
}
