using UnityEngine;

namespace GravedadCero.UI
{
    /// <summary>
    /// Entrada suave de un elemento: aparece con un fundido y un pequeño desplazamiento
    /// tras un retraso opcional. Usa tiempo no escalado (funciona en pausa).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIReveal : MonoBehaviour
    {
        CanvasGroup group;
        RectTransform rect;
        Vector2 restPosition;
        Vector2 offset;
        float delay;
        float duration = 0.35f;
        float elapsed = -1f;

        /// <summary>Añade (o reutiliza) el efecto y lo reproduce.</summary>
        public static UIReveal Play(GameObject target, float delay = 0f, Vector2? offset = null, float duration = 0.35f)
        {
            var reveal = target.GetComponent<UIReveal>() ?? target.AddComponent<UIReveal>();
            reveal.Restart(delay, offset ?? new Vector2(-24f, 0f), duration);
            return reveal;
        }

        public bool Finished => elapsed >= delay + duration;

        void Awake()
        {
            group = GetComponent<CanvasGroup>();
            rect = (RectTransform)transform;
        }

        void Restart(float startDelay, Vector2 startOffset, float length)
        {
            if (group == null) Awake();
            if (elapsed < 0f || Finished) restPosition = rect.anchoredPosition;
            delay = startDelay;
            offset = startOffset;
            duration = length;
            elapsed = 0f;
            group.alpha = 0f;
            rect.anchoredPosition = restPosition + offset;
        }

        void Update()
        {
            if (elapsed < 0f || Finished) return;
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01((elapsed - delay) / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            group.alpha = eased;
            rect.anchoredPosition = Vector2.Lerp(restPosition + offset, restPosition, eased);
        }
    }
}
