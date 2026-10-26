using UnityEngine;

namespace GravedadCero.UI
{
    /// <summary>Gira lentamente un objeto sobre su eje vertical (vista previa del menú).</summary>
    public class MenuTurntable : MonoBehaviour
    {
        [SerializeField] float degreesPerSecond = 20f;

        void Update()
        {
            transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.Self);
        }
    }
}
