using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GravedadCero.Level
{
    /// <summary>
    /// Fundido a negro entre escenas. Solo puede haber una transición a la vez: mientras hay
    /// una en curso, las nuevas peticiones se ignoran (evita cargas dobles por pulsar varias
    /// veces Enter, Retroceso o un botón). Usa tiempo no escalado y deja Time.timeScale en 1
    /// antes de cargar, así que funciona también si se pide desde la pausa.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        const float FadeSeconds = 0.3f;

        static SceneTransition instance;

        CanvasGroup group;
        bool busy;

        /// <summary>True mientras se oscurece la pantalla o se carga la escena siguiente.</summary>
        public static bool IsTransitioning => instance != null && instance.busy;

        /// <summary>Número de escenas cargadas con este sistema (lo usan las pruebas).</summary>
        public static int LoadCount { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void FadeInOnStart()
        {
            LoadCount = 0;
            Ensure().StartCoroutine(instance.Fade(1f, 0f));
        }

        /// <summary>Carga una escena (por nombre o ruta) con fundido. Devuelve false si ya hay una transición.</summary>
        public static bool Load(string scene)
        {
            var transition = Ensure();
            if (transition.busy) return false;
            transition.StartCoroutine(transition.Run(scene));
            return true;
        }

        static SceneTransition Ensure()
        {
            if (instance != null) return instance;
            var go = new GameObject("SceneTransition");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<SceneTransition>();
            instance.Build();
            return instance;
        }

        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            gameObject.AddComponent<GraphicRaycaster>();
            group = gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            var black = new GameObject("Black", typeof(RectTransform), typeof(Image));
            black.transform.SetParent(transform, false);
            var rect = (RectTransform)black.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            black.GetComponent<Image>().color = new Color(0.008f, 0.015f, 0.03f, 1f);
            group.alpha = 0f;
        }

        IEnumerator Run(string scene)
        {
            busy = true;
            group.blocksRaycasts = true; // ningún clic llega a la escena durante el fundido
            yield return Fade(group.alpha, 1f);
            Time.timeScale = 1f;
            AsyncOperation load = SceneManager.LoadSceneAsync(scene);
            while (load != null && !load.isDone) yield return null;
            LoadCount++;
            yield return null;
            yield return Fade(1f, 0f);
            group.blocksRaycasts = false;
            busy = false;
        }

        IEnumerator Fade(float from, float to)
        {
            for (float t = 0f; t < FadeSeconds; t += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, t / FadeSeconds);
                yield return null;
            }
            group.alpha = to;
        }
    }
}
