using GravedadCero.Gravity;
using UnityEngine;

namespace GravedadCero.Player
{
    /// <summary>
    /// Mueve a Kai sobre la superficie en la que está parado: caminar, saltar, detectar el
    /// suelo y orientar el cuerpo. Todo se calcula respecto a GravityBody.Up y nunca respecto
    /// a Vector3.up, para que el mismo código funcione en paredes y techos.
    /// No lee el teclado: recibe órdenes de PlayerInputHandler.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(GravityBody))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] float moveSpeed = 6f;
        [SerializeField] float groundAcceleration = 50f;
        [SerializeField] float airAcceleration = 15f;
        [Tooltip("Grados por segundo al girar hacia la dirección de movimiento.")]
        [SerializeField] float turnSpeed = 720f;

        [Header("Salto")]
        [SerializeField] float jumpHeight = 1.4f;
        [Tooltip("Tiempo tras dejar el suelo en el que todavía se puede saltar.")]
        [SerializeField] float coyoteTime = 0.12f;
        [Tooltip("Tiempo que se recuerda un salto pulsado justo antes de aterrizar.")]
        [SerializeField] float jumpBufferTime = 0.12f;

        [Header("Suelo")]
        [SerializeField] LayerMask groundLayers = ~0;
        [Tooltip("Pendiente máxima caminable, en grados. Las más inclinadas se tratan como pared.")]
        [SerializeField] float maxSlopeAngle = 45f;
        [SerializeField] float groundCheckDistance = 0.15f;

        [Header("Orientación")]
        [Tooltip("Grados por segundo al alinear el cuerpo con la gravedad.")]
        [SerializeField] float alignSpeed = 540f;

        const float SkinWidth = 0.05f;
        // Tras saltar se ignora el suelo un instante, para no "pegarse" a él al despegar.
        const float JumpGroundLockTime = 0.15f;

        Rigidbody body;
        CapsuleCollider capsule;
        GravityBody gravityBody;
        readonly RaycastHit[] groundHits = new RaycastHit[8];
        readonly Collider[] overlaps = new Collider[8];

        Vector3 moveInput;
        Vector3 lastUp;
        float lastGroundedTime = float.NegativeInfinity;
        float lastJumpRequestTime = float.NegativeInfinity;
        float lastJumpTime = float.NegativeInfinity;

        public bool IsGrounded { get; private set; }
        public Vector3 GroundNormal { get; private set; }
        public Vector3 Up => gravityBody.Up;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();
            gravityBody = GetComponent<GravityBody>();

