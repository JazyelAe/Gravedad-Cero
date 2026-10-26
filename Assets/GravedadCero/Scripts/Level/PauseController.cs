using System;
using UnityEngine;

namespace GravedadCero.Level
{
    /// <summary>
    /// Estado de pausa de una sección. Pausar pone Time.timeScale a 0 (se detienen la
    /// física, las animaciones y todo lo que use tiempo escalado) y muestra el cursor.
    /// La entrada del jugador no la detiene timeScale: PlayerInputHandler consulta
    /// IsPaused y deja de enviar órdenes. Al reanudar, reiniciar o salir, el tiempo y el
    /// cursor se restauran.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        [SerializeField] LevelManager levelManager;

        public bool IsPaused { get; private set; }

        /// <summary>No se puede pausar con la sección completada (la victoria tiene sus propias teclas).</summary>
        public bool CanPause => levelManager == null || !levelManager.IsCompleted;

        /// <summary>Se lanza con el nuevo estado cada vez que cambia.</summary>
        public event Action<bool> PauseChanged;

        /// <summary>
        /// Lo asigna la interfaz de pausa: Esc primero intenta "volver atrás" dentro del menú
        /// (por ejemplo, cerrar una confirmación). Si devuelve false, Esc reanuda.
        /// </summary>
        public Func<bool> BackHandler { get; set; }

        void Awake()
        {
            // Una escena recién cargada siempre empieza con el tiempo normal.
            Time.timeScale = 1f;
        }

        void OnDestroy()
        {
            if (IsPaused) Time.timeScale = 1f;
        }

        /// <summary>Lo que hace la tecla Esc durante la sección.</summary>
        public void HandleCancel()
        {
            if (!IsPaused) { SetPaused(true); return; }
            if (BackHandler != null && BackHandler()) return;
            SetPaused(false);
        }

        public void SetPaused(bool paused)
        {
            if (paused == IsPaused) return;
            if (paused && !CanPause) return;

            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
            PauseChanged?.Invoke(paused);
        }

        public void RestartSection()
        {
            SetPaused(false);
            Time.timeScale = 1f;
            if (levelManager != null) levelManager.Restart();
        }

        public void ExitToMenu()
        {
            SetPaused(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (levelManager != null) levelManager.ExitToMenu();
        }
    }
}
