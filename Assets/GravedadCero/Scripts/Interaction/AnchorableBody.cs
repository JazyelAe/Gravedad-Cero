using System;
using UnityEngine;

namespace GravedadCero.Interaction
{
    /// <summary>
    /// Objeto con física que el Módulo Gravitacional puede anclar en el aire. Anclado, el
    /// Rigidbody pasa a cinemático: queda fijo en el mundo y GravityBody deja de moverlo
    /// (AddForce no afecta a cuerpos cinemáticos), así que no hace falta otra gravedad.
    /// Al soltarlo vuelve a ser dinámico y obedece la gravedad global de ese momento.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class AnchorableBody : MonoBehaviour
    {
        [Tooltip("Objeto que se enciende mientras está anclado (por ejemplo, una luz).")]
        [SerializeField] GameObject anchoredIndicator;

        Rigidbody body;

        public bool IsAnchored { get; private set; }

        public Rigidbody Body => body;

        /// <summary>Se lanza al anclar (true) o soltar (false).</summary>
        public event Action<bool> AnchorChanged;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            if (anchoredIndicator != null) anchoredIndicator.SetActive(false);
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
