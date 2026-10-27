using System.Linq;
using GravedadCero.Enemies;
using GravedadCero.Level;
using GravedadCero.Puzzles;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace GravedadCero.EditorTools
{
    /// <summary>
    /// Piezas para construir niveles por código con el Space Station Kit de Kenney (módulos de
    /// 4 m): salas cerradas, líneas de pared con puertas, plataformas, pasarelas, contenedores,
    /// mecanismos (placas, interruptores, compuertas), puntos de control, drones y registros.
    /// Las salas siempre se cierran por los seis lados: cualquier giro de gravedad lleva a una
    /// superficie. Solo se usa en el editor; los constructores de cada nivel la llaman.
    ///
    /// Medidas del kit (escala de importación ×4): suelo 4×1.2×4 con el origen abajo; pared
    /// 4×4 de 1.2 de grosor, centrada en su línea; pasarela 4×0.6×2.8; contenedores de 2.4 a
    /// 3.6 m de alto (Kai salta 1.4 m).
    /// </summary>
    public static class LevelKit
    {
        const string Models = "Assets/GravedadCero/Models/Kenney_SpaceStation/Models";
        public static readonly Color PaleCyan = new Color(0.55f, 0.85f, 1f);
        public static readonly Color Amber = new Color(1f, 0.6f, 0.1f);
        public static readonly Color Red = new Color(1f, 0.25f, 0.15f);

        public static Transform Geometry, Decoration, Puzzle, Enemies, Lighting;
        public static Material White, CyanMat, Dark, AmberMat, RedMat;
        static GameObject boxPrefab;

        // ---------- Preparación ----------

        /// <summary>
        /// Vacía la geometría, la decoración, los puzles, los enemigos y las luces puntuales de la
        /// escena abierta. Conserva Kai, cámara, sistemas, HUD, pausa, EventSystem y la luz base.
        /// </summary>
        public static void Begin()
        {
            White = Load<Material>("Assets/GravedadCero/Materials/M_PanelWhite.mat");
            CyanMat = Load<Material>("Assets/GravedadCero/Materials/M_EmissiveCyan.mat");
            Dark = Load<Material>("Assets/GravedadCero/Materials/M_MetalDark.mat");
            AmberMat = Load<Material>("Assets/GravedadCero/Materials/M_WarningAmber.mat");
            RedMat = Load<Material>("Assets/GravedadCero/Materials/M_DangerRed.mat");
            boxPrefab = Load<GameObject>("Assets/GravedadCero/Prefabs/CargoBox.prefab");

            foreach (string name in new[] { "Geometry", "Decoration", "Puzzle", "Enemies" })
            {
                var old = FindRoot(name);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }
            Lighting = FindRoot("Lighting") ?? new GameObject("Lighting").transform;
            foreach (Transform t in Lighting.Cast<Transform>().ToList())
            {
                var l = t.GetComponent<Light>();
                if (l != null && l.type == LightType.Point) Object.DestroyImmediate(t.gameObject);
            }
            Geometry = new GameObject("Geometry").transform;
            Decoration = new GameObject("Decoration").transform;
            Puzzle = new GameObject("Puzzle").transform;
            Enemies = new GameObject("Enemies").transform;
        }

        public static Transform FindRoot(string name)
        {
            var go = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(g => g.name == name);
            return go != null ? go.transform : null;
        }

        // ---------- Arquitectura ----------

        /// <summary>Suelo de baldosas de 4 m con la cara superior a la altura topY.</summary>
        public static void Floor(float x0, float x1, float z0, float z1, float topY = 0f, string model = "floor")
        {
            for (float x = x0 + 2; x < x1; x += 4)
                for (float z = z0 + 2; z < z1; z += 4)
                    Static(model, new Vector3(x, topY - 1.2f, z), 0);
        }

        /// <summary>Techo con la cara inferior a la altura bottomY (no proyecta sombras).</summary>
        public static void Ceiling(float x0, float x1, float z0, float z1, float bottomY)
        {
            for (float x = x0 + 2; x < x1; x += 4)
                for (float z = z0 + 2; z < z1; z += 4)
                    NoShadows(Static("floor", new Vector3(x, bottomY, z), 0));
        }

        /// <summary>
        /// Línea de pared a z constante, de x0 a x1 y de altura height (múltiplo de 4). yaw 0 mira
        /// al norte (+z), 180 al sur. doorX: centros de módulo con hueco de puerta (en el nivel 0).
        /// </summary>
        public static void WallX(float z, float x0, float x1, float height, float yaw, params float[] doorX) =>
            WallXRange(z, x0, x1, 0f, height, yaw, doorX.Select(d => new Vector2(d, 0f)).ToArray());

        /// <summary>Como WallX, pero de y0 a y1 (múltiplos de 4) y con puertas en (x, altura de su base).</summary>
        public static void WallXRange(float z, float x0, float x1, float y0, float y1, float yaw, params Vector2[] doors)
        {
            for (float x = x0 + 2; x < x1; x += 4)
                for (float y = y0; y < y1; y += 4)
                {
                    bool door = doors.Any(d => Mathf.Abs(d.x - x) < 0.1f && Mathf.Abs(d.y - y) < 0.1f);
                    Static(door ? "wall-door" : WallVariant(x, y - y0, z), new Vector3(x, y, z), yaw);
                }
        }

        /// <summary>Línea de pared a x constante, de z0 a z1. yaw 90 mira al este, -90 al oeste.</summary>
        public static void WallZ(float x, float z0, float z1, float height, float yaw, params float[] doorZ) =>
            WallZRange(x, z0, z1, 0f, height, yaw, doorZ.Select(d => new Vector2(d, 0f)).ToArray());

        /// <summary>Como WallZ, pero de y0 a y1 y con puertas en (z, altura de su base).</summary>
        public static void WallZRange(float x, float z0, float z1, float y0, float y1, float yaw, params Vector2[] doors)
        {
            for (float z = z0 + 2; z < z1; z += 4)
                for (float y = y0; y < y1; y += 4)
                {
                    bool door = doors.Any(d => Mathf.Abs(d.x - z) < 0.1f && Mathf.Abs(d.y - y) < 0.1f);
                    Static(door ? "wall-door" : WallVariant(z, y - y0, x), new Vector3(x, y, z), yaw);
                }
        }

        // Variedad sin aleatoriedad: ventanas y paneles en el nivel 1, pilares de vez en cuando.
        static string WallVariant(float along, float y, float line)
        {
            int i = Mathf.RoundToInt(along / 4f + line * 0.37f);
            int level = Mathf.RoundToInt(y / 4f);
            if (level % 3 == 1 && Mathf.Abs(i) % 5 == 2) return "wall-window";
            if (level == 0 && Mathf.Abs(i) % 6 == 3) return "wall-pillar";
            if (level % 3 == 0 && Mathf.Abs(i) % 7 == 5) return "wall-banner";
            return "wall";
        }

        /// <summary>Bloque macizo (por ejemplo, un andén) de baldosas apiladas hasta topY (múltiplo de 1.2).</summary>
        public static void Block(float x0, float x1, float z0, float z1, float topY, string topModel = "floor-panel")
        {
            for (float y = 1.2f; y <= topY + 0.01f; y += 1.2f)
                Floor(x0, x1, z0, z1, y, y + 1.2f > topY + 0.01f ? topModel : "floor");
        }

        /// <summary>
        /// Pasarela de 2.8 m de fondo a la altura y (cara superior y + 0.6), pegada a una pared.
        /// alongX: true = paralela al eje X con centro en z = fixedCoord; false = paralela a Z
        /// con centro en x = fixedCoord. openSide (+1/-1) indica hacia dónde queda el lado abierto,
        /// donde va la barandilla fina (rail, 0.4 m) que no corta el paso a lo largo.
        /// </summary>
        public static void Catwalk(bool alongX, float fixedCoord, float from, float to, float y, float openSide)
        {
            float yaw = alongX ? 0f : 90f;
            float railOffset = openSide * 1.2f;
            for (float c = from + 2; c < to; c += 4)
            {
                var pos = alongX ? new Vector3(c, y, fixedCoord) : new Vector3(fixedCoord, y, c);
                Static("balcony-floor", pos, yaw);
                var railPos = alongX ? new Vector3(c, y + 0.6f, fixedCoord + railOffset) : new Vector3(fixedCoord + railOffset, y + 0.6f, c);
                Static("rail", railPos, yaw);
            }
        }

        public static GameObject Prop(string model, Vector3 position, float yaw = 0f) => Static(model, position, yaw);

        /// <summary>Pieza con una rotación cualquiera (por ejemplo, una tubería tumbada).</summary>
        public static GameObject StaticRot(string model, Vector3 position, Quaternion rotation)
        {
            var go = Static(model, position, 0f);
            go.transform.rotation = rotation;
            return go;
        }

        /// <summary>Tubería vertical de altura height (tramos de 2 m) pegada a una pared.</summary>
        public static void PipeColumn(Vector3 basePosition, float height, float yaw)
        {
            for (float y = 0; y < height - 0.01f; y += 2f) Static(y == 0 ? "pipe-ring" : "pipe", basePosition + Vector3.up * y, yaw);
        }

        // ---------- Mecanismos ----------

        public static DoorController Door(string name, Vector3 position)
        {
            var doorGo = new GameObject(name);
            doorGo.transform.SetParent(Puzzle, false);
            doorGo.transform.position = position;
            var panel = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>($"{Models}/door-single.fbx"), doorGo.transform);
            PrefabUtility.UnpackPrefabInstance(panel, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            panel.transform.localPosition = Vector3.zero;
            panel.transform.localRotation = Quaternion.identity;
            panel.name = "Panel";
            foreach (var mc in panel.GetComponentsInChildren<MeshCollider>()) Object.DestroyImmediate(mc);
            var box = panel.AddComponent<BoxCollider>();
            box.center = new Vector3(0, 1.4f, 0);
            box.size = new Vector3(1.6f, 2.8f, 0.4f);
            var door = doorGo.AddComponent<DoorController>();
            SetRef(door, "panel", panel.transform);
            return door;
        }

        /// <summary>Placa en una pared con normal ±X (wallSide +1 = pared este, -1 = oeste).</summary>
        public static PressurePlate WallPlate(string name, Vector3 position, Vector3 triggerSize, float wallSide, Activatable target = null)
        {
            var plate = PlateBase(name, position, triggerSize);
            Primitive(plate.transform, "Surface", new Vector3(0.83f * wallSide, 0, 0), new Vector3(0.1f, triggerSize.y - 0.2f, triggerSize.z - 0.2f), Dark);
            var frame = Primitive(plate.transform, "Indicator", new Vector3(0.86f * wallSide, 0, 0), new Vector3(0.06f, triggerSize.y + 0.1f, triggerSize.z + 0.1f), White);
            SetRef(plate, "indicator", frame.GetComponent<Renderer>());
            if (target != null) Targets(plate, target);
            return plate;
        }

        public static PressurePlate FloorPlate(string name, Vector3 position, Vector3 triggerSize, Activatable target = null)
        {
            var plate = PlateBase(name, position, triggerSize);
            float bottom = -triggerSize.y / 2f;
            Primitive(plate.transform, "Surface", new Vector3(0, bottom + 0.06f, 0), new Vector3(triggerSize.x - 0.2f, 0.1f, triggerSize.z - 0.2f), Dark);
            var frame = Primitive(plate.transform, "Indicator", new Vector3(0, bottom + 0.03f, 0), new Vector3(triggerSize.x + 0.1f, 0.05f, triggerSize.z + 0.1f), White);
            SetRef(plate, "indicator", frame.GetComponent<Renderer>());
            if (target != null) Targets(plate, target);
            return plate;
        }

        static PressurePlate PlateBase(string name, Vector3 position, Vector3 triggerSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Puzzle, false);
            go.transform.position = position;
            var trigger = go.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = triggerSize;
            return go.AddComponent<PressurePlate>();
        }

        static void Targets(PressurePlate plate, Activatable target)
        {
            var so = new SerializedObject(plate);
            var targets = so.FindProperty("targets");
            targets.arraySize = 1;
            targets.GetArrayElementAtIndex(0).objectReferenceValue = target;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static GameObject Box(string name, Vector3 position)
        {
            var box = (GameObject)PrefabUtility.InstantiatePrefab(boxPrefab, Puzzle);
            box.name = name;
            box.transform.position = position;
            return box;
        }

        /// <summary>Punto de control que ocupa toda la sección transversal en z; Kai reaparece en spawn.</summary>
        public static Checkpoint CheckpointAt(string name, Vector3 spawn, float width, float height, float centerX = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Puzzle, false);
            go.transform.SetPositionAndRotation(spawn, Quaternion.identity);
            var c = go.AddComponent<BoxCollider>();
            c.isTrigger = true;
            c.center = new Vector3(centerX - spawn.x, height / 2f - spawn.y, 0);
            c.size = new Vector3(width, height, 1.5f);
            return go.AddComponent<Checkpoint>();
        }

        /// <summary>Punto de control con un volumen arbitrario (en coordenadas del mundo).</summary>
        public static Checkpoint CheckpointBox(string name, Vector3 spawn, Bounds volume)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Puzzle, false);
            go.transform.SetPositionAndRotation(spawn, Quaternion.identity);
            var c = go.AddComponent<BoxCollider>();
            c.isTrigger = true;
            c.center = volume.center - spawn;
            c.size = volume.size;
            return go.AddComponent<Checkpoint>();
        }

        /// <summary>
        /// Ventila de vapor intermitente en una pared: zona peligrosa (solo encendida), franja de
        /// aviso sobre la pared y volumen de vapor. band es el volumen peligroso; wallNormal apunta
        /// desde la pared hacia el interior de la sala.
        /// </summary>
        public static TimedHazard SteamVent(string name, Bounds band, Vector3 wallNormal, float off, float warn, float on, float offset)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Puzzle, false);
            go.transform.position = band.center;
            var c = go.AddComponent<BoxCollider>();
            c.isTrigger = true;
            c.size = band.size;
            go.AddComponent<HazardZone>();

            // Franja sobre la pared (fina en la dirección de la normal) y volumen de vapor.
            Vector3 thin = new Vector3(Mathf.Abs(wallNormal.x), Mathf.Abs(wallNormal.y), Mathf.Abs(wallNormal.z));
            Vector3 stripSize = Vector3.Scale(band.size, Vector3.one - thin) + thin * 0.05f;
            float depth = Vector3.Dot(band.size, thin);
            var strip = Primitive(go.transform, "Strip", -wallNormal * (depth / 2f - 0.04f), stripSize, RedMat);
            var steam = Primitive(go.transform, "Steam", Vector3.zero, band.size * 0.98f, SteamMaterial());
            steam.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            var hazard = go.AddComponent<TimedHazard>();
            hazard.Configure(off, warn, on, offset, new[] { strip.GetComponent<Renderer>() }, steam.transform);
            return hazard;
        }

        /// <summary>Material semitransparente para el vapor (se crea una vez en Materials).</summary>
        static Material SteamMaterial()
        {
            const string path = "Assets/GravedadCero/Materials/M_Steam.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;
            mat = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = "M_Steam" };
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;
            mat.SetColor("_BaseColor", new Color(0.92f, 0.95f, 1f, 0.32f));
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        public static void Hazard(string name, Vector3 position, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Puzzle, false);
            go.transform.position = position;
            var c = go.AddComponent<BoxCollider>();
            c.isTrigger = true;
            c.size = size;
            go.AddComponent<HazardZone>();
        }

        /// <summary>Sala de salida de 4×8 m tras una compuerta en la pared norte (z = doorZ).</summary>
        public static void ExitRoom(float doorZ, LevelManager levelManager, float floorY = 0f)
        {
            for (int i = 0; i < 2; i++)
            {
                float cz = doorZ + 2 + i * 4;
                Static("floor-panel", new Vector3(0, floorY - 1.2f, cz), 0);
                NoShadows(Static("floor", new Vector3(0, floorY + 4f, cz), 0));
                Static("wall", new Vector3(-2, floorY, cz), 90);
                Static("wall", new Vector3(2, floorY, cz), -90);
            }
            Static("wall", new Vector3(0, floorY, doorZ + 8), 180);
            var exitGo = new GameObject("LevelExit");
            exitGo.transform.SetParent(Puzzle, false);
            exitGo.transform.position = new Vector3(0, floorY + 1f, doorZ + 5.5f);
            var t = exitGo.AddComponent<BoxCollider>();
            t.isTrigger = true;
            t.size = new Vector3(2.6f, 2f, 2.4f);
            SetRef(exitGo.AddComponent<LevelExit>(), "levelManager", levelManager);
            Primitive(exitGo.transform, "ExitMarker", new Vector3(0, -0.97f, 0), new Vector3(2.4f, 0.05f, 2.2f), CyanMat);
        }

        /// <summary>Interruptor (por ejemplo, una válvula). Su arriba local apunta hacia fuera de la superficie.</summary>
        public static SwitchController Switch(string name, Vector3 position, Quaternion rotation, Activatable target, bool oneShot = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Puzzle, false);
            go.transform.SetPositionAndRotation(position, rotation);
            var baseGo = Primitive(go.transform, "Base", new Vector3(0, -0.15f, 0), new Vector3(0.6f, 0.3f, 0.6f), Dark);
            baseGo.AddComponent<BoxCollider>();
            var indicator = Primitive(go.transform, "Indicator", new Vector3(0, 0.03f, 0), new Vector3(0.3f, 0.06f, 0.3f), White);
            var sw = go.AddComponent<SwitchController>();
            var so = new SerializedObject(sw);
            var targets = so.FindProperty("targets");
            targets.arraySize = 1;
            targets.GetArrayElementAtIndex(0).objectReferenceValue = target;
            so.FindProperty("indicator").objectReferenceValue = indicator.GetComponent<Renderer>();
            so.FindProperty("mode").enumValueIndex = (int)(oneShot ? SwitchController.SwitchMode.OneShot : SwitchController.SwitchMode.Lever);
            so.ApplyModifiedPropertiesWithoutUndo();
            return sw;
        }

        public static SignalLatch Latch(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Puzzle, false);
            return go.AddComponent<SignalLatch>();
        }

        /// <summary>Compuerta lógica "Y" entre placas y señales que activa target.</summary>
        public static ActivationGate Gate(string name, PressurePlate[] plates, Activatable[] signals, Activatable target)
        {
            var gate = new GameObject(name).AddComponent<ActivationGate>();
            gate.transform.SetParent(Puzzle, false);
            var so = new SerializedObject(gate);
            var inputs = so.FindProperty("inputs");
            inputs.arraySize = plates.Length;
            for (int i = 0; i < plates.Length; i++) inputs.GetArrayElementAtIndex(i).objectReferenceValue = plates[i];
            var sig = so.FindProperty("signals");
            sig.arraySize = signals.Length;
            for (int i = 0; i < signals.Length; i++) sig.GetArrayElementAtIndex(i).objectReferenceValue = signals[i];
            var targets = so.FindProperty("targets");
            targets.arraySize = 1;
            targets.GetArrayElementAtIndex(0).objectReferenceValue = target;
            so.ApplyModifiedPropertiesWithoutUndo();
            return gate;
        }

        // ---------- Enemigos y secretos ----------

        /// <summary>Dron de seguridad hecho con primitivas: cuerpo oscuro, anillo claro y ojo luminoso.</summary>
        public static SecurityDrone Drone(string name, Vector3[] waypoints, Bounds zone)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Enemies, false);
            go.transform.position = waypoints[0];
            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            var hull = go.AddComponent<SphereCollider>();
            hull.radius = 0.55f;
            var hitZone = go.AddComponent<SphereCollider>();
            hitZone.isTrigger = true;
            hitZone.radius = 1.05f;

            var visual = new GameObject("Body").transform;
            visual.SetParent(go.transform, false);
            Primitive(visual, "Hull", Vector3.zero, Vector3.one * 1.05f, Dark, PrimitiveType.Sphere);
            Primitive(visual, "Ring", Vector3.zero, new Vector3(1.7f, 0.05f, 1.7f), White, PrimitiveType.Cylinder);
            Primitive(visual, "Fin_Top", new Vector3(0, 0.55f, -0.1f), new Vector3(0.12f, 0.35f, 0.6f), White);
            var eye = Primitive(visual, "Eye", new Vector3(0, 0.05f, 0.45f), new Vector3(0.38f, 0.38f, 0.2f), CyanMat, PrimitiveType.Sphere);
            var lightGo = new GameObject("EyeLight");
            lightGo.transform.SetParent(visual, false);
            lightGo.transform.localPosition = new Vector3(0, 0.05f, 0.7f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 5f;
            light.intensity = 3f;
            light.shadows = LightShadows.None;

            var drone = go.AddComponent<SecurityDrone>();
            drone.Configure(waypoints, zone);
            SetRef(drone, "eye", eye.GetComponent<Renderer>());
            SetRef(drone, "eyeLight", light);
            return drone;
        }

        public static DataLog DataLogAt(string name, Vector3 position, string message)
        {
            var go = new GameObject("DataLog_" + name);
            go.transform.SetParent(Puzzle, false);
            go.transform.position = position;
            var c = go.AddComponent<SphereCollider>();
            c.isTrigger = true;
            c.radius = 0.9f;
            var visual = new GameObject("Visual").transform;
            visual.SetParent(go.transform, false);
            var core = Primitive(visual, "Core", Vector3.zero, new Vector3(0.45f, 0.45f, 0.45f), CyanMat);
            core.transform.localRotation = Quaternion.Euler(45, 0, 45);
            Primitive(visual, "Ring", Vector3.zero, new Vector3(0.9f, 0.03f, 0.9f), White, PrimitiveType.Cylinder);
            var lightGo = new GameObject("Glow");
            lightGo.transform.SetParent(visual, false);
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = PaleCyan;
            l.range = 4f;
            l.intensity = 2.5f;
            l.shadows = LightShadows.None;
            var log = go.AddComponent<DataLog>();
            log.Configure(name, message, visual);
            return log;
        }

        // ---------- Utilidades ----------

        public static void Point(string name, Vector3 position, Color color, float intensity, float range)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Lighting, false);
            go.transform.position = position;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.range = range;
            l.shadows = LightShadows.None;
        }

        public static GameObject Strip(string name, Vector3 position, Vector3 scale, Material material) => Primitive(Decoration, name, position, scale, material);

        public static GameObject Static(string model, Vector3 position, float yaw)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>($"{Models}/{model}.fbx"), Geometry);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            foreach (var t in go.GetComponentsInChildren<Transform>()) t.gameObject.isStatic = true;
            return go;
        }

        public static GameObject Primitive(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material,
            PrimitiveType type = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        static void NoShadows(GameObject go)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
        }

        public static T Load<T>(string path) where T : Object =>
            AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new System.Exception("No se encontró " + path);

        public static void SetRef(Object component, string field, Object value)
        {
            var so = new SerializedObject(component);
            var prop = so.FindProperty(field) ?? throw new System.Exception("Campo no encontrado: " + field);
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
