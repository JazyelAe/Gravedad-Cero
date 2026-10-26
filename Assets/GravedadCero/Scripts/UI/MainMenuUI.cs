using System;
using System.Collections.Generic;
using GravedadCero.Player;
using GravedadCero.Save;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GravedadCero.UI
{
    /// <summary>
    /// Menú principal construido por código: Continuar, Nueva partida, Secciones (solo las
    /// desbloqueadas), Personaje (elegir skin con vista previa) y Salir. Lee y escribe el
    /// progreso a través de GameProgress.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Serializable]
        public struct Section
        {
            public string title;
            public string sceneName;
        }

        [SerializeField] Section[] sections = new Section[0];
        [Tooltip("Modelo de Kai de la vista previa.")]
        [SerializeField] KaiSkinApplier preview;

        static readonly Color Cyan = new Color(0.45f, 0.9f, 1f);
        static readonly Color PanelColor = new Color(0.04f, 0.07f, 0.11f, 0.82f);
        static readonly Color ButtonColor = new Color(0.12f, 0.17f, 0.24f, 0.95f);
        static readonly Color ButtonHighlight = new Color(0.10f, 0.55f, 0.70f, 1f);

        Font font;
        Transform root;
        readonly Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
        Text skinLabel;

        public IReadOnlyList<Section> Sections => sections;

        void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildCanvas();
            BuildMainPanel();
            BuildSectionsPanel();
            BuildCharacterPanel();
            ShowPanel("Main");
        }

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // ---------- Acciones (públicas para poder probarlas) ----------

        public void Continue() => LoadSection(Mathf.Min(GameProgress.UnlockedSection, sections.Length - 1));

        public void NewGame()
        {
            GameProgress.ResetProgress();
            LoadSection(0);
        }

        public void LoadSection(int index)
        {
            if (index < 0 || index >= sections.Length || index > GameProgress.UnlockedSection) return;
            SceneManager.LoadScene(sections[index].sceneName);
        }

        public void ChangeSkin(int delta)
        {
            if (preview == null || preview.Catalog == null) return;
            int index = preview.Catalog.Wrap(preview.CurrentIndex + delta);
            preview.Apply(index);
            GameProgress.SelectedSkin = index;
            RefreshSkinLabel();
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void ShowPanel(string name)
        {
            foreach (var pair in panels) pair.Value.SetActive(pair.Key == name);
            if (name == "Main") RebuildMainButtons();
            if (name == "Sections") RebuildSectionButtons();
            if (name == "Character") RefreshSkinLabel();
            SelectFirstButton(panels[name]);
        }

        // ---------- Construcción de la interfaz ----------

        void BuildCanvas()
        {
            var canvasGo = new GameObject("Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            root = canvasGo.transform;

            Label(root, "Title", "GRAVEDAD CERO", 96, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(120, -110), new Vector2(1200, 120), Color.white, FontStyle.Bold);
            Label(root, "Subtitle", "Estación orbital Helios-7", 30, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(126, -225), new Vector2(1200, 50), Cyan);
            Label(root, "Footer", "Ratón o flechas para navegar · Enter para elegir", 20, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(120, 40), new Vector2(1200, 40), new Color(1, 1, 1, 0.8f));
        }

        void BuildMainPanel() => panels["Main"] = Panel("Main");

        void RebuildMainButtons()
        {
            var panel = panels["Main"].transform;
            Clear(panel);
            if (GameProgress.HasProgress)
                AddButton(panel, $"Continuar · {sections[Mathf.Min(GameProgress.UnlockedSection, sections.Length - 1)].title}", Continue);
            AddButton(panel, "Nueva partida", NewGame);
            AddButton(panel, "Secciones", () => ShowPanel("Sections"));
            AddButton(panel, "Personaje", () => ShowPanel("Character"));
            AddButton(panel, "Salir", Quit);
        }

        void BuildSectionsPanel() => panels["Sections"] = Panel("Sections");

        void RebuildSectionButtons()
        {
            var panel = panels["Sections"].transform;
            Clear(panel);
            for (int i = 0; i < sections.Length; i++)
            {
                int index = i;
                bool unlocked = i <= GameProgress.UnlockedSection;
                var button = AddButton(panel, unlocked ? sections[i].title : $"{sections[i].title} · bloqueada", () => LoadSection(index));
                button.interactable = unlocked;
            }
            AddButton(panel, "Volver", () => ShowPanel("Main"));
        }

        void BuildCharacterPanel()
        {
            var panelGo = Panel("Character");
            panels["Character"] = panelGo;
            var panel = panelGo.transform;
            skinLabel = Label(panel, "SkinName", "", 34, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), Vector2.zero, new Vector2(520, 70), Cyan, FontStyle.Bold);
            skinLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 70;
            AddButton(panel, "<  Anterior", () => ChangeSkin(-1));
            AddButton(panel, "Siguiente  >", () => ChangeSkin(1));
            AddButton(panel, "Volver", () => ShowPanel("Main"));
        }

        void RefreshSkinLabel()
        {
            if (skinLabel == null || preview == null || preview.Catalog == null || preview.Catalog.Count == 0) return;
            skinLabel.text = $"{preview.Catalog.Get(preview.CurrentIndex).displayName}  ({preview.CurrentIndex + 1}/{preview.Catalog.Count})";
        }

        GameObject Panel(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(root, false);
            var rect = (RectTransform)go.transform;
            // Anclado arriba a la izquierda, bajo el título; la altura se ajusta a los botones.
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(120, -300);
            rect.sizeDelta = new Vector2(600, 0);
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

        Button AddButton(Transform parent, string text, Action onClick)
        {
            var go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(520, 74);
            go.GetComponent<Image>().color = Color.white;
            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = ButtonColor;
            colors.highlightedColor = ButtonHighlight;
            colors.selectedColor = ButtonHighlight;
            colors.pressedColor = Cyan;
            colors.disabledColor = new Color(0.1f, 0.12f, 0.15f, 0.6f);
            button.colors = colors;
            button.onClick.AddListener(() => onClick());
            var label = Label(go.transform, "Label", text, 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 74), Color.white, FontStyle.Normal);
            label.raycastTarget = false;
            return button;
        }

        Text Label(Transform parent, string name, string text, int size, TextAnchor anchor, Vector2 pivotAnchor,
            Vector2 position, Vector2 boxSize, Color color, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = pivotAnchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = boxSize;
            var label = go.GetComponent<Text>();
            label.font = font;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = anchor;
            label.color = color;
            label.text = text;
            return label;
        }

        static void Clear(Transform panel)
        {
            for (int i = panel.childCount - 1; i >= 0; i--) Destroy(panel.GetChild(i).gameObject);
            panel.DetachChildren();
        }

        static void SelectFirstButton(GameObject panel)
        {
            if (EventSystem.current == null) return;
            foreach (var button in panel.GetComponentsInChildren<Button>())
            {
                if (!button.interactable) continue;
                EventSystem.current.SetSelectedGameObject(button.gameObject);
                return;
            }
        }
    }
}
