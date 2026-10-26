using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GravedadCero.UI
{
    /// <summary>
    /// Piezas de interfaz compartidas por el menú principal, el HUD y la pausa: canvas,
    /// textos, paneles, botones, deslizadores y casillas con el mismo estilo de estación.
    /// Se construyen por código para no depender de assets de UI.
    /// </summary>
    public static class UIFactory
    {
        public static readonly Color Cyan = new Color(0.45f, 0.9f, 1f);
        public static readonly Color Amber = new Color(1f, 0.7f, 0.25f);
        public static readonly Color PanelColor = new Color(0.04f, 0.07f, 0.11f, 0.82f);
        public static readonly Color ButtonColor = new Color(0.12f, 0.17f, 0.24f, 0.95f);
        public static readonly Color ButtonHighlight = new Color(0.10f, 0.55f, 0.70f, 1f);
        public static readonly Color DisabledColor = new Color(0.1f, 0.12f, 0.15f, 0.6f);

        static Font font;

        public static Font Font => font != null ? font : (font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

        /// <summary>Canvas a pantalla completa que escala con la resolución (referencia 1920x1080).</summary>
        public static Transform Canvas(Transform parent, string name, int sortingOrder = 0)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return go.transform;
        }

        public static Text Label(Transform parent, string name, string text, int size, TextAnchor alignment, Vector2 anchor,
            Vector2 position, Vector2 boxSize, Color color, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = boxSize;
            var label = go.GetComponent<Text>();
            label.font = Font;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = alignment;
            label.color = color;
            label.supportRichText = true;
            label.text = text;
            return label;
        }

        /// <summary>Imagen que cubre todo su padre (fondos y oscurecidos).</summary>
        public static Image FullScreenImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        /// <summary>Panel vertical que ajusta su altura al contenido.</summary>
        public static GameObject Panel(Transform parent, string name, Vector2 anchor, Vector2 position, float width)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(width, 0);
            go.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            go.GetComponent<Image>().color = PanelColor;
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 40, 40);
            layout.spacing = 18;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            return go;
        }

        public static Button Button(Transform parent, string text, Action onClick, float height = 74f)
        {
            var go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(520, height);
            go.GetComponent<Image>().color = Color.white;
            var button = go.GetComponent<Button>();
            ApplyColors(button);
            button.onClick.AddListener(() => onClick());
            var label = Label(go.transform, "Label", text, 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, height), Color.white);
            label.raycastTarget = false;
            return button;
        }

        /// <summary>Texto que participa en un panel vertical con una altura fija.</summary>
        public static Text PanelText(Transform panel, string name, string text, int size, float height, Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var label = Label(panel, name, text, size, alignment, new Vector2(0.5f, 1), Vector2.zero, new Vector2(520, height), color, style);
            label.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            return label;
        }

        /// <summary>Deslizador horizontal con su etiqueta encima, dentro de un panel vertical.</summary>
        public static Slider Slider(Transform panel, string name, float min, float max, float value, Action<float> onChanged)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider), typeof(LayoutElement));
            go.transform.SetParent(panel, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(520, 44);
            go.GetComponent<LayoutElement>().preferredHeight = 44;

            var background = Stretch(go.transform, "Background", new Vector2(0, 0.35f), new Vector2(1, 0.65f));
            background.color = ButtonColor;
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            SetStretch((RectTransform)fillArea.transform, new Vector2(0, 0.35f), new Vector2(1, 0.65f), new Vector2(0, 0), new Vector2(-20, 0));
            var fill = Stretch(fillArea.transform, "Fill", Vector2.zero, Vector2.one);
            fill.color = ButtonHighlight;
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            SetStretch((RectTransform)handleArea.transform, Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));
            var handle = Stretch(handleArea.transform, "Handle", Vector2.zero, new Vector2(0, 1));
            ((RectTransform)handle.transform).sizeDelta = new Vector2(24, 0);
            handle.color = Color.white;

            var slider = go.GetComponent<Slider>();
            slider.fillRect = (RectTransform)fill.transform;
            slider.handleRect = (RectTransform)handle.transform;
            slider.targetGraphic = handle;
            slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.SetValueWithoutNotify(value);
            ApplyColors(slider);
            slider.onValueChanged.AddListener(v => onChanged(v));
            return slider;
        }

        /// <summary>Casilla de verificación con texto, dentro de un panel vertical.</summary>
        public static Toggle Toggle(Transform panel, string text, bool value, Action<bool> onChanged)
        {
            var go = new GameObject(text, typeof(RectTransform), typeof(Toggle), typeof(LayoutElement));
            go.transform.SetParent(panel, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(520, 60);
            go.GetComponent<LayoutElement>().preferredHeight = 60;

            var box = new GameObject("Box", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(go.transform, false);
            var boxRect = (RectTransform)box.transform;
            boxRect.anchorMin = boxRect.anchorMax = boxRect.pivot = new Vector2(0, 0.5f);
            boxRect.anchoredPosition = new Vector2(10, 0);
            boxRect.sizeDelta = new Vector2(40, 40);
            var boxImage = box.GetComponent<Image>();
            boxImage.color = Color.white;
            var check = Stretch(box.transform, "Checkmark", new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.8f));
            check.color = Cyan;
            var label = Label(go.transform, "Label", text, 28, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(70, 0), new Vector2(440, 60), Color.white);
            label.raycastTarget = false;

            var toggle = go.GetComponent<Toggle>();
            toggle.targetGraphic = boxImage;
            toggle.graphic = check;
            toggle.SetIsOnWithoutNotify(value);
            ApplyColors(toggle);
            toggle.onValueChanged.AddListener(v => onChanged(v));
            return toggle;
        }

        public static void Clear(Transform panel)
        {
            for (int i = panel.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(panel.GetChild(i).gameObject);
            panel.DetachChildren();
        }

        /// <summary>Selecciona el primer control activo del panel, para navegar con teclado.</summary>
        public static void SelectFirst(GameObject panel)
        {
            if (EventSystem.current == null) return;
            foreach (var selectable in panel.GetComponentsInChildren<Selectable>())
            {
                if (!selectable.interactable) continue;
                EventSystem.current.SetSelectedGameObject(selectable.gameObject);
                return;
            }
        }

        static void ApplyColors(Selectable selectable)
        {
            var colors = selectable.colors;
            colors.normalColor = selectable is UnityEngine.UI.Button ? ButtonColor : Color.white;
            colors.highlightedColor = ButtonHighlight;
            colors.selectedColor = ButtonHighlight;
            colors.pressedColor = Cyan;
            colors.disabledColor = DisabledColor;
            selectable.colors = colors;
        }

        static Image Stretch(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            SetStretch((RectTransform)go.transform, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            return go.GetComponent<Image>();
        }

        static void SetStretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
