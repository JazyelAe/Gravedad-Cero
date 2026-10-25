using UnityEngine;

namespace GravedadCero.Interaction
{
    /// <summary>
    /// Contrato común de todo lo que Kai puede usar con la tecla E: interruptores, consolas,
    /// lectores de tarjetas, etc. PlayerInteractor solo conoce esta interfaz, así que añadir
    /// un objeto nuevo no obliga a modificar el código del jugador.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Texto corto para el indicador de interacción, por ejemplo "Activar".</summary>
        string Prompt { get; }

        /// <summary>Punto del mundo usado para medir distancia y orientación.</summary>
        Vector3 InteractionPoint { get; }

        /// <summary>Si ahora mismo se puede usar (por ejemplo, un botón de un solo uso ya pulsado no).</summary>
        bool CanInteract(GameObject interactor);

        void Interact(GameObject interactor);
    }
}
