using UnityEngine;

namespace GravedadCero.Gravity
{
    /// <summary>
    /// Aplica a un Rigidbody una gravedad propia con dirección configurable, en lugar de
    /// usar Physics.gravity. Así cada objeto conoce su "abajo" y el jugador puede alinear
    /// su orientación con él. En el Sprint 2, GravityController cambiará esta dirección.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class GravityBody : MonoBehaviour
    {
        [Tooltip("Hacia dónde cae el objeto. No hace falta que esté normalizada.")]
        [SerializeField] Vector3 gravityDirection = Vector3.down;

        [Tooltip("Aceleración de la gravedad en m/s². Más alta que 9.81 para que los saltos se sientan firmes.")]
        [SerializeField, Min(0f)] float gravityStrength = 20f;

        Rigidbody body;

        /// <summary>Dirección de la gravedad, siempre normalizada.</summary>
        public Vector3 GravityDirection =>
            gravityDirection.sqrMagnitude > 1e-6f ? gravityDirection.normalized : Vector3.down;

        public float GravityStrength => gravityStrength;

        public Vector3 GravityAcceleration => GravityDirection * gravityStrength;

        /// <summary>"Arriba" para este objeto: lo opuesto a la gravedad.</summary>
        public Vector3 Up => -GravityDirection;

        public void SetGravityDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude < 1e-6f) return;
            gravityDirection = direction.normalized;
        }

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            // Desactivamos la gravedad global de Unity: la aplicamos nosotros en FixedUpdate.
            body.useGravity = false;
        }

        void FixedUpdate()
        {
            body.AddForce(GravityAcceleration, ForceMode.Acceleration);
        }
    }
}
