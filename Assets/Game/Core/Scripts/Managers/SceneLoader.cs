using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceTrader.Core
{
    public class SceneLoader : MonoBehaviour
    {
        [Tooltip("Oyun açılışında additive yüklenecek içerik sahnesi")]
        public string firstContentScene = "Zephyra";

        private void Start()
        {
            if (!string.IsNullOrEmpty(firstContentScene))
                StartCoroutine(LoadContent(firstContentScene));
        }

        public IEnumerator LoadContent(string sceneName)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning($"[SceneLoader] Scene not in build: {sceneName}");
                yield break;
            }
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            yield return op;
        }

        public IEnumerator UnloadContent(string sceneName)
        {
            var scn = SceneManager.GetSceneByName(sceneName);
            if (!scn.isLoaded)
                yield break;

            yield return SceneManager.UnloadSceneAsync(sceneName);
            yield return Resources.UnloadUnusedAssets();
        }
    }
}
