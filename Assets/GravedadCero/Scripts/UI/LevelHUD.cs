using GravedadCero.Interaction;
using GravedadCero.Level;
using GravedadCero.Player;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GravedadCero.UI
{
    /// <summary>
    /// Interfaz de la sección con el estilo Helios-7, construida por código: una tarjeta de
    /// introducción (sección, objetivo, pista y controles esenciales) que se desvanece cuando
    /// Kai empieza a moverse o a los pocos segundos, un objetivo compacto arriba a la
    /// izquierda, la integridad del traje abajo a la izquierda, la indicación contextual abajo al
    /// centro ([Clic] Anclar, [E] Activar), avisos (punto de control, registro de datos), un
    /// destello rojo al recibir un golpe, la pantalla de derrota (Reintentar, Reiniciar, Menú) y la pantalla
    /// de victoria con botones (Continuar, Repetir, Menú) que funcionan con ratón y teclado.
    /// La lista completa de controles está en el menú de pausa. Solo lee estado de otros
    /// componentes y, en la victoria, le pide a LevelManager que cambie de escena.
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

        const float MoveSpeedToDismiss = 0.5f;
        const float IgnoreMovementSeconds = 0.3f; // Kai se asienta en el suelo al aparecer
        const float IntroWidth = 1180f;
        const float IntroInner = IntroWidth - 72f;
        const float ObjectiveWidth = 600f;
        const float BannerSeconds = 0.6f;
        const float NoticeSeconds = 3f;

        CanvasGroup hudGroup;
        PauseController pauseController;
        GameObject pauseTip;
        CanvasGroup notice;
        Image noticeFrame;
        TMP_Text noticeTitle;
        TMP_Text noticeText;
        float noticeLeft;
        PlayerHealth health;
        Image[] integritySegments;
        GameObject integrityBar;
        Image damageFlash;
        float damageFlashLeft;
        GameObject promptChip;
        GameObject promptKeyClick;
        GameObject promptKeyUse;
        TMP_Text promptLabel;
        GameObject victoryPanel;
        RectTransform victoryBand;
        Button continueButton;
        FallRecovery recovery;
        GameObject defeatPanel;
        TMP_Text defeatTitle;
        TMP_Text defeatDetail;
        Button retryButton;
        float defeatDelay = -1f;
        const float DefeatDelaySeconds = 0.6f;
        CanvasGroup intro;
        CanvasGroup compactObjective;
        Rigidbody motorBody;
        float introElapsed;
        float victoryElapsed = -1f;

        public IntroState Intro { get; private set; } = IntroState.Showing;
        public float IntroAlpha => intro != null ? intro.alpha : 0f;

        /// <summary>True cuando se ve la pantalla de victoria.</summary>
        public bool VictoryVisible => victoryPanel != null && victoryPanel.activeSelf;

        void Awake()
        {
            if (motor != null)
            {
                motorBody = motor.GetComponent<Rigidbody>();
                health = motor.GetComponent<PlayerHealth>();
                recovery = motor.GetComponent<FallRecovery>();
            }
            Transform root = UIFactory.Canvas(transform, "HUD Canvas");
            hudGroup = root.gameObject.AddComponent<CanvasGroup>();
            pauseController = FindAnyObjectByType<PauseController>();
            string title = levelManager != null ? levelManager.LevelTitle : "";
            string objective = levelManager != null ? levelManager.Objective : "";
            string hint = levelManager != null ? levelManager.Hint : "";

            BuildIntro(root, title, objective, hint);
            BuildCompactObjective(root, title, objective);
            BuildPrompt(root);
            BuildPauseTip(root);
            BuildNotice(root);
            BuildIntegrity(root);
            BuildVictory(root, title);
            BuildDefeat(root, title);
        }

        void OnEnable()
        {
            if (levelManager != null) levelManager.Completed += ShowVictory;
            if (pauseController != null) pauseController.PauseChanged += OnPauseChanged;
            if (recovery != null) { recovery.Downed += OnDowned; PlayerInputHandler.DefeatRetry = RetryFromCheckpoint; }
            foreach (var checkpoint in FindObjectsByType<Checkpoint>()) checkpoint.Reached += ShowCheckpointNotice;
            foreach (var log in FindObjectsByType<DataLog>()) log.Collected += ShowDataLogNotice;
            if (health != null) { health.IntegrityChanged += RefreshIntegrity; health.Damaged += OnDamaged; }
        }

        void OnDisable()
        {
            if (levelManager != null) levelManager.Completed -= ShowVictory;
            if (pauseController != null) pauseController.PauseChanged -= OnPauseChanged;
            if (recovery != null)
            {
                recovery.Downed -= OnDowned;
                if (PlayerInputHandler.DefeatRetry == (System.Action)RetryFromCheckpoint) PlayerInputHandler.DefeatRetry = null;
            }
            foreach (var checkpoint in FindObjectsByType<Checkpoint>()) checkpoint.Reached -= ShowCheckpointNotice;
            foreach (var log in FindObjectsByType<DataLog>()) log.Collected -= ShowDataLogNotice;
            if (health != null) { health.IntegrityChanged -= RefreshIntegrity; health.Damaged -= OnDamaged; }
        }

        // PlayerHealth inicializa su integridad en su propio Awake, que puede ir después del nuestro.
        void Start()
        {
            if (health != null) RefreshIntegrity(health.Integrity);
        }

        // El menú de pausa tiene su propia composición: el HUD se oculta mientras está abierto.
        void OnPauseChanged(bool paused) => hudGroup.alpha = paused ? 0f : 1f;

        void Update()
        {
            UpdateIntro();
            UpdateNotice();
            UpdateDamageFlash();
            if (UpdateDefeat()) return;

            if (levelManager != null && levelManager.IsCompleted)
            {
                promptChip.SetActive(false);
                UpdateVictory();
                return;
            }

            AnchorableBody target = gravitationalTool != null ? gravitationalTool.Target : null;
            if (target != null)
            {
                ShowPrompt(true, target.IsAnchored ? "Soltar caja" : "Anclar caja", UITheme.Cyan);
            }
            else if (interactor != null && interactor.Current != null)
            {
                ShowPrompt(false, interactor.Current.Prompt, UITheme.Amber);
            }
            else promptChip.SetActive(false);
        }

        /// <summary>Empieza a desvanecer la introducción (si sigue visible).</summary>
        public void DismissIntro()
        {
            if (Intro == IntroState.Showing) Intro = IntroState.Fading;
        }

        // ---------- Introducción y objetivo ----------

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
            bool completed = levelManager != null && levelManager.IsCompleted;
            compactObjective.alpha = completed ? 0f : 1f - intro.alpha;
        }

        bool KaiIsMoving()
        {
            if (motor == null || motorBody == null) return false;
            return Vector3.ProjectOnPlane(motorBody.linearVelocity, motor.Up).magnitude > MoveSpeedToDismiss;
        }

        void BuildIntro(Transform root, string title, string objective, string hint)
        {
            var panel = UIFactory.Panel(root, "Intro", new Vector2(0.5f, 1), new Vector2(0, -48), IntroWidth);
            intro = panel.AddComponent<CanvasGroup>();
            intro.blocksRaycasts = false;
            intro.interactable = false;
            var t = panel.transform;
            panel.GetComponent<VerticalLayoutGroup>().spacing = 10;

            string code = UIFactory.SectionCode(title);
            var kicker = UIFactory.Caps(t, "Kicker", string.IsNullOrEmpty(code) ? "Estación Helios-7" : $"Estación Helios-7  /  Sección {code}", UITheme.Amber);
            kicker.gameObject.AddComponent<LayoutElement>().preferredHeight = 30;
            var name = UIFactory.Title(t, "Title", UIFactory.SectionName(title).ToUpperInvariant(), 44, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 1), Vector2.zero, new Vector2(IntroInner, 58), UITheme.Text);
            name.gameObject.AddComponent<LayoutElement>().preferredHeight = 58;
            UIFactory.Spacer(t, 2);

            UIFactory.Header(t, "Objetivo", UITheme.Cyan);
            UIFactory.FitHeight(UIFactory.PanelText(t, "Objective", objective, UITheme.SizeBody, 40, UITheme.Text, TextAnchor.MiddleLeft), IntroInner);
            if (!string.IsNullOrEmpty(hint))
            {
                UIFactory.Header(t, "Pista");
                UIFactory.FitHeight(UIFactory.PanelText(t, "Hint", hint, UITheme.SizeSmall, 36, UITheme.TextSecondary, TextAnchor.MiddleLeft), IntroInner);
            }

            UIFactory.Header(t, "Controles", UITheme.CyanDim);
            var row1 = UIFactory.Row(t, "Controls1", 46, 34, TextAnchor.MiddleLeft);
            UIFactory.KeyHint(row1, new[] { "W", "A", "S", "D" }, "Mover");
            UIFactory.KeyHint(row1, new[] { "Espacio" }, "Saltar");
            UIFactory.KeyHint(row1, new[] { "Q", "R", "F" }, "Cambiar gravedad");
            var row2 = UIFactory.Row(t, "Controls2", 46, 34, TextAnchor.MiddleLeft);
            UIFactory.KeyHint(row2, new[] { "Clic" }, "Anclar caja");
            UIFactory.KeyHint(row2, new[] { "E" }, "Usar");
            UIFactory.KeyHint(row2, new[] { "Retroceso" }, "Reiniciar");
            UIFactory.KeyHint(row2, new[] { "Esc" }, "Pausa");
        }

        void BuildCompactObjective(Transform root, string title, string objective)
        {
            var panel = UIFactory.Panel(root, "Objective", new Vector2(0, 1), new Vector2(40, -36), ObjectiveWidth);
            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(26, 26, 14, 18);
            layout.spacing = 4;
            compactObjective = panel.AddComponent<CanvasGroup>();
            compactObjective.blocksRaycasts = false;
            compactObjective.alpha = 0f;
            string code = UIFactory.SectionCode(title);
            var caps = UIFactory.Caps(panel.transform, "Label", string.IsNullOrEmpty(code) ? "Objetivo" : $"Objetivo  /  {code}", UITheme.Amber, 18);
            caps.gameObject.AddComponent<LayoutElement>().preferredHeight = 28;
            UIFactory.FitHeight(UIFactory.WithShadow(UIFactory.PanelText(panel.transform, "Text", objective, UITheme.SizeSmall, 34, UITheme.Text, TextAnchor.MiddleLeft)),
                ObjectiveWidth - 52, 4);
        }

        // ---------- Indicación contextual ----------

        void BuildPrompt(Transform root)
        {
            promptChip = new GameObject("Prompt", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            promptChip.transform.SetParent(root, false);
            UIFactory.Place((RectTransform)promptChip.transform, new Vector2(0.5f, 0f), new Vector2(0, 110), new Vector2(300, 56));
            UIFactory.Frame(promptChip.transform, new Color(UITheme.Deep.r, UITheme.Deep.g, UITheme.Deep.b, 0.85f), UITheme.CyanDim);
            var layout = promptChip.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(12, 22, 9, 9);
            layout.spacing = 14;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            var fitter = promptChip.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            promptKeyClick = UIFactory.KeyChip(promptChip.transform, "Clic", UITheme.SizeSmall * 0.85f).gameObject;
            promptKeyUse = UIFactory.KeyChip(promptChip.transform, "E", UITheme.SizeSmall * 0.85f).gameObject;
            promptLabel = UIFactory.Label(promptChip.transform, "Label", "", UITheme.SizeBody, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(300, 38), UITheme.Text, FontStyle.Bold);
            promptLabel.textWrappingMode = TextWrappingModes.NoWrap;
            promptChip.SetActive(false);
        }

        void ShowPrompt(bool click, string action, Color accent)
        {
            promptChip.SetActive(true);
            promptKeyClick.SetActive(click);
            promptKeyUse.SetActive(!click);
            if (promptLabel.text != action) promptLabel.text = action;
            promptLabel.color = accent;
        }

        void BuildPauseTip(Transform root)
        {
            var tip = UIFactory.KeyHint(root, new[] { "Esc" }, "Pausa y controles", UITheme.SizeCaption);
            tip.GetComponent<LayoutElement>().ignoreLayout = true;
            UIFactory.Place(tip, new Vector2(0, 0), new Vector2(40, 30), tip.sizeDelta);
            pauseTip = tip.gameObject;
            UIFactory.WithShadow(tip.Find("Action").GetComponent<TMP_Text>()).color = UITheme.TextSecondary;
        }

        // ---------- Avisos (punto de control, registro de datos) ----------

        /// <summary>True mientras se ve un aviso; NoticeTitle dice cuál.</summary>
        public bool NoticeVisible => notice != null && notice.alpha > 0f;
        public string NoticeTitle => noticeTitle != null ? noticeTitle.text : "";

        void BuildNotice(Transform root)
        {
            var chip = UIFactory.Panel(root, "Notice", new Vector2(1, 1), new Vector2(-40, -36), 460);
            var layout = chip.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 12, 14);
            layout.spacing = 2;
            noticeFrame = chip.transform.Find("Frame").GetComponent<Image>();
            noticeTitle = UIFactory.Caps(chip.transform, "Label", "", UITheme.Success, 18);
            noticeTitle.alignment = TextAlignmentOptions.Midline;
            noticeTitle.gameObject.AddComponent<LayoutElement>().preferredHeight = 28;
            noticeText = UIFactory.PanelText(chip.transform, "Text", "", UITheme.SizeSmall, 32, UITheme.Text);
            notice = chip.AddComponent<CanvasGroup>();
            notice.blocksRaycasts = false;
            notice.alpha = 0f;
        }

        void ShowNotice(string title, string text, Color color)
        {
            noticeTitle.text = title.ToUpperInvariant();
            noticeTitle.color = color;
            noticeText.text = text;
            UIFactory.FitHeight(noticeText, 460 - 48, 4);
            noticeFrame.color = new Color(color.r, color.g, color.b, 0.6f);
            noticeLeft = NoticeSeconds;
        }

        void ShowCheckpointNotice(Checkpoint checkpoint) => ShowNotice("Punto de control", "Si caes o pierdes el traje, reapareces aquí.", UITheme.Success);

        void ShowDataLogNotice(DataLog log) =>
            ShowNotice("Registro de datos encontrado", string.IsNullOrEmpty(log.Message) ? "Guardado en el menú de secciones." : log.Message, UITheme.Cyan);

        void UpdateNotice()
        {
            noticeLeft = Mathf.Max(0f, noticeLeft - Time.unscaledDeltaTime);
            // Aparece rápido y se desvanece durante el último medio segundo.
            float target = Mathf.Clamp01(noticeLeft / 0.5f);
            notice.alpha = Mathf.MoveTowards(notice.alpha, target, Time.unscaledDeltaTime * 6f);
        }

        // ---------- Integridad del traje ----------

        void BuildIntegrity(Transform root)
        {
            if (health == null) return;
            var bar = new GameObject("Integrity", typeof(RectTransform), typeof(CanvasGroup));
            integrityBar = bar;
            bar.transform.SetParent(root, false);
            UIFactory.Place((RectTransform)bar.transform, new Vector2(0, 0), new Vector2(40, 76), new Vector2(360, 54));
            var caps = UIFactory.WithShadow(UIFactory.Caps(bar.transform, "Label", "Integridad del traje", UITheme.TextSecondary, 16));
            UIFactory.Place(caps.rectTransform, new Vector2(0, 1), Vector2.zero, new Vector2(360, 22));
            integritySegments = new Image[health.MaxIntegrity];
            for (int i = 0; i < integritySegments.Length; i++)
            {
                var seg = UIFactory.Image(bar.transform, "Segment" + i, UITheme.PanelFill, UITheme.Cyan, new Vector2(0, 0), new Vector2(i * 62f, 2f), new Vector2(56, 20));
                seg.rectTransform.pivot = new Vector2(0, 0);
                integritySegments[i] = seg;
            }
            damageFlash = UIFactory.FullScreenImage(root, "DamageFlash", new Color(UITheme.Danger.r, UITheme.Danger.g, UITheme.Danger.b, 0f));
            damageFlash.transform.SetAsFirstSibling();
            RefreshIntegrity(health.Integrity);
        }

        /// <summary>Segmentos de integridad encendidos en el HUD (0 si no hay barra).</summary>
        public int IntegrityShown { get; private set; }

        void RefreshIntegrity(int integrity)
        {
            if (integritySegments == null) return;
            IntegrityShown = Mathf.Clamp(integrity, 0, integritySegments.Length);
            Color low = integrity <= 1 ? UITheme.Danger : UITheme.Cyan;
            for (int i = 0; i < integritySegments.Length; i++)
                integritySegments[i].color = i < integrity ? low : new Color(1f, 1f, 1f, 0.12f);
        }

        void OnDamaged(int remaining) => damageFlashLeft = 0.35f;

        void UpdateDamageFlash()
        {
            if (damageFlash == null) return;
            damageFlashLeft = Mathf.Max(0f, damageFlashLeft - Time.unscaledDeltaTime);
            var c = damageFlash.color;
            c.a = damageFlashLeft / 0.35f * 0.35f;
            damageFlash.color = c;
        }

        // ---------- Derrota ----------

        /// <summary>True mientras se ve la pantalla de derrota; DefeatTitle dice cuál.</summary>
        public bool DefeatVisible => defeatPanel != null && defeatPanel.activeSelf;
        public string DefeatTitle => defeatTitle != null ? defeatTitle.text : "";

        void BuildDefeat(Transform root, string title)
        {
            string code = UIFactory.SectionCode(title);
            defeatPanel = new GameObject("Defeat", typeof(RectTransform));
            defeatPanel.transform.SetParent(root, false);
            UIFactory.Stretch((RectTransform)defeatPanel.transform);
            var v = defeatPanel.transform;
            var dim = UIFactory.FullScreenImage(v, "Dim", new Color(0.12f, 0.02f, 0.02f, 0.6f));
            dim.raycastTarget = true;
            var scan = UIFactory.FullScreenImage(v, "Scanlines", new Color(1, 1, 1, 0.025f), UITheme.Scanlines);
            scan.type = Image.Type.Tiled;

            var band = UIFactory.Image(v, "Band", null, new Color(UITheme.Panel.r, UITheme.Panel.g, UITheme.Panel.b, 0.92f), new Vector2(0.5f, 0.5f), new Vector2(0, 190), new Vector2(0, 230));
            band.rectTransform.anchorMin = new Vector2(0, 0.5f);
            band.rectTransform.anchorMax = new Vector2(1, 0.5f);
            band.rectTransform.sizeDelta = new Vector2(0, 230);
            foreach (float y in new[] { 1f, 0f })
            {
                var line = UIFactory.Image(band.transform, y > 0 ? "TopLine" : "BottomLine", null, UITheme.Danger, new Vector2(0.5f, y), Vector2.zero, new Vector2(0, 2));
                line.rectTransform.anchorMin = new Vector2(0, y);
                line.rectTransform.anchorMax = new Vector2(1, y);
            }

            var heading = new GameObject("Heading", typeof(RectTransform), typeof(CanvasGroup));
            heading.transform.SetParent(v, false);
            UIFactory.Place((RectTransform)heading.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 190), new Vector2(1400, 230));
            var h = heading.transform;
            var kicker = UIFactory.Caps(h, "Kicker", string.IsNullOrEmpty(code) ? "Intento fallido" : $"Sección {code}  /  Intento fallido", UITheme.Danger);
            kicker.alignment = TextAlignmentOptions.Midline;
            UIFactory.Place(kicker.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -26), new Vector2(1400, 30));
            defeatTitle = UIFactory.Title(h, "Title", "HAS CAÍDO", 72, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 4), new Vector2(1400, 92), UITheme.Text);
            defeatTitle.characterSpacing = 4f;
            defeatTitle.textWrappingMode = TextWrappingModes.NoWrap;
            defeatDetail = UIFactory.Label(h, "Subtitle", "", UITheme.SizeBody, TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 34), new Vector2(1400, 40), UITheme.TextSecondary);

            var panel = UIFactory.Panel(v, "Options", new Vector2(0.5f, 0.5f), new Vector2(0, 40), 600, "Qué hacer");
            panel.AddComponent<CanvasGroup>();
            ((RectTransform)panel.transform).pivot = new Vector2(0.5f, 1f);
            var p = panel.transform;
            retryButton = UIFactory.Button(p, "Reintentar", RetryFromCheckpoint, 80, "Desde el último punto de control");
            UIFactory.Button(p, "Reiniciar sección", Restart, 80, "Empezar desde el principio");
            UIFactory.Button(p, "Menú principal", ExitToMenu, 80, "Las secciones completadas se conservan");

            var hints = UIFactory.Row(v, "Hints", 44, 34, TextAnchor.MiddleCenter);
            hints.GetComponent<LayoutElement>().ignoreLayout = true;
            UIFactory.KeyHint(hints, new[] { "Enter" }, "Elegir opción resaltada", UITheme.SizeCaption);
            UIFactory.KeyHint(hints, new[] { "Retroceso" }, "Reiniciar sección", UITheme.SizeCaption);
            UIFactory.KeyHint(hints, new[] { "Esc" }, "Menú principal", UITheme.SizeCaption);
            UIFactory.Place(hints, new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(1100, 44));
            defeatPanel.SetActive(false);
        }

        void OnDowned(FallRecovery.Cause cause)
        {
            DismissIntro();
            defeatTitle.text = cause == FallRecovery.Cause.Fall ? "HAS CAÍDO" : cause == FallRecovery.Cause.Hazard ? "ZONA PELIGROSA" : "TRAJE DESTRUIDO";
            defeatDetail.text = cause == FallRecovery.Cause.Fall ? "Kai se ha perdido en el vacío."
                : cause == FallRecovery.Cause.Hazard ? "Kai ha tocado una zona peligrosa."
                : "El traje de Kai ha perdido toda su integridad.";
            promptChip.SetActive(false);
            // Un instante para ver qué ha pasado antes de detener el juego.
            defeatDelay = DefeatDelaySeconds;
        }

        /// <summary>Devuelve true mientras la derrota ocupa la pantalla (o está a punto de hacerlo).</summary>
        bool UpdateDefeat()
        {
            if (defeatDelay >= 0f)
            {
                defeatDelay -= Time.unscaledDeltaTime;
                if (defeatDelay < 0f) ShowDefeat();
                return true;
            }
            return DefeatVisible;
        }

        void ShowDefeat()
        {
            if (recovery == null || !recovery.IsDown) return;
            defeatPanel.SetActive(true);
            SetGameplayHints(false);
            Time.timeScale = 0f;
            UIReveal.Play(defeatPanel.transform.Find("Heading").gameObject, 0f, new Vector2(0f, -16f), 0.35f);
            UIReveal.Play(defeatPanel.transform.Find("Options").gameObject, 0.15f, new Vector2(0f, -20f), 0.35f);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        }

        // La pista de pausa y la integridad no se muestran sobre la pantalla de derrota.
        void SetGameplayHints(bool visible)
        {
            pauseTip.SetActive(visible);
            if (integrityBar != null) integrityBar.SetActive(visible);
        }

        /// <summary>Reaparece en el último punto de control y reanuda el juego.</summary>
        public void RetryFromCheckpoint()
        {
            if (recovery == null || !recovery.IsDown) return;
            defeatDelay = -1f;
            defeatPanel.SetActive(false);
            SetGameplayHints(true);
            Time.timeScale = 1f;
            recovery.Respawn();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        // ---------- Victoria ----------

        void BuildVictory(Transform root, string title)
        {
            bool hasNext = levelManager != null && levelManager.HasNext;
            string code = UIFactory.SectionCode(title);
            victoryPanel = new GameObject("Victory", typeof(RectTransform));
            victoryPanel.transform.SetParent(root, false);
            UIFactory.Stretch((RectTransform)victoryPanel.transform);
            var v = victoryPanel.transform;
            var dim = UIFactory.FullScreenImage(v, "Dim", new Color(UITheme.Deep.r, UITheme.Deep.g, UITheme.Deep.b, 0.72f));
            dim.raycastTarget = true; // los clics no atraviesan la pantalla de victoria
            var scan = UIFactory.FullScreenImage(v, "Scanlines", new Color(1, 1, 1, 0.025f), UITheme.Scanlines);
            scan.type = Image.Type.Tiled;

            // Banda horizontal que se despliega desde el centro.
            var band = UIFactory.Image(v, "Band", null, new Color(UITheme.Panel.r, UITheme.Panel.g, UITheme.Panel.b, 0.92f), new Vector2(0.5f, 0.5f), new Vector2(0, 190), new Vector2(0, 230));
            victoryBand = band.rectTransform;
            victoryBand.anchorMin = new Vector2(0, 0.5f);
            victoryBand.anchorMax = new Vector2(1, 0.5f);
            victoryBand.sizeDelta = new Vector2(0, 230);
            foreach (float y in new[] { 1f, 0f })
            {
                var line = UIFactory.Image(band.transform, y > 0 ? "TopLine" : "BottomLine", null, UITheme.Cyan, new Vector2(0.5f, y), Vector2.zero, new Vector2(0, 2));
                line.rectTransform.anchorMin = new Vector2(0, y);
                line.rectTransform.anchorMax = new Vector2(1, y);
            }

            var heading = new GameObject("Heading", typeof(RectTransform), typeof(CanvasGroup));
            heading.transform.SetParent(v, false);
            UIFactory.Place((RectTransform)heading.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 190), new Vector2(1400, 230));
            var h = heading.transform;
            var kicker = UIFactory.Caps(h, "Kicker", string.IsNullOrEmpty(code) ? "Sección completada" : $"Sección {code}  /  Completada", UITheme.Success);
            kicker.alignment = TextAlignmentOptions.Midline;
            UIFactory.Place(kicker.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -26), new Vector2(1400, 30));
            var big = UIFactory.Title(h, "Title", hasNext ? "SECCIÓN SUPERADA" : "SECTOR COMPLETADO", 72, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0, 4), new Vector2(1400, 92), UITheme.Text);
            big.characterSpacing = 4f;
            big.textWrappingMode = TextWrappingModes.NoWrap;
            string detail = hasNext ? "Nueva sección desbloqueada en el menú de secciones." : "Has superado todas las secciones del sector 1.";
            UIFactory.Label(h, "Subtitle", $"{UIFactory.SectionName(title)}  ·  <color=#{ColorUtility.ToHtmlStringRGB(UITheme.Cyan)}>{detail}</color>",
                UITheme.SizeBody, TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 34), new Vector2(1400, 40), UITheme.TextSecondary);

            var panel = UIFactory.Panel(v, "Options", new Vector2(0.5f, 0.5f), new Vector2(0, 40), 600, "Siguiente paso");
            panel.AddComponent<CanvasGroup>();
            ((RectTransform)panel.transform).pivot = new Vector2(0.5f, 1f);
            var p = panel.transform;
            if (hasNext)
            {
                continueButton = UIFactory.Button(p, "Continuar", Continue, 80, "Ir a la siguiente sección");
                UIFactory.Button(p, "Repetir sección", Restart, 80, "Jugarla otra vez desde el principio");
                UIFactory.Button(p, "Menú principal", ExitToMenu, 80, "El progreso ya está guardado");
            }
            else
            {
                continueButton = UIFactory.Button(p, "Menú principal", Continue, 80, "El progreso ya está guardado");
                UIFactory.Button(p, "Repetir sección", Restart, 80, "Jugarla otra vez desde el principio");
            }

            var hints = UIFactory.Row(v, "Hints", 44, 34, TextAnchor.MiddleCenter);
            hints.GetComponent<LayoutElement>().ignoreLayout = true;
            UIFactory.KeyHint(hints, new[] { "Enter" }, "Elegir opción resaltada", UITheme.SizeCaption);
            UIFactory.KeyHint(hints, new[] { "Retroceso" }, "Repetir", UITheme.SizeCaption);
            UIFactory.KeyHint(hints, new[] { "Esc" }, "Menú principal", UITheme.SizeCaption);
            UIFactory.Place(hints, new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(1100, 44));

            victoryPanel.SetActive(false);
        }

        // Los botones de victoria pasan por LevelManager; SceneTransition descarta cualquier
        // segunda carga (por ejemplo, clic y Enter en el mismo instante).
        public void Continue() { if (levelManager != null) levelManager.Continue(); }
        public void Restart() { if (levelManager != null) levelManager.Restart(); }
        public void ExitToMenu() { if (levelManager != null) levelManager.ExitToMenu(); }

        void ShowVictory()
        {
            DismissIntro();
            pauseTip.SetActive(false);
            if (integrityBar != null) integrityBar.SetActive(false);
            victoryPanel.SetActive(true);
            victoryElapsed = 0f;
            victoryBand.localScale = new Vector3(0f, 1f, 1f);
            UIReveal.Play(victoryPanel.transform.Find("Heading").gameObject, 0.15f, new Vector2(0f, -16f), 0.45f);
            UIReveal.Play(victoryPanel.transform.Find("Options").gameObject, 0.35f, new Vector2(0f, -20f), 0.4f);
            // En la victoria el ratón sirve para elegir: se muestra el cursor.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (EventSystem.current != null && continueButton != null) EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
        }

        void UpdateVictory()
        {
            if (victoryElapsed < 0f) return;
            victoryElapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(victoryElapsed / BannerSeconds);
            victoryBand.localScale = new Vector3(1f - Mathf.Pow(1f - t, 3f), 1f, 1f);
        }
    }
}
