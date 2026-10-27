using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GravedadCero.Level;
using GravedadCero.Save;
using GravedadCero.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GravedadCero.Tests
{
    /// <summary>
    /// Capturas de todas las interfaces en Play Mode a varias resoluciones, con una revisión
    /// automática de textos que no caben en su recuadro o quedan fuera de pantalla.
    ///
    /// Uso en el editor: menú Gravedad Cero → Capturas de interfaz.
    /// Línea de comandos (sin -nographics, necesita gráficos):
    ///   Unity.exe -batchmode -projectPath . -executeMethod GravedadCero.Tests.UIScreenshotRunner.RunFromCommandLine -logFile log.txt
    /// Resultado: Logs/UIScreens/&lt;resolución&gt;/&lt;pantalla&gt;.png y Logs/UIScreens/report.txt.
    /// Restaura el progreso y la skin guardados al terminar.
    /// </summary>
    [InitializeOnLoad]
    public static class UIScreenshotRunner
    {
        const string RunningKey = "GC_UISHOTS_RUNNING";
        const string BatchKey = "GC_UISHOTS_BATCH";
        const string PrevUnlockKey = "GC_UISHOTS_PREV_UNLOCK";
        const string PrevSkinKey = "GC_UISHOTS_PREV_SKIN";
        const string OutDir = "Logs/UIScreens";

        static readonly Vector2Int[] Resolutions =
        {
            new Vector2Int(1280, 720), new Vector2Int(1920, 1080), new Vector2Int(2560, 1080), new Vector2Int(1024, 768),
        };

        static readonly Stack<IEnumerator> routines = new Stack<IEnumerator>();
        static readonly List<string> report = new List<string>();
        static readonly List<string> logProblems = new List<string>();
        static int problems;
        static float waitUntil;
        static bool started;

        static UIScreenshotRunner()
        {
            if (!SessionState.GetBool(RunningKey, false)) return;
            EditorApplication.update += Tick;
            Application.logMessageReceived += (m, s, t) =>
            {
                if (t == LogType.Error || t == LogType.Exception || t == LogType.Assert || t == LogType.Warning) logProblems.Add($"{t}: {m}");
            };
        }

        [MenuItem("Gravedad Cero/Capturas de interfaz")]
        public static void RunFromMenu() => Begin(false);

        public static void RunFromCommandLine() => Begin(true);

        static void Begin(bool batch)
        {
            if (EditorApplication.isPlaying) return;
            if (!batch && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            SessionState.SetBool(RunningKey, true);
            SessionState.SetBool(BatchKey, batch);
            SessionState.SetInt(PrevUnlockKey, PlayerPrefs.GetInt("gravedadcero.unlockedSection", 0));
            SessionState.SetInt(PrevSkinKey, PlayerPrefs.GetInt("gravedadcero.skin", 0));
            if (Directory.Exists(OutDir)) Directory.Delete(OutDir, true);
            EditorSceneManager.OpenScene("Assets/GravedadCero/Scenes/MainMenu.unity");
            EditorApplication.EnterPlaymode();
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            if (!started) { started = true; routines.Push(RunAll()); }
            if (Time.realtimeSinceStartup < waitUntil) return;
            while (routines.Count > 0)
            {
                var top = routines.Peek();
                if (!top.MoveNext()) { routines.Pop(); continue; }
                if (top.Current is IEnumerator nested) { routines.Push(nested); continue; }
                if (top.Current is float seconds) waitUntil = Time.realtimeSinceStartup + seconds;
                return;
            }
            Finish();
        }

        static IEnumerator RunAll()
        {
            yield return 2f;
            // Estado de ejemplo: la 1-1 completada, la 1-2 disponible y la 1-3 bloqueada.
            GameProgress.UnlockSection(1);
            var menu = Object.FindAnyObjectByType<MainMenuUI>();
            foreach (string panel in new[] { "Main", "Sections", "Character", "ConfirmNewGame" })
            {
                menu.ShowPanel(panel);
                yield return panel == "Character" ? 1.6f : 0.8f;
                CaptureAll("menu_" + panel);
            }
            foreach (var extra in ExtraShots()) yield return extra;
        }

        /// <summary>Pantallas de las secciones: introducción, HUD, pausa (con sus paneles) y victoria.</summary>
        static IEnumerable<IEnumerator> ExtraShots()
        {
            yield return LevelShots("Level_01_DockingBay", "1-1", true);
            yield return LevelShots("Level_01_3_CargoLock", "1-3", false);
        }

        static IEnumerator LevelShots(string scene, string code, bool full)
        {
            EditorSceneManager.LoadSceneInPlayMode($"Assets/GravedadCero/Scenes/{scene}.unity", new LoadSceneParameters(LoadSceneMode.Single));
            float start = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != scene && Time.realtimeSinceStartup - start < 15f) yield return null;
            yield return 1.5f;
            CaptureAll($"level{code}_intro");
            var hud = Object.FindAnyObjectByType<LevelHUD>();
            var level = Object.FindAnyObjectByType<LevelManager>();
            if (full)
            {
                hud.DismissIntro();
                yield return 1f;
                CaptureAll($"level{code}_hud");

                var pause = Object.FindAnyObjectByType<PauseController>();
                var menu = Object.FindAnyObjectByType<PauseMenuUI>();
                pause.SetPaused(true);
                foreach (string panel in new[] { "Main", "Controls", "Settings" })
                {
                    menu.ShowPanel(panel);
                    yield return 0.6f;
                    CaptureAll($"level{code}_pause_{panel}");
                }
                menu.ShowPanel("Main");
                menu.RequestExitToMenu();
                yield return 0.6f;
                CaptureAll($"level{code}_pause_Confirm");
                menu.CancelPending();
                pause.SetPaused(false);
            }
            if (full)
            {
                var recovery = GameObject.FindWithTag("Player").GetComponent<GravedadCero.Player.FallRecovery>();
                recovery.Fail(GravedadCero.Player.FallRecovery.Cause.Fall);
                yield return 1.4f;
                CaptureAll($"level{code}_defeat");
                hud.RetryFromCheckpoint();
                yield return 1f;
            }
            level.Complete();
            yield return 1.4f;
            CaptureAll($"level{code}_victory");
        }

        // ---------- Captura y revisión ----------

        public static void CaptureAll(string name)
        {
            foreach (var res in Resolutions) Capture(name, res.x, res.y);
        }

        static void Capture(string name, int width, int height)
        {
            var cam = Camera.main;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var canvases = Object.FindObjectsByType<Canvas>().Where(c => c.isRootCanvas && c.isActiveAndEnabled).ToArray();
            var modes = canvases.Select(c => c.renderMode).ToArray();
            cam.targetTexture = rt;
            foreach (var c in canvases)
            {
                c.renderMode = RenderMode.ScreenSpaceCamera;
                c.worldCamera = cam;
                c.planeDistance = 0.5f - Mathf.Clamp(c.sortingOrder, 0, 1000) * 0.0001f;
            }
            Canvas.ForceUpdateCanvases();
            foreach (var c in canvases) LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)c.transform);
            Canvas.ForceUpdateCanvases();

            string res = $"{width}x{height}";
            foreach (var c in canvases) Inspect(c, cam, name, res);

            RenderPipeline.SubmitRenderRequest(cam, new RenderPipeline.StandardRequest { destination = rt });
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            cam.targetTexture = null;
            for (int i = 0; i < canvases.Length; i++) canvases[i].renderMode = modes[i];
            Directory.CreateDirectory($"{OutDir}/{res}");
            File.WriteAllBytes($"{OutDir}/{res}/{name}.png", tex.EncodeToPNG());
            Object.Destroy(tex);
            rt.Release();
        }

        static void Inspect(Canvas canvas, Camera cam, string shot, string res)
        {
            var corners = new Vector3[4];
            foreach (var text in canvas.GetComponentsInChildren<TMP_Text>())
            {
                if (!text.isActiveAndEnabled || string.IsNullOrWhiteSpace(text.text)) continue;
                if (text.GetComponentsInParent<CanvasGroup>().Any(g => g.alpha < 0.05f)) continue;
                text.ForceMeshUpdate();
                Rect rect = text.rectTransform.rect;
                bool noWrap = text.textWrappingMode == TextWrappingModes.NoWrap;
                Vector2 preferred = text.GetPreferredValues(text.text, noWrap ? 0f : rect.width, 0f);
                string path = PathOf(text.transform);
                if (noWrap && preferred.x > rect.width + 2f) Problem($"[{res}] {shot}: texto más ancho que su recuadro ({preferred.x:F0} > {rect.width:F0}): {path} \"{Short(text.text)}\"");
                if (preferred.y > rect.height + 4f) Problem($"[{res}] {shot}: texto más alto que su recuadro ({preferred.y:F0} > {rect.height:F0}): {path} \"{Short(text.text)}\"");
                text.rectTransform.GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    Vector3 vp = cam.WorldToViewportPoint(corner);
                    if (vp.x < -0.001f || vp.x > 1.001f || vp.y < -0.001f || vp.y > 1.001f)
                    {
                        Problem($"[{res}] {shot}: texto fuera de pantalla: {path} \"{Short(text.text)}\"");
                        break;
                    }
                }
            }
        }

        static void Problem(string line)
        {
            problems++;
            report.Add("REVISAR " + line);
        }

        static string Short(string s)
        {
            s = s.Replace("\n", " ");
            return s.Length > 50 ? s.Substring(0, 50) + "…" : s;
        }

        static string PathOf(Transform t)
        {
            var parts = new List<string>();
            for (int i = 0; t != null && i < 4; i++, t = t.parent) parts.Insert(0, t.name);
            return string.Join("/", parts);
        }

        static void Finish()
        {
            EditorApplication.update -= Tick;
            PlayerPrefs.SetInt("gravedadcero.unlockedSection", SessionState.GetInt(PrevUnlockKey, 0));
            PlayerPrefs.SetInt("gravedadcero.skin", SessionState.GetInt(PrevSkinKey, 0));
            PlayerPrefs.Save();
            foreach (var p in logProblems) report.Add("CONSOLA " + p);
            report.Add($"\nRESULTADO: {problems} textos a revisar, {logProblems.Count} mensajes de consola ({DateTime.Now:yyyy-MM-dd HH:mm})");
            Directory.CreateDirectory(OutDir);
            File.WriteAllText($"{OutDir}/report.txt", string.Join("\n", report));
            Debug.Log("Capturas de interfaz terminadas: " + Path.GetFullPath(OutDir));
            bool batch = SessionState.GetBool(BatchKey, false);
            SessionState.EraseBool(RunningKey);
            if (batch) EditorApplication.Exit(problems == 0 && logProblems.Count == 0 ? 0 : 1);
            else EditorApplication.ExitPlaymode();
        }
    }
}
