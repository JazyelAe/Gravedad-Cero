using System;
using UnityEngine;

namespace GravedadCero.Puzzles
{
    /// <summary>
    /// Base de todo mecanismo que puede estar activado o desactivado (puertas, plataformas,
    /// campos de energía...). Los interruptores y otros disparadores solo hablan con esta
    /// clase, así que cualquier mecanismo nuevo se conecta sin tocar el interruptor.
    /// </summary>
    public abstract class Activatable : MonoBehaviour
    {
        [Tooltip("Estado inicial. En una puerta, activado = abierta.")]
        [SerializeField] bool startActivated;

        public bool IsActivated { get; private set; }

        /// <summary>Se lanza cada vez que cambia el estado, con el estado nuevo.</summary>
        public event Action<bool> ActivationChanged;

        protected virtual void Awake()
        {
            IsActivated = startActivated;
        }

        protected virtual void Start()
        {
            // Coloca el mecanismo en su estado inicial sin animación.
            OnActivationChanged(IsActivated, instant: true);
        }

        public void SetActivated(bool activated)
        {
            if (activated == IsActivated) return;
            IsActivated = activated;
            OnActivationChanged(activated, instant: false);
            ActivationChanged?.Invoke(activated);
        }

        public void Toggle()
        {
            SetActivated(!IsActivated);
        }

        /// <summary>Lo que hace cada mecanismo al cambiar de estado.</summary>
        protected abstract void OnActivationChanged(bool activated, bool instant);
    }
}
