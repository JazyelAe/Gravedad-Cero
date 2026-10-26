using UnityEngine;

namespace GravedadCero.Level
{
    /// <summary>
    /// Marca un volumen peligroso (por ejemplo, el fondo de un pozo). No hace nada por sí
    /// mismo: FallRecovery detecta este componente al entrar en el trigger y devuelve a Kai
    /// al punto de aparición. Así la zona no necesita conocer al jugador.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class HazardZone : MonoBehaviour
    {
        void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }
    }
}
