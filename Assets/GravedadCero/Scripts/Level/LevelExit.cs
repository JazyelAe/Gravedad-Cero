using UnityEngine;

namespace GravedadCero.Level
{
    /// <summary>
    /// Zona de salida: cuando Kai (etiqueta Player) entra en el trigger, el nivel se completa.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class LevelExit : MonoBehaviour
    {
        [SerializeField] LevelManager levelManager;

        void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body == null || !body.CompareTag("Player")) return;
            if (levelManager != null) levelManager.Complete();
        }
    }
}
