using UnityEngine;

namespace GravedadCero.Save
{
    /// <summary>
    /// Guardado básico con PlayerPrefs: la sección más alta desbloqueada y la skin elegida.
    /// Son datos sencillos y verificables; no se guarda el estado físico de los niveles.
    /// </summary>
    public static class GameProgress
    {
        const string UnlockedKey = "gravedadcero.unlockedSection";
        const string SkinKey = "gravedadcero.skin";

        /// <summary>Índice de la sección más alta disponible (0 = solo la primera).</summary>
        public static int UnlockedSection => PlayerPrefs.GetInt(UnlockedKey, 0);

        public static bool HasProgress => UnlockedSection > 0;

        public static void UnlockSection(int index)
        {
            if (index <= UnlockedSection) return;
            PlayerPrefs.SetInt(UnlockedKey, index);
            PlayerPrefs.Save();
        }

        public static void ResetProgress()
        {
            PlayerPrefs.SetInt(UnlockedKey, 0);
            PlayerPrefs.Save();
        }

        public static int SelectedSkin
        {
            get => PlayerPrefs.GetInt(SkinKey, 0);
            set
            {
                PlayerPrefs.SetInt(SkinKey, value);
                PlayerPrefs.Save();
            }
        }
    }
}
