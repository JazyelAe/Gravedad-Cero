using GravedadCero.Audio;
using GravedadCero.Level;
using GravedadCero.Player;
using GravedadCero.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static GravedadCero.EditorTools.LevelKit;

namespace GravedadCero.EditorTools
{
    /// <summary>
    /// Construye la sección 1-1, Muelle de acoplamiento: esclusa de entrada (A), muelle de carga
    /// con andén elevado y báscula (B), hangar de contenedores con un dron de seguridad y la pinza
    /// de la grúa (C) y sala de control con la salida (D). Ver Documentation/LEVEL_01.md.
    /// Conserva Kai, cámara, sistemas, HUD y pausa; rehace todo lo demás.
    ///
    /// Menú: Gravedad Cero → Niveles → Reconstruir 1-1.
    /// Línea de comandos: -executeMethod GravedadCero.EditorTools.Level01Builder.RunFromCommandLine
    /// </summary>
    public static class Level01Builder
    {
        const string ScenePath = "Assets/GravedadCero/Scenes/Level_01_DockingBay.unity";

        // Límites de las zonas (x de oeste a este, z de sur a norte; alturas en metros).
        const float AHalf = 6f, AZ0 = 0f, AZ1 = 12f, AHeight = 8f;
        const float BHalf = 14f, BZ1 = 36f, BHeight = 12f;
        const float CHalf = 18f, CZ1 = 68f, CHeight = 16f;

        [MenuItem("Gravedad Cero/Niveles/Reconstruir 1-1")]
        public static void RunFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        public static void RunFromCommandLine() => Build();

        public static void Build()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            Begin();
            var levelManager = Object.FindAnyObjectByType<LevelManager>() ?? throw new System.Exception("No hay LevelManager");

            BuildEntrance();
            BuildDock(out var doorB);
            BuildHangar(out var doorC);
            ExitRoom(CZ1, levelManager);
            Point("Light_Exit", new Vector3(0, 3.2f, CZ1 + 4), PaleCyan, 5f, 8f);

            var kai = GameObject.FindWithTag("Player");
            kai.transform.SetPositionAndRotation(new Vector3(0, 0.05f, 3f), Quaternion.identity);
            if (kai.GetComponent<PlayerHealth>() == null) kai.AddComponent<PlayerHealth>();

