using GravedadCero.Gravity;
using UnityEngine;

namespace GravedadCero.Cameras
{
    /// <summary>
    /// Cámara orbital en tercera persona. Su "arriba" sigue al de la gravedad del objetivo
    /// girando de forma gradual, para que el horizonte acompañe a Kai en paredes y techos.
    /// Se acerca al jugador cuando una pared se interpone.
    /// </summary>
    public class GravityCamera : MonoBehaviour
    {
        [Header("Objetivo")]
        [SerializeField] Transform target;
        [SerializeField] GravityBody targetGravity;
        [Tooltip("Altura del punto de giro sobre los pies de Kai.")]
        [SerializeField] float pivotHeight = 1.6f;

        [Header("Órbita")]
        [SerializeField] float distance = 5f;
        [Tooltip("Grados por píxel de movimiento del mouse.")]
        [SerializeField] float sensitivity = 0.12f;
        [SerializeField] float minPitch = -35f;
        [SerializeField] float maxPitch = 70f;
        [Tooltip("Grados por segundo al reorientarse hacia una nueva gravedad.")]
        [SerializeField] float upAlignSpeed = 180f;

        [Header("Colisión")]
        [SerializeField] LayerMask obstacleLayers = ~0;
        [SerializeField] float collisionRadius = 0.25f;
        [SerializeField] float minDistance = 0.6f;
        [Tooltip("Metros por segundo al alejarse de nuevo tras un obstáculo.")]
        [SerializeField] float zoomOutSpeed = 6f;

        readonly RaycastHit[] obstacleHits = new RaycastHit[8];

        // Marco de la órbita: su "arriba" es el de la cámara y su "adelante", la dirección horizontal.
        Quaternion orbitFrame;
        float pitch = 15f;
        float currentDistance;
        Vector2 pendingLook;

        public void AddLookInput(Vector2 delta)
        {
            pendingLook += delta;
        }

        void Start()
        {
            if (target == null)
            {
                Debug.LogError($"{nameof(GravityCamera)}: falta asignar el objetivo.", this);
                enabled = false;
                return;
            }

            Vector3 up = CurrentTargetUp();
            orbitFrame = Quaternion.LookRotation(Vector3.ProjectOnPlane(target.forward, up), up);
            currentDistance = distance;
        }

        void LateUpdate()
        {
            float deltaTime = Time.deltaTime;

            // 1) Inclinar el marco hacia el "arriba" de la gravedad sin perder la dirección horizontal.
            Quaternion aligned = Quaternion.FromToRotation(orbitFrame * Vector3.up, CurrentTargetUp()) * orbitFrame;
            orbitFrame = Quaternion.RotateTowards(orbitFrame, aligned, upAlignSpeed * deltaTime);

            // 2) Mouse: giro horizontal alrededor del "arriba" de la cámara e inclinación vertical limitada.
            orbitFrame *= Quaternion.AngleAxis(pendingLook.x * sensitivity, Vector3.up);
            pitch = Mathf.Clamp(pitch - pendingLook.y * sensitivity, minPitch, maxPitch);
            pendingLook = Vector2.zero;

            Quaternion rotation = orbitFrame * Quaternion.Euler(pitch, 0f, 0f);
            Vector3 pivot = target.position + target.up * pivotHeight;
            Vector3 back = rotation * Vector3.back;

            // 3) Acercarse si hay un obstáculo entre Kai y la posición deseada.
            float allowedDistance = distance;
            int count = Physics.SphereCastNonAlloc(pivot, collisionRadius, back, obstacleHits, distance,
                obstacleLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (obstacleHits[i].collider.transform.IsChildOf(target)) continue;
                allowedDistance = Mathf.Min(allowedDistance, obstacleHits[i].distance);
            }
            allowedDistance = Mathf.Max(allowedDistance, minDistance);

            // Acercarse de golpe evita atravesar la pared; alejarse con suavidad evita tirones.
            currentDistance = allowedDistance < currentDistance
                ? allowedDistance
                : Mathf.MoveTowards(currentDistance, allowedDistance, zoomOutSpeed * deltaTime);

            transform.SetPositionAndRotation(pivot + back * currentDistance, rotation);
        }

        Vector3 CurrentTargetUp()
        {
            return targetGravity != null ? targetGravity.Up : Vector3.up;
        }
    }
}
