using System;
using UnityEngine;

namespace GravedadCero.Interaction
{
    /// <summary>
    /// Objeto con física que el Módulo Gravitacional puede anclar en el aire. Anclado, el
    /// Rigidbody pasa a cinemático: queda fijo en el mundo y GravityBody deja de moverlo
    /// (AddForce no afecta a cuerpos cinemáticos), así que no hace falta otra gravedad.
    /// Al soltarlo vuelve a ser dinámico y obedece la gravedad global de ese momento.
    /// Si cae en una zona peligrosa, vuelve a su posición inicial (así nunca se pierde una
    /// caja necesaria para un puzle).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class AnchorableBody : MonoBehaviour
    {
        [Tooltip("Objeto que se enciende mientras está anclado (por ejemplo, una luz).")]
        [SerializeField] GameObject anchoredIndicator;

        Rigidbody body;
        Vector3 homePosition;
        Quaternion homeRotation;

        public bool IsAnchored { get; private set; }

        public Rigidbody Body => body;

        /// <summary>Se lanza al anclar (true) o soltar (false).</summary>
        public event Action<bool> AnchorChanged;

        /// <summary>Se lanza cuando la caja vuelve a su posición inicial.</summary>
        public event Action<AnchorableBody> Respawned;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            homePosition = body.position;
            homeRotation = body.rotation;
            if (anchoredIndicator != null) anchoredIndicator.SetActive(false);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<GravedadCero.Level.HazardZone>() != null) ReturnHome();
        }

        /// <summary>Devuelve la caja a su posición inicial, suelta y quieta.</summary>
        public void ReturnHome()
        {
            Release();
            body.position = homePosition;
            body.rotation = homeRotation;
            transform.SetPositionAndRotation(homePosition, homeRotation);
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            Respawned?.Invoke(this);
        }

        void OnDisable()
        {
            // Si se desactiva o destruye estando anclado, no debe quedar "colgado" en ese estado.
            if (IsAnchored) Release();
        }

        public void Anchor()
        {
            if (IsAnchored) return;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            SetAnchored(true);
        }

        public void Release()
        {
            if (!IsAnchored) return;
            body.isKinematic = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.WakeUp();
            SetAnchored(false);
        }

        void SetAnchored(bool anchored)
        {
            IsAnchored = anchored;
            if (anchoredIndicator != null) anchoredIndicator.SetActive(anchored);
            AnchorChanged?.Invoke(anchored);
        }
    }
}
