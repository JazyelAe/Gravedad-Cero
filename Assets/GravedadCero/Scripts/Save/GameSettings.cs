using UnityEngine;

namespace GravedadCero.Save
{
    /// <summary>
    /// Opciones del jugador guardadas con PlayerPrefs: sensibilidad del mouse, inversión
    /// del eje Y de la cámara y volumen de la música y de los efectos. Los valores se guardan en memoria para no leer PlayerPrefs
    /// en cada frame; GravityCamera los consulta directamente, así que los cambios se
    /// aplican al instante.
    /// </summary>
    public static class GameSettings
    {
        const string SensitivityKey = "gravedadcero.mouseSensitivity";
        const string InvertYKey = "gravedadcero.invertY";
        const string MusicVolumeKey = "gravedadcero.musicVolume";
        const string SfxVolumeKey = "gravedadcero.sfxVolume";

        public const float MinSensitivity = 0.03f;
        public const float MaxSensitivity = 0.40f;
        public const float DefaultSensitivity = 0.12f;
        public const float DefaultMusicVolume = 0.6f;
        public const float DefaultSfxVolume = 0.8f;

        static bool loaded;
        static float sensitivity;
        static bool invertY;
        static float musicVolume;
        static float sfxVolume;

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

        /// <summary>Volumen de la música, de 0 a 1.</summary>
        public static float MusicVolume
        {
            get { EnsureLoaded(); return musicVolume; }
            set
            {
                EnsureLoaded();
                musicVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
                PlayerPrefs.Save();
            }
        }

        /// <summary>Volumen de los efectos de sonido y de la interfaz, de 0 a 1.</summary>
        public static float SfxVolume
        {
            get { EnsureLoaded(); return sfxVolume; }
            set
            {
                EnsureLoaded();
                sfxVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
                PlayerPrefs.Save();
            }
        }

        /// <summary>Vuelve a leer los valores guardados (por ejemplo, al iniciar el juego).</summary>
        public static void Reload()
        {
            sensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(SensitivityKey, DefaultSensitivity), MinSensitivity, MaxSensitivity);
            invertY = PlayerPrefs.GetInt(InvertYKey, 0) == 1;
            musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, DefaultMusicVolume));
            sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, DefaultSfxVolume));
            loaded = true;
        }

        static void EnsureLoaded()
        {
            if (!loaded) Reload();
        }
    }
}
