using System;
using System.Collections.Generic;
using GravedadCero.Player;
using GravedadCero.Save;
using UnityEngine;
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

        static readonly Color Cyan = UIFactory.Cyan;

        Transform root;
        readonly Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
        Text skinLabel;

        public IReadOnlyList<Section> Sections => sections;

        void Awake()
        {
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
            root = UIFactory.Canvas(transform, "Menu Canvas");

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

        // Atajos a UIFactory con la posición de los paneles del menú principal.
        GameObject Panel(string name) => UIFactory.Panel(root, name, new Vector2(0, 1), new Vector2(120, -300), 600);

        static Button AddButton(Transform parent, string text, Action onClick) => UIFactory.Button(parent, text, onClick);

        static Text Label(Transform parent, string name, string text, int size, TextAnchor anchor, Vector2 pivotAnchor,
            Vector2 position, Vector2 boxSize, Color color, FontStyle style = FontStyle.Normal) =>
            UIFactory.Label(parent, name, text, size, anchor, pivotAnchor, position, boxSize, color, style);

        static void Clear(Transform panel) => UIFactory.Clear(panel);

        static void SelectFirstButton(GameObject panel) => UIFactory.SelectFirst(panel);
    }
}
