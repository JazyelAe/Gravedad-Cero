using UnityEngine;

namespace GravedadCero.Puzzles
{
    /// <summary>
    /// Puerta lógica "Y": activa sus objetivos solo mientras todas sus entradas están
    /// activas: placas presionadas y señales (por ejemplo, SignalLatch de un interruptor)
    /// activadas. Las placas conectadas a una compuerta no deben tener objetivos propios;
    /// esta compuerta lógica es la que decide.
    /// </summary>
    public class ActivationGate : MonoBehaviour
    {
        [SerializeField] PressurePlate[] inputs = new PressurePlate[0];
        [Tooltip("Señales que también tienen que estar activadas.")]
        [SerializeField] Activatable[] signals = new Activatable[0];
        [SerializeField] Activatable[] targets = new Activatable[0];

        public bool IsOpen { get; private set; }

        void FixedUpdate()
        {
            bool allOn = inputs.Length + signals.Length > 0;
            foreach (PressurePlate plate in inputs)
                if (plate == null || !plate.IsPressed) { allOn = false; break; }
            foreach (Activatable signal in signals)
                if (signal == null || !signal.IsActivated) { allOn = false; break; }

            if (allOn == IsOpen) return;
            IsOpen = allOn;
            foreach (Activatable target in targets)
                if (target != null) target.SetActivated(allOn);
        }
    }
}
