using UnityEngine;

namespace GravedadCero.Player
{
    /// <summary>
    /// Pasa el estado de PlayerMotor al Animator del modelo de Kai: velocidad sobre la
    /// superficie (para la mezcla reposo/correr) y si está en el suelo (para el salto).
    /// La velocidad se mide en el plano perpendicular a la gravedad, así que las
    /// animaciones funcionan igual en paredes y techos. No mueve al personaje: el
    /// movimiento sigue siendo físico y el Animator no usa root motion.
    /// </summary>
    [RequireComponent(typeof(PlayerMotor), typeof(Rigidbody))]
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [Tooltip("Suavizado del parámetro de velocidad, en segundos.")]
        [SerializeField] float speedDampTime = 0.1f;

        static readonly int SpeedId = Animator.StringToHash("Speed");
        static readonly int GroundedId = Animator.StringToHash("Grounded");

        PlayerMotor motor;
        Rigidbody body;

        void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            body = GetComponent<Rigidbody>();
            if (animator != null) animator.applyRootMotion = false;
        }

        void Update()
        {
            if (animator == null) return;

            float planarSpeed = Vector3.ProjectOnPlane(body.linearVelocity, motor.Up).magnitude;
            animator.SetFloat(SpeedId, planarSpeed, speedDampTime, Time.deltaTime);
            animator.SetBool(GroundedId, motor.IsGrounded);
        }
    }
}
