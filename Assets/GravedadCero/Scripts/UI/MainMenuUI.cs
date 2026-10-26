using System;
using System.Collections.Generic;
using GravedadCero.Level;
using GravedadCero.Player;
using GravedadCero.Save;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GravedadCero.UI
{
    /// <summary>
    /// Menú principal con la identidad Helios-7: logotipo, columna de opciones sobre un
    /// degradado, vista previa de Kai, placa con el personaje y barra de estado inferior.
    /// Pantallas: Principal (Continuar, Nueva partida, Secciones, Personaje, Salir),
    /// Secciones (tarjetas con estado), Personaje (skins con muestras de color) y una
    /// confirmación para Nueva partida cuando ya hay progreso. Lee y escribe el progreso
    /// a través de GameProgress y carga escenas con SceneTransition.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Serializable]
        public struct Section
        {
            public string title;
            public string sceneName;
            [TextArea(1, 2)] public string description;
        }

        [SerializeField] Section[] sections = new Section[0];
        [Tooltip("Modelo de Kai de la vista previa.")]
        [SerializeField] KaiSkinApplier preview;
        [SerializeField] MenuCameraDrift cameraDrift;
        [SerializeField] string version = "BETA 0.2";

        Transform root;
        readonly Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
        GameObject nameplate;
        TMP_Text nameplateSkin;
        TMP_Text skinName;
        TMP_Text skinCount;
        Image statusDot;
        readonly List<Image> swatchRings = new List<Image>();

        public IReadOnlyList<Section> Sections => sections;

        /// <summary>Pantalla visible: "Main", "Sections", "Character" o "ConfirmNewGame".</summary>
        public string CurrentPanel { get; private set; }

        void Awake()
        {
            root = UIFactory.Canvas(transform, "Menu Canvas");
            UIFactory.Backdrop(root);
            BuildBrand();
            BuildNameplate();
            BuildStatusBar();
            panels["Main"] = Panel("Main", "Menú principal", 600);
            panels["Sections"] = Panel("Sections", "Secciones del sector 1", 820);
            panels["Character"] = Panel("Character", "Personaje", 640);
            panels["ConfirmNewGame"] = Panel("ConfirmNewGame", "Confirmar", 680);
            BuildCharacterPanel();
            BuildConfirmPanel();
            ShowPanel("Main");
        }

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update()
        {
            if (statusDot != null)
            {
                var c = UITheme.Amber;
                statusDot.color = new Color(c.r, c.g, c.b, 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 2.2f)));
            }
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && CurrentPanel != "Main") ShowPanel("Main");
        }

        // ---------- Acciones (públicas para poder probarlas) ----------

        public void Continue() => LoadSection(Mathf.Min(GameProgress.UnlockedSection, sections.Length - 1));

        /// <summary>Lo que hace el botón: si hay progreso, pide confirmación antes de borrarlo.</summary>
        public void RequestNewGame()
        {
            if (GameProgress.HasProgress) ShowPanel("ConfirmNewGame");
            else NewGame();
        }

        public void NewGame()
        {
            GameProgress.ResetProgress();
            LoadSection(0);
        }

        public void LoadSection(int index)
        {
            if (index < 0 || index >= sections.Length || index > GameProgress.UnlockedSection) return;
            SceneTransition.Load(sections[index].sceneName);
        }

        public void ChangeSkin(int delta)
        {
            if (preview == null || preview.Catalog == null) return;
            SelectSkin(preview.CurrentIndex + delta);
        }

        public void SelectSkin(int index)
        {
            if (preview == null || preview.Catalog == null) return;
            index = preview.Catalog.Wrap(index);
            preview.Apply(index);
            GameProgress.SelectedSkin = index;
            RefreshSkin();
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void ShowPanel(string name)
        {
            CurrentPanel = name;
            foreach (var pair in panels) pair.Value.SetActive(pair.Key == name);
            if (name == "Main") RebuildMainButtons();
            if (name == "Sections") RebuildSectionCards();
            if (name == "Character") RefreshSkin();
            if (nameplate != null) nameplate.SetActive(name != "Character");
            if (cameraDrift != null) cameraDrift.SetFocus(name == "Character");
            UIReveal.Play(panels[name], 0f, new Vector2(-24f, 0f), 0.28f);
            UIFactory.SelectFirst(panels[name]);
        }

        // ---------- Fondo, logotipo y barras ----------

        void BuildBrand()
        {
            var brand = new GameObject("Brand", typeof(RectTransform), typeof(CanvasGroup));
            brand.transform.SetParent(root, false);
            var rect = (RectTransform)brand.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(120, -64);
            rect.sizeDelta = new Vector2(1100, 220);
            var kicker = UIFactory.Caps(brand.transform, "Kicker", "Estación orbital Helios-7  /  Sector 1", UITheme.Amber, UITheme.SizeCaption);
            UIFactory.Place(kicker.rectTransform, new Vector2(0, 1), new Vector2(0, -16), new Vector2(900, 32), new Vector2(0, 1));
            var title = UIFactory.Title(brand.transform, "Title", "GRAVEDAD CERO", UITheme.SizeDisplay, TextAnchor.UpperLeft, new Vector2(0, 1),
                new Vector2(-4, -46), new Vector2(1100, 106), UITheme.Text);
            title.characterSpacing = 2f;
            title.textWrappingMode = TextWrappingModes.NoWrap;
            UIFactory.Image(brand.transform, "Accent", null, UITheme.Cyan, new Vector2(0, 1), new Vector2(0, -156), new Vector2(240, 5)).rectTransform.pivot = new Vector2(0, 1);
            UIFactory.Image(brand.transform, "Line", null, UITheme.CyanDim, new Vector2(0, 1), new Vector2(0, -160), new Vector2(720, 1)).rectTransform.pivot = new Vector2(0, 1);
            var tagline = UIFactory.Label(brand.transform, "Tagline", "Puzles y plataformas en gravedad variable", UITheme.SizeBody, TextAnchor.UpperLeft,
                new Vector2(0, 1), new Vector2(0, -176), new Vector2(900, 40), UITheme.TextSecondary);
            UIReveal.Play(brand, 0.05f, new Vector2(-40f, 0f), 0.6f);
        }

        void BuildNameplate()
        {
            nameplate = new GameObject("Nameplate", typeof(RectTransform), typeof(CanvasGroup));
            nameplate.transform.SetParent(root, false);
            var rect = (RectTransform)nameplate.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1, 0);
            rect.anchoredPosition = new Vector2(-120, 110);
            rect.sizeDelta = new Vector2(460, 140);
            UIFactory.Frame(nameplate.transform, new Color(UITheme.Deep.r, UITheme.Deep.g, UITheme.Deep.b, 0.78f), UITheme.CyanDim);
            var catalog = preview != null ? preview.Catalog : null;
            var role = UIFactory.Caps(nameplate.transform, "Role", catalog != null ? catalog.Role : "", UITheme.Amber);
            role.alignment = TextAlignmentOptions.MidlineRight;
            UIFactory.Place(role.rectTransform, new Vector2(1, 1), new Vector2(-28, -18), new Vector2(420, 30));
            UIFactory.Title(nameplate.transform, "Name", (catalog != null ? catalog.CharacterName : "Kai").ToUpperInvariant(),
                44, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-28, -48), new Vector2(420, 54), UITheme.Text);
            nameplateSkin = UIFactory.Label(nameplate.transform, "Skin", "", UITheme.SizeSmall, TextAnchor.UpperRight,
                new Vector2(1, 1), new Vector2(-28, -100), new Vector2(420, 30), UITheme.TextSecondary);
            UIReveal.Play(nameplate, 0.45f, new Vector2(30f, 0f), 0.5f);
        }

        void BuildStatusBar()
        {
            var bar = UIFactory.Image(root, "StatusBar", null, new Color(UITheme.Deep.r, UITheme.Deep.g, UITheme.Deep.b, 0.82f), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 64));
            bar.rectTransform.anchorMin = new Vector2(0, 0);
            bar.rectTransform.anchorMax = new Vector2(1, 0);
            bar.rectTransform.pivot = new Vector2(0.5f, 0);
            var topLine = UIFactory.Image(bar.transform, "TopLine", null, UITheme.CyanDim, new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 1));
            topLine.rectTransform.anchorMin = new Vector2(0, 1);
            topLine.rectTransform.anchorMax = new Vector2(1, 1);

            statusDot = UIFactory.Image(bar.transform, "Dot", UITheme.Dot, UITheme.Amber, new Vector2(0, 0.5f), new Vector2(120, 0), new Vector2(16, 16));
            var status = UIFactory.Caps(bar.transform, "Status", "Sistema gravitacional: inestable", UITheme.TextSecondary);
            UIFactory.Place(status.rectTransform, new Vector2(0, 0.5f), new Vector2(146, 0), new Vector2(620, 30), new Vector2(0, 0.5f));

            var hints = UIFactory.Row(bar.transform, "Hints", 40f, 22f, TextAnchor.MiddleRight);
            UIFactory.Place(hints, new Vector2(1, 0.5f), new Vector2(-330, 0), new Vector2(620, 40), new Vector2(1, 0.5f));
            UIFactory.KeyHint(hints, new[] { "Flechas" }, "Navegar", UITheme.SizeCaption, 110);
            UIFactory.KeyHint(hints, new[] { "Enter" }, "Elegir", UITheme.SizeCaption, 80);
            UIFactory.KeyHint(hints, new[] { "Esc" }, "Volver", UITheme.SizeCaption, 80);
            var ver = UIFactory.Caps(bar.transform, "Version", version, UITheme.CyanDim);
            ver.alignment = TextAlignmentOptions.MidlineRight;
            UIFactory.Place(ver.rectTransform, new Vector2(1, 0.5f), new Vector2(-120, 0), new Vector2(200, 30), new Vector2(1, 0.5f));
            UIReveal.Play(bar.gameObject.AddComponent<CanvasGroup>().gameObject, 0.6f, new Vector2(0f, -20f), 0.5f);
        }

        // ---------- Pantallas ----------

        GameObject Panel(string name, string header, float width)
        {
            var panel = UIFactory.Panel(root, name, new Vector2(0, 1), new Vector2(120, -290), width, header);
            panel.AddComponent<CanvasGroup>();
            return panel;
        }

        void RebuildMainButtons()
        {
            var panel = panels["Main"].transform;
            UIFactory.Clear(panel);
            int unlocked = Mathf.Min(GameProgress.UnlockedSection, sections.Length - 1);
            if (GameProgress.HasProgress)
                UIFactory.Button(panel, "Continuar", Continue, 84, sections.Length > 0 ? sections[unlocked].title : null);
            UIFactory.Button(panel, "Nueva partida", RequestNewGame, 84, GameProgress.HasProgress ? "Borra el progreso guardado" : "Empieza en la sección 1-1");
            int available = Mathf.Min(GameProgress.UnlockedSection + 1, sections.Length);
            UIFactory.Button(panel, "Secciones", () => ShowPanel("Sections"), 84, $"{available} de {sections.Length} disponibles");
            string skin = preview != null && preview.Catalog != null && preview.Catalog.Count > 0 ? "Traje: " + preview.Catalog.Get(preview.CurrentIndex).displayName : "";
            UIFactory.Button(panel, "Personaje", () => ShowPanel("Character"), 84, skin);
            UIFactory.Button(panel, "Salir", Quit);
            RefreshNameplate();
        }

        void RebuildSectionCards()
        {
            var panel = panels["Sections"].transform;
            UIFactory.Clear(panel);
            for (int i = 0; i < sections.Length; i++)
            {
                int index = i;
                bool completed = i < GameProgress.UnlockedSection;
                bool unlocked = i <= GameProgress.UnlockedSection;
                var card = UIFactory.Button(panel, UIFactory.SectionName(sections[i].title), () => LoadSection(index), 112,
                    unlocked ? sections[i].description : "Completa la sección anterior para desbloquearla.");
                card.interactable = unlocked;
                var t = card.transform;
                ShiftLabels(t, 150f);
                var chevron = t.Find("Chevron") as RectTransform;
                if (chevron != null) chevron.anchorMin = chevron.anchorMax = new Vector2(1, 0.32f);
                var code = UIFactory.Title(t, "Code", UIFactory.SectionCode(sections[i].title), 38, TextAnchor.MiddleLeft, new Vector2(0, 0.5f),
                    new Vector2(34, 0), new Vector2(110, 60), unlocked ? UITheme.Cyan : UITheme.Locked);
                code.rectTransform.pivot = new Vector2(0, 0.5f);
                string state = completed ? "Completada" : unlocked ? "Disponible" : "Bloqueada";
                Color stateColor = completed ? UITheme.Success : unlocked ? UITheme.Amber : UITheme.Locked;
                var stateLabel = UIFactory.Caps(t, "State", state, stateColor, 18);
                stateLabel.alignment = TextAlignmentOptions.MidlineRight;
                UIFactory.Place(stateLabel.rectTransform, new Vector2(1, 1), new Vector2(-64, -18), new Vector2(220, 26), new Vector2(1, 1));
                if (completed || !unlocked)
                {
                    var icon = UIFactory.Image(t, "StateIcon", completed ? UITheme.Check : UITheme.Lock, stateColor, new Vector2(1, 1), new Vector2(-30, -16), new Vector2(24, 24));
                    icon.rectTransform.pivot = new Vector2(1, 1);
                }
            }
            UIFactory.Button(panel, "Volver", () => ShowPanel("Main"));
        }

        void BuildCharacterPanel()
        {
            var panel = panels["Character"].transform;
            var catalog = preview != null ? preview.Catalog : null;
            var name = UIFactory.Title(panel, "Name", (catalog != null ? catalog.CharacterName : "Kai").ToUpperInvariant(), 48, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 1), Vector2.zero, new Vector2(560, 58), UITheme.Text);
            name.gameObject.AddComponent<LayoutElement>().preferredHeight = 58;
            var role = UIFactory.Caps(panel, "Role", catalog != null ? catalog.Role : "", UITheme.Amber);
            role.gameObject.AddComponent<LayoutElement>().preferredHeight = 30;
            UIFactory.PanelText(panel, "Description", catalog != null ? catalog.Description : "", UITheme.SizeSmall, 92, UITheme.TextSecondary, TextAnchor.UpperLeft);

            UIFactory.Header(panel, "Traje", UITheme.CyanDim);
            var swatches = UIFactory.Row(panel, "Swatches", 66f, 14f);
            swatchRings.Clear();
            int count = catalog != null ? catalog.Count : 0;
            for (int i = 0; i < count; i++)
            {
                int index = i;
                var go = new GameObject("Swatch " + catalog.Get(i).displayName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
                go.transform.SetParent(swatches, false);
                ((RectTransform)go.transform).sizeDelta = new Vector2(60, 60);
                var le = go.GetComponent<LayoutElement>();
                le.preferredWidth = 60;
                le.preferredHeight = 60;
                var img = go.GetComponent<Image>();
                img.sprite = UITheme.PanelFill;
                img.type = Image.Type.Sliced;
                img.color = catalog.Get(i).swatch.a > 0f ? catalog.Get(i).swatch : UITheme.Text;
                var button = go.GetComponent<Button>();
                button.targetGraphic = img;
                var colors = button.colors;
                colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
                colors.selectedColor = new Color(1.15f, 1.15f, 1.15f);
                button.colors = colors;
                button.onClick.AddListener(() => SelectSkin(index));
                var ring = UIFactory.FullScreenImage(go.transform, "Ring", UITheme.Cyan, UITheme.PanelLine);
                ring.rectTransform.offsetMin = new Vector2(-6, -6);
                ring.rectTransform.offsetMax = new Vector2(6, 6);
                swatchRings.Add(ring);
            }

            var skinRow = UIFactory.Row(panel, "SkinRow", 46f, 16f);
            skinName = UIFactory.Label(skinRow, "SkinName", "", UITheme.SizeHeading, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), Vector2.zero, new Vector2(300, 46), UITheme.Text, FontStyle.Bold);
            skinName.textWrappingMode = TextWrappingModes.NoWrap;
            skinName.gameObject.AddComponent<LayoutElement>().preferredWidth = 300;
            skinCount = UIFactory.Label(skinRow, "SkinCount", "", UITheme.SizeSmall, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), Vector2.zero, new Vector2(70, 46), UITheme.TextSecondary);
            skinCount.gameObject.AddComponent<LayoutElement>().preferredWidth = 70;
            var equipped = UIFactory.Row(skinRow, "Equipped", 32f, 6f);
            equipped.GetComponent<LayoutElement>().preferredWidth = 140;
            equipped.sizeDelta = new Vector2(140, 32);
            var check = UIFactory.Image(equipped, "Check", UITheme.Check, UITheme.Success, new Vector2(0, 0.5f), Vector2.zero, new Vector2(24, 24));
            check.gameObject.AddComponent<LayoutElement>().preferredWidth = 24;
            var eqLabel = UIFactory.Caps(equipped, "Label", "Equipado", UITheme.Success, 18);
            eqLabel.rectTransform.sizeDelta = new Vector2(110, 28);
            eqLabel.gameObject.AddComponent<LayoutElement>().preferredWidth = 110;

            var arrows = UIFactory.Row(panel, "Arrows", 60f, 14f);
            var prev = UIFactory.Button(arrows, "Anterior", () => ChangeSkin(-1), 60f);
            var next = UIFactory.Button(arrows, "Siguiente", () => ChangeSkin(1), 60f);
            foreach (var b in new[] { prev, next })
            {
                ((RectTransform)b.transform).sizeDelta = new Vector2(277, 60);
                b.GetComponent<LayoutElement>().preferredWidth = 277;
            }
            UIFactory.Button(panel, "Volver", () => ShowPanel("Main"), 60f);
        }

        void BuildConfirmPanel()
        {
            var panel = panels["ConfirmNewGame"].transform;
            UIFactory.PanelText(panel, "Message", "¿Empezar una nueva partida?\n<size=80%><color=#A8BDD1>Se borrará el progreso de las secciones desbloqueadas. Tu traje elegido se conserva.</color></size>",
                UITheme.SizeBody, 120, UITheme.Text, TextAnchor.MiddleLeft);
            UIFactory.Button(panel, "Cancelar", () => ShowPanel("Main"));
            UIFactory.Button(panel, "Empezar de nuevo", NewGame);
        }

        void RefreshSkin()
        {
            if (preview == null || preview.Catalog == null || preview.Catalog.Count == 0) return;
            var catalog = preview.Catalog;
            int current = preview.CurrentIndex;
            if (skinName != null) skinName.text = catalog.Get(current).displayName;
            if (skinCount != null) skinCount.text = $"{current + 1} / {catalog.Count}";
            for (int i = 0; i < swatchRings.Count; i++)
                swatchRings[i].color = i == current ? UITheme.Cyan : new Color(1, 1, 1, 0.12f);
            RefreshNameplate();
        }

        void RefreshNameplate()
        {
            if (nameplateSkin == null || preview == null || preview.Catalog == null || preview.Catalog.Count == 0) return;
            nameplateSkin.text = "Traje: " + preview.Catalog.Get(preview.CurrentIndex).displayName;
        }

        // ---------- Utilidades ----------

        static void ShiftLabels(Transform button, float x)
        {
            foreach (string child in new[] { "Label", "Subtitle" })
            {
                var t = button.Find(child) as RectTransform;
                if (t == null) continue;
                t.anchoredPosition = new Vector2(x, t.anchoredPosition.y);
                t.sizeDelta = new Vector2(t.sizeDelta.x + 120f, t.sizeDelta.y);
            }
        }
    }
}
