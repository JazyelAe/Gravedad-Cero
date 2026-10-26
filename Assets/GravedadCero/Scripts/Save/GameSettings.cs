using UnityEngine;

namespace GravedadCero.Save
{
    /// <summary>
    /// Opciones del jugador guardadas con PlayerPrefs: sensibilidad del mouse e inversión
    /// del eje Y de la cámara. Los valores se guardan en memoria para no leer PlayerPrefs
    /// en cada frame; GravityCamera los consulta directamente, así que los cambios se
    /// aplican al instante.
    /// </summary>
    public static class GameSettings
    {
        const string SensitivityKey = "gravedadcero.mouseSensitivity";
        const string InvertYKey = "gravedadcero.invertY";

        public const float MinSensitivity = 0.03f;
        public const float MaxSensitivity = 0.40f;
        public const float DefaultSensitivity = 0.12f;

        static bool loaded;
        static float sensitivity;
        static bool invertY;

        /// <summary>Grados de giro de cámara por píxel de movimiento del mouse.</summary>
        public static float MouseSensitivity
        {
            get { EnsureLoaded(); return sensitivity; }
            set
            {
                EnsureLoaded();
                sensitivity = Mathf.Clamp(value, MinSensitivity, MaxSensitivity);
                PlayerPrefs.SetFloat(SensitivityKey, sensitivity);
                PlayerPrefs.Save();
            }
        }

        public static bool InvertY
        {
            get { EnsureLoaded(); return invertY; }
            set
            {
                EnsureLoaded();
                invertY = value;
                PlayerPrefs.SetInt(InvertYKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        /// <summary>Vuelve a leer los valores guardados (por ejemplo, al iniciar el juego).</summary>
        public static void Reload()
        {
            sensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(SensitivityKey, DefaultSensitivity), MinSensitivity, MaxSensitivity);
            invertY = PlayerPrefs.GetInt(InvertYKey, 0) == 1;
            loaded = true;
        }

        static void EnsureLoaded()
        {
            if (!loaded) Reload();
        }
    }
}
