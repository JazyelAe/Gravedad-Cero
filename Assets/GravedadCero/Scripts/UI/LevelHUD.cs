using GravedadCero.Interaction;
using GravedadCero.Level;
using UnityEngine;
using UnityEngine.UI;

namespace GravedadCero.UI
{
    /// <summary>
    /// Interfaz mínima del nivel, construida por código para no depender de assets de UI:
    /// objetivo arriba a la izquierda, controles abajo, mira central, indicación contextual
    /// ([Clic] Anclar, [E] Activar) y mensaje de victoria. Solo lee estado de otros
    /// componentes; no cambia nada del juego.
    /// </summary>
    public class LevelHUD : MonoBehaviour
    {
        [SerializeField] LevelManager levelManager;
        [SerializeField] GravitationalTool gravitationalTool;
        [SerializeField] PlayerInteractor interactor;
        const string ControlsText =
            "WASD mover · Espacio saltar · Q/R girar gravedad · F invertir · Clic anclar/soltar · E usar · Retroceso reiniciar · Esc menú";

        static readonly Color Cyan = new Color(0.45f, 0.9f, 1f);
        static readonly Color Amber = new Color(1f, 0.7f, 0.25f);
        static readonly Color Neutral = new Color(1f, 1f, 1f, 0.6f);

        Font font;
        Text crosshair;
        Text prompt;
        GameObject victoryPanel;

        void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var canvasGo = new GameObject("HUD Canvas", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            Transform root = canvasGo.transform;

            string objectiveText = levelManager != null ? $"{levelManager.LevelTitle}\n<b>Objetivo:</b> {levelManager.Objective}" : "";
            if (levelManager != null && !string.IsNullOrEmpty(levelManager.Hint)) objectiveText += $"\n<i>{levelManager.Hint}</i>";
            Label(root, "Objective", objectiveText, 26, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(40, -36), new Vector2(900, 160), Color.white);
            Label(root, "Controls", ControlsText, 20, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(40, 30), new Vector2(1600, 40), Neutral);
            crosshair = Label(root, "Crosshair", "+", 34, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60, 60), Neutral);
            prompt = Label(root, "Prompt", "", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -70), new Vector2(800, 50), Cyan);

            victoryPanel = new GameObject("Victory", typeof(RectTransform), typeof(Image));
            victoryPanel.transform.SetParent(root, false);
            var panelRect = (RectTransform)victoryPanel.transform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
            victoryPanel.GetComponent<Image>().color = new Color(0.02f, 0.04f, 0.07f, 0.75f);
            bool hasNext = levelManager != null && levelManager.HasNext;
            Label(victoryPanel.transform, "Title", hasNext ? "SECCIÓN SUPERADA" : "SECTOR COMPLETADO", 64, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(1400, 90), Cyan);
            Label(victoryPanel.transform, "Subtitle",
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
            if (levelManager != null && levelManager.IsCompleted)
            {
                prompt.text = "";
                crosshair.text = "";
                return;
            }

            AnchorableBody target = gravitationalTool != null ? gravitationalTool.Target : null;
            if (target != null)
            {
                crosshair.color = Cyan;
                prompt.color = Cyan;
                prompt.text = target.IsAnchored ? "[Clic] Soltar caja" : "[Clic] Anclar caja";
            }
            else if (interactor != null && interactor.Current != null)
            {
                crosshair.color = Neutral;
                prompt.color = Amber;
                prompt.text = $"[E] {interactor.Current.Prompt}";
            }
            else
            {
                crosshair.color = Neutral;
                prompt.text = "";
            }
        }

        void ShowVictory()
        {
            victoryPanel.SetActive(true);
        }

        Text Label(Transform parent, string name, string text, int size, TextAnchor anchor, Vector2 pivotAnchor,
            Vector2 position, Vector2 boxSize, Color color)
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
            label.alignment = anchor;
            label.color = color;
            label.supportRichText = true;
            label.text = text;
            return label;
        }
    }
}
