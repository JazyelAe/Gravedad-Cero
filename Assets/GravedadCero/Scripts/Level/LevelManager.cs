using System;
using GravedadCero.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GravedadCero.Level
{
    /// <summary>
    /// Estado de una sección: el objetivo que se muestra al jugador, si ya se completó y
    /// cómo continuar (siguiente sección, reinicio o menú). Al completarse desbloquea la
    /// siguiente sección en GameProgress. No conoce al jugador ni la interfaz.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] string levelTitle = "Módulo de Acoplamiento";
        [TextArea(1, 3)]
        [SerializeField] string objective = "Abre la compuerta y llega a la salida.";
        [TextArea(1, 3)]
        [SerializeField] string hint = "";

        [Header("Progreso")]
        [Tooltip("Posición de esta sección en la lista del menú (0 = primera).")]
        [SerializeField, Min(0)] int sectionIndex;
        [Tooltip("Escena que se carga al continuar. Vacío = fin del sector, vuelve al menú.")]
        [SerializeField] string nextScene = "";
        [SerializeField] string menuScene = "MainMenu";

        public string LevelTitle => levelTitle;
        public string Objective => objective;
        public string Hint => hint;
        public bool IsCompleted { get; private set; }
        public bool HasNext => !string.IsNullOrEmpty(nextScene);

        public event Action Completed;

        public void Complete()
        {
            if (IsCompleted) return;
            IsCompleted = true;
            GameProgress.UnlockSection(sectionIndex + 1);
            Debug.Log($"Sección completada: {levelTitle}");
            Completed?.Invoke();
        }

        /// <summary>Recarga la escena actual desde cero.</summary>
        public void Restart()
        {
            // Por ruta y no por índice: así funciona aunque la escena se haya cargado sin
            // índice de Build Settings (por ejemplo, desde el editor o las pruebas).
            SceneManager.LoadScene(SceneManager.GetActiveScene().path);
        }

        /// <summary>Carga la siguiente sección o, si no hay, vuelve al menú.</summary>
        public void Continue()
        {
            SceneManager.LoadScene(HasNext ? nextScene : menuScene);
        }

        public void ExitToMenu()
        {
            SceneManager.LoadScene(menuScene);
        }
    }
}
