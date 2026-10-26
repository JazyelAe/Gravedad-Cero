using System;
using System.Collections.Generic;
using GravedadCero.Level;
using GravedadCero.Save;
using UnityEngine;
using UnityEngine.UI;

namespace GravedadCero.UI
{
    /// <summary>
    /// Menú de pausa de una sección: Reanudar, Controles, Configuración, Reiniciar sección y
    /// Volver al menú. Reiniciar y Volver al menú piden confirmación porque descartan el
    /// avance de la sección (las secciones completadas siguen guardadas). Solo muestra el
    /// estado de PauseController y le pide acciones; no toca el tiempo por su cuenta.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] PauseController pauseController;
        [SerializeField] LevelManager levelManager;

        const string FullControls =
            "<b>WASD</b>  moverse (relativo a la cámara)\n" +
            "<b>Mouse</b>  girar la cámara\n" +
            "<b>Espacio</b>  saltar\n" +
            "<b>Q / R</b>  girar la gravedad a la izquierda / derecha de Kai\n" +
            "<b>F</b>  invertir la gravedad\n" +
            "<b>Clic izquierdo</b>  anclar o soltar la caja de la mira\n" +
            "<b>E</b>  usar interruptores\n" +
            "<b>Retroceso</b>  reiniciar la sección al instante\n" +
            "<b>Esc</b>  pausa";

        readonly Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
        GameObject root;
        Text confirmMessage;
        Text sensitivityValue;
        Slider sensitivitySlider;
        Toggle invertToggle;
        Action pendingConfirm;

        /// <summary>Panel visible ("Main", "Controls", "Settings", "Confirm") o null si está cerrado.</summary>
        public string CurrentPanel { get; private set; }

        void Awake()
        {
            var canvas = UIFactory.Canvas(transform, "Pause Canvas", sortingOrder: 10);
            root = canvas.gameObject;
            UIFactory.FullScreenImage(canvas, "Dim", new Color(0.01f, 0.02f, 0.04f, 0.6f));
            UIFactory.Label(canvas, "Title", "PAUSA", 64, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -110), new Vector2(1200, 90), Color.white, FontStyle.Bold);
            UIFactory.Label(canvas, "Section", levelManager != null ? levelManager.LevelTitle : "", 28, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1), new Vector2(0, -195), new Vector2(1200, 50), UIFactory.Cyan);

            BuildMainPanel(canvas);
            BuildControlsPanel(canvas);
            BuildSettingsPanel(canvas);
            BuildConfirmPanel(canvas);
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
            Confirm("¿Reiniciar la sección?\nSe perderá el avance de esta sección.", pauseController.RestartSection);

        public void RequestExitToMenu() =>
            Confirm("¿Volver al menú principal?\nSe perderá el avance de esta sección.\nLas secciones completadas se conservan.", pauseController.ExitToMenu);

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

        void Confirm(string message, Action action)
        {
            pendingConfirm = action;
            confirmMessage.text = message;
            ShowPanel("Confirm");
        }

        GameObject Panel(Transform canvas, string name, float width = 600)
        {
            var panel = UIFactory.Panel(canvas, name, new Vector2(0.5f, 1), new Vector2(0, -270), width);
            panels[name] = panel;
            return panel;
        }

        void BuildMainPanel(Transform canvas)
        {
            var panel = Panel(canvas, "Main").transform;
            UIFactory.Button(panel, "Reanudar", Resume);
            UIFactory.Button(panel, "Controles", () => ShowPanel("Controls"));
            UIFactory.Button(panel, "Configuración", () => ShowPanel("Settings"));
            UIFactory.Button(panel, "Reiniciar sección", RequestRestart);
            UIFactory.Button(panel, "Volver al menú", RequestExitToMenu);
        }

        void BuildControlsPanel(Transform canvas)
        {
            var panel = Panel(canvas, "Controls", 900).transform;
            UIFactory.PanelText(panel, "List", FullControls, 26, 330, Color.white, TextAnchor.UpperLeft);
            if (levelManager != null && !string.IsNullOrEmpty(levelManager.Hint))
                UIFactory.PanelText(panel, "Hint", $"<i>Pista: {levelManager.Hint}</i>", 24, 70, UIFactory.Amber, TextAnchor.UpperLeft);
            UIFactory.Button(panel, "Volver", () => ShowPanel("Main"));
        }

        void BuildSettingsPanel(Transform canvas)
        {
            var panel = Panel(canvas, "Settings").transform;
            sensitivityValue = UIFactory.PanelText(panel, "SensitivityLabel", "", 28, 44, Color.white);
            sensitivitySlider = UIFactory.Slider(panel, "Sensitivity", GameSettings.MinSensitivity, GameSettings.MaxSensitivity,
                GameSettings.MouseSensitivity, SetSensitivity);
            invertToggle = UIFactory.Toggle(panel, "Invertir eje Y de la cámara", GameSettings.InvertY, SetInvertY);
            UIFactory.Button(panel, "Volver", () => ShowPanel("Main"));
        }

        void BuildConfirmPanel(Transform canvas)
        {
            var panel = Panel(canvas, "Confirm", 760).transform;
            confirmMessage = UIFactory.PanelText(panel, "Message", "", 28, 150, Color.white);
            UIFactory.Button(panel, "Cancelar", CancelPending);
            UIFactory.Button(panel, "Confirmar", ConfirmPending);
        }

        void RefreshSettings()
        {
            float sensitivity = GameSettings.MouseSensitivity;
            float percent = sensitivity / GameSettings.DefaultSensitivity * 100f;
            sensitivityValue.text = $"Sensibilidad del mouse: {percent:F0} %";
            sensitivitySlider.SetValueWithoutNotify(sensitivity);
            invertToggle.SetIsOnWithoutNotify(GameSettings.InvertY);
        }
    }
}
