using System;
using System.Collections.Generic;
using GravedadCero.Level;
using GravedadCero.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GravedadCero.UI
{
    /// <summary>
    /// Menú de pausa de una sección, con la misma composición que el menú principal: título
    /// a la izquierda, panel de opciones debajo y, a la derecha, el objetivo de la sección.
    /// Opciones: Reanudar, Controles, Configuración, Reiniciar sección y Volver al menú.
    /// Reiniciar y Volver al menú piden confirmación porque descartan el avance de la sección
    /// (las secciones completadas siguen guardadas). Solo muestra el estado de
    /// PauseController y le pide acciones; no toca el tiempo por su cuenta.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] PauseController pauseController;
        [SerializeField] LevelManager levelManager;

        static readonly (string[] keys, string action)[] FullControls =
        {
            (new[] { "W", "A", "S", "D" }, "Moverse (relativo a la cámara)"),
            (new[] { "Mouse" }, "Girar la cámara"),
            (new[] { "Espacio" }, "Saltar"),
            (new[] { "Q", "R" }, "Girar la gravedad a la izquierda / derecha"),
            (new[] { "F" }, "Invertir la gravedad"),
            (new[] { "Clic" }, "Anclar o soltar la caja de la mira"),
            (new[] { "E" }, "Usar interruptores"),
            (new[] { "Retroceso" }, "Reiniciar la sección al instante"),
            (new[] { "Esc" }, "Pausa / volver"),
        };

        const float KeyColumn = 250f;

        readonly Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
        GameObject root;
        GameObject objectiveCard;
        TMP_Text confirmTitle;
        TMP_Text confirmDetail;
        TMP_Text sensitivityValue;
        Slider sensitivitySlider;
        TMP_Text musicValue;
        Slider musicSlider;
        TMP_Text sfxValue;
        Slider sfxSlider;
        Toggle invertToggle;
        Action pendingConfirm;

        /// <summary>Panel visible ("Main", "Controls", "Settings", "Confirm") o null si está cerrado.</summary>
        public string CurrentPanel { get; private set; }

        void Awake()
        {
            var canvas = UIFactory.Canvas(transform, "Pause Canvas", sortingOrder: 10);
            root = canvas.gameObject;
            UIFactory.FullScreenImage(canvas, "Dim", new Color(0.01f, 0.02f, 0.04f, 0.55f));
            UIFactory.Backdrop(canvas, 1300f, 0.9f);

            BuildHeading(canvas);
            BuildObjectiveCard(canvas);
            BuildMainPanel(canvas);
            BuildControlsPanel(canvas);
            BuildSettingsPanel(canvas);
            BuildConfirmPanel(canvas);
            BuildHints(canvas);
            root.SetActive(false);
        }

        void OnEnable()
        {
            if (pauseController == null) return;
            pauseController.PauseChanged += OnPauseChanged;
            pauseController.BackHandler = Back;
        }

        void OnDisable()
        {
            if (pauseController == null) return;
            pauseController.PauseChanged -= OnPauseChanged;
            if (pauseController.BackHandler == (Func<bool>)Back) pauseController.BackHandler = null;
        }

        // ---------- Acciones (públicas para poder probarlas) ----------

        public void Resume() => pauseController.SetPaused(false);

        public void RequestRestart() =>
            Confirm("¿Reiniciar la sección?", "Se perderá el avance de esta sección.", pauseController.RestartSection);

        public void RequestExitToMenu() =>
            Confirm("¿Volver al menú principal?", "Se perderá el avance de esta sección. Las secciones completadas se conservan.", pauseController.ExitToMenu);

        public void ConfirmPending()
        {
            Action action = pendingConfirm;
            pendingConfirm = null;
            action?.Invoke();
        }

        public void CancelPending()
        {
            pendingConfirm = null;
            ShowPanel("Main");
        }

        public void SetSensitivity(float value)
        {
            GameSettings.MouseSensitivity = value;
            RefreshSettings();
        }

        public void SetMusicVolume(float value)
        {
            GameSettings.MusicVolume = value;
            RefreshSettings();
        }

        public void SetSfxVolume(float value)
        {
            GameSettings.SfxVolume = value;
            RefreshSettings();
        }

        public void SetInvertY(bool value)
        {
            GameSettings.InvertY = value;
            RefreshSettings();
        }

        /// <summary>Esc dentro de la pausa: vuelve al panel principal; desde él, reanuda.</summary>
        public bool Back()
        {
            if (CurrentPanel == null || CurrentPanel == "Main") return false;
            if (CurrentPanel == "Confirm") pendingConfirm = null;
            ShowPanel("Main");
            return true;
        }

        public void ShowPanel(string name)
        {
            CurrentPanel = name;
            foreach (var pair in panels) pair.Value.SetActive(pair.Key == name);
            if (name == "Settings") RefreshSettings();
            // El objetivo se oculta con los paneles anchos para que no se solapen en 4:3.
            objectiveCard.SetActive(name == "Main" || name == "Settings");
            UIReveal.Play(panels[name], 0f, new Vector2(-24f, 0f), 0.24f);
            UIFactory.SelectFirst(panels[name]);
        }

        // ---------- Construcción ----------

        void OnPauseChanged(bool paused)
        {
            root.SetActive(paused);
            if (paused) ShowPanel("Main");
            else
            {
                CurrentPanel = null;
                pendingConfirm = null;
            }
        }

        void Confirm(string title, string detail, Action action)
        {
            pendingConfirm = action;
            confirmTitle.text = title;
            confirmDetail.text = detail;
            UIFactory.FitHeight(confirmDetail, 680 - 72);
            ShowPanel("Confirm");
        }

        GameObject Panel(Transform canvas, string name, string header, float width)
        {
            var panel = UIFactory.Panel(canvas, name, new Vector2(0, 1), new Vector2(120, -290), width, header);
            panel.AddComponent<CanvasGroup>();
            panels[name] = panel;
            return panel;
        }

        void BuildHeading(Transform canvas)
        {
            string title = levelManager != null ? levelManager.LevelTitle : "";
            string code = UIFactory.SectionCode(title);
            var heading = new GameObject("Heading", typeof(RectTransform));
            heading.transform.SetParent(canvas, false);
            UIFactory.Place((RectTransform)heading.transform, new Vector2(0, 1), new Vector2(120, -64), new Vector2(1100, 200));
            var h = heading.transform;
            var kicker = UIFactory.Caps(h, "Kicker", string.IsNullOrEmpty(code) ? "Estación Helios-7  /  Sistema en espera" : $"Sección {code}  /  Sistema en espera", UITheme.Amber);
            UIFactory.Place(kicker.rectTransform, new Vector2(0, 1), new Vector2(0, -16), new Vector2(900, 32));
            var big = UIFactory.Title(h, "Title", "PAUSA", UITheme.SizeTitle + 16, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(-3, -48), new Vector2(900, 90), UITheme.Text);
            big.characterSpacing = 4f;
            UIFactory.Image(h, "Accent", null, UITheme.Cyan, new Vector2(0, 1), new Vector2(0, -142), new Vector2(200, 5)).rectTransform.pivot = new Vector2(0, 1);
            UIFactory.Image(h, "Line", null, UITheme.CyanDim, new Vector2(0, 1), new Vector2(0, -146), new Vector2(720, 1)).rectTransform.pivot = new Vector2(0, 1);
            UIFactory.Label(h, "Section", UIFactory.SectionName(title), UITheme.SizeBody, TextAnchor.UpperLeft, new Vector2(0, 1),
                new Vector2(0, -160), new Vector2(900, 40), UITheme.TextSecondary);
        }

        void BuildObjectiveCard(Transform canvas)
        {
            const float width = 560f;
            objectiveCard = UIFactory.Panel(canvas, "ObjectiveCard", new Vector2(1, 1), new Vector2(-120, -290), width, "Objetivo");
            var t = objectiveCard.transform;
            string objective = levelManager != null ? levelManager.Objective : "";
            string hint = levelManager != null ? levelManager.Hint : "";
            UIFactory.FitHeight(UIFactory.PanelText(t, "Objective", objective, UITheme.SizeBody, 40, UITheme.Text, TextAnchor.MiddleLeft), width - 72);
            if (string.IsNullOrEmpty(hint)) return;
            UIFactory.Header(t, "Pista");
            UIFactory.FitHeight(UIFactory.PanelText(t, "Hint", hint, UITheme.SizeSmall, 36, UITheme.TextSecondary, TextAnchor.MiddleLeft), width - 72);
        }

        void BuildMainPanel(Transform canvas)
        {
            var panel = Panel(canvas, "Main", "Opciones", 600).transform;
            UIFactory.Button(panel, "Reanudar", Resume, 80, "Volver a la sección");
            UIFactory.Button(panel, "Controles", () => ShowPanel("Controls"), 80, "Teclado y mouse");
            UIFactory.Button(panel, "Configuración", () => ShowPanel("Settings"), 80, "Cámara y sonido");
            UIFactory.Button(panel, "Reiniciar sección", RequestRestart, 80, "Empezar desde el principio");
            UIFactory.Button(panel, "Volver al menú", RequestExitToMenu, 80, "Las secciones completadas se conservan");
        }

        void BuildControlsPanel(Transform canvas)
        {
            var panel = Panel(canvas, "Controls", "Controles", 860).transform;
            var table = new GameObject("Table", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            table.transform.SetParent(panel, false);
            var layout = table.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 4;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            const float rowHeight = 44f;
            foreach (var (keys, action) in FullControls)
            {
                var row = UIFactory.Row(table.transform, "Row " + action, rowHeight, 0f);
                var keyCell = UIFactory.Row(row, "Keys", rowHeight, 8f);
                keyCell.sizeDelta = new Vector2(KeyColumn, rowHeight);
                foreach (string key in keys) UIFactory.KeyChip(keyCell, key);
                UIFactory.Label(row, "Action", action, UITheme.SizeSmall, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), Vector2.zero,
                    new Vector2(860 - 72 - KeyColumn, rowHeight), UITheme.Text).textWrappingMode = TextWrappingModes.NoWrap;
            }
            float tableHeight = FullControls.Length * rowHeight + (FullControls.Length - 1) * layout.spacing;
            ((RectTransform)table.transform).sizeDelta = new Vector2(0, tableHeight);
            table.GetComponent<LayoutElement>().preferredHeight = tableHeight;
            UIFactory.Spacer(panel, 2);
            UIFactory.Button(panel, "Volver", () => ShowPanel("Main"), 64);
        }

        void BuildSettingsPanel(Transform canvas)
        {
            var panel = Panel(canvas, "Settings", "Configuración", 600).transform;
            sensitivitySlider = SliderRow(panel, "Sensitivity", "Sensibilidad del mouse", GameSettings.MinSensitivity, GameSettings.MaxSensitivity,
                GameSettings.MouseSensitivity, SetSensitivity, out sensitivityValue);
            invertToggle = UIFactory.Toggle(panel, "Invertir eje Y de la cámara", GameSettings.InvertY, SetInvertY);
            musicSlider = SliderRow(panel, "Music", "Volumen de la música", 0f, 1f, GameSettings.MusicVolume, SetMusicVolume, out musicValue);
            sfxSlider = SliderRow(panel, "Sfx", "Volumen de los efectos", 0f, 1f, GameSettings.SfxVolume, SetSfxVolume, out sfxValue);
            UIFactory.Button(panel, "Volver", () => ShowPanel("Main"), 64);
        }

        /// <summary>Etiqueta con su valor a la derecha y, debajo, el deslizador.</summary>
        static Slider SliderRow(Transform panel, string name, string text, float min, float max, float value, Action<float> onChanged, out TMP_Text valueLabel)
        {
            var row = UIFactory.Row(panel, name + "Row", 40, 0f);
            var label = UIFactory.Label(row, "Label", text, UITheme.SizeBody, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), Vector2.zero, new Vector2(400, 40), UITheme.Text);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            valueLabel = UIFactory.Label(row, "Value", "", UITheme.SizeBody, TextAnchor.MiddleRight, new Vector2(0, 0.5f), Vector2.zero, new Vector2(128, 40), UITheme.Cyan, FontStyle.Bold);
            return UIFactory.Slider(panel, name, min, max, value, onChanged);
        }

        void BuildConfirmPanel(Transform canvas)
        {
            var panel = Panel(canvas, "Confirm", "Atención", 680);
            // Contorno ámbar: la acción descarta el avance de la sección.
            panel.transform.Find("Frame").GetComponent<Image>().color = new Color(UITheme.Amber.r, UITheme.Amber.g, UITheme.Amber.b, 0.6f);
            var t = panel.transform;
            confirmTitle = UIFactory.PanelText(t, "Title", "", UITheme.SizeHeading, 48, UITheme.Text, TextAnchor.MiddleLeft, FontStyle.Bold);
            confirmDetail = UIFactory.PanelText(t, "Detail", "", UITheme.SizeBody, 40, UITheme.TextSecondary, TextAnchor.MiddleLeft);
            UIFactory.Spacer(t, 4);
            UIFactory.Button(t, "Cancelar", CancelPending);
            UIFactory.Button(t, "Confirmar", ConfirmPending);
        }

        void BuildHints(Transform canvas)
        {
            var hints = UIFactory.Row(canvas, "Hints", 40f, 30f);
            hints.GetComponent<LayoutElement>().ignoreLayout = true;
            UIFactory.KeyHint(hints, new[] { "Flechas" }, "Navegar", UITheme.SizeCaption, 110);
            UIFactory.KeyHint(hints, new[] { "Enter" }, "Elegir", UITheme.SizeCaption, 80);
            UIFactory.KeyHint(hints, new[] { "Esc" }, "Volver / reanudar", UITheme.SizeCaption, 200);
            UIFactory.KeyHint(hints, new[] { "Retroceso" }, "Reiniciar al instante", UITheme.SizeCaption, 240);
            UIFactory.Place(hints, new Vector2(0, 0), new Vector2(120, 34), new Vector2(1100, 40));
        }

        void RefreshSettings()
        {
            float sensitivity = GameSettings.MouseSensitivity;
            float percent = sensitivity / GameSettings.DefaultSensitivity * 100f;
            sensitivityValue.text = $"{percent:F0} %";
            sensitivitySlider.SetValueWithoutNotify(sensitivity);
            musicValue.text = $"{GameSettings.MusicVolume * 100f:F0} %";
            musicSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
            sfxValue.text = $"{GameSettings.SfxVolume * 100f:F0} %";
            sfxSlider.SetValueWithoutNotify(GameSettings.SfxVolume);
            invertToggle.SetIsOnWithoutNotify(GameSettings.InvertY);
        }
    }
}
