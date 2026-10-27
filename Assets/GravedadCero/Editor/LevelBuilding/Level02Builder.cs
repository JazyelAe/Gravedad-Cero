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
    /// Construye la sección 1-2, Pozo de mantenimiento: una galería de servicio con un foso que se
    /// cruza por el techo, y un pozo vertical de 20 × 16 × 48 m que se baja caminando por sus
    /// paredes. Tiene tres descansillos (este, oeste y la cornisa de la sala de bombas), una
    /// plataforma suspendida a la que solo se llega desde el techo, ventilas de vapor
    /// intermitentes, dos válvulas que abren la sala de bombas y un dron que vigila la parte
    /// central. El fondo es refrigerante (zona roja). Ver Documentation/LEVEL_01.md.
    ///
    /// Menú: Gravedad Cero → Niveles → Reconstruir 1-2.
    /// Línea de comandos: -executeMethod GravedadCero.EditorTools.Level02Builder.RunFromCommandLine
    /// </summary>
    public static class Level02Builder
    {
        const string ScenePath = "Assets/GravedadCero/Scenes/Level_01_2_MaintenanceShaft.unity";

        // Galería (x ±4, z -20..0, 8 m de alto) y pozo (x ±10, z 0..16, de y = -40 a y = 8).
        const float GHalf = 4f, GZ0 = -20f, GHeight = 8f;
        const float SHalf = 10f, SZ1 = 16f, STop = 8f, SBottom = -40f;
        // Alturas de las superficies de los descansillos.
        const float L1Y = -14f, PlatformY = -20f, L2Y = -28f, LedgeY = -36f;

        [MenuItem("Gravedad Cero/Niveles/Reconstruir 1-2")]
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

            var doorShaft = BuildGallery();
            BuildShaft(doorShaft, levelManager);

            var kai = GameObject.FindWithTag("Player");
            kai.transform.SetPositionAndRotation(new Vector3(0, 0.05f, GZ0 + 1.5f), Quaternion.identity);
            if (kai.GetComponent<PlayerHealth>() == null) kai.AddComponent<PlayerHealth>();

            var so = new SerializedObject(levelManager);
            so.FindProperty("levelTitle").stringValue = "Sector 1-2 · Pozo de mantenimiento";
            so.FindProperty("objective").stringValue = "Baja por el pozo hasta la sala de bombas y abre sus dos válvulas.";
            so.FindProperty("hint").stringValue =
                "Cruza el foso de la galería por el techo (F). En el pozo, baja caminando por las paredes, " +
                "espera a que se apague el vapor y vuelve a la gravedad normal encima de cada descansillo.";
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene);
            SetMenuLogCount("Level_01_2_MaintenanceShaft", 1);
            EnsureVentSound();
            Debug.Log("Level02Builder: OK");
        }

        // ---------- Galería de servicio (enseña a usar el techo) ----------

        static Puzzles.DoorController BuildGallery()
        {
            Floor(-GHalf, GHalf, GZ0, -16f);
            Floor(-GHalf, GHalf, -16f, -8f, -8f, "floor-panel");   // foso de 8 m
            Floor(-GHalf, GHalf, -8f, 0f);
            Ceiling(-GHalf, GHalf, GZ0, 0f, GHeight);
            WallX(GZ0, -GHalf, GHalf, GHeight, 0);
            WallZ(-GHalf, GZ0, 0f, GHeight, 90);
            WallZ(GHalf, GZ0, 0f, GHeight, -90);
            WallZRange(-GHalf, -16f, -8f, -8f, 0f, 90);
            WallZRange(GHalf, -16f, -8f, -8f, 0f, -90);
            WallXRange(-16f, -GHalf, GHalf, -8f, 0f, 0);
            WallXRange(-8f, -GHalf, GHalf, -8f, 0f, 180);
            Hazard("GalleryPitHazard", new Vector3(0, -6.5f, -12f), new Vector3(8f, 3f, 8f));
            Strip("GalleryPitGlow", new Vector3(0, -7.95f, -12f), new Vector3(7.8f, 0.05f, 7.8f), RedMat);
            Strip("GalleryPitEdge_S", new Vector3(0, 0.03f, -16.15f), new Vector3(7.8f, 0.04f, 0.3f), AmberMat);
            Strip("GalleryPitEdge_N", new Vector3(0, 0.03f, -7.85f), new Vector3(7.8f, 0.04f, 0.3f), AmberMat);
            Prop("container-tall", new Vector3(-2.6f, 0f, GZ0 + 2.5f), 0);
            Prop("computer-wide", new Vector3(2.4f, 0f, -3f), -90);
            PipeColumn(new Vector3(3.3f, 0f, -18f), 8f, -90);

            var doorShaft = Door("Door_Shaft", new Vector3(0, 0, 0));
            Switch("Switch_GalleryCeiling", new Vector3(0, GHeight - 0.3f, -12f), Quaternion.Euler(180, 0, 0), doorShaft);
            Strip("GalleryCeilingMarker", new Vector3(0, GHeight - 0.04f, -12f), new Vector3(2.2f, 0.04f, 2.2f), AmberMat);

            Point("Light_Gallery_0", new Vector3(0, 6.5f, -17f), PaleCyan, 6f, 10f);
            Point("Light_Gallery_1", new Vector3(0, 6.5f, -4f), PaleCyan, 6f, 10f);
            Point("Light_Gallery_Pit", new Vector3(0, -5f, -12f), Red, 5f, 9f);
            Point("Light_Amber_GallerySwitch", new Vector3(0, 6.6f, -12f), Amber, 3f, 5f);
            return doorShaft;
        }

        // ---------- Pozo ----------

        static void BuildShaft(Puzzles.DoorController doorShaft, LevelManager levelManager)
        {
            Floor(-SHalf, SHalf, 0f, SZ1, SBottom, "floor-panel");
            Ceiling(-SHalf, SHalf, 0f, SZ1, STop);
            WallZRange(-SHalf, 0f, SZ1, SBottom, STop, 90);
            WallZRange(SHalf, 0f, SZ1, SBottom, STop, -90);
            WallXRange(0f, -SHalf, SHalf, SBottom, STop, 0, new Vector2(0, 0));
            WallXRange(SZ1, -SHalf, SHalf, SBottom, STop, 180, new Vector2(0, LedgeY));

            // Plataforma superior, a lo largo de la pared sur, con barandilla.
            Floor(-SHalf, SHalf, 0f, 4f, 0f);
            for (float x = -8f; x <= 8f; x += 4f) Prop("rail", new Vector3(x, 0f, 3.8f), 0);
            CheckpointBox("Checkpoint_Top", new Vector3(0, 0.05f, 2f), new Bounds(new Vector3(0, 4f, 1.25f), new Vector3(20f, 8f, 1.5f)));

            // Descansillo 1 (pared este) con la válvula 1.
            Floor(6f, SHalf, 4f, 12f, L1Y);
            var valve1 = Latch("Latch_Valve1");
            Switch("Valve_1", new Vector3(8f, L1Y + 0.3f, 5.2f), Quaternion.identity, valve1);
            CheckpointBox("Checkpoint_East", new Vector3(8f, L1Y + 0.05f, 9.5f), new Bounds(new Vector3(8f, L1Y + 2f, 8f), new Vector3(3.4f, 4f, 8f)));

            // Plataforma suspendida en el centro (solo se llega desde el techo) con el registro.
            Floor(-2f, 2f, 6f, 10f, PlatformY, "floor-panel");
            PipeColumn(new Vector3(-1.6f, PlatformY, 6.4f), STop - PlatformY, 0);
            PipeColumn(new Vector3(1.6f, PlatformY, 9.6f), STop - PlatformY, 0);
            Strip("PlatformCeilingMarker", new Vector3(0, STop - 0.03f, 8f), new Vector3(4f, 0.04f, 4f), AmberMat);
            DataLogAt("pozo", new Vector3(0, PlatformY + 0.8f, 8f), "ORION: \"Las bombas se pararon solas. El refrigerante sigue subiendo.\"");

            // Descansillo 2 (pared oeste). La válvula 2 está en la propia pared, más arriba.
            Floor(-SHalf, -6f, 4f, 12f, L2Y);
            for (float z = 6f; z <= 10f; z += 4f) Prop("rail", new Vector3(-6.2f, L2Y, z), 90);
            var valve2 = Latch("Latch_Valve2");
            Switch("Valve_2", new Vector3(-SHalf + 0.9f, PlatformY, 8f), Quaternion.Euler(0, 0, -90), valve2);
            Strip("Valve2Marker", new Vector3(-SHalf + 0.63f, PlatformY, 8f), new Vector3(0.04f, 2.2f, 2.2f), AmberMat);
            CheckpointBox("Checkpoint_West", new Vector3(-8f, L2Y + 0.05f, 9.5f), new Bounds(new Vector3(-8f, L2Y + 2f, 8f), new Vector3(3.4f, 4f, 8f)));

            // Cornisa de la sala de bombas (pared norte) y compuerta que abren las dos válvulas.
            Floor(-6f, 6f, 12f, SZ1, LedgeY);
            for (float x = -4f; x <= 4f; x += 4f) Prop("rail", new Vector3(x, LedgeY, 12.2f), 0);
            var doorExit = Door("Door_Exit", new Vector3(0, LedgeY, SZ1));
            Gate("Gate_PumpRoom", new Puzzles.PressurePlate[0], new Puzzles.Activatable[] { valve1, valve2 }, doorExit);
            CheckpointBox("Checkpoint_Ledge", new Vector3(0, LedgeY + 0.05f, 13.5f), new Bounds(new Vector3(0, LedgeY + 2f, 13.7f), new Vector3(12f, 4f, 3.4f)));
            Prop("computer-system", new Vector3(-4.2f, LedgeY, 14.2f), 180);
            ExitRoom(SZ1, levelManager, LedgeY);

            // Ventilas de vapor intermitentes en cada tramo de pared.
            SteamVent("Vent_East", new Bounds(new Vector3(SHalf - 1.5f, -7f, 8f), new Vector3(1.8f, 2f, 14.8f)), Vector3.left, 2.2f, 0.7f, 1.8f, 0f);
            SteamVent("Vent_West", new Bounds(new Vector3(-SHalf + 1.5f, -23f, 8f), new Vector3(1.8f, 2f, 14.8f)), Vector3.right, 2.2f, 0.7f, 1.8f, 1.3f);
            SteamVent("Vent_North", new Bounds(new Vector3(0, -31f, SZ1 - 1.5f), new Vector3(18.8f, 2f, 1.8f)), Vector3.back, 2.0f, 0.7f, 1.6f, 0.6f);

            // Tuberías tumbadas: obstáculos que hay que saltar al bajar por las paredes.
            for (float z = 0.6f; z < 15f; z += 2f) StaticRot("pipe", new Vector3(-SHalf + 1.1f, -16f, z), Quaternion.Euler(90, 0, 0));
            for (float x = -7.4f; x < 9f; x += 2f) StaticRot("pipe", new Vector3(x, -28.8f, SZ1 - 1.1f), Quaternion.Euler(0, 0, 90));
            PipeColumn(new Vector3(SHalf - 0.8f, SBottom, 1.4f), STop - SBottom, -90);
            PipeColumn(new Vector3(-SHalf + 0.8f, SBottom, 14.6f), STop - SBottom, 90);

            // Fondo: refrigerante.
            Hazard("CoolantHazard", new Vector3(0, SBottom + 1.5f, 8f), new Vector3(20f, 3f, 16f));
            Strip("CoolantGlow", new Vector3(0, SBottom + 0.05f, 8f), new Vector3(19.8f, 0.05f, 15.8f), RedMat);

            // Dron: vigila la parte central del pozo, entre y = -25 y y = -15.
            var zone = new Bounds(new Vector3(0, -20f, 8f), new Vector3(17.6f, 10f, 14f));
            Drone("SecurityDrone_Shaft", new[]
            {
                new Vector3(-5f, -17.5f, 3f), new Vector3(5f, -17.5f, 3f),
                new Vector3(5f, -17.5f, 13f), new Vector3(-5f, -17.5f, 13f),
            }, zone);

            for (float y = 4f; y > SBottom; y -= 8f)
            {
                Point($"Light_Shaft_W{y}", new Vector3(-6f, y, 8f), PaleCyan, 8f, 14f);
                Point($"Light_Shaft_E{y}", new Vector3(6f, y, 8f), PaleCyan, 8f, 14f);
            }
            Point("Light_Coolant", new Vector3(0, SBottom + 3f, 8f), Red, 8f, 16f);
            Point("Light_Amber_Valve1", new Vector3(7f, L1Y + 1.5f, 5.2f), Amber, 3f, 5f);
            Point("Light_Amber_Valve2", new Vector3(-8f, PlatformY, 8f), Amber, 3f, 5f);
            Point("Light_Amber_Ledge", new Vector3(0, LedgeY + 3f, 14f), Amber, 4f, 7f);
            Point("Light_Amber_Platform", new Vector3(0, PlatformY + 2f, 8f), Amber, 2.5f, 5f);
            Point("Light_Exit", new Vector3(0, LedgeY + 3.2f, SZ1 + 4f), PaleCyan, 5f, 8f);
        }

        // ---------- Menú y audio ----------

        static void SetMenuLogCount(string sceneName, int count)
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

        static void EnsureVentSound()
        {
            var catalog = Load<AudioCatalog>("Assets/GravedadCero/Audio/Resources/AudioCatalog.asset");
            if (catalog.steamVent == null) catalog.steamVent = Load<AudioClip>("Assets/GravedadCero/Audio/Kenney_SciFiSounds/thrusterFire_000.ogg");
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }
    }
}
