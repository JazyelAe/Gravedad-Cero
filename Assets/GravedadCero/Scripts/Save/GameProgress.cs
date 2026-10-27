using UnityEngine;

namespace GravedadCero.Save
{
    /// <summary>
    /// Guardado básico con PlayerPrefs: la sección más alta desbloqueada, la skin elegida y
    /// los registros de datos encontrados.
    /// Son datos sencillos y verificables; no se guarda el estado físico de los niveles.
    /// </summary>
    public static class GameProgress
    {
        const string UnlockedKey = "gravedadcero.unlockedSection";
        const string SkinKey = "gravedadcero.skin";
        const string LogsKey = "gravedadcero.dataLogs";

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
            PlayerPrefs.SetString(LogsKey, "");
            PlayerPrefs.Save();
        }

        // ---------- Registros de datos ----------

        static string[] Logs => PlayerPrefs.GetString(LogsKey, "").Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

        public static bool HasLog(string id) => System.Array.IndexOf(Logs, id) >= 0;

        /// <summary>Guarda un registro encontrado. Devuelve false si ya estaba guardado.</summary>
        public static bool CollectLog(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Contains(",") || HasLog(id)) return false;
            string current = PlayerPrefs.GetString(LogsKey, "");
            PlayerPrefs.SetString(LogsKey, current.Length == 0 ? id : current + "," + id);
            PlayerPrefs.Save();
            return true;
        }

        /// <summary>Cuántos registros guardados empiezan por el prefijo (por ejemplo, la escena).</summary>
        public static int CountLogs(string prefix)
        {
            int count = 0;
            foreach (string id in Logs) if (id.StartsWith(prefix)) count++;
            return count;
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
