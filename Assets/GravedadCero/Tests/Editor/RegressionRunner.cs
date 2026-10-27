using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GravedadCero.Audio;
using GravedadCero.Cameras;
using GravedadCero.Enemies;
using GravedadCero.Gravity;
using GravedadCero.Interaction;
using GravedadCero.Level;
using GravedadCero.Player;
using GravedadCero.Puzzles;
using GravedadCero.Save;
using GravedadCero.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GravedadCero.Tests
{
    /// <summary>
    /// Pruebas de regresión en Play Mode del juego completo: menú, secciones 1-1, 1-2 y 1-3,
    /// pausa, HUD, configuración de cámara y Test_Gravity. Simulan al jugador dando órdenes
    /// directas a los componentes o, en las pruebas de pausa, pulsando teclas en un teclado
    /// virtual del Input System (no un teclado físico), y comprueban el resultado.
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
        const string SensitivityPref = "gravedadcero.mouseSensitivity";
        const string InvertPref = "gravedadcero.invertY";
        const string MusicPref = "gravedadcero.musicVolume";
        const string SfxPref = "gravedadcero.sfxVolume";
        const string LogsPref = "gravedadcero.dataLogs";
        const string PrevLogsKey = "GC_REGRESSION_PREV_LOGS";
        const string PrevMusicKey = "GC_REGRESSION_PREV_MUSIC";
        const string PrevSfxKey = "GC_REGRESSION_PREV_SFX";
        const string PrevSensitivityKey = "GC_REGRESSION_PREV_SENS";
        const string PrevInvertKey = "GC_REGRESSION_PREV_INVERT";
        const string ScenesDir = "Assets/GravedadCero/Scenes/";
        const float GlobalTimeout = 600f;

        static readonly Stack<IEnumerator> routines = new Stack<IEnumerator>();
        static readonly List<string> lines = new List<string>();
        static readonly List<string> logProblems = new List<string>();
        static string suite = "";
        static int failures;
        static float waitUntil;
        static float waitUntilRealtime;
        static bool started;
        static Keyboard testKeyboard;
        static InputSettings.EditorInputBehaviorInPlayMode previousEditorInputBehavior;
        static InputSettings.BackgroundBehavior previousBackgroundBehavior;

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
            SessionState.SetFloat(PrevSensitivityKey, PlayerPrefs.GetFloat(SensitivityPref, GameSettings.DefaultSensitivity));
            SessionState.SetInt(PrevInvertKey, PlayerPrefs.GetInt(InvertPref, 0));
            SessionState.SetFloat(PrevMusicKey, PlayerPrefs.GetFloat(MusicPref, GameSettings.DefaultMusicVolume));
            SessionState.SetFloat(PrevSfxKey, PlayerPrefs.GetFloat(SfxPref, GameSettings.DefaultSfxVolume));
            SessionState.SetString(PrevLogsKey, PlayerPrefs.GetString(LogsPref, ""));
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
        // yield return new Realtime(s) -> esperar segundos reales (sirve con el juego en pausa)
        // yield return <IEnumerator> -> ejecutar una sub-rutina

        sealed class Realtime
        {
            public readonly float seconds;
            public Realtime(float seconds) { this.seconds = seconds; }
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            if (!started)
            {
                started = true;
                routines.Push(RunAll());
            }
            if (Time.realtimeSinceStartup > GlobalTimeout) { Check("tiempo máximo global", false); Finish(); return; }
            if (Time.time < waitUntil || Time.realtimeSinceStartup < waitUntilRealtime) return;

            while (routines.Count > 0)
            {
                IEnumerator top = routines.Peek();
                if (!top.MoveNext()) { routines.Pop(); continue; }
                if (top.Current is IEnumerator nested) { routines.Push(nested); continue; }
                if (top.Current is float seconds) waitUntil = Time.time + seconds;
                if (top.Current is Realtime real) waitUntilRealtime = Time.realtimeSinceStartup + real.seconds;
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
            yield return RotationRingSuite();
            yield return CoreSuite();
            yield return PauseAndHudSuite();
            yield return VictoryAndTransitionSuite();
            yield return AudioSuite();
            yield return DefeatSuite();
            yield return IntroTimeoutSuite();
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
            PlayerPrefs.SetFloat(SensitivityPref, SessionState.GetFloat(PrevSensitivityKey, GameSettings.DefaultSensitivity));
            PlayerPrefs.SetInt(InvertPref, SessionState.GetInt(PrevInvertKey, 0));
            PlayerPrefs.SetFloat(MusicPref, SessionState.GetFloat(PrevMusicKey, GameSettings.DefaultMusicVolume));
            PlayerPrefs.SetFloat(SfxPref, SessionState.GetFloat(PrevSfxKey, GameSettings.DefaultSfxVolume));
            PlayerPrefs.SetString(LogsPref, SessionState.GetString(PrevLogsKey, ""));
            PlayerPrefs.Save();
            GameSettings.Reload();
            if (testKeyboard != null)
            {
                InputSystem.RemoveDevice(testKeyboard);
                InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorInputBehavior;
                InputSystem.settings.backgroundBehavior = previousBackgroundBehavior;
                testKeyboard = null;
            }

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

            /// <summary>Camina hacia el norte hasta pasar z (por ejemplo, una compuerta).</summary>
            public IEnumerator WalkPast(float z, float timeout, string checkName)
            {
                float start = Time.time, realStart = Time.realtimeSinceStartup;
                // El límite en tiempo real evita colgarse si el juego queda detenido (por ejemplo, en una derrota).
                while (Position.z < z && Time.time - start < timeout && Time.realtimeSinceStartup - realStart < timeout + 5f)
                {
                    motor.SetMoveInput(Vector3.forward);
                    yield return null;
                }
                motor.SetMoveInput(Vector3.zero);
                Check(checkName, Position.z >= z, $"pos={Position}");
            }

            public IEnumerator WalkUntilCompleted(Vector3 direction, float timeout, string checkName)
            {
                float start = Time.time, realStart = Time.realtimeSinceStartup;
                while (!level.IsCompleted && Time.time - start < timeout && Time.realtimeSinceStartup - realStart < timeout + 5f)
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
            Check("cinco secciones configuradas", menu.Sections.Count == 5);

            GameProgress.ResetProgress();
            var preview = Object.FindAnyObjectByType<KaiSkinApplier>();
            // La prueba parte de la primera skin, sea cual sea la que tenga guardada el jugador.
            preview.Apply(0);
            GameProgress.SelectedSkin = 0;
            menu.ShowPanel("Main");
            menu.LoadSection(1);
            yield return 0.5f;
            Check("una sección bloqueada no se carga", SceneManager.GetActiveScene().name == "MainMenu");

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
            var doorB = Named<DoorController>("Door_B");
            var doorC = Named<DoorController>("Door_C");
            var lift = Named<PressurePlate>("PressurePlate_Lift");
            var crane = Named<PressurePlate>("PressurePlate_Crane");
            var dockBox = Named<AnchorableBody>("CargoBox_Dock");
            var hangarBox = Named<AnchorableBody>("CargoBox_Hangar");
            var drone = Named<SecurityDrone>("SecurityDrone_Hangar");
            var log = Named<DataLog>("DataLog_muelle");
            var health = p.go.GetComponent<PlayerHealth>();
            var hud = Object.FindAnyObjectByType<LevelHUD>();
            Check("referencias: 2 compuertas, 2 placas, 2 cajas, dron, registro, integridad", doorB && doorC && lift && crane && dockBox && hangarBox
                && drone && log && health && hud && p.tool && p.gravity && p.level && p.cam);
            Check("el dron no tiene GravityBody (la gravedad no le afecta)", drone.GetComponent<GravityBody>() == null);
            Check("Kai con Animator humanoide y PlayerAnimator", p.animator && p.animator.isHuman && p.go.GetComponent<PlayerAnimator>());
            yield return 1.5f;

            Check("caja en reposo sobre el andén", dockBox.transform.position.y > 2.3f && dockBox.transform.position.y < 2.7f, $"pos={dockBox.transform.position}");
            Check("báscula libre y compuerta B cerrada al inicio", !lift.IsPressed && !doorB.IsActivated);
            Check("Kai en el suelo, en reposo y con el traje entero", p.motor.IsGrounded && p.animator.GetFloat("Speed") < 0.2f && health.Integrity == health.MaxIntegrity);
            Check("el HUD muestra la integridad completa", hud.IntegrityShown == health.MaxIntegrity, $"segmentos={hud.IntegrityShown}");

            // A → B: la esclusa se cruza caminando por el paso central.
            float maxSpeed = 0f;
            float walkStart = Time.time;
            while (p.Position.z < 14f && Time.time - walkStart < 6f)
            {
                p.motor.SetMoveInput(Vector3.forward);
                maxSpeed = Mathf.Max(maxSpeed, p.animator.GetFloat("Speed"));
                yield return null;
            }
            p.motor.SetMoveInput(Vector3.zero);
            Check("Kai cruza la esclusa hasta el muelle", p.Position.z >= 14f, $"pos={p.Position}");
            Check("el Animator recibe la velocidad de carrera", maxSpeed > 4f, $"max={maxSpeed:F2}");
            Check("al entrar en el muelle se guarda el punto de control", Named<Checkpoint>("Checkpoint_Dock").IsReached);
            yield return 0.5f;

            // B: la caja del andén, lanzada al este, cae sobre la báscula.
            Check("R gira la gravedad a +X", p.gravity.RotateRight(p.go.transform) && p.gravity.Direction == Vector3.right);
            yield return 0.25f;
            Check("Animator pasa a salto/caída en el aire", !p.animator.GetBool("Grounded"));
            yield return 2.75f;
            Check("la caja vuela del andén a la báscula y abre la compuerta B", dockBox.transform.position.x > 11f && lift.IsPressed && doorB.IsActivated,
                $"box={dockBox.transform.position}");
            Check("Kai se alinea con la pared este", Vector3.Dot(p.Up, Vector3.left) > 0.95f, $"up={p.Up}");
            p.Teleport(new Vector3(13.3f, 3f, 18.5f), Quaternion.LookRotation(Vector3.forward, Vector3.left));
            yield return 0.6f;
            p.AimAt(dockBox, away: true);
            Check("la mira no selecciona nada lejos de la caja", p.tool.FindTarget() == null);
            p.AimAt(dockBox);
            Check("la mira selecciona la caja", p.tool.FindTarget() == dockBox);
            Check("clic ancla la caja", p.tool.ToggleAnchor() && dockBox.IsAnchored);
            Vector3 anchored = dockBox.transform.position;
            p.gravityCamera.enabled = true;
            p.gravity.ResetGravity();
            yield return 2.5f;
            Check("la caja anclada no se mueve al cambiar la gravedad", Vector3.Distance(dockBox.transform.position, anchored) < 0.05f);
            Check("con la caja anclada la compuerta B sigue abierta", lift.IsPressed && doorB.IsActivated);
            Check("Kai vuelve al suelo", p.Position.y < 1f && Vector3.Dot(p.Up, Vector3.up) > 0.95f, $"pos={p.Position}");

            // Ruta alternativa: por la pared oeste hasta la pasarela norte y su registro.
            SetGravity(p, Vector3.left);
            yield return 2.5f;
            p.Teleport(new Vector3(-13.35f, 9.8f, 33.8f), Quaternion.LookRotation(Vector3.forward, Vector3.right));
            yield return 0.3f;
            p.gravity.ResetGravity();
            yield return 1.5f;
            Check("desde la pared oeste, Kai cae sobre la pasarela norte", p.motor.IsGrounded && p.Position.y > 8.3f, $"pos={p.Position}");
            walkStart = Time.time;
            while (!log.IsCollected && Time.time - walkStart < 6f) { p.motor.SetMoveInput(Vector3.right); yield return null; }
            p.motor.SetMoveInput(Vector3.zero);
            Check("al final de la pasarela Kai recoge el registro de datos", log.IsCollected && GameProgress.HasLog(log.Id), $"pos={p.Position}");
            Check("el HUD avisa del registro encontrado", hud.NoticeVisible && hud.NoticeTitle.Contains("REGISTRO"), $"aviso={hud.NoticeTitle}");

            // B → C.
            p.Teleport(new Vector3(0, 0.05f, 31f), Quaternion.identity);
            yield return 0.5f;
            yield return p.WalkPast(38.5f, 6f, "Kai cruza la compuerta B al hangar");
            Check("al entrar en el hangar se guarda el punto de control", Named<Checkpoint>("Checkpoint_Hangar").IsReached);
            yield return 0.5f;
            Check("en la entrada (zona segura) el dron patrulla sin ver a Kai", drone.Current == SecurityDrone.State.Patrol, $"estado={drone.Current}");

            // El dron detecta a Kai en su pasillo, lo persigue y lo golpea.
            Vector3 dronePos = drone.transform.position;
            p.Teleport(new Vector3(0f, 0.05f, Mathf.Clamp(dronePos.z, 44f, 60f)), Quaternion.identity);
            float t = Time.time;
            while (drone.Current == SecurityDrone.State.Patrol && Time.time - t < 2f) yield return null;
            Check("el dron detecta a Kai en su pasillo (distancia y línea de visión)", drone.Current == SecurityDrone.State.Alert || drone.Current == SecurityDrone.State.Chase,
                $"estado={drone.Current} dron={dronePos}");
            t = Time.time;
            while (health.Integrity == health.MaxIntegrity && Time.time - t < 5f) yield return null;
            Check("el dron alcanza a Kai y le quita una unidad de integridad", health.Integrity == health.MaxIntegrity - 1 && health.IsInvulnerable, $"integridad={health.Integrity}");
            Check("tras golpear, el dron se retira", drone.Current == SecurityDrone.State.Retreat, $"estado={drone.Current}");
            Check("el HUD muestra la integridad perdida", hud.IntegrityShown == health.MaxIntegrity - 1, $"segmentos={hud.IntegrityShown}");
            yield return new Realtime(0.1f);
            Check("durante la invulnerabilidad no hay un segundo golpe", !health.TakeHit(drone.transform.position) && health.Integrity == health.MaxIntegrity - 1);

            // Subir al techo deja a Kai fuera de la zona del dron.
            yield return 1.6f;
            Check("F lleva a Kai al techo", p.gravity.Flip() && p.gravity.Direction == Vector3.up);
            yield return 4f;
            Check("en el techo, Kai queda fuera del alcance del dron", drone.Current != SecurityDrone.State.Chase && drone.Current != SecurityDrone.State.Alert
                && health.Integrity == health.MaxIntegrity - 1, $"estado={drone.Current}");
            Check("el dron sigue dentro de su zona (no atraviesa contenedores ni sube con la gravedad)",
                drone.PatrolZone.Contains(drone.transform.position), $"dron={drone.transform.position}");

            // Una caja que golpea al dron lo aturde; después la caja vuelve a su sitio.
            var hangarBody = hangarBox.GetComponent<Rigidbody>();
            hangarBody.position = drone.transform.position + Vector3.up * 2.4f;
            hangarBody.rotation = Quaternion.identity;
            hangarBox.transform.SetPositionAndRotation(hangarBody.position, Quaternion.identity);
            hangarBody.angularVelocity = Vector3.zero;
            hangarBody.linearVelocity = Vector3.down * 12f;
            t = Time.time;
            while (drone.Current != SecurityDrone.State.Stunned && Time.time - t < 1f) yield return null;
            Check("una caja que cae sobre el dron lo aturde", drone.Current == SecurityDrone.State.Stunned, $"estado={drone.Current}");
            hangarBox.ReturnHome();
            Check("la caja vuelve a su posición inicial", Vector3.Distance(hangarBox.transform.position, new Vector3(-12f, 2.4f, 52f)) < 0.3f, $"box={hangarBox.transform.position}");

            // C: la grúa. Con la gravedad hacia arriba, la caja sube al techo y se lleva al este.
            yield return 3f;
            Check("la caja del hangar sube al techo", hangarBox.transform.position.y > 14.5f, $"box={hangarBox.transform.position}");
            SetGravity(p, Vector3.right);
            yield return 3f;
            Check("por el techo, la caja llega a la pinza de la grúa y abre la compuerta C", crane.IsPressed && doorC.IsActivated, $"box={hangarBox.transform.position}");
            p.Teleport(new Vector3(17.3f, 12f, 47f), Quaternion.LookRotation(Vector3.forward, Vector3.left));
            yield return 0.6f;
            p.AimAt(hangarBox);
            Check("clic ancla la caja en la grúa", p.tool.ToggleAnchor() && hangarBox.IsAnchored);
            Check("anclarla suelta la caja del muelle", !dockBox.IsAnchored);
            p.gravityCamera.enabled = true;
            p.gravity.ResetGravity();
            yield return 3f;
            Check("con la caja anclada la compuerta C sigue abierta", crane.IsPressed && doorC.IsActivated);

            // Sin integridad, Kai reaparece en el último punto de control con el traje reparado.
            p.Teleport(new Vector3(0, 0.05f, 64f), Quaternion.identity);
            yield return 0.4f;
            Check("junto a la compuerta C se guarda el punto de control", Named<Checkpoint>("Checkpoint_Gate").IsReached);
            yield return 1.6f;
            health.TakeHit(p.Position + Vector3.forward);
            yield return 1.7f;
            health.TakeHit(p.Position + Vector3.forward);
            yield return ExpectDefeat("sin integridad", "TRAJE DESTRUIDO");
            Check("sin integridad, Kai reaparece en el punto de control con el traje reparado",
                health.Integrity == health.MaxIntegrity && Mathf.Abs(p.Position.z - 64.5f) < 1f && p.gravity.Direction == Vector3.down,
                $"integridad={health.Integrity} pos={p.Position}");
            yield return 1f;

            yield return p.WalkUntilCompleted(Vector3.forward, 8f, "Kai llega a la sala de control y completa 1-1");
            Check("completar 1-1 desbloquea 1-2", GameProgress.UnlockedSection >= 1);
            var victoryPause = Object.FindAnyObjectByType<PauseController>();
            victoryPause.HandleCancel();
            Check("en la pantalla de victoria no se abre la pausa", !victoryPause.IsPaused && Time.timeScale == 1f);
        }

        // =====================================================================
        // Sección 1-2: Pozo de mantenimiento
        // =====================================================================

        static IEnumerator MaintenanceShaftSuite()
        {
            suite = "1-2";
            yield return LoadScene("Level_01_2_MaintenanceShaft");
            var p = Player.Find();
            var doorShaft = Named<DoorController>("Door_Shaft");
            var doorExit = Named<DoorController>("Door_Exit");
            var valve1 = Named<SignalLatch>("Latch_Valve1");
            var valve2 = Named<SignalLatch>("Latch_Valve2");
            var ventEast = Named<TimedHazard>("Vent_East");
            var drone = Named<SecurityDrone>("SecurityDrone_Shaft");
            var log = Named<DataLog>("DataLog_pozo");
            var cpTop = Named<Checkpoint>("Checkpoint_Top");
            var cpEast = Named<Checkpoint>("Checkpoint_East");
            var cpWest = Named<Checkpoint>("Checkpoint_West");
            var cpLedge = Named<Checkpoint>("Checkpoint_Ledge");
            Check("referencias: 2 compuertas, 2 válvulas, 3 ventilas, dron, registro y 4 puntos de control", doorShaft && doorExit && valve1 && valve2
                && ventEast && Object.FindObjectsByType<TimedHazard>().Length == 3 && drone && log && cpTop && cpEast && cpWest && cpLedge);
            yield return 1.5f;
            Check("Kai en el suelo de la galería y compuertas cerradas al inicio", p.motor.IsGrounded && !doorShaft.IsActivated && !doorExit.IsActivated);

            // Galería: el foso se cruza por el techo, donde está el interruptor.
            p.Teleport(new Vector3(0, 0.05f, -12f), Quaternion.identity);
            yield return ExpectDefeat("al caer al foso de la galería", "ZONA PELIGROSA");
            float t = Time.time;
            while (!(p.Position.z < -17f && p.Position.y > -1f && Time.time - t > 0.3f) && Time.time - t < 5f) yield return null;
            Check("caer al foso de la galería devuelve a Kai al inicio", p.Position.z < -17f && p.Position.y > -1f, $"pos={p.Position}");
            yield return 1f;
            Check("F lleva a Kai al techo de la galería", p.gravity.Flip() && p.gravity.Direction == Vector3.up);
            yield return 2.5f;
            p.Teleport(new Vector3(0, 7.95f, -13.4f), Quaternion.LookRotation(Vector3.forward, Vector3.down));
            yield return 0.6f;
            Check("el interruptor del techo, sobre el foso, queda al alcance", p.interactor.Current != null);
            p.interactor.TryInteract();
            yield return 0.3f;
            Check("el interruptor abre la compuerta del pozo", doorShaft.IsActivated);
            // Por el techo hasta pasado el foso; ahí se vuelve al suelo.
            p.Teleport(new Vector3(0, 7.95f, -5f), Quaternion.LookRotation(Vector3.forward, Vector3.down));
            yield return 0.3f;
            p.gravity.Flip();
            yield return 2.5f;
            Check("pasado el foso, la gravedad normal deja a Kai en el suelo de la galería", p.motor.IsGrounded && p.Position.y < 1f && !p.go.GetComponent<FallRecovery>().IsDown, $"pos={p.Position}");
            p.Teleport(new Vector3(0, 0.05f, -5f), Quaternion.identity);
            yield return 0.3f;
            yield return p.WalkPast(1.5f, 6f, "Kai entra en la plataforma superior del pozo");
            Check("en la plataforma superior se guarda el punto de control", cpTop.IsReached);

            // El fondo es refrigerante.
            p.Teleport(new Vector3(0, -30f, 8f), Quaternion.identity);
            yield return ExpectDefeat("al caer al refrigerante", "ZONA PELIGROSA");
            t = Time.time;
            while (!(p.Position.z < 3f && p.Position.y > -1f && Time.time - t > 0.3f) && Time.time - t < 6f) yield return null;
            Check("caer al refrigerante devuelve a Kai a la plataforma superior", p.Position.z < 3f && p.Position.y > -1f, $"pos={p.Position}");
            yield return 1f;

            // Ventila de la pared este: se cruza apagada; encendida devuelve al punto de control.
            Check("R lleva la gravedad a la pared este", p.gravity.RotateRight(p.go.transform) && p.gravity.Direction == Vector3.right);
            yield return 2.5f;
            t = Time.time;
            while (!(ventEast.Current == TimedHazard.Phase.Off && ventEast.TimeToNextPhase > 1.2f) && Time.time - t < 6f) yield return null;
            p.Teleport(new Vector3(9.35f, -7f, 8f), Quaternion.LookRotation(Vector3.down, Vector3.left));
            yield return 0.5f;
            Check("con la ventila apagada, Kai puede estar en su franja", p.Position.y < -5f && p.Position.x > 8f, $"pos={p.Position} fase={ventEast.Current}");
            t = Time.time;
            while (ventEast.Current != TimedHazard.Phase.On && Time.time - t < 6f) yield return null;
            yield return ExpectDefeat("al tocar el vapor", "ZONA PELIGROSA");
            Check("con la ventila encendida, el vapor devuelve a Kai al punto de control", p.Position.z < 3f && p.Position.y > -1f && p.gravity.Direction == Vector3.down,
                $"pos={p.Position}");
            yield return 1f;

            // Descansillo este y válvula 1.
            Check("R vuelve a llevar a Kai a la pared este", p.gravity.RotateRight(p.go.transform) && p.gravity.Direction == Vector3.right);
            yield return 2.5f;
            p.Teleport(new Vector3(9.35f, -10.5f, 8f), Quaternion.LookRotation(Vector3.down, Vector3.left));
            yield return 0.3f;
            p.gravity.ResetGravity();
            yield return 1.5f;
            Check("encima del descansillo este, la gravedad normal deja a Kai sobre él", p.motor.IsGrounded && Mathf.Abs(p.Position.y + 14f) < 0.5f, $"pos={p.Position}");
            Check("en el descansillo este se guarda el punto de control", cpEast.IsReached);
            p.Teleport(new Vector3(8f, -13.95f, 6.6f), Quaternion.LookRotation(Vector3.back));
            yield return 0.6f;
            Check("la válvula 1 queda al alcance", p.interactor.Current != null);
            p.interactor.TryInteract();
            yield return 0.3f;
            Check("con una válvula la sala de bombas sigue cerrada", valve1.IsActivated && !doorExit.IsActivated);

            // Ruta secreta: desde el techo del pozo se cae sobre la plataforma suspendida.
            Check("F lleva a Kai al techo del pozo", p.gravity.Flip() && p.gravity.Direction == Vector3.up);
            yield return 2.5f;
            p.Teleport(new Vector3(0, 7.95f, 8f), Quaternion.LookRotation(Vector3.forward, Vector3.down));
            yield return 0.5f;
            p.gravity.Flip();
            t = Time.time;
            while (!log.IsCollected && Time.time - t < 4f) yield return null;
            yield return 0.5f;
            Check("desde el techo, Kai cae sobre la plataforma suspendida y recoge el registro", log.IsCollected && p.Position.y > -21f && p.Position.y < -18f,
                $"pos={p.Position}");
            t = Time.time;
            while (drone.Current == SecurityDrone.State.Patrol && Time.time - t < 3f) yield return null;
            Check("en la plataforma, el dron del pozo detecta a Kai", drone.Current == SecurityDrone.State.Alert || drone.Current == SecurityDrone.State.Chase,
                $"estado={drone.Current}");
            drone.Stun(); // el aturdido con cajas se prueba en 1-1; aquí solo deja seguir la ruta

            // Pared oeste: válvula 2 y descansillo oeste.
            SetGravity(p, Vector3.left);
            yield return 2f;
            p.Teleport(new Vector3(-9.35f, -21.4f, 8f), Quaternion.LookRotation(Vector3.up, Vector3.right));
            yield return 0.6f;
            Check("la válvula 2, en la pared oeste, queda al alcance", p.interactor.Current != null);
            p.interactor.TryInteract();
            yield return 0.3f;
            Check("con las dos válvulas la sala de bombas se abre", valve2.IsActivated && doorExit.IsActivated);
            p.Teleport(new Vector3(-9.35f, -25f, 8f), Quaternion.LookRotation(Vector3.down, Vector3.right));
            yield return 0.3f;
            p.gravity.ResetGravity();
            yield return 1.5f;
            Check("encima del descansillo oeste, la gravedad normal deja a Kai sobre él", p.motor.IsGrounded && Mathf.Abs(p.Position.y + 28f) < 0.5f, $"pos={p.Position}");
            Check("en el descansillo oeste se guarda el punto de control", cpWest.IsReached);

            // Pared norte hasta la cornisa de la sala de bombas.
            SetGravity(p, Vector3.forward);
            yield return 2.5f;
            Check("gravedad al norte: Kai cruza el pozo hasta la pared norte", p.Position.z > 13.5f, $"pos={p.Position}");
            p.Teleport(new Vector3(0, -33.5f, 15.35f), Quaternion.LookRotation(Vector3.down, Vector3.back));
            yield return 0.3f;
            p.gravity.ResetGravity();
            yield return 1.5f;
            Check("encima de la cornisa, la gravedad normal deja a Kai sobre ella", p.motor.IsGrounded && Mathf.Abs(p.Position.y + 36f) < 0.5f, $"pos={p.Position}");
            Check("en la cornisa se guarda el punto de control", cpLedge.IsReached);

            p.Teleport(new Vector3(0, -35.95f, 14f), Quaternion.identity);
            yield return p.WalkUntilCompleted(Vector3.forward, 8f, "Kai entra en la sala de bombas y completa 1-2");
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
            var door = Named<DoorController>("Door_1");
            var plateA = Named<PressurePlate>("PressurePlate_WestLow");
            var plateB = Named<PressurePlate>("PressurePlate_EastHigh");
            var boxA = Named<AnchorableBody>("CargoBox_A");
            var boxB = Named<AnchorableBody>("CargoBox_B");
            Check("referencias: 2 placas, 2 cajas, compuerta lógica", plateA && plateB && boxA && boxB && Named<ActivationGate>("ActivationGate"));
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
            yield return p.WalkPast(29f, 8f, "Kai cruza la primera esclusa");

            // Segunda esclusa: una placa en el techo (oeste) y otra en el suelo junto a la pared este.
            // La gravedad es global: la primera esclusa ya movió estas cajas al oeste.
            var door2 = Named<DoorController>("Door_2");
            var plateC = Named<PressurePlate>("PressurePlate_Ceiling");
            var plateD = Named<PressurePlate>("PressurePlate_EastFloor");
            var boxC = Named<AnchorableBody>("CargoBox_C");
            var boxD = Named<AnchorableBody>("CargoBox_D");
            Check("segunda esclusa: placas libres y compuerta 2 cerrada", !plateC.IsPressed && !plateD.IsPressed && !door2.IsActivated);
            Check("F sube las cajas al techo", p.gravity.Flip() && p.gravity.Direction == Vector3.up);
            yield return 2.5f;
            Check("la caja C presiona la placa del techo", plateC.IsPressed, $"boxC={boxC.transform.position}");
            Check("con una placa la compuerta 2 sigue cerrada", !plateD.IsPressed && !door2.IsActivated);
            p.Teleport(new Vector3(0, 7.95f, 30f), Quaternion.LookRotation(Vector3.forward, Vector3.down));
            yield return 0.6f;
            p.AimAt(boxC);
            Check("clic ancla la caja C en el techo", p.tool.ToggleAnchor() && boxC.IsAnchored);
            Check("anclar la caja C suelta la de la primera esclusa", !boxB.IsAnchored);
            p.gravityCamera.enabled = true;
            SetGravity(p, Vector3.right);
            yield return 2.5f;
            Check("la caja D va del techo a la pared este", boxD.transform.position.x > 3.8f, $"boxD={boxD.transform.position}");
            p.gravity.ResetGravity();
            yield return 3f;
            Check("la caja D cae sobre la placa del suelo y la compuerta 2 se abre", plateD.IsPressed && plateC.IsPressed && door2.IsActivated, $"boxD={boxD.transform.position}");

            p.Teleport(new Vector3(0, 0.05f, 45f), Quaternion.identity);
            yield return p.WalkUntilCompleted(Vector3.forward, 8f, "Kai sale y completa 1-3");
            Check("completar 1-3 desbloquea 1-4", GameProgress.UnlockedSection >= 3);

            p.level.Continue();
            bool loaded = false;
            yield return WaitForScene("Level_01_4_RotationRing", 10f, r => loaded = r);
            Check("Continuar carga la sección 1-4", loaded);
            yield return 0.5f;
        }

        // =====================================================================
        // Sección 1-4: Anillo de rotación
        // =====================================================================

        static IEnumerator RotationRingSuite()
        {
            suite = "1-4";
            yield return LoadScene("Level_01_4_RotationRing");
            var p = Player.Find();
            var door = Named<DoorController>("Door_1");
            var latchWall = Named<SignalLatch>("Latch_EastWall");
            var latchCeiling = Named<SignalLatch>("Latch_Ceiling");
            var cp1 = Named<Checkpoint>("Checkpoint_1");
            var cp2 = Named<Checkpoint>("Checkpoint_2");
            Check("referencias: compuerta, 2 interruptores, 2 puntos de control, 3 franjas", door && latchWall && latchCeiling && cp1 && cp2
                && Object.FindObjectsByType<HazardZone>().Length == 3);
            Check("1-4 lleva a 1-5", p.level.HasNext && p.level.NextSceneName == "Level_01_5_Core");
            yield return 1.5f;
            Check("Kai en el suelo y compuerta cerrada al inicio", p.motor.IsGrounded && !door.IsActivated);

            p.Teleport(new Vector3(0, 0.05f, 9f), Quaternion.identity);
            yield return ExpectDefeat(null, null);
            float t = Time.time;
            while (!(p.Position.z < 4f && Time.time - t > 0.3f) && Time.time - t < 5f) yield return null;
            Check("pisar la franja roja del suelo devuelve a Kai al inicio", p.Position.z < 4f, $"pos={p.Position}");
            yield return 1f;

            Check("R lleva la gravedad a la pared este", p.gravity.RotateRight(p.go.transform) && p.gravity.Direction == Vector3.right);
            yield return 2.5f;
            p.Teleport(new Vector3(5.3f, 2.5f, 11.6f), Quaternion.LookRotation(Vector3.forward, Vector3.left));
            yield return 0.6f;
            Check("sobre la franja del suelo, la pared es segura y el interruptor queda al alcance", p.interactor.Current != null && p.Position.z > 6f);
            p.interactor.TryInteract();
            yield return 0.3f;
            Check("con un interruptor la compuerta sigue cerrada", latchWall.IsActivated && !door.IsActivated);

            p.Teleport(new Vector3(5.3f, 4f, 23f), Quaternion.LookRotation(Vector3.forward, Vector3.left));
            yield return 0.4f;
            Check("pasar por la pared también guarda el punto de control 1", cp1.IsReached);
            SetGravity(p, Vector3.up);
            yield return 2.5f;
            p.Teleport(new Vector3(0, 7.95f, 30.6f), Quaternion.LookRotation(Vector3.forward, Vector3.down));
            yield return 0.6f;
            Check("el interruptor del techo queda al alcance", p.interactor.Current != null);
            p.interactor.TryInteract();
            yield return 0.3f;
            Check("con los dos interruptores la compuerta se abre", latchCeiling.IsActivated && door.IsActivated);

            // La franja de la pared este devuelve al punto de control 1, con la gravedad normal.
            SetGravity(p, Vector3.right);
            p.Teleport(new Vector3(5.3f, 4f, 32f), Quaternion.LookRotation(Vector3.forward, Vector3.left));
            yield return ExpectDefeat(null, null);
            t = Time.time;
            while (!(p.Position.z < 24f && Time.time - t > 0.3f) && Time.time - t < 5f) yield return null;
            Check("tocar la franja de la pared devuelve al punto de control 1", Mathf.Abs(p.Position.z - 23f) < 1.5f && p.gravity.Direction == Vector3.down, $"pos={p.Position}");
            Check("los interruptores siguen activados tras reaparecer", latchWall.IsActivated && latchCeiling.IsActivated && door.IsActivated);
            yield return 1f;

            p.Teleport(new Vector3(0, 0.05f, 40.5f), Quaternion.identity);
            yield return 0.4f;
            Check("punto de control 2 guardado", cp2.IsReached);
            p.gravity.Flip();
            yield return 2.5f;
            p.Teleport(new Vector3(0, 7.95f, 50f), Quaternion.LookRotation(Vector3.forward, Vector3.down));
            yield return ExpectDefeat(null, null);
            t = Time.time;
            while (!(p.Position.z < 42f && Time.time - t > 0.3f) && Time.time - t < 5f) yield return null;
            Check("tocar la franja del techo devuelve al punto de control 2", Mathf.Abs(p.Position.z - 41f) < 1.5f && p.gravity.Direction == Vector3.down, $"pos={p.Position}");
            yield return 1f;

            yield return p.WalkUntilCompleted(Vector3.forward, 10f, "Kai cruza la compuerta y completa 1-4");
            Check("completar 1-4 desbloquea 1-5", GameProgress.UnlockedSection >= 4);
            p.level.Continue();
            bool loaded = false;
            yield return WaitForScene("Level_01_5_Core", 10f, r => loaded = r);
            Check("Continuar carga la sección 1-5", loaded);
            yield return 0.5f;
        }

        // =====================================================================
        // Sección 1-5: Núcleo del reactor
        // =====================================================================

        static IEnumerator CoreSuite()
        {
            suite = "1-5";
            yield return LoadScene("Level_01_5_Core");
            var p = Player.Find();
            var door = Named<DoorController>("Door_1");
            var latch = Named<SignalLatch>("Latch_Ceiling");
            var plate = Named<PressurePlate>("PressurePlate_North");
            var box = Named<AnchorableBody>("CargoBox");
            Check("referencias: compuerta, interruptor, placa, caja y punto de control", door && latch && plate && box && Named<Checkpoint>("Checkpoint_FarSide"));
            Check("1-5 es el final del sector", !p.level.HasNext);
            yield return 1.5f;

            p.Teleport(new Vector3(0, 0.05f, 12f), Quaternion.identity);
            yield return ExpectDefeat(null, null);
            float t = Time.time;
            while (!(p.Position.z < 4f && p.Position.y > -1f && Time.time - t > 0.3f) && Time.time - t < 5f) yield return null;
            Check("caer al pozo del núcleo devuelve a Kai al inicio", p.Position.z < 4f && p.Position.y > -1f, $"pos={p.Position}");
            yield return 1f;

            Check("F lleva a Kai y la caja al techo", p.gravity.Flip() && p.gravity.Direction == Vector3.up);
            yield return 2.5f;
            Check("la caja queda en el techo", box.transform.position.y > 6.3f, $"box={box.transform.position}");
            p.Teleport(new Vector3(0, 7.95f, 10.6f), Quaternion.LookRotation(Vector3.forward, Vector3.down));
            yield return 0.6f;
            Check("el interruptor del techo, sobre el pozo, queda al alcance", p.interactor.Current != null);
            p.interactor.TryInteract();
            yield return 0.3f;
            Check("con el interruptor y sin la placa la compuerta sigue cerrada", latch.IsActivated && !door.IsActivated);

            // Kai, cabeza abajo en el techo, mira al este: su derecha es el norte.
            p.go.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.down);
            p.body.rotation = p.go.transform.rotation;
            Check("girando la gravedad mirando a un lado, apunta al norte", p.gravity.RotateRight(p.go.transform) && p.gravity.Direction == Vector3.forward,
                $"dir={p.gravity.Direction}");
            yield return 3.5f;
            Check("la caja vuela sobre el pozo hasta la pared norte", box.transform.position.z > 31f, $"box={box.transform.position}");
            Check("Kai cruza el pozo y guarda el punto de control", p.Position.z > 30f && Named<Checkpoint>("Checkpoint_FarSide").IsReached, $"pos={p.Position}");
            p.gravity.ResetGravity();
            yield return 3f;
            Check("la caja cae sobre la placa norte y la compuerta se abre", plate.IsPressed && door.IsActivated, $"box={box.transform.position}");
            Check("Kai aterriza junto a la compuerta", p.motor.IsGrounded && p.Position.z > 28f && p.Position.y < 1f, $"pos={p.Position}");

            p.Teleport(new Vector3(0, 0.05f, 31f), Quaternion.identity);
            yield return p.WalkUntilCompleted(Vector3.forward, 8f, "Kai sale y completa 1-5");
            Check("completar 1-5 desbloquea el progreso final", GameProgress.UnlockedSection >= 5);
            p.level.Continue();
            bool loaded = false;
            yield return WaitForScene("MainMenu", 10f, r => loaded = r);
            Check("al final del sector, Continuar vuelve al menú", loaded);
            yield return 0.5f;
        }

        static T Named<T>(string name) where T : Component =>
            Object.FindObjectsByType<T>(FindObjectsInactive.Include).FirstOrDefault(c => c.name == name);

        /// <summary>Gira la gravedad hacia una dirección perpendicular a la actual, como haría Q o R.</summary>
        static void SetGravity(Player p, Vector3 direction)
        {
            var reference = new GameObject("GravityReference").transform;
            reference.rotation = Quaternion.FromToRotation(Vector3.right, direction);
            bool ok = p.gravity.RotateRight(reference);
            Object.Destroy(reference.gameObject);
            if (!ok || p.gravity.Direction != direction) Check($"girar la gravedad hacia {direction}", false, $"actual={p.gravity.Direction}");
        }

        // =====================================================================
        // Teclado virtual: pulsaciones que pasan por el Input System y PlayerInputHandler
        // =====================================================================

        static void EnsureKeyboard()
        {
            if (testKeyboard != null) return;
            // En el editor, el teclado solo llega al juego si la ventana Game tiene el foco.
            // Y en modo batch el editor no tiene foco: sin IgnoreFocus el teclado se desactiva.
            previousEditorInputBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            previousBackgroundBehavior = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            testKeyboard = InputSystem.AddDevice<Keyboard>("RegressionKeyboard");
        }

        static IEnumerator Press(Key key)
        {
            EnsureKeyboard();
            InputSystem.QueueStateEvent(testKeyboard, new KeyboardState(key));
            yield return null;
            yield return null;
            InputSystem.QueueStateEvent(testKeyboard, new KeyboardState());
            yield return null;
            yield return null;
        }

        static IEnumerator Hold(Key key, float gameSeconds)
        {
            EnsureKeyboard();
            InputSystem.QueueStateEvent(testKeyboard, new KeyboardState(key));
            yield return gameSeconds;
            InputSystem.QueueStateEvent(testKeyboard, new KeyboardState());
            yield return null;
            yield return null;
        }

        // =====================================================================
        // Pausa, HUD y configuración de cámara (sección 1-1)
        // =====================================================================

        static IEnumerator PauseAndHudSuite()
        {
            suite = "Pausa";
            yield return LoadScene("Level_01_DockingBay");
            var kai = GameObject.FindWithTag("Player");
            var body = kai.GetComponent<Rigidbody>();
            var hud = Object.FindAnyObjectByType<LevelHUD>();
            var pause = Object.FindAnyObjectByType<PauseController>();
            var menu = Object.FindAnyObjectByType<PauseMenuUI>();
            var gravity = Object.FindAnyObjectByType<GravityController>();
            var cam = Object.FindAnyObjectByType<GravityCamera>();
            Check("referencias: HUD, pausa, menú de pausa y EventSystem", hud && pause && menu && UnityEngine.EventSystems.EventSystem.current != null);
            Check("al empezar se ve la tarjeta de introducción", hud.Intro == LevelHUD.IntroState.Showing && hud.IntroAlpha > 0.9f);
            yield return 1f;
            Check("con Kai quieto la introducción sigue visible", hud.Intro == LevelHUD.IntroState.Showing);
            yield return Hold(Key.W, 0.6f);
            yield return new Realtime(0.8f);
            Check("al moverse Kai (tecla W) la introducción se desvanece", hud.Intro == LevelHUD.IntroState.Hidden && hud.IntroAlpha == 0f, $"estado={hud.Intro}");

            // La pausa congela la física.
            body.linearVelocity = Vector3.zero;
            body.position = new Vector3(0f, 5f, 6f);
            kai.transform.position = body.position;
            yield return 0.15f;
            yield return Press(Key.Escape);
            Check("Esc abre la pausa", pause.IsPaused && Time.timeScale == 0f && menu.CurrentPanel == "Main");
            Check("en pausa el cursor está visible y libre", Cursor.visible && Cursor.lockState == CursorLockMode.None);
            Vector3 frozen = kai.transform.position;
            yield return new Realtime(0.6f);
            Check("en pausa Kai no avanza (caída congelada)", Vector3.Distance(kai.transform.position, frozen) < 0.001f,
                $"delta={Vector3.Distance(kai.transform.position, frozen):F4}");
            yield return Press(Key.R);
            Check("en pausa la tecla R no cambia la gravedad", gravity.Direction == Vector3.down);
            Check("en pausa el controlador de gravedad rechaza cambios", !gravity.RotateRight(kai.transform) && gravity.Direction == Vector3.down);

            yield return Press(Key.Escape);
            Check("Esc reanuda con el tiempo normal", !pause.IsPaused && Time.timeScale == 1f);
            // En modo batch no hay ventana: Unity no puede bloquear el cursor de verdad,
            // así que solo se comprueba que se oculta. El bloqueo real es prueba manual.
            Check("al reanudar el cursor se oculta", !Cursor.visible);
            yield return 0.3f;
            Check("al reanudar Kai sigue cayendo", kai.transform.position.y < frozen.y - 0.05f, $"y={kai.transform.position.y:F2}");
            yield return 1.5f;

            // Esc repetido alterna sin duplicar estados.
            yield return Press(Key.Escape);
            bool firstPaused = pause.IsPaused;
            yield return Press(Key.Escape);
            Check("Esc dos veces seguidas abre y cierra (sin pausas duplicadas)", firstPaused && !pause.IsPaused && Time.timeScale == 1f
                && Object.FindObjectsByType<PauseMenuUI>().Length == 1 && Object.FindObjectsByType<PauseController>().Length == 1);

            // Esc dentro de un submenú vuelve al panel principal.
            yield return Press(Key.Escape);
            menu.ShowPanel("Controls");
            yield return Press(Key.Escape);
            Check("Esc en Controles vuelve al panel principal sin reanudar", pause.IsPaused && menu.CurrentPanel == "Main");

            // Configuración: sensibilidad e inversión del eje Y.
            menu.ShowPanel("Settings");
            menu.SetSensitivity(0.3f);
            menu.SetInvertY(false);
            menu.SetSensitivity(5f);
            Check("la sensibilidad se limita a un máximo seguro", Mathf.Approximately(GameSettings.MouseSensitivity, GameSettings.MaxSensitivity));
            menu.SetSensitivity(0.3f);
            menu.Resume();
            yield return null;
            float pitch0 = cam.Pitch;
            cam.AddLookInput(new Vector2(0f, 10f));
            yield return null;
            float normalDelta = cam.Pitch - pitch0;
            menu.SetInvertY(true);
            float pitch1 = cam.Pitch;
            cam.AddLookInput(new Vector2(0f, 10f));
            yield return null;
            float invertedDelta = cam.Pitch - pitch1;
            Check("la sensibilidad se aplica al instante (10 px x 0.3 = 3 grados)", Mathf.Abs(normalDelta + 3f) < 0.05f, $"delta={normalDelta:F2}");
            Check("invertir Y cambia el sentido de la cámara", Mathf.Abs(invertedDelta - 3f) < 0.05f, $"delta={invertedDelta:F2}");
            Check("la configuración no cambia la gravedad", gravity.Direction == Vector3.down);

            // Reiniciar desde la pausa pide confirmación.
            yield return Press(Key.Escape);
            var levelBefore = Object.FindAnyObjectByType<LevelManager>();
            menu.RequestRestart();
            yield return new Realtime(0.3f);
            Check("Reiniciar pide confirmación y no reinicia todavía", menu.CurrentPanel == "Confirm" && pause.IsPaused
                && Object.FindAnyObjectByType<LevelManager>() == levelBefore);
            yield return Press(Key.Escape);
            Check("Esc en la confirmación la cancela", menu.CurrentPanel == "Main" && pause.IsPaused);
            menu.RequestRestart();
            menu.ConfirmPending();
            float t = Time.realtimeSinceStartup;
            while (!SectionReloaded(levelBefore) && Time.realtimeSinceStartup - t < 10f) yield return null;
            yield return 0.5f;
            pause = Object.FindAnyObjectByType<PauseController>();
            Check("Reiniciar confirmado recarga la sección sin quedar congelada",
                Object.FindAnyObjectByType<LevelManager>() != levelBefore && Time.timeScale == 1f && !pause.IsPaused);

            // La configuración persiste tras reiniciar.
            GameSettings.Reload();
            cam = Object.FindAnyObjectByType<GravityCamera>();
            float pitch2 = cam.Pitch;
            cam.AddLookInput(new Vector2(0f, 10f));
            yield return null;
            Check("la configuración persiste tras reiniciar (PlayerPrefs)", Mathf.Approximately(GameSettings.MouseSensitivity, 0.3f)
                && GameSettings.InvertY && Mathf.Abs(cam.Pitch - pitch2 - 3f) < 0.05f);

            // Retroceso reinicia al instante, sin confirmación.
            var levelBeforeBackspace = Object.FindAnyObjectByType<LevelManager>();
            yield return Press(Key.Backspace);
            t = Time.realtimeSinceStartup;
            while (!SectionReloaded(levelBeforeBackspace) && Time.realtimeSinceStartup - t < 10f) yield return null;
            Check("Retroceso reinicia al instante sin confirmación", Object.FindAnyObjectByType<LevelManager>() != levelBeforeBackspace);
            yield return 0.5f;

            // Volver al menú pide confirmación y conserva el progreso.
            pause = Object.FindAnyObjectByType<PauseController>();
            menu = Object.FindAnyObjectByType<PauseMenuUI>();
            int unlockedBefore = GameProgress.UnlockedSection;
            yield return Press(Key.Escape);
            menu.RequestExitToMenu();
            yield return new Realtime(0.3f);
            Check("Volver al menú pide confirmación y no sale todavía", menu.CurrentPanel == "Confirm" && SceneManager.GetActiveScene().name == "Level_01_DockingBay");
            menu.ConfirmPending();
            bool loaded = false;
            yield return WaitForScene("MainMenu", 10f, r => loaded = r);
            yield return 0.3f;
            Check("Volver al menú confirmado carga el menú con el tiempo restaurado", loaded && Time.timeScale == 1f && Cursor.visible);
            Check("volver al menú conserva las secciones completadas", GameProgress.UnlockedSection == unlockedBefore, $"unlocked={GameProgress.UnlockedSection}");
        }

        // =====================================================================
        // Pantalla de victoria, transiciones y textos de la interfaz
        // =====================================================================

        static IEnumerator VictoryAndTransitionSuite()
        {
            suite = "Victoria";
            yield return LoadScene("Level_01_DockingBay");
            var level = Object.FindAnyObjectByType<LevelManager>();
            var hud = Object.FindAnyObjectByType<LevelHUD>();
            yield return 0.5f;
            level.Complete();
            yield return new Realtime(0.8f);
            Check("al completar se ve la pantalla de victoria", hud.VictoryVisible);
            Check("en la victoria el cursor está visible y libre para los botones", Cursor.visible && Cursor.lockState == CursorLockMode.None);
            Check("en la victoria Continuar está resaltado", SelectedName() == "Continuar", $"seleccionado={SelectedName()}");
            CheckGlyphs("1-1 (HUD, pausa y victoria)");

            yield return Press(Key.DownArrow);
            Check("Flecha abajo resalta Repetir sección", SelectedName() == "Repetir sección", $"seleccionado={SelectedName()}");
            yield return Press(Key.UpArrow);
            Check("Flecha arriba vuelve a Continuar", SelectedName() == "Continuar", $"seleccionado={SelectedName()}");

            int loads = SceneTransition.LoadCount;
            yield return Press(Key.Enter);
            bool loaded = false;
            yield return WaitForScene("Level_01_2_MaintenanceShaft", 10f, r => loaded = r);
            yield return new Realtime(0.5f);
            Check("Enter en Continuar carga la sección 1-2 una sola vez", loaded && SceneTransition.LoadCount == loads + 1, $"cargas={SceneTransition.LoadCount - loads}");

            // Clic en Repetir y Enter en el mismo instante: una sola carga (la del clic).
            level = Object.FindAnyObjectByType<LevelManager>();
            level.Complete();
            yield return new Realtime(0.8f);
            var retry = FindButton("Repetir sección");
            Check("la victoria de 1-2 tiene el botón Repetir sección", retry != null);
            if (retry == null) yield break;
            loads = SceneTransition.LoadCount;
            Click(retry.gameObject);
            yield return Press(Key.Enter);
            float t = Time.realtimeSinceStartup;
            while (!SectionReloaded(level) && Time.realtimeSinceStartup - t < 10f) yield return null;
            yield return new Realtime(0.8f);
            Check("clic en Repetir y Enter a la vez: una sola carga y se repite 1-2",
                SectionReloaded(level) && SceneManager.GetActiveScene().name == "Level_01_2_MaintenanceShaft" && SceneTransition.LoadCount == loads + 1,
                $"cargas={SceneTransition.LoadCount - loads} escena={SceneManager.GetActiveScene().name}");
            Check("tras repetir la sección vuelve a jugarse (sin victoria ni pausa)", !Object.FindAnyObjectByType<LevelManager>().IsCompleted
                && !Object.FindAnyObjectByType<LevelHUD>().VictoryVisible && Time.timeScale == 1f);

            // El ratón encima de Menú principal lo resalta y Enter lo pulsa.
            level = Object.FindAnyObjectByType<LevelManager>();
            level.Complete();
            yield return new Realtime(0.8f);
            var menuButton = FindButton("Menú principal");
            Check("la victoria de 1-2 tiene el botón Menú principal", menuButton != null);
            if (menuButton == null) yield break;
            Hover(menuButton.gameObject);
            yield return null;
            Check("pasar el ratón por Menú principal lo resalta", SelectedName() == "Menú principal", $"seleccionado={SelectedName()}");
            yield return Press(Key.Enter);
            yield return WaitForScene("MainMenu", 10f, r => loaded = r);
            yield return new Realtime(0.5f);
            Check("Enter pulsa el botón resaltado con el ratón (vuelve al menú)", loaded && Time.timeScale == 1f);

            // Menú: Nueva partida con progreso pide confirmación.
            var menu = Object.FindAnyObjectByType<MainMenuUI>();
            int unlocked = GameProgress.UnlockedSection;
            menu.RequestNewGame();
            yield return new Realtime(0.3f);
            Check("Nueva partida con progreso pide confirmación", menu.CurrentPanel == "ConfirmNewGame" && SceneManager.GetActiveScene().name == "MainMenu"
                && GameProgress.UnlockedSection == unlocked);
            menu.ShowPanel("Main");
            Check("cancelar la confirmación conserva el progreso", GameProgress.UnlockedSection == unlocked && unlocked > 0);
            foreach (string panel in new[] { "Sections", "Character", "Main" }) menu.ShowPanel(panel);
            CheckGlyphs("menú principal");

            // Transición iniciada desde la pausa: Retroceso con el juego en pausa.
            yield return LoadScene("Level_01_DockingBay");
            var pause = Object.FindAnyObjectByType<PauseController>();
            level = Object.FindAnyObjectByType<LevelManager>();
            yield return Press(Key.Escape);
            Check("pausa abierta antes de reiniciar", pause.IsPaused && Time.timeScale == 0f);
            loads = SceneTransition.LoadCount;
            yield return Press(Key.Backspace);
            Check("durante el fundido la sección figura en transición", SceneTransition.IsTransitioning);
            yield return Press(Key.Escape);
            level.Restart(); // una segunda petición mientras carga se descarta
            t = Time.realtimeSinceStartup;
            while ((SceneTransition.IsTransitioning || !SectionReloaded(level)) && Time.realtimeSinceStartup - t < 10f) yield return null;
            yield return new Realtime(0.3f);
            pause = Object.FindAnyObjectByType<PauseController>();
            Check("Retroceso desde la pausa recarga una sola vez, sin pausa ni tiempo congelado",
                SectionReloaded(level) && SceneTransition.LoadCount == loads + 1 && !pause.IsPaused && Time.timeScale == 1f,
                $"cargas={SceneTransition.LoadCount - loads} pausa={pause.IsPaused} timeScale={Time.timeScale}");
            yield return 0.5f;
        }

        // =====================================================================
        // Audio: música por escena, efectos y volumen
        // =====================================================================

        static IEnumerator AudioSuite()
        {
            suite = "Audio";
            var catalog = GameAudio.Catalog;
            Check("catálogo de audio cargado desde Resources", catalog != null && catalog.MusicFor("MainMenu") != null && catalog.victory != null);

            yield return LoadScene("MainMenu");
            yield return new Realtime(0.8f);
            Check("en el menú suena su música", GameAudio.CurrentMusic == catalog.MusicFor("MainMenu"), $"pista={ClipName(GameAudio.CurrentMusic)}");
            int count = GameAudio.SfxCount;
            yield return Press(Key.DownArrow);
            Check("moverse entre botones suena", GameAudio.SfxCount > count && GameAudio.LastSfx == catalog.uiMove, $"último={ClipName(GameAudio.LastSfx)}");

            yield return LoadScene("Level_01_DockingBay");
            yield return new Realtime(0.8f);
            Check("en 1-1 suena su música", GameAudio.CurrentMusic == catalog.MusicFor("Level_01_DockingBay"), $"pista={ClipName(GameAudio.CurrentMusic)}");
            Check("hay un solo GameAudio tras cambiar de escena", Object.FindObjectsByType<GameAudio>().Length == 1);
            var p = Player.Find();
            count = GameAudio.SfxCount;
            p.gravity.RotateRight(p.go.transform);
            Check("cambiar la gravedad suena", GameAudio.SfxCount == count + 1 && GameAudio.LastSfx == catalog.gravityChange, $"último={ClipName(GameAudio.LastSfx)}");
            p.gravity.ResetGravity();
            yield return 1.5f;

            var menu = Object.FindAnyObjectByType<PauseMenuUI>();
            menu.SetMusicVolume(0.25f);
            menu.SetSfxVolume(1.5f);
            Check("los volúmenes se guardan y se limitan a 0-1", Mathf.Approximately(GameSettings.MusicVolume, 0.25f) && Mathf.Approximately(GameSettings.SfxVolume, 1f));
            GameSettings.Reload();
            Check("los volúmenes persisten (PlayerPrefs)", Mathf.Approximately(GameSettings.MusicVolume, 0.25f) && Mathf.Approximately(GameSettings.SfxVolume, 1f));

            p.level.Complete();
            yield return null;
            Check("al completar la sección suena la victoria", GameAudio.LastSfx == catalog.victory, $"último={ClipName(GameAudio.LastSfx)}");
            Check("la música de la sección sigue durante la victoria", GameAudio.CurrentMusic == catalog.MusicFor("Level_01_DockingBay"));

            yield return LoadScene("Level_01_2_MaintenanceShaft");
            yield return new Realtime(0.8f);
            Check("en 1-2 suena su música", GameAudio.CurrentMusic == catalog.MusicFor("Level_01_2_MaintenanceShaft"), $"pista={ClipName(GameAudio.CurrentMusic)}");
            p = Player.Find();
            p.Teleport(new Vector3(0, 0.05f, -12f), Quaternion.identity); // foso de la galería
            float t = Time.time;
            while (GameAudio.LastSfx != catalog.respawn && Time.time - t < 5f) yield return null;
            Check("caer al fondo rojo suena al reaparecer", GameAudio.LastSfx == catalog.respawn, $"último={ClipName(GameAudio.LastSfx)}");
            yield return ExpectDefeat(null, null);
        }

        static string ClipName(AudioClip clip) => clip != null ? clip.name : "(ninguno)";

        // =====================================================================
        // Pantalla de derrota (con el teclado virtual, sin desactivar la entrada de Kai)
        // =====================================================================

        static IEnumerator DefeatSuite()
        {
            suite = "Derrota";
            yield return LoadScene("Level_01_2_MaintenanceShaft");
            yield return 1f;
            var kai = GameObject.FindWithTag("Player");
            var hud = Object.FindAnyObjectByType<LevelHUD>();
            var recovery = kai.GetComponent<FallRecovery>();
            var pause = Object.FindAnyObjectByType<PauseController>();

            Drop(kai, new Vector3(0, 0.05f, -12f));
            yield return WaitDefeat(hud);
            Check("al caer al foso aparece la pantalla de derrota", hud.DefeatVisible && hud.DefeatTitle == "ZONA PELIGROSA", $"título={hud.DefeatTitle}");
            Check("durante la derrota el juego se detiene y el cursor queda libre", Time.timeScale == 0f && Cursor.visible && Cursor.lockState == CursorLockMode.None);
            Check("Reintentar está resaltado", SelectedName() == "Reintentar", $"seleccionado={SelectedName()}");
            yield return Press(Key.Enter);
            yield return new Realtime(0.3f);
            Check("Enter reintenta: Kai vuelve al punto de control con el tiempo normal",
                !hud.DefeatVisible && !recovery.IsDown && Time.timeScale == 1f && kai.transform.position.z < -17f, $"pos={kai.transform.position}");
            yield return 1f;

            var level = Object.FindAnyObjectByType<LevelManager>();
            Drop(kai, new Vector3(0, 0.05f, -12f));
            yield return WaitDefeat(hud);
            int loads = SceneTransition.LoadCount;
            yield return Press(Key.Backspace);
            float t = Time.realtimeSinceStartup;
            while ((SceneTransition.IsTransitioning || !SectionReloaded(level)) && Time.realtimeSinceStartup - t < 10f) yield return null;
            yield return new Realtime(0.3f);
            Check("Retroceso en la derrota reinicia la sección una vez, con el tiempo normal",
                SectionReloaded(level) && SceneTransition.LoadCount == loads + 1 && Time.timeScale == 1f, $"cargas={SceneTransition.LoadCount - loads}");
            yield return 1f;

            kai = GameObject.FindWithTag("Player");
            hud = Object.FindAnyObjectByType<LevelHUD>();
            pause = Object.FindAnyObjectByType<PauseController>();
            Drop(kai, new Vector3(0, 0.05f, -12f));
            yield return WaitDefeat(hud);
            yield return Press(Key.Escape);
            Check("Esc en la derrota no abre la pausa", !pause.IsPaused);
            bool loaded = false;
            yield return WaitForScene("MainMenu", 10f, r => loaded = r);
            yield return new Realtime(0.3f);
            Check("Esc en la derrota vuelve al menú con el tiempo normal", loaded && Time.timeScale == 1f);
        }

        static void Drop(GameObject kai, Vector3 position)
        {
            var body = kai.GetComponent<Rigidbody>();
            body.linearVelocity = Vector3.zero;
            body.position = position;
            kai.transform.position = position;
        }

        static IEnumerator WaitDefeat(LevelHUD hud)
        {
            float start = Time.realtimeSinceStartup;
            while (!hud.DefeatVisible && Time.realtimeSinceStartup - start < 4f) yield return null;
        }

        /// <summary>
        /// Tras una caída: espera la pantalla de derrota, comprueba que el juego se detiene y pulsa
        /// Reintentar. Con label null solo la acepta (la caída ya se comprueba en otra suite).
        /// </summary>
        static IEnumerator ExpectDefeat(string label, string title)
        {
            var hud = Object.FindAnyObjectByType<LevelHUD>();
            if (hud == null) yield break;
            yield return WaitDefeat(hud);
            if (label != null)
                Check($"{label} aparece la pantalla de derrota ({title}) y el juego se detiene",
                    hud.DefeatVisible && hud.DefeatTitle == title && Time.timeScale == 0f, $"título={hud.DefeatTitle} visible={hud.DefeatVisible}");
            hud.RetryFromCheckpoint();
            yield return null;
        }

        static string SelectedName()
        {
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            return selected != null ? selected.name : "(ninguno)";
        }

        static Button FindButton(string name) =>
            Object.FindObjectsByType<Button>().FirstOrDefault(b => b.name == name && b.gameObject.activeInHierarchy);

        static void Hover(GameObject target) =>
            ExecuteEvents.Execute(target, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);

        static void Click(GameObject target)
        {
            var data = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(target, data, ExecuteEvents.pointerEnterHandler);
            ExecuteEvents.Execute(target, data, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(target, data, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(target, data, ExecuteEvents.pointerClickHandler);
        }

        /// <summary>Todos los textos de la interfaz (también los ocultos) tienen sus caracteres en su propia fuente.</summary>
        static void CheckGlyphs(string where)
        {
            var missing = new List<string>();
            foreach (var text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include))
            {
                if (text.font == null || string.IsNullOrEmpty(text.text)) continue;
                string plain = new string(Regex.Replace(text.text, "<[^>]+>", "").Where(c => !char.IsControl(c)).ToArray());
                if (!text.font.HasCharacters(plain, out uint[] chars, false, false))
                    missing.Add($"{text.name} ({text.font.name}): {string.Join(" ", chars.Distinct().Select(c => char.ConvertFromUtf32((int)c)))}");
            }
            Check($"textos de {where} sin caracteres que falten en su fuente", missing.Count == 0, string.Join(" | ", missing));
        }

        /// <summary>True cuando ya existe un LevelManager nuevo (la escena terminó de recargarse).</summary>
        static bool SectionReloaded(LevelManager previous)
        {
            var current = Object.FindAnyObjectByType<LevelManager>();
            return current != null && current != previous && Object.FindAnyObjectByType<GravityCamera>() != null;
        }

        static IEnumerator IntroTimeoutSuite()
        {
            suite = "HUD";
            yield return LoadScene("Level_01_2_MaintenanceShaft");
            var hud = Object.FindAnyObjectByType<LevelHUD>();
            yield return 7f;
            Check("con Kai quieto la introducción sigue visible a los 7 s", hud.Intro == LevelHUD.IntroState.Showing);
            yield return 2f;
            yield return new Realtime(0.3f);
            Check("con Kai quieto la introducción se oculta tras 8 s", hud.Intro == LevelHUD.IntroState.Hidden, $"estado={hud.Intro}");
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