            var so = new SerializedObject(levelManager);
            so.FindProperty("levelTitle").stringValue = "Sector 1-1 · Muelle de acoplamiento";
            so.FindProperty("objective").stringValue = "Abre el muelle y la grúa del hangar, y llega a la sala de control.";
            so.FindProperty("hint").stringValue =
                "Muelle: lanza la caja del andén contra la báscula de la pared este y ánclala. " +
                "Hangar: esquiva al dron entre los contenedores; para la grúa, sube antes la caja al techo.";
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene);
            UpdateMenuLogCount("Level_01_DockingBay", 1);
            EnsureAudioCatalog();
            Debug.Log("Level01Builder: OK");
        }

        // ---------- A: esclusa de entrada ----------

        static void BuildEntrance()
        {
            Floor(-AHalf, AHalf, AZ0, AZ1);
            Ceiling(-AHalf, AHalf, AZ0, AZ1, AHeight);
            WallX(AZ0, -AHalf, AHalf, AHeight, 0);
            WallZ(-AHalf, AZ0, AZ1, AHeight, 90);
            WallZ(AHalf, AZ0, AZ1, AHeight, -90);
            // Escalones de 1.2 m a los lados del paso central: se pueden saltar (salto de 1.4 m).
            Block(-AHalf, -2f, 6f, 10f, 1.2f);
            Block(2f, AHalf, 6f, 10f, 1.2f);
            Prop("container", new Vector3(-4.2f, 1.2f, 8.5f), 20);
            Prop("computer-wide", new Vector3(4.3f, 0f, 2.5f), -90);
            PipeColumn(new Vector3(-5.2f, 0, 1.2f), 8f, 90);
            Point("Light_A_0", new Vector3(0, 6.5f, 3f), PaleCyan, 7f, 11f);
            Point("Light_A_1", new Vector3(0, 6.5f, 9f), PaleCyan, 7f, 11f);
        }

        // ---------- B: muelle de carga ----------

        static void BuildDock(out Puzzles.DoorController doorB)
        {
            Floor(-BHalf, BHalf, AZ1, BZ1);
            Ceiling(-BHalf, BHalf, AZ1, BZ1, BHeight);
            WallX(AZ1, -BHalf, BHalf, BHeight, 180, 0f);   // pared sur, con el paso desde la esclusa
            WallZ(-BHalf, AZ1, BZ1, BHeight, 90);
            WallZ(BHalf, AZ1, BZ1, BHeight, -90);

            // Andén de carga al oeste, 2.4 m de alto: Kai no llega saltando.
            Block(-BHalf, -6f, 16f, 32f, 2.4f);
            Strip("DockEdge", new Vector3(-6.15f, 2.43f, 24f), new Vector3(0.3f, 0.04f, 16f), AmberMat);
            Prop("container-tall", new Vector3(-11.5f, 2.4f, 18.5f), 0);
            Prop("container", new Vector3(-11.5f, 2.4f, 29.5f), 15);
            Box("CargoBox_Dock", new Vector3(-8f, 2.4f, 24f));

            // Montacargas de la pared este: la báscula es la placa. Cubre de 2 a 7.5 m porque la
            // caja, al salir del borde del andén, se eleva un poco durante el vuelo.
            doorB = Door("Door_B", new Vector3(0, 0, BZ1));
            WallPlate("PressurePlate_Lift", new Vector3(12.5f, 4.75f, 24f), new Vector3(1.8f, 5.5f, 3.8f), 1, doorB);
            PipeColumn(new Vector3(13.2f, 0, 21.3f), 10f, -90);
            PipeColumn(new Vector3(13.2f, 0, 26.7f), 10f, -90);
            Prop("display-wall-wide", new Vector3(13.4f, 8.6f, 24f), -90);
            Strip("LiftMarker", new Vector3(13.36f, 4.75f, 24f), new Vector3(0.04f, 5.9f, 4.4f), AmberMat);

            // Maquinaria y contenedores fuera de la trayectoria de la caja (z 21..27).
            Prop("computer-system", new Vector3(9.5f, 0f, 14.6f), 0);
            Prop("container-wide", new Vector3(6f, 0f, 16f), 10);
            Prop("container", new Vector3(9.5f, 0f, 31.5f), 0);
            Prop("container-tall", new Vector3(6f, 0f, 32.5f), -10);
            Prop("container", new Vector3(-3.5f, 0f, 31.5f), 30);
            Prop("skip", new Vector3(-3.5f, 0f, 20f), 0);

            // Ruta alternativa: pasarela norte a 8 m, a la que se llega por la pared oeste.
            Catwalk(true, BZ1 - 2f, -BHalf, 2f, 8f, -1);
            DataLogAt("muelle", new Vector3(-0.5f, 9.4f, BZ1 - 2f), "ORION: \"El muelle lleva días cerrado. Nadie ha firmado la salida de la carga.\"");
            Point("Light_Amber_Catwalk", new Vector3(-0.5f, 10.5f, BZ1 - 2.5f), Amber, 2.5f, 5f);

            CheckpointAt("Checkpoint_Dock", new Vector3(0, 0.05f, AZ1 + 1.5f), BHalf * 2, BHeight);
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 2; j++)
                    Point($"Light_B_{i}{j}", new Vector3(-8f + j * 16f, 10f, 16f + i * 8f), PaleCyan, 8f, 14f);
            Point("Light_Amber_Lift", new Vector3(11f, 4.75f, 24f), Amber, 4.5f, 8f);
            Point("Light_Amber_DoorB", new Vector3(0, 3.6f, BZ1 - 2f), Amber, 3f, 5f);
        }

        // ---------- C: hangar con dron y grúa ----------

        static void BuildHangar(out Puzzles.DoorController doorC)
        {
            Floor(-CHalf, CHalf, BZ1, CZ1);
            Ceiling(-CHalf, CHalf, BZ1, CZ1, CHeight);
            WallX(BZ1, -CHalf, CHalf, CHeight, 180, 0f);   // pared entre muelle y hangar
            WallZ(-CHalf, BZ1, CZ1, CHeight, 90);
            WallZ(CHalf, BZ1, CZ1, CHeight, -90);
            WallX(CZ1, -CHalf, CHalf, CHeight, 180, 0f);

            // Bloques de contenedores a los lados del pasillo central (cobertura frente al dron).
            Prop("container-wide", new Vector3(-12f, 0f, 42f), 0);
            Prop("container-tall", new Vector3(-12f, 0f, 46.5f), 0);
            Prop("container", new Vector3(-12f, 0f, 52f), 0);           // base de la caja del hangar
            Prop("container-wide", new Vector3(-12f, 0f, 58f), 0);
            Prop("container", new Vector3(-12f, 2.8f, 58f), 25);
            Prop("container-flat", new Vector3(-7f, 0f, 44f), 0);
            Prop("container", new Vector3(-7f, 0f, 56f), 0);
            Prop("container-tall", new Vector3(12f, 0f, 42f), 0);
            Prop("container-wide", new Vector3(12f, 0f, 47f), 0);
            Prop("container", new Vector3(12f, 2.8f, 47f), -15);
            Prop("container-flat", new Vector3(12f, 0f, 54f), 180);
            Prop("container-tall", new Vector3(12f, 0f, 60f), 0);
            Prop("container", new Vector3(7f, 0f, 50f), 0);
            Prop("container-wide", new Vector3(7f, 0f, 60f), 0);
            Prop("computer-system", new Vector3(-13.5f, 0f, 65.4f), 180);
            Prop("computer-wide", new Vector3(13.6f, 0f, 65.6f), 180);
            Box("CargoBox_Hangar", new Vector3(-12f, 2.4f, 52f));

            // Pasarelas altas (8 m) en las dos paredes largas: ruta segura por encima del dron.
            Catwalk(false, -CHalf + 2f, BZ1 + 4f, CZ1 - 4f, 8f, 1);
            Catwalk(false, CHalf - 2f, BZ1 + 4f, CZ1 - 4f, 8f, -1);

            // Grúa: raíles en el techo y la pinza (placa) en lo alto de la pared este.
            for (float x = -14f; x < 16f; x += 4f)
            {
                Prop("rail", new Vector3(x + 2f, CHeight - 1.7f, 49.8f), 0);
                Prop("rail", new Vector3(x + 2f, CHeight - 1.7f, 54.2f), 0);
            }
            doorC = Door("Door_C", new Vector3(0, 0, CZ1));
            WallPlate("PressurePlate_Crane", new Vector3(16.5f, 14.2f, 52f), new Vector3(1.8f, 3.4f, 3.8f), 1, doorC);
            Strip("CraneMarker", new Vector3(17.36f, 14.2f, 52f), new Vector3(0.04f, 3.8f, 4.4f), AmberMat);
            PipeColumn(new Vector3(17.2f, 0, 49.4f), 12f, -90);
            PipeColumn(new Vector3(17.2f, 0, 54.6f), 12f, -90);

            // Dron: patrulla el pasillo central entre z = 42 y z = 62, por debajo de 6.5 m. La entrada
            // (z < 42) y la puerta de la grúa (z > 62) quedan fuera de su zona: son zonas seguras.
            var zone = new Bounds(new Vector3(0, 3.5f, 52f), new Vector3(16f, 6f, 20f));
            Drone("SecurityDrone_Hangar", new[]
            {
                new Vector3(4.5f, 1.8f, 60f), new Vector3(-4.5f, 1.8f, 60f),
                new Vector3(-4.5f, 1.8f, 44f), new Vector3(4.5f, 1.8f, 44f),
            }, zone);
            Strip("PatrolLine_W", new Vector3(-4.5f, 0.02f, 52f), new Vector3(0.12f, 0.02f, 16f), AmberMat);
            Strip("PatrolLine_E", new Vector3(4.5f, 0.02f, 52f), new Vector3(0.12f, 0.02f, 16f), AmberMat);
            Strip("SafeLine_South", new Vector3(0, 0.02f, 42f), new Vector3(16f, 0.02f, 0.2f), AmberMat);
            Strip("SafeLine_North", new Vector3(0, 0.02f, 62f), new Vector3(16f, 0.02f, 0.2f), AmberMat);

            CheckpointAt("Checkpoint_Hangar", new Vector3(0, 0.05f, BZ1 + 2.5f), CHalf * 2, CHeight);
            CheckpointAt("Checkpoint_Gate", new Vector3(0, 0.05f, CZ1 - 3.5f), CHalf * 2, CHeight);
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 2; j++)
                    Point($"Light_C_{i}{j}", new Vector3(-9f + j * 18f, 13f, 40f + i * 8f), PaleCyan, 9f, 16f);
            Point("Light_Center", new Vector3(0, 9f, 50f), PaleCyan, 6f, 14f);
            Point("Light_Amber_Crane", new Vector3(15f, 13f, 52f), Amber, 5f, 8f);
            Point("Light_Amber_DoorC", new Vector3(0, 3.6f, CZ1 - 2f), Amber, 3f, 5f);
        }

        // ---------- Menú y audio ----------

        static void UpdateMenuLogCount(string sceneName, int count)
        {
            var scene = EditorSceneManager.OpenScene("Assets/GravedadCero/Scenes/MainMenu.unity");
            var menu = Object.FindAnyObjectByType<MainMenuUI>();
            var so = new SerializedObject(menu);
            var sections = so.FindProperty("sections");
            for (int i = 0; i < sections.arraySize; i++)
            {
                var e = sections.GetArrayElementAtIndex(i);
                if (e.FindPropertyRelative("sceneName").stringValue == sceneName) e.FindPropertyRelative("dataLogs").intValue = count;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.OpenScene(ScenePath);
        }

        /// <summary>Asigna los sonidos de drones, golpes, registros y cajas si el catálogo aún no los tiene.</summary>
        static void EnsureAudioCatalog()
        {
            var catalog = Load<AudioCatalog>("Assets/GravedadCero/Audio/Resources/AudioCatalog.asset");
            AudioClip K(string n) => Load<AudioClip>($"Assets/GravedadCero/Audio/Kenney_SciFiSounds/{n}.ogg");
            if (catalog.droneHum == null) catalog.droneHum = K("spaceEngineSmall_000");
            if (catalog.droneAlert == null) catalog.droneAlert = K("computerNoise_000");
            if (catalog.droneStunned == null) catalog.droneStunned = K("explosionCrunch_000");
            if (catalog.playerHit == null) catalog.playerHit = K("impactMetal_002");
            if (catalog.dataLog == null) catalog.dataLog = K("forceField_002");
            if (catalog.boxRespawn == null) catalog.boxRespawn = K("laserRetro_002");
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }
    }
}
