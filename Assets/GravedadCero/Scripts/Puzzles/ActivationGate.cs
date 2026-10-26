using UnityEngine;

namespace GravedadCero.Puzzles
{
    /// <summary>
    /// Puerta lógica "Y": activa sus objetivos solo mientras todas las placas de entrada
    /// están presionadas. Las placas conectadas a una compuerta no deben tener objetivos
    /// propios; esta compuerta lógica es la que decide.
    /// </summary>
    public class ActivationGate : MonoBehaviour
    {
        [SerializeField] PressurePlate[] inputs = new PressurePlate[0];
        [SerializeField] Activatable[] targets = new Activatable[0];

        public bool IsOpen { get; private set; }

        void FixedUpdate()
        {
            bool allPressed = inputs.Length > 0;
            foreach (PressurePlate plate in inputs)
                if (plate == null || !plate.IsPressed) { allPressed = false; break; }

            if (allPressed == IsOpen) return;
            IsOpen = allPressed;
            foreach (Activatable target in targets)
                if (target != null) target.SetActivated(allPressed);
        }
    }
}
