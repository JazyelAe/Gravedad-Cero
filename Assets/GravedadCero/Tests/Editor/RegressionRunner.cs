using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GravedadCero.Cameras;
using GravedadCero.Gravity;
using GravedadCero.Interaction;
using GravedadCero.Level;
using GravedadCero.Player;
using GravedadCero.Puzzles;
using GravedadCero.Save;
using GravedadCero.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GravedadCero.Tests
{
    /// <summary>
    /// Pruebas de regresión en Play Mode del juego completo: menú, secciones 1-1, 1-2 y 1-3,
    /// y Test_Gravity. Simulan al jugador dando órdenes directas a los componentes (no
    /// pulsan teclas reales) y comprueban el resultado de la física y los puzles.
    ///
    /// Uso en el editor: menú Gravedad Cero → Ejecutar pruebas de regresión.
    /// Uso por línea de comandos:
    ///   Unity.exe -batchmode -projectPath . -executeMethod GravedadCero.Tests.RegressionRunner.RunFromCommandLine -logFile log.txt
    /// El informe se guarda en Logs/RegressionReport.txt. El progreso guardado del jugador
    /// (PlayerPrefs) se restaura al terminar.
    /// </summary>
    [InitializeOnLoad]
    public static class RegressionRunner
    {
        const string RunningKey = "GC_REGRESSION_RUNNING";
        const string BatchKey = "GC_REGRESSION_BATCH";
        const string PrevUnlockKey = "GC_REGRESSION_PREV_UNLOCK";
        const string PrevSkinKey = "GC_REGRESSION_PREV_SKIN";
        const string UnlockPref = "gravedadcero.unlockedSection";
        const string SkinPref = "gravedadcero.skin";
        const string ScenesDir = "Assets/GravedadCero/Scenes/";
        const float GlobalTimeout = 600f;

        static readonly Stack<IEnumerator> routines = new Stack<IEnumerator>();
        static readonly List<string> lines = new List<string>();
        static readonly List<string> logProblems = new List<string>();
        static string suite = "";
        static int failures;
        static float waitUntil;
        static bool started;

        static RegressionRunner()
        {
            if (!SessionState.GetBool(RunningKey, false)) return;
            EditorApplication.update += Tick;
            Application.logMessageReceived += OnLog;
        }

        [MenuItem("Gravedad Cero/Ejecutar pruebas de regresión")]
        public static void RunFromMenu() => Begin(batch: false);

        public static void RunFromCommandLine() => Begin(batch: true);

        static void Begin(bool batch)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Sal de Play Mode antes de ejecutar las pruebas de regresión.");
                return;
            }
            if (!batch && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            SessionState.SetBool(RunningKey, true);
            SessionState.SetBool(BatchKey, batch);
            SessionState.SetInt(PrevUnlockKey, PlayerPrefs.GetInt(UnlockPref, 0));
            SessionState.SetInt(PrevSkinKey, PlayerPrefs.GetInt(SkinPref, 0));
            EditorSceneManager.OpenScene(ScenesDir + "MainMenu.unity");
            EditorApplication.EnterPlaymode();
        }

        static void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert || type == LogType.Warning)
                logProblems.Add($"[{suite}] {type}: {message}");
        }

        // ---------- Pequeño planificador de corrutinas en el editor ----------
        // yield return null    -> siguiente frame
        // yield return <float> -> esperar esos segundos de juego
        // yield return <IEnumerator> -> ejecutar una sub-rutina

        static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            if (!started)
            {
                started = true;
                routines.Push(RunAll());
            }
            if (Time.realtimeSinceStartup > GlobalTimeout) { Check("tiempo máximo global", false); Finish(); return; }
            if (Time.time < waitUntil) return;

            while (routines.Count > 0)
            {
                IEnumerator top = routines.Peek();
                if (!top.MoveNext()) { routines.Pop(); continue; }
                if (top.Current is IEnumerator nested) { routines.Push(nested); continue; }
                if (top.Current is float seconds) waitUntil = Time.time + seconds;
                return;
            }
            Finish();
        }

        static IEnumerator RunAll()
        {
            yield return MenuSuite();
            yield return DockingBaySuite();
            yield return MaintenanceShaftSuite();
            yield return CargoLockSuite();
            yield return TestGravitySuite();
        }

        static void Check(string name, bool ok, string detail = "")
        {
            if (!ok) failures++;
            lines.Add($"{(ok ? "PASS" : "FAIL")} [{suite}] {name} {detail}".TrimEnd());
        }

        static void Finish()
        {
            Check("consola sin errores ni advertencias", logProblems.Count == 0, string.Join(" | ", logProblems));
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog;

            PlayerPrefs.SetInt(UnlockPref, SessionState.GetInt(PrevUnlockKey, 0));
            PlayerPrefs.SetInt(SkinPref, SessionState.GetInt(PrevSkinKey, 0));
            PlayerPrefs.Save();

            int passed = 0;
            foreach (string l in lines) if (l.StartsWith("PASS")) passed++;
            string report = string.Join("\n", lines) + $"\n\nRESULTADO: {passed}/{lines.Count} superadas, {failures} fallidas ({DateTime.Now:yyyy-MM-dd HH:mm})";
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/RegressionReport.txt", report);
            if (failures == 0) Debug.Log("Pruebas de regresión: TODAS SUPERADAS\n" + report);
            else Debug.LogError($"Pruebas de regresión: {failures} FALLIDAS\n" + report);

            bool batch = SessionState.GetBool(BatchKey, false);
            SessionState.EraseBool(RunningKey);
            if (batch) EditorApplication.Exit(failures == 0 ? 0 : 1);
            else EditorApplication.ExitPlaymode();
        }

        // ---------- Utilidades de escena ----------

        static IEnumerator LoadScene(string name)
        {
            EditorSceneManager.LoadSceneInPlayMode(ScenesDir + name + ".unity", new LoadSceneParameters(LoadSceneMode.Single));
            float start = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != name)
            {
                if (Time.realtimeSinceStartup - start > 15f) { Check($"cargar {name}", false); yield break; }
                yield return null;
            }
            yield return 0.5f;
        }

        static IEnumerator WaitForScene(string name, float timeout, Action<bool> result)
        {
            float start = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != name)
            {
                if (Time.realtimeSinceStartup - start > timeout) { result(false); yield break; }
                yield return null;
            }
            result(true);
        }

        /// <summary>Referencias del jugador en la escena actual. Desactiva su lectura de teclado.</summary>
        sealed class Player
        {
            public GameObject go;
            public Rigidbody body;
            public PlayerMotor motor;
            public GravitationalTool tool;
            public PlayerInteractor interactor;
            public Animator animator;
            public GravityController gravity;
            public LevelManager level;
            public GravityCamera gravityCamera;
            public Camera cam;

            public static Player Find()
            {
                var go = GameObject.FindWithTag("Player");
                if (go == null) return null;
                var p = new Player
                {
                    go = go,
                    body = go.GetComponent<Rigidbody>(),
                    motor = go.GetComponent<PlayerMotor>(),
                    tool = go.GetComponent<GravitationalTool>(),
                    interactor = go.GetComponent<PlayerInteractor>(),
                    animator = go.GetComponentInChildren<Animator>(),
                    gravity = Object.FindAnyObjectByType<GravityController>(),
                    level = Object.FindAnyObjectByType<LevelManager>(),
                    gravityCamera = Object.FindAnyObjectByType<GravityCamera>(),
                    cam = Camera.main,
                };
                go.GetComponent<PlayerInputHandler>().enabled = false;
                return p;
            }

            public Vector3 Position => go.transform.position;
            public Vector3 Up => go.transform.up;

            public void Teleport(Vector3 position, Quaternion rotation)
            {
                body.linearVelocity = Vector3.zero;
                body.position = position;
                body.rotation = rotation;
                go.transform.SetPositionAndRotation(position, rotation);
            }

            /// <summary>Coloca la cámara detrás de Kai mirando a la caja, como lo haría el jugador.</summary>
            public void AimAt(AnchorableBody box, bool away = false)
            {
                gravityCamera.enabled = false;
                Vector3 origin = Position + Up * 1.2f;
                Vector3 boxCenter = box.transform.TransformPoint(new Vector3(0, 0.72f, 0));
                Vector3 camPos = origin + Up * 0.6f + (origin - boxCenter).normalized * 3f;
                cam.transform.position = camPos;
                cam.transform.rotation = Quaternion.LookRotation(away ? camPos - boxCenter : boxCenter - camPos, Up);
            }

            public IEnumerator WalkUntilCompleted(Vector3 direction, float timeout, string checkName)
            {
                float start = Time.time;
                while (!level.IsCompleted && Time.time - start < timeout)
                {
                    motor.SetMoveInput(direction);
                    yield return null;
                }
                motor.SetMoveInput(Vector3.zero);
                Check(checkName, level.IsCompleted, $"t={Time.time - start:F1}s pos={Position}");
            }
        }

        // =====================================================================
        // Menú principal
        // =====================================================================

        static IEnumerator MenuSuite()
        {
            suite = "Menú";
            float start = Time.realtimeSinceStartup;
            MainMenuUI menu = null;
            while ((menu = Object.FindAnyObjectByType<MainMenuUI>()) == null)
            {
                if (Time.realtimeSinceStartup - start > 10f) { Check("menú presente", false); yield break; }
                yield return null;
            }
            yield return 0.5f;
            Check("menú presente con EventSystem", UnityEngine.EventSystems.EventSystem.current != null);
            Check("tres secciones configuradas", menu.Sections.Count == 3);

            GameProgress.ResetProgress();
            menu.ShowPanel("Main");
            menu.LoadSection(1);
            yield return 0.5f;
            Check("una sección bloqueada no se carga", SceneManager.GetActiveScene().name == "MainMenu");

            var preview = Object.FindAnyObjectByType<KaiSkinApplier>();
            menu.ChangeSkin(1);
            Check("Siguiente cambia la skin y la guarda", preview.CurrentIndex == 1 && GameProgress.SelectedSkin == 1,
                $"skin={preview.Catalog.Get(preview.CurrentIndex).displayName}");
            menu.ChangeSkin(-2);
            Check("Anterior da la vuelta al catálogo", preview.CurrentIndex == preview.Catalog.Count - 1);
            menu.ChangeSkin(2);
            menu.ShowPanel("Character");
            menu.ShowPanel("Sections");
            menu.ShowPanel("Main");

            menu.NewGame();
            bool loaded = false;
            yield return WaitForScene("Level_01_DockingBay", 10f, r => loaded = r);
            Check("Nueva partida carga la sección 1-1", loaded);
            if (!loaded) yield break;
            yield return 0.5f;
            var applier = GameObject.FindWithTag("Player").GetComponentInChildren<KaiSkinApplier>();
            var renderer = applier.GetComponentInChildren<SkinnedMeshRenderer>();
            Check("la skin elegida en el menú se aplica en la sección", applier.CurrentIndex == 1 && renderer.sharedMaterial == applier.Catalog.Get(1).material,
                $"mat={renderer.sharedMaterial.name}");

            Object.FindAnyObjectByType<LevelManager>().ExitToMenu();
            yield return WaitForScene("MainMenu", 10f, r => loaded = r);
            Check("volver al menú carga MainMenu", loaded);
            yield return 0.5f;
            Check("tras volver hay un solo menú y un solo EventSystem",
                Object.FindObjectsByType<MainMenuUI>().Length == 1 && Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>().Length == 1);
        }

        // =====================================================================
        // Sección 1-1: Muelle de acoplamiento
        // =====================================================================

        static IEnumerator DockingBaySuite()
        {
            suite = "1-1";
            yield return LoadScene("Level_01_DockingBay");
            var p = Player.Find();
            var box = Object.FindAnyObjectByType<AnchorableBody>();
            var boxBody = box.GetComponent<Rigidbody>();
            var plate = Object.FindAnyObjectByType<PressurePlate>();
            var door = Object.FindAnyObjectByType<DoorController>();
            Check("referencias de escena", p != null && p.tool && box && plate && door && p.gravity && p.level && p.cam);
            Check("Kai con Animator humanoide y PlayerAnimator", p.animator && p.animator.isHuman && p.go.GetComponent<PlayerAnimator>());
            yield return 1.5f;

            Check("caja en reposo sobre la plataforma", box.transform.position.y > 2.1f && box.transform.position.y < 2.6f, $"pos={box.transform.position}");
            Check("placa libre y compuerta cerrada al inicio", !plate.IsPressed && !door.IsActivated);
            Check("Kai en el suelo y Animator en reposo", p.motor.IsGrounded && p.animator.GetFloat("Speed") < 0.2f);
            Check("R gira la gravedad a +X", p.gravity.RotateRight(p.go.transform) && p.gravity.Direction == Vector3.right);
            yield return 0.25f;
            Check("Animator pasa a salto/caída en el aire", !p.animator.GetBool("Grounded"));
            yield return 2.25f;

            Check("la caja suelta sigue la gravedad global", box.transform.position.x > 3.8f, $"pos={box.transform.position}");
            Check("la caja presiona la placa y la compuerta se abre", plate.IsPressed && door.IsActivated);
            Check("Kai se alinea con la pared este", Vector3.Dot(p.Up, Vector3.left) > 0.95f, $"up={p.Up}");

            p.Teleport(new Vector3(5.3f, 3f, 12.5f), Quaternion.LookRotation(Vector3.forward, Vector3.left));
            yield return 0.6f;
            p.AimAt(box, away: true);
            Check("la mira no selecciona nada lejos de la caja", p.tool.FindTarget() == null);
            p.AimAt(box);
            Check("la mira selecciona la caja", p.tool.FindTarget() == box);
            Check("clic ancla la caja", p.tool.ToggleAnchor() && box.IsAnchored && boxBody.isKinematic);
            Vector3 anchored = box.transform.position;
            p.gravityCamera.enabled = true;
            p.gravity.ResetGravity();
            yield return 2.5f;

            Check("la caja anclada no se mueve al cambiar la gravedad", Vector3.Distance(box.transform.position, anchored) < 0.05f);
            Check("placa presionada y compuerta abierta con la caja anclada", plate.IsPressed && door.IsActivated);
            Check("Kai vuelve al suelo", p.Position.y < 1f && Vector3.Dot(p.Up, Vector3.up) > 0.95f, $"pos={p.Position}");

            p.Teleport(new Vector3(0f, 0.05f, 18f), Quaternion.identity);
            float maxSpeed = 0f;
            float walkStart = Time.time;
            while (!p.level.IsCompleted && Time.time - walkStart < 8f)
            {
                p.motor.SetMoveInput(Vector3.forward);
                maxSpeed = Mathf.Max(maxSpeed, p.animator.GetFloat("Speed"));
                yield return null;
            }
            p.motor.SetMoveInput(Vector3.zero);
            Check("Kai cruza la compuerta y completa 1-1", p.level.IsCompleted, $"pos={p.Position}");
            Check("el Animator recibe la velocidad de carrera", maxSpeed > 4f, $"max={maxSpeed:F2}");
            Check("completar 1-1 desbloquea 1-2", GameProgress.UnlockedSection >= 1);

            box.Release();
            yield return 2.5f;
            Check("al soltarla, la caja cae y la compuerta se cierra", box.transform.position.y < 1.8f && !plate.IsPressed && !door.IsActivated);
        }

        // =====================================================================
        // Sección 1-2: Pozo de mantenimiento
        // =====================================================================

        static IEnumerator MaintenanceShaftSuite()
        {
            suite = "1-2";
            yield return LoadScene("Level_01_2_MaintenanceShaft");
            var p = Player.Find();
            var door = Object.FindAnyObjectByType<DoorController>();
            yield return 1.5f;
            Check("Kai en el suelo y compuerta cerrada al inicio", p.motor.IsGrounded && !door.IsActivated);

            p.Teleport(new Vector3(0, 0.05f, 20f), Quaternion.identity);
            float t = Time.time;
            while (!(p.Position.z < 6f && p.Position.y > -1f && Time.time - t > 0.3f) && Time.time - t < 5f) yield return null;
            Check("caer al fondo rojo devuelve a Kai al inicio", p.Position.z < 6f && p.Position.y > -1f, $"t={Time.time - t:F1}s");
            yield return 1f;

            Check("R lleva la gravedad a la pared este", p.gravity.RotateRight(p.go.transform) && p.gravity.Direction == Vector3.right);
            yield return 2.5f;
            Check("Kai de pie en la pared este", Vector3.Dot(p.Up, Vector3.left) > 0.95f && p.motor.IsGrounded, $"pos={p.Position}");

            t = Time.time;
            while (p.Position.z < 30.5f && Time.time - t < 10f) { p.motor.SetMoveInput(Vector3.forward); yield return null; }
            p.motor.SetMoveInput(Vector3.zero);
            Check("Kai cruza el pozo caminando por la pared", p.Position.z >= 30.5f, $"t={Time.time - t:F1}s pos={p.Position}");
            yield return 0.8f;

            Check("Q devuelve la gravedad al suelo", p.gravity.RotateLeft(p.go.transform) && p.gravity.Direction == Vector3.down);
            yield return 2.5f;
            Check("Kai aterriza al otro lado del pozo", p.motor.IsGrounded && p.Position.z > 26f && p.Position.y > -0.5f, $"pos={p.Position}");

            Check("F lleva la gravedad al techo", p.gravity.Flip() && p.gravity.Direction == Vector3.up);
            yield return 2.5f;
            Check("Kai de pie en el techo", Vector3.Dot(p.Up, Vector3.down) > 0.95f && p.Position.y > 7f, $"pos={p.Position}");

            p.Teleport(new Vector3(0, 7.95f, 34.6f), Quaternion.LookRotation(Vector3.forward, Vector3.down));
            yield return 0.6f;
            Check("el interruptor del techo queda al alcance", p.interactor.Current != null);
            p.interactor.TryInteract();
            yield return 0.3f;
            Check("el interruptor abre la compuerta", door.IsActivated);

            p.gravity.Flip();
            yield return 2.5f;
            p.Teleport(new Vector3(0, 0.05f, 38f), Quaternion.identity);
            yield return p.WalkUntilCompleted(Vector3.forward, 8f, "Kai cruza la compuerta y completa 1-2");
            Check("completar 1-2 desbloquea 1-3", GameProgress.UnlockedSection >= 2);

            p.level.Continue();
            bool loaded = false;
            yield return WaitForScene("Level_01_3_CargoLock", 10f, r => loaded = r);
            Check("Continuar carga la sección 1-3", loaded);
            yield return 0.5f;
        }

        // =====================================================================
        // Sección 1-3: Esclusa de carga
        // =====================================================================

        static IEnumerator CargoLockSuite()
        {
            suite = "1-3";
            yield return LoadScene("Level_01_3_CargoLock");
            var p = Player.Find();
            var door = Object.FindAnyObjectByType<DoorController>();
            PressurePlate plateA = null, plateB = null;
            foreach (var plate in Object.FindObjectsByType<PressurePlate>())
                if (plate.name.Contains("West")) plateA = plate; else plateB = plate;
            AnchorableBody boxA = null, boxB = null;
            foreach (var b in Object.FindObjectsByType<AnchorableBody>())
                if (b.name.EndsWith("A")) boxA = b; else boxB = b;
            Check("referencias: 2 placas, 2 cajas, compuerta lógica", plateA && plateB && boxA && boxB && Object.FindAnyObjectByType<ActivationGate>());
            yield return 1.5f;

            Check("estado inicial: placas libres y compuerta cerrada", !plateA.IsPressed && !plateB.IsPressed && !door.IsActivated);
            p.gravity.RotateRight(p.go.transform);
            yield return 2.5f;
            Check("con gravedad al este, la caja B presiona la placa alta", plateB.IsPressed, $"boxB={boxB.transform.position}");
            Check("la caja A queda abajo sin presionar la placa baja", boxA.transform.position.x > 3f && !plateA.IsPressed);
            Check("con una sola placa la compuerta sigue cerrada", !door.IsActivated);

            p.Teleport(new Vector3(5.3f, 3f, 12.5f), Quaternion.LookRotation(Vector3.forward, Vector3.left));
            yield return 0.6f;
            p.AimAt(boxB);
            Check("la mira selecciona la caja B", p.tool.FindTarget() == boxB);
            Check("clic ancla la caja B", p.tool.ToggleAnchor() && boxB.IsAnchored);
            Vector3 anchored = boxB.transform.position;
            p.gravityCamera.enabled = true;
            Check("F lleva la gravedad al oeste", p.gravity.Flip() && p.gravity.Direction == Vector3.left);
            yield return 3f;

            Check("la caja A vuela al oeste y presiona la placa baja", plateA.IsPressed, $"boxA={boxA.transform.position}");
            Check("la caja B anclada sigue en la placa alta", plateB.IsPressed && Vector3.Distance(boxB.transform.position, anchored) < 0.05f);
            Check("con las dos placas la compuerta se abre", door.IsActivated);
            p.gravity.ResetGravity();
            yield return 2.5f;
            Check("con gravedad normal la caja A sigue en su placa por su peso", plateA.IsPressed, $"boxA={boxA.transform.position}");
            Check("la compuerta sigue abierta", door.IsActivated && plateB.IsPressed);

            p.Teleport(new Vector3(0, 0.05f, 21f), Quaternion.identity);
            yield return p.WalkUntilCompleted(Vector3.forward, 8f, "Kai sale y completa 1-3");
            Check("completar 1-3 desbloquea el progreso final", GameProgress.UnlockedSection >= 3);
            Check("1-3 es el final del sector", !p.level.HasNext);

            p.level.Continue();
            bool loaded = false;
            yield return WaitForScene("MainMenu", 10f, r => loaded = r);
            Check("al final del sector, Continuar vuelve al menú", loaded);
            yield return 0.5f;
        }

        // =====================================================================
        // Test_Gravity (regresión de mecánicas base)
        // =====================================================================

        static IEnumerator TestGravitySuite()
        {
            suite = "Test_Gravity";
            yield return LoadScene("Test_Gravity");
            var motor = Object.FindAnyObjectByType<PlayerMotor>();
            var gravity = Object.FindAnyObjectByType<GravityController>();
            motor.GetComponent<PlayerInputHandler>().enabled = false;
            var animator = motor.GetComponentInChildren<Animator>();
            Check("Kai con modelo animado", animator && animator.isHuman && motor.GetComponent<PlayerAnimator>());
            yield return 1.5f;
            Check("Kai en el suelo", motor.IsGrounded, $"pos={motor.transform.position}");
            Check("puerta presente", Object.FindAnyObjectByType<DoorController>() != null);
            Check("F invierte la gravedad", gravity.Flip() && gravity.Direction == Vector3.up);
            yield return 2.5f;
            Check("Kai cae al techo y se alinea", motor.transform.position.y > 10f && Vector3.Dot(motor.transform.up, Vector3.down) > 0.95f,
                $"pos={motor.transform.position}");
        }
    }
}
