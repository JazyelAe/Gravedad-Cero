using GravedadCero.Interaction;
using GravedadCero.Level;
using GravedadCero.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GravedadCero.UI
{
    /// <summary>
    /// Interfaz de la sección, construida por código para no depender de assets de UI:
    /// una tarjeta de introducción (nombre, objetivo, pista y controles esenciales) que se
    /// desvanece cuando Kai empieza a moverse o a los pocos segundos, un objetivo compacto
    /// arriba a la izquierda, la mira, la indicación contextual ([Clic] Anclar, [E] Activar)
    /// y el mensaje de victoria. La lista completa de controles está en el menú de pausa.
    /// Solo lee estado de otros componentes; no cambia nada del juego.
    /// </summary>
    public class LevelHUD : MonoBehaviour
    {
        public enum IntroState { Showing, Fading, Hidden }

        [SerializeField] LevelManager levelManager;
        [SerializeField] GravitationalTool gravitationalTool;
        [SerializeField] PlayerInteractor interactor;
        [Tooltip("Se usa para ocultar la introducción en cuanto Kai empieza a moverse.")]
        [SerializeField] PlayerMotor motor;
        [SerializeField] float introMaxSeconds = 8f;
        [SerializeField] float introFadeSeconds = 0.5f;

        const string EssentialControls = "WASD mover · Espacio saltar · Q / R / F gravedad · Clic anclar · Esc pausa";
        const float MoveSpeedToDismiss = 0.5f;
        const float IgnoreMovementSeconds = 0.3f; // Kai se asienta en el suelo al aparecer

        static readonly Color Neutral = new Color(1f, 1f, 1f, 0.6f);

        TMP_Text crosshair;
        TMP_Text prompt;
        GameObject victoryPanel;
        CanvasGroup intro;
        CanvasGroup compactObjective;
        Rigidbody motorBody;
        float introElapsed;

        public IntroState Intro { get; private set; } = IntroState.Showing;
        public float IntroAlpha => intro != null ? intro.alpha : 0f;

        void Awake()
        {
            if (motor != null) motorBody = motor.GetComponent<Rigidbody>();
            Transform root = UIFactory.Canvas(transform, "HUD Canvas");
            string title = levelManager != null ? levelManager.LevelTitle : "";
            string objective = levelManager != null ? levelManager.Objective : "";
            string hint = levelManager != null ? levelManager.Hint : "";

            BuildIntro(root, title, objective, hint);

            compactObjective = Group(UIFactory.Label(root, "Objective", $"<b>Objetivo:</b> {objective}", 24, TextAnchor.UpperLeft,
                new Vector2(0, 1), new Vector2(40, -32), new Vector2(1100, 40), Color.white).gameObject);
            compactObjective.alpha = 0f;
            UIFactory.Label(root, "PauseTip", "Esc: pausa y controles", 20, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(40, 30), new Vector2(600, 40), Neutral);
            crosshair = UIFactory.Label(root, "Crosshair", "+", 34, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60, 60), Neutral);
            prompt = UIFactory.Label(root, "Prompt", "", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -70), new Vector2(800, 50), UIFactory.Cyan);

            victoryPanel = UIFactory.FullScreenImage(root, "Victory", new Color(0.02f, 0.04f, 0.07f, 0.75f)).gameObject;
            bool hasNext = levelManager != null && levelManager.HasNext;
            UIFactory.Label(victoryPanel.transform, "Title", hasNext ? "SECCIÓN SUPERADA" : "SECTOR COMPLETADO", 64, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(1400, 90), UIFactory.Cyan);
            UIFactory.Label(victoryPanel.transform, "Subtitle",
                hasNext ? "Enter: siguiente sección · Retroceso: repetir · Esc: menú" : "Enter: menú principal · Retroceso: repetir",
                28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(1400, 50), Color.white);
            victoryPanel.SetActive(false);
        }

        void OnEnable()
        {
            if (levelManager != null) levelManager.Completed += ShowVictory;
        }

        void OnDisable()
        {
            if (levelManager != null) levelManager.Completed -= ShowVictory;
        }

        void Update()
        {
            UpdateIntro();

            if (levelManager != null && levelManager.IsCompleted)
            {
                prompt.text = "";
                crosshair.text = "";
                return;
            }

            AnchorableBody target = gravitationalTool != null ? gravitationalTool.Target : null;
            if (target != null)
            {
                crosshair.color = UIFactory.Cyan;
                prompt.color = UIFactory.Cyan;
                prompt.text = target.IsAnchored ? "[Clic] Soltar caja" : "[Clic] Anclar caja";
            }
            else if (interactor != null && interactor.Current != null)
            {
                crosshair.color = Neutral;
                prompt.color = UIFactory.Amber;
                prompt.text = $"[E] {interactor.Current.Prompt}";
            }
            else
            {
                crosshair.color = Neutral;
                prompt.text = "";
            }
        }

        /// <summary>Empieza a desvanecer la introducción (si sigue visible).</summary>
        public void DismissIntro()
        {
            if (Intro == IntroState.Showing) Intro = IntroState.Fading;
        }

        void UpdateIntro()
        {
            if (Intro == IntroState.Showing)
            {
                // Tiempo escalado: en pausa la tarjeta no se consume.
                introElapsed += Time.deltaTime;
                if (introElapsed >= introMaxSeconds || (introElapsed > IgnoreMovementSeconds && KaiIsMoving())) DismissIntro();
            }
            if (Intro == IntroState.Fading)
            {
                intro.alpha = Mathf.MoveTowards(intro.alpha, 0f, Time.unscaledDeltaTime / Mathf.Max(0.01f, introFadeSeconds));
                if (intro.alpha <= 0f)
                {
                    Intro = IntroState.Hidden;
                    intro.gameObject.SetActive(false);
                }
            }
            compactObjective.alpha = 1f - intro.alpha;
        }

        bool KaiIsMoving()
        {
            if (motor == null || motorBody == null) return false;
            return Vector3.ProjectOnPlane(motorBody.linearVelocity, motor.Up).magnitude > MoveSpeedToDismiss;
        }

        void BuildIntro(Transform root, string title, string objective, string hint)
        {
            var panel = UIFactory.Panel(root, "Intro", new Vector2(0.5f, 1), new Vector2(0, -50), 1120);
            intro = panel.AddComponent<CanvasGroup>();
            intro.blocksRaycasts = false;
            intro.interactable = false;
            var t = panel.transform;
            UIFactory.PanelText(t, "Title", title, 40, 56, UIFactory.Cyan, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.PanelText(t, "Objective", $"<b>Objetivo:</b> {objective}", 28, 44, Color.white);
            if (!string.IsNullOrEmpty(hint)) UIFactory.PanelText(t, "Hint", $"<i>{hint}</i>", 24, 64, UIFactory.Amber);
            UIFactory.PanelText(t, "Controls", EssentialControls, 22, 36, new Color(1f, 1f, 1f, 0.8f));
            // Los textos del panel usan el ancho completo de la tarjeta.
            foreach (var label in panel.GetComponentsInChildren<TMP_Text>()) ((RectTransform)label.transform).sizeDelta = new Vector2(1040, ((RectTransform)label.transform).sizeDelta.y);
        }

        static CanvasGroup Group(GameObject go) => go.AddComponent<CanvasGroup>();

        void ShowVictory()
        {
            DismissIntro();
            victoryPanel.SetActive(true);
        }
    }
}
