using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GravedadCero.Gravity;
using GravedadCero.Level;
using GravedadCero.Player;
using GravedadCero.UI;
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
    /// Capturas para el README en Play Mode, sobre la sección 1-1: una vista general del escenario
    /// completo (sin techos, desde arriba) y varias de la cámara en tercera persona siguiendo a Kai,
    /// en el suelo y sobre una pared con la gravedad girada.
    ///
    /// Uso en el editor: menú Gravedad Cero → Capturas del README.
    /// Línea de comandos (sin -nographics):
    ///   Unity.exe -batchmode -projectPath . -executeMethod GravedadCero.Tests.ReadmeScreenshotRunner.RunFromCommandLine -logFile log.txt
    /// Resultado: Documentation/Images/*.png.
    /// </summary>
    [InitializeOnLoad]
    public static class ReadmeScreenshotRunner
    {
        const string RunningKey = "GC_README_SHOTS_RUNNING";
        const string BatchKey = "GC_README_SHOTS_BATCH";
        const string OutDir = "Documentation/Images";
        const string Scene = "Level_01_DockingBay";
        const int Width = 1920, Height = 1080;

        static readonly Stack<IEnumerator> routines = new Stack<IEnumerator>();
        static float waitUntil;
        static bool started;

        static ReadmeScreenshotRunner()
        {
            if (!SessionState.GetBool(RunningKey, false)) return;
            EditorApplication.update += Tick;
        }

        [MenuItem("Gravedad Cero/Capturas del README")]
        public static void RunFromMenu() => Begin(false);

        public static void RunFromCommandLine() => Begin(true);

        static void Begin(bool batch)
        {
            if (EditorApplication.isPlaying) return;
            if (!batch && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            SessionState.SetBool(RunningKey, true);
            SessionState.SetBool(BatchKey, batch);
            EditorSceneManager.OpenScene($"Assets/GravedadCero/Scenes/{Scene}.unity");
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
            var hud = Object.FindAnyObjectByType<LevelHUD>();
            hud.DismissIntro();
            var motor = GameObject.FindWithTag("Player").GetComponent<PlayerMotor>();
            var gravity = Object.FindAnyObjectByType<GravityController>();
            yield return 1f;

            // Vista general: cámara temporal sobre el nivel, sin techos ni interfaz.
            CaptureOverview("escenario_completo");

            // Cámara de seguimiento: Kai camina hacia el norte con la cámara detrás.
            yield return Walk(motor, Vector3.forward, 1.6f);
            yield return 0.4f;
            CaptureGame("camara_seguimiento");

            // Gravedad hacia la pared oeste: la cámara gira con el "arriba" de Kai.
            var reference = new GameObject("GravityReference").transform;
            reference.rotation = Quaternion.FromToRotation(Vector3.right, Vector3.left);
            gravity.RotateRight(reference);
            Object.Destroy(reference.gameObject);
            yield return 2.2f;
            yield return Walk(motor, Vector3.forward, 1.2f);
            yield return 0.4f;
            CaptureGame("camara_seguimiento_pared");
            gravity.ResetGravity();
        }

        static IEnumerator Walk(PlayerMotor motor, Vector3 direction, float seconds)
        {
            float start = Time.time;
            while (Time.time - start < seconds)
            {
                motor.SetMoveInput(direction);
                yield return null;
            }
            motor.SetMoveInput(Vector3.zero);
        }

        static void CaptureOverview(string name)
        {
            var renderers = Object.FindObjectsByType<Renderer>().Where(r => r.enabled && r.gameObject.activeInHierarchy).ToArray();
            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);

            // Techos: piezas planas en lo alto sin nada encima. Las pasarelas tienen techo y se quedan.
            bool Flat(Bounds b) => b.center.y > 3f && b.size.y < Mathf.Min(b.size.x, b.size.z) * 0.5f;
            bool OverlapXZ(Bounds a, Bounds b) =>
                a.min.x < b.max.x - 0.1f && b.min.x < a.max.x - 0.1f && a.min.z < b.max.z - 0.1f && b.min.z < a.max.z - 0.1f;
            var flats = renderers.Where(r => Flat(r.bounds)).ToList();
            var hidden = flats.Where(r => !flats.Any(o => o != r && o.bounds.min.y > r.bounds.max.y && OverlapXZ(o.bounds, r.bounds))).ToList();
            foreach (var r in hidden) r.enabled = false;
            var canvases = Object.FindObjectsByType<Canvas>().Where(c => c.isRootCanvas && c.enabled).ToList();
            foreach (var c in canvases) c.enabled = false;

            var go = new GameObject("OverviewCamera");
            var cam = go.AddComponent<Camera>();
            cam.CopyFrom(Camera.main);
            cam.fieldOfView = 35f;
            cam.farClipPlane = 500f;
            Quaternion rotation = Quaternion.Euler(58f, 32f, 0f);
            float radius = bounds.extents.magnitude;
            float distance = radius / Mathf.Sin(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * 0.82f;
            go.transform.SetPositionAndRotation(bounds.center - rotation * Vector3.forward * distance, rotation);

            Render(cam, name);
            Object.Destroy(go);
            foreach (var r in hidden) r.enabled = true;
            foreach (var c in canvases) c.enabled = true;
        }

        /// <summary>Captura la cámara del juego con el HUD, como se ve al jugar.</summary>
        static void CaptureGame(string name)
        {
            var cam = Camera.main;
            var canvases = Object.FindObjectsByType<Canvas>().Where(c => c.isRootCanvas && c.isActiveAndEnabled).ToArray();
            var modes = canvases.Select(c => c.renderMode).ToArray();
            foreach (var c in canvases)
            {
                c.renderMode = RenderMode.ScreenSpaceCamera;
                c.worldCamera = cam;
                c.planeDistance = 0.5f - Mathf.Clamp(c.sortingOrder, 0, 1000) * 0.0001f;
            }
            Canvas.ForceUpdateCanvases();
            foreach (var c in canvases) LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)c.transform);
            Canvas.ForceUpdateCanvases();
            Render(cam, name);
            for (int i = 0; i < canvases.Length; i++) canvases[i].renderMode = modes[i];
        }

        static void Render(Camera cam, string name)
        {
            var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            var previous = cam.targetTexture;
            cam.targetTexture = rt;
            RenderPipeline.SubmitRenderRequest(cam, new RenderPipeline.StandardRequest { destination = rt });
            RenderTexture.active = rt;
            var tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            cam.targetTexture = previous;
            Directory.CreateDirectory(OutDir);
            File.WriteAllBytes($"{OutDir}/{name}.png", tex.EncodeToPNG());
            Object.Destroy(tex);
            rt.Release();
        }

        static void Finish()
        {
            EditorApplication.update -= Tick;
            Debug.Log("Capturas del README terminadas: " + Path.GetFullPath(OutDir));
            bool batch = SessionState.GetBool(BatchKey, false);
            SessionState.EraseBool(RunningKey);
            if (batch) EditorApplication.Exit(0);
            else EditorApplication.ExitPlaymode();
        }
    }
}
