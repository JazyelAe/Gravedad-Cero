using GravedadCero.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GravedadCero.UI
{
    /// <summary>
    /// Estados visuales de un botón Helios-7: normal, seleccionado (teclado o ratón encima),
    /// presionado y desactivado. Al pasar el ratón por encima el botón también queda
    /// seleccionado, así teclado y ratón muestran siempre el mismo foco. Suena al moverse
    /// entre botones y al pulsarlos. Usa tiempo no escalado para funcionar también en pausa.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public class UIButtonFX : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        static readonly Color NormalFill = new Color(0.075f, 0.137f, 0.212f, 0.92f);
        static readonly Color SelectedFill = new Color(0.06f, 0.36f, 0.48f, 0.98f);
        static readonly Color PressedFill = new Color(0.10f, 0.55f, 0.70f, 1f);
        static readonly Color DisabledFill = new Color(0.05f, 0.07f, 0.10f, 0.7f);

        Selectable selectable;
        Image fill;
        Image accent;
        TMP_Text label;
        TMP_Text subtitle;
        Image chevron;
        float highlight;
        bool pressed;
        float accentHeight;

        // Último botón seleccionado: al abrir un panel nuevo el foco salta sin sonar.
        static GameObject lastSelected;

        public void Bind(Selectable target, Image fillImage, Image accentImage, TMP_Text labelText, TMP_Text subtitleText, Image chevronImage)
        {
            selectable = target;
            fill = fillImage;
            accent = accentImage;
            label = labelText;
            subtitle = subtitleText;
            chevron = chevronImage;
            accentHeight = accent.rectTransform.sizeDelta.y;
            if (target is Button button) button.onClick.AddListener(() => GameAudio.Play(GameAudio.Catalog.uiClick, 0.7f));
            Apply(0f);
        }

        bool IsSelected => EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;

        void Update()
        {
            if (selectable == null) return;
            bool interactable = selectable.IsInteractable();
            float target = interactable && IsSelected ? 1f : 0f;
            highlight = Mathf.MoveTowards(highlight, target, Time.unscaledDeltaTime * 9f);
            Apply(highlight);
        }

        void Apply(float t)
        {
            bool interactable = selectable == null || selectable.IsInteractable();
            fill.color = !interactable ? DisabledFill : pressed ? PressedFill : Color.Lerp(NormalFill, SelectedFill, t);
            accent.color = interactable ? Color.Lerp(UITheme.CyanDim, UITheme.Cyan, t) : new Color(1, 1, 1, 0.08f);
            accent.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(4f, 8f, t), accentHeight);
            label.color = interactable ? UITheme.Text : UITheme.Locked;
            if (subtitle != null) subtitle.color = interactable ? Color.Lerp(UITheme.TextSecondary, UITheme.Text, t * 0.6f) : UITheme.Locked;
            var c = UITheme.Cyan;
            chevron.color = new Color(c.r, c.g, c.b, t);
            chevron.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(-34f, -24f, t), 0f);
            transform.localScale = Vector3.one * (pressed ? 0.985f : 1f + 0.012f * t);
        }

        public void OnSelect(BaseEventData eventData)
        {
            bool moved = lastSelected != null && lastSelected != gameObject && lastSelected.activeInHierarchy;
            lastSelected = gameObject;
            if (moved) GameAudio.Play(GameAudio.Catalog.uiMove, 0.6f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (selectable != null && selectable.IsInteractable() && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnPointerDown(PointerEventData eventData) => pressed = selectable != null && selectable.IsInteractable();

        public void OnPointerUp(PointerEventData eventData) => pressed = false;

        public void OnPointerExit(PointerEventData eventData) => pressed = false;

        void OnDisable()
        {
            pressed = false;
            highlight = 0f;
            if (fill != null) Apply(0f);
        }
    }
}