            // La rotación la controla este script, no la física.
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            // Sin fricción, para no quedar pegado a las paredes al saltar contra ellas.
            // El deslizamiento en rampas se compensa en Move().
            capsule.material = new PhysicsMaterial("KaiFrictionless")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };

            GroundNormal = Up;
            lastUp = Up;
        }

        /// <summary>Dirección deseada en el mundo, con magnitud entre 0 y 1.</summary>
        public void SetMoveInput(Vector3 worldDirection)
        {
            moveInput = Vector3.ClampMagnitude(worldDirection, 1f);
        }

        public void RequestJump()
        {
            lastJumpRequestTime = Time.time;
        }

        void FixedUpdate()
        {
            Vector3 up = Up;
            if (Vector3.Dot(up, lastUp) < 0.999f) OnGravityChanged(lastUp);
            lastUp = up;

            UpdateGrounded(up);
            TryJump(up);
            Move();
            Rotate(up);
        }

        void OnGravityChanged(Vector3 previousUp)
        {
            // Se cancela la velocidad "vertical" del sistema anterior (por ejemplo, la subida
            // de un salto) para que Kai empiece a caer limpiamente hacia la nueva superficie.
            Vector3 velocity = body.linearVelocity;
            body.linearVelocity = velocity - previousUp * Vector3.Dot(velocity, previousUp);

            // El suelo anterior ya no cuenta: no se puede saltar desde él ni con coyote time.
            lastGroundedTime = float.NegativeInfinity;
            lastJumpRequestTime = float.NegativeInfinity;
        }

        void UpdateGrounded(Vector3 up)
        {
            IsGrounded = false;
            GroundNormal = up;
            if (Time.time < lastJumpTime + JumpGroundLockTime) return;

            // Lanzamos una esfera desde la parte baja de la cápsula hacia la gravedad.
            float castRadius = capsule.radius * 0.9f;
            Vector3 bottomSphere = body.position + body.rotation *
                (capsule.center - Vector3.up * (capsule.height * 0.5f - capsule.radius));
            Vector3 origin = bottomSphere + up * SkinWidth;
            float castDistance = SkinWidth + (capsule.radius - castRadius) + groundCheckDistance;

            int count = Physics.SphereCastNonAlloc(origin, castRadius, -up, groundHits, castDistance,
                groundLayers, QueryTriggerInteraction.Ignore);

            float closest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = groundHits[i];
                // Distancia 0 significa que la esfera ya empezaba dentro de ese collider.
                if (hit.collider == capsule || hit.distance <= 0f) continue;
                if (Vector3.Angle(hit.normal, up) > maxSlopeAngle) continue;
                if (hit.distance < closest)
                {
                    closest = hit.distance;
                    GroundNormal = hit.normal;
                    IsGrounded = true;
                }
            }

            if (IsGrounded) lastGroundedTime = Time.time;
        }

        void TryJump(Vector3 up)
        {
            bool jumpBuffered = Time.time - lastJumpRequestTime <= jumpBufferTime;
            bool recentlyGrounded = Time.time - lastGroundedTime <= coyoteTime;
            if (!jumpBuffered || !recentlyGrounded) return;

            // v = sqrt(2 * g * h) alcanza exactamente jumpHeight con la gravedad actual.
            float jumpSpeed = Mathf.Sqrt(2f * gravityBody.GravityStrength * jumpHeight);
            Vector3 velocity = body.linearVelocity;
            // Sustituimos la velocidad "vertical" (relativa a la gravedad) por la del salto.
            body.linearVelocity = velocity - up * Vector3.Dot(velocity, up) + up * jumpSpeed;

            lastJumpTime = Time.time;
            lastJumpRequestTime = float.NegativeInfinity;
            lastGroundedTime = float.NegativeInfinity;
            IsGrounded = false;
            GroundNormal = up;
        }

        void Move()
        {
            // En el suelo nos movemos sobre su plano (así se suben rampas); en el aire,
            // sobre el plano perpendicular a la gravedad.
            Vector3 plane = GroundNormal;
            Vector3 planarVelocity = Vector3.ProjectOnPlane(body.linearVelocity, plane);

            Vector3 desired = Vector3.ProjectOnPlane(moveInput, plane);
            if (desired.sqrMagnitude > 1e-6f)
                desired = desired.normalized * (moveInput.magnitude * moveSpeed);

            float acceleration = IsGrounded ? groundAcceleration : airAcceleration;
            Vector3 newPlanar = Vector3.MoveTowards(planarVelocity, desired, acceleration * Time.fixedDeltaTime);
            body.AddForce(newPlanar - planarVelocity, ForceMode.VelocityChange);

            if (IsGrounded)
            {
                // Anula la parte de la gravedad paralela a la rampa: así Kai no resbala estando quieto.
                Vector3 slopeGravity = Vector3.ProjectOnPlane(gravityBody.GravityAcceleration, GroundNormal);
                body.AddForce(-slopeGravity, ForceMode.Acceleration);
            }
        }

        void Rotate(Vector3 up)
        {
            Quaternion current = body.rotation;

            // 1) Alinear el "arriba" del cuerpo con el opuesto a la gravedad.
            Quaternion rotation = GravityController.RotateUpTowards(current, up, alignSpeed * Time.fixedDeltaTime);
            bool tilting = Quaternion.Angle(current, rotation) > 0.01f;

            // 2) Girar alrededor de ese "arriba" hacia la dirección de movimiento.
            Vector3 currentUp = rotation * Vector3.up;
            Vector3 facing = Vector3.ProjectOnPlane(moveInput, currentUp);
            if (facing.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(facing, currentUp);
                rotation = Quaternion.RotateTowards(rotation, target, turnSpeed * Time.fixedDeltaTime);
            }

            if (!tilting)
            {
                body.MoveRotation(rotation);
                return;
            }

            // Durante un cambio de gravedad se gira alrededor del centro de la cápsula y no de
            // los pies: así el cuerpo no barre medio metro hacia una pared cercana.
            Vector3 worldCenter = body.position + current * capsule.center;
            Vector3 position = worldCenter - rotation * capsule.center;
            position += ComputeDepenetration(position, rotation);
            body.position = position;
            body.rotation = rotation;
        }

        /// <summary>
        /// Si con la pose indicada la cápsula quedaría dentro de algún collider, devuelve el
        /// desplazamiento mínimo para sacarla. Evita que la física la expulse de golpe.
        /// </summary>
        Vector3 ComputeDepenetration(Vector3 position, Quaternion rotation)
        {
            Vector3 center = position + rotation * capsule.center;
            Vector3 axis = rotation * Vector3.up * Mathf.Max(capsule.height * 0.5f - capsule.radius, 0f);
            int count = Physics.OverlapCapsuleNonAlloc(center + axis, center - axis, capsule.radius, overlaps,
                groundLayers, QueryTriggerInteraction.Ignore);

            Vector3 push = Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                Collider other = overlaps[i];
                if (other == capsule) continue;
                if (Physics.ComputePenetration(capsule, position + push, rotation,
                        other, other.transform.position, other.transform.rotation,
                        out Vector3 direction, out float distance))
                {
                    push += direction * distance;
                }
            }
            return push;
        }
    }
}
