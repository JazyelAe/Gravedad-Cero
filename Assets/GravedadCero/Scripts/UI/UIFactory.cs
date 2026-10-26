using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GravedadCero.UI
{
    /// <summary>
    /// Piezas de interfaz con el estilo "Helios-7" (ver UITheme): canvas, textos TextMeshPro,
    /// paneles con esquinas recortadas, botones animados, deslizadores, casillas, recuadros
    /// de tecla e iconos. Todo se construye por código, así que todas las pantallas
    /// comparten automáticamente la misma identidad visual.
    /// </summary>
    public static class UIFactory
    {
        // Alias usados por el código anterior.
        public static Color Cyan => UITheme.Cyan;
        public static Color Amber => UITheme.Amber;

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
            // Ajuste por alto: siempre hay 1080 unidades verticales; las pantallas anchas
            // ganan espacio a los lados y las 4:3 lo pierden (la composición se ancla a la izquierda).
            scaler.matchWidthOrHeight = 1f;
            return go.transform;
        }

        // ---------- Texto ----------

        /// <summary>Texto con la fuente de lectura (Exo 2).</summary>
        public static TMP_Text Label(Transform parent, string name, string text, float size, TextAnchor alignment, Vector2 anchor,
            Vector2 position, Vector2 boxSize, Color color, FontStyle style = FontStyle.Normal)
        {
            return Text(parent, name, text, UITheme.BodyFont, size, alignment, anchor, position, boxSize, color, style);
        }

        /// <summary>Texto con la fuente de títulos (Orbitron). No usar · ‹ › « » ^.</summary>
        public static TMP_Text Title(Transform parent, string name, string text, float size, TextAnchor alignment, Vector2 anchor,
            Vector2 position, Vector2 boxSize, Color color)
        {
            return Text(parent, name, text, UITheme.TitleFont, size, alignment, anchor, position, boxSize, color, FontStyle.Bold);
        }

        public static TMP_Text Text(Transform parent, string name, string text, TMP_FontAsset font, float size, TextAnchor alignment,
            Vector2 anchor, Vector2 position, Vector2 boxSize, Color color, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = boxSize;
            var label = go.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = size;
            label.fontStyle = ToTmp(style);
            label.alignment = ToTmp(alignment);
            label.color = color;
            label.richText = true;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            label.text = text;
            return label;
        }

        /// <summary>Etiqueta pequeña en mayúsculas y espaciada (por ejemplo, OBJETIVO).</summary>
        public static TMP_Text Caps(Transform parent, string name, string text, Color color, float size = UITheme.SizeCaption)
        {
            var label = Label(parent, name, text.ToUpperInvariant(), size, TextAnchor.MiddleLeft, new Vector2(0, 1), Vector2.zero, new Vector2(400, size * 1.5f), color, FontStyle.Bold);
            label.characterSpacing = 12f;
            return label;
        }

        /// <summary>Añade sombra suave al texto para leerlo sobre la escena.</summary>
        public static T WithShadow<T>(T label) where T : TMP_Text
        {
            if (label.font == UITheme.BodyFont) label.fontSharedMaterial = UITheme.BodyShadowMaterial;
            return label;
        }

        // ---------- Imágenes ----------

        public static Image Image(Transform parent, string name, Sprite sprite, Color color, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            if (sprite != null && sprite.border != Vector4.zero) image.type = UnityEngine.UI.Image.Type.Sliced;
            return image;
        }

        /// <summary>Imagen que cubre todo su padre (fondos, oscurecidos, marcos).</summary>
        public static Image FullScreenImage(Transform parent, string name, Color color, Sprite sprite = null)
        {
            var image = Image(parent, name, sprite, color, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Stretch((RectTransform)image.transform);
            return image;
        }

        public static void Stretch(RectTransform rect, float inset = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        /// <summary>Relleno + contorno con esquinas recortadas detrás de un elemento (no participa en el layout).</summary>
        public static void Frame(Transform target, Color fill, Color line)
        {
            var bg = target.GetComponent<Image>();
            if (bg == null) bg = target.gameObject.AddComponent<Image>();
            bg.sprite = UITheme.PanelFill;
            bg.type = UnityEngine.UI.Image.Type.Sliced;
            bg.color = fill;
            var frame = FullScreenImage(target, "Frame", line, UITheme.PanelLine);
            frame.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        }

        // ---------- Paneles y filas ----------

        /// <summary>Panel vertical con estilo Helios-7 que ajusta su altura al contenido.</summary>
        public static GameObject Panel(Transform parent, string name, Vector2 anchor, Vector2 position, float width, string header = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(width, 0);
            go.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Frame(go.transform, UITheme.Panel, UITheme.CyanDim);
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(36, 36, 30, 34);
            layout.spacing = 14;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            if (!string.IsNullOrEmpty(header)) Header(go.transform, header);
            return go;
        }

        /// <summary>Cabecera de panel: etiqueta ámbar en mayúsculas y línea de acento.</summary>
        public static void Header(Transform panel, string text, Color? color = null)
        {
            var row = new GameObject("Header", typeof(RectTransform), typeof(LayoutElement));
            row.transform.SetParent(panel, false);
            ((RectTransform)row.transform).sizeDelta = new Vector2(400, 40);
            row.GetComponent<LayoutElement>().preferredHeight = 40;
            var label = Caps(row.transform, "Label", text, color ?? UITheme.Amber);
            var lr = (RectTransform)label.transform;
            lr.anchorMin = new Vector2(0, 0.5f);
            lr.anchorMax = new Vector2(1, 0.5f);
            lr.pivot = new Vector2(0, 0.5f);
            lr.offsetMin = new Vector2(0, -14);
            lr.offsetMax = new Vector2(0, 14);
            var line = Image(row.transform, "Line", null, UITheme.CyanDim, new Vector2(0, 0), Vector2.zero, new Vector2(0, 2));
            var rr = (RectTransform)line.transform;
            rr.anchorMin = new Vector2(0, 0);
            rr.anchorMax = new Vector2(1, 0);
            rr.pivot = new Vector2(0, 0);
            rr.sizeDelta = new Vector2(0, 2);
            var accent = Image(row.transform, "Accent", null, UITheme.Cyan, new Vector2(0, 0), Vector2.zero, new Vector2(64, 3));
            ((RectTransform)accent.transform).pivot = new Vector2(0, 0);
        }

        /// <summary>Fila horizontal con altura fija dentro de un panel vertical.</summary>
        public static RectTransform Row(Transform parent, string name, float height, float spacing = 12f, TextAnchor align = TextAnchor.MiddleLeft)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(400, height);
            go.GetComponent<LayoutElement>().preferredHeight = height;
            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = align;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return rect;
        }

        /// <summary>Texto que participa en un panel vertical con una altura fija.</summary>
        public static TMP_Text PanelText(Transform panel, string name, string text, float size, float height, Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var label = Label(panel, name, text, size, alignment, new Vector2(0.5f, 1), Vector2.zero, new Vector2(520, height), color, style);
            label.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            return label;
        }

        /// <summary>Espacio vertical vacío en un panel.</summary>
        public static void Spacer(Transform panel, float height)
        {
            var go = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(panel, false);
            // Los paneles no controlan la altura de sus hijos: cuenta la del RectTransform.
            ((RectTransform)go.transform).sizeDelta = new Vector2(0, height);
            go.GetComponent<LayoutElement>().preferredHeight = height;
        }

        // ---------- Controles ----------

        public static Button Button(Transform parent, string text, Action onClick, float height = 68f, string subtitle = null)
        {
            var go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(520, height);
            go.GetComponent<LayoutElement>().preferredHeight = height;
            var fill = go.GetComponent<Image>();
            fill.sprite = UITheme.PanelFill;
            fill.type = UnityEngine.UI.Image.Type.Sliced;
            var button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = fill;
            button.onClick.AddListener(() => onClick());

            var accent = Image(go.transform, "Accent", null, UITheme.CyanDim, new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(4, height - 24));
            ((RectTransform)accent.transform).pivot = new Vector2(0, 0.5f);

            bool hasSubtitle = !string.IsNullOrEmpty(subtitle);
            var label = Label(go.transform, "Label", text, UITheme.SizeBody, TextAnchor.MiddleLeft, new Vector2(0, 0.5f),
                new Vector2(34, hasSubtitle ? 11 : 0), new Vector2(420, 40), UITheme.Text, FontStyle.Bold);
            ((RectTransform)label.transform).pivot = new Vector2(0, 0.5f);
            TMP_Text sub = null;
            if (hasSubtitle)
            {
                sub = Label(go.transform, "Subtitle", subtitle, UITheme.SizeCaption, TextAnchor.MiddleLeft, new Vector2(0, 0.5f),
                    new Vector2(34, -17), new Vector2(440, 28), UITheme.TextSecondary);
                ((RectTransform)sub.transform).pivot = new Vector2(0, 0.5f);
            }

            var chevron = Image(go.transform, "Chevron", UITheme.Triangle, UITheme.Cyan, new Vector2(1, 0.5f), new Vector2(-26, 0), new Vector2(18, 18));
            chevron.rectTransform.localEulerAngles = new Vector3(0, 0, -90);

            var fx = go.AddComponent<UIButtonFX>();
            fx.Bind(button, fill, accent, label, sub, chevron);
            return button;
        }

        /// <summary>Deslizador horizontal con estilo Helios-7, dentro de un panel vertical.</summary>
        public static Slider Slider(Transform panel, string name, float min, float max, float value, Action<float> onChanged)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider), typeof(LayoutElement));
            go.transform.SetParent(panel, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(520, 48);
            go.GetComponent<LayoutElement>().preferredHeight = 48;

            var track = Image(go.transform, "Track", UITheme.PanelFill, UITheme.PanelRaised, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetAnchors(track.rectTransform, new Vector2(0, 0.32f), new Vector2(1, 0.68f), Vector2.zero, Vector2.zero);
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            SetAnchors((RectTransform)fillArea.transform, new Vector2(0, 0.32f), new Vector2(1, 0.68f), new Vector2(4, 4), new Vector2(-14, -4));
            var fill = Image(fillArea.transform, "Fill", null, UITheme.Cyan, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Stretch(fill.rectTransform);
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            SetAnchors((RectTransform)handleArea.transform, Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));
            var handle = Image(handleArea.transform, "Handle", UITheme.PanelFill, UITheme.Text, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetAnchors(handle.rectTransform, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, Vector2.zero);
            handle.rectTransform.sizeDelta = new Vector2(22, 0);
            handle.raycastTarget = true;

            var slider = go.GetComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.SetValueWithoutNotify(value);
            var colors = slider.colors;
            colors.normalColor = UITheme.Text;
            colors.highlightedColor = UITheme.Cyan;
            colors.selectedColor = UITheme.Cyan;
            colors.pressedColor = UITheme.Amber;
            slider.colors = colors;
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

            var box = Image(go.transform, "Box", UITheme.PanelFill, UITheme.PanelRaised, new Vector2(0, 0.5f), new Vector2(4, 0), new Vector2(42, 42));
            box.rectTransform.pivot = new Vector2(0, 0.5f);
            box.raycastTarget = true;
            var boxLine = FullScreenImage(box.transform, "Frame", UITheme.Cyan, UITheme.PanelLine);
            boxLine.raycastTarget = false;
            var check = Image(box.transform, "Check", UITheme.Check, UITheme.Cyan, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30, 30));
            var label = Label(go.transform, "Label", text, UITheme.SizeBody, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(64, 0), new Vector2(440, 50), UITheme.Text);
            label.rectTransform.pivot = new Vector2(0, 0.5f);

            var toggle = go.GetComponent<Toggle>();
            toggle.targetGraphic = box;
            toggle.graphic = check;
            toggle.SetIsOnWithoutNotify(value);
            var colors = toggle.colors;
            colors.normalColor = UITheme.Text;
            colors.highlightedColor = UITheme.Cyan;
            colors.selectedColor = UITheme.Cyan;
            colors.pressedColor = UITheme.Amber;
            toggle.colors = colors;
            toggle.onValueChanged.AddListener(v => onChanged(v));
            return toggle;
        }

        // ---------- Teclas ----------

        /// <summary>Recuadro con el nombre de una tecla (por ejemplo, "E", "Esc", "Clic").</summary>
        public static RectTransform KeyChip(Transform parent, string key, float size = UITheme.SizeCaption)
        {
            var go = new GameObject("Key " + key, typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            Frame(go.transform, new Color(UITheme.Deep.r, UITheme.Deep.g, UITheme.Deep.b, 0.92f), UITheme.Cyan);
            var label = Label(go.transform, "Label", key, size, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, size * 1.6f), UITheme.Text, FontStyle.Bold);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            float width = Mathf.Max(size * 2.1f, label.GetPreferredValues(key).x + size * 1.1f);
            float height = size * 1.75f;
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(width, height);
            label.rectTransform.sizeDelta = new Vector2(width, height);
            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = height;
            return rect;
        }

        /// <summary>Fila "tecla(s) + acción" para controles e indicaciones.</summary>
        public static RectTransform KeyHint(Transform parent, string[] keys, string action, float size = UITheme.SizeSmall, float actionWidth = 360f)
        {
            var row = Row(parent, "Hint " + action, size * 1.9f, 8f);
            float width = 0f;
            foreach (string key in keys) width += KeyChip(row, key, size * 0.85f).sizeDelta.x + 8f;
            var label = Label(row, "Action", action, size, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), Vector2.zero, new Vector2(actionWidth, size * 1.6f), UITheme.Text);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            float actionPreferred = Mathf.Min(actionWidth, label.GetPreferredValues(action).x + 4f);
            label.rectTransform.sizeDelta = new Vector2(actionPreferred, size * 1.6f);
            label.gameObject.AddComponent<LayoutElement>().preferredWidth = actionPreferred;
            width += actionPreferred;
            row.sizeDelta = new Vector2(width, row.sizeDelta.y);
            row.GetComponent<LayoutElement>().preferredWidth = width;
            return row;
        }

        // ---------- Fondos ----------

        /// <summary>
        /// Fondo común de las pantallas de menú: degradado oscuro a la izquierda, tramado de
        /// líneas y riel vertical con marcas (detalle de interfaz de nave).
        /// </summary>
        public static void Backdrop(Transform root, float shadeWidth = 1250f, float shadeAlpha = 0.94f)
        {
            var shade = Image(root, "LeftShade", UITheme.GradientLeft, new Color(UITheme.Deep.r, UITheme.Deep.g, UITheme.Deep.b, shadeAlpha),
                new Vector2(0, 0.5f), Vector2.zero, new Vector2(shadeWidth, 0));
            shade.rectTransform.anchorMin = new Vector2(0, 0);
            shade.rectTransform.anchorMax = new Vector2(0, 1);
            shade.rectTransform.pivot = new Vector2(0, 0.5f);
            var scan = FullScreenImage(root, "Scanlines", new Color(1, 1, 1, 0.025f), UITheme.Scanlines);
            scan.type = UnityEngine.UI.Image.Type.Tiled;
            var rail = Image(root, "Rail", null, UITheme.CyanDim, new Vector2(0, 0.5f), new Vector2(76, 30), new Vector2(2, 820));
            for (int i = 0; i < 5; i++)
                Image(rail.transform, "Tick", null, i == 0 ? UITheme.Amber : UITheme.CyanDim, new Vector2(0.5f, 1f), new Vector2(0, -i * 205f), new Vector2(i == 0 ? 14 : 8, 2));
        }

        // ---------- Utilidades ----------

        /// <summary>Coloca un RectTransform con un mismo ancla (y pivote, si no se indica otro).</summary>
        public static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size, Vector2? pivot = null)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot ?? anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        /// <summary>"Sector 1-1 · Muelle de acoplamiento" → "1-1" (vacío si no tiene código).</summary>
        public static string SectionCode(string title)
        {
            int dot = title.IndexOf('·');
            if (dot <= 0) return "";
            string code = title.Substring(0, dot).Trim();
            int space = code.LastIndexOf(' ');
            return space >= 0 ? code.Substring(space + 1) : code;
        }

        /// <summary>"Sector 1-1 · Muelle de acoplamiento" → "Muelle de acoplamiento".</summary>
        public static string SectionName(string title)
        {
            int dot = title.IndexOf('·');
            return dot > 0 ? title.Substring(dot + 1).Trim() : title;
        }

        /// <summary>Ajusta la altura de un texto de panel a lo que ocupa con el ancho dado.</summary>
        public static TMP_Text FitHeight(TMP_Text label, float width, float extra = 6f)
        {
            float height = Mathf.Ceil(label.GetPreferredValues(label.text, width, 0f).y + extra);
            label.rectTransform.sizeDelta = new Vector2(width, height);
            var le = label.GetComponent<LayoutElement>();
            if (le != null) le.preferredHeight = height;
            return label;
        }

        public static void Clear(Transform panel)
        {
            for (int i = panel.childCount - 1; i >= 0; i--)
            {
                var child = panel.GetChild(i);
                if (child.name == "Frame" || child.name == "Header") continue; // conserva marco y cabecera
                UnityEngine.Object.Destroy(child.gameObject);
                child.SetParent(null);
            }
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

        static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        static FontStyles ToTmp(FontStyle style)
        {
            switch (style)
            {
                case FontStyle.Bold: return FontStyles.Bold;
                case FontStyle.Italic: return FontStyles.Italic;
                case FontStyle.BoldAndItalic: return FontStyles.Bold | FontStyles.Italic;
                default: return FontStyles.Normal;
            }
        }

        static TextAlignmentOptions ToTmp(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.MidlineLeft;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Midline;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.MidlineRight;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                default: return TextAlignmentOptions.BottomRight;
            }
        }
    }
}
