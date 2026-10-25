using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GravedadCero.Level
{
    /// <summary>
    /// Estado de un nivel: el objetivo que se muestra al jugador, si ya se completó y cómo
    /// reiniciarlo. No conoce al jugador ni la interfaz; ellos consultan este estado.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] string levelTitle = "Módulo de Acoplamiento";
        [TextArea(1, 3)]
        [SerializeField] string objective = "Abre la compuerta y llega a la salida.";
        [TextArea(1, 3)]
        [SerializeField] string hint = "";

        public string LevelTitle => levelTitle;
        public string Objective => objective;
        public string Hint => hint;
        public bool IsCompleted { get; private set; }

        public event Action Completed;

        public void Complete()
        {
            if (IsCompleted) return;
            IsCompleted = true;
            Debug.Log($"Nivel completado: {levelTitle}");
            Completed?.Invoke();
        }

        /// <summary>Recarga la escena actual desde cero.</summary>
        public void Restart()
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex >= 0 ? scene.buildIndex : 0);
        }
    }
}
