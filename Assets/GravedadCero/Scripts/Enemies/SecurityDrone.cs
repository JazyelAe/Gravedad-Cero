using System;
using GravedadCero.Interaction;
using GravedadCero.Player;
using UnityEngine;

namespace GravedadCero.Enemies
{
    /// <summary>
    /// Dron de seguridad: flota con sus propios imanes, así que los cambios de gravedad no le
    /// afectan (no tiene GravityBody). Patrulla entre puntos fijos; si ve a Kai (distancia y
    /// línea de visión), se pone en alerta un instante y lo persigue sin salir de su zona de
    /// patrulla. Al tocarlo le quita integridad y se retira. Si pierde a Kai de vista, vuelve a
    /// su ruta. Una caja que lo golpea con fuerza lo aturde unos segundos.
    /// Se mueve como cuerpo cinemático comprobando antes cada paso, para no atravesar superficies.
    /// El color del ojo indica el estado: cian patrulla, ámbar alerta, rojo persecución.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SecurityDrone : MonoBehaviour
    {
        public enum State { Patrol, Alert, Chase, Retreat, Stunned }

        [Header("Ruta")]
        [Tooltip("Puntos de patrulla en coordenadas del mundo, recorridos en bucle.")]
        [SerializeField] Vector3[] waypoints = new Vector3[0];
        [Tooltip("Volumen del que el dron nunca sale (también al perseguir).")]
        [SerializeField] Bounds patrolZone = new Bounds(Vector3.zero, Vector3.one * 20f);

        [Header("Movimiento")]
        [SerializeField] float patrolSpeed = 2.5f;
        [SerializeField] float chaseSpeed = 4.2f;
        [SerializeField] float hullRadius = 0.55f;
        [SerializeField] LayerMask obstacleLayers = ~0;

        [Header("Detección")]
        [SerializeField] float detectRange = 11f;
        [SerializeField] float alertSeconds = 0.7f;
        [SerializeField] float loseSightSeconds = 2.5f;

        [Header("Ataque")]
        [SerializeField] float retreatSeconds = 1.4f;
        [SerializeField] float stunSeconds = 4f;
        [Tooltip("Velocidad relativa mínima de una caja para aturdir al dron.")]
        [SerializeField] float stunImpactSpeed = 4f;

        [Header("Indicador")]
        [SerializeField] Renderer eye;
        [SerializeField] Light eyeLight;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
        static readonly Color PatrolColor = new Color(0.2f, 0.85f, 1f);
        static readonly Color AlertColor = new Color(1f, 0.65f, 0.15f);
        static readonly Color ChaseColor = new Color(1f, 0.2f, 0.15f);
        static readonly Color StunnedColor = new Color(0.25f, 0.28f, 0.32f);

        Rigidbody body;
        MaterialPropertyBlock block;
        Transform player;
        PlayerHealth playerHealth;
        int waypointIndex;
        float stateTime;
        float lastSeenTime;
        Vector3 hoverBase;

        public State Current { get; private set; } = State.Patrol;

        /// <summary>Se lanza al cambiar de estado (para el sonido y el HUD).</summary>
        public event Action<SecurityDrone, State> StateChanged;

        /// <summary>Se lanza cada vez que el dron golpea a Kai.</summary>
        public event Action<SecurityDrone> HitPlayer;

        public Bounds PatrolZone => patrolZone;

        /// <summary>Configura la ruta (lo usa el constructor de niveles).</summary>
        public void Configure(Vector3[] points, Bounds zone)
        {
            waypoints = points;
            patrolZone = zone;
        }

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            block = new MaterialPropertyBlock();
        }

        void Start()
        {
            var kai = GameObject.FindWithTag("Player");
            if (kai != null)
            {
                player = kai.transform;
                playerHealth = kai.GetComponent<PlayerHealth>();
            }
            hoverBase = body.position;
            SetState(State.Patrol);
        }

        void FixedUpdate()
        {
            stateTime += Time.fixedDeltaTime;
            bool sees = CanSeePlayer();
            if (sees) lastSeenTime = Time.time;

            switch (Current)
            {
                case State.Patrol:
                    if (sees) { SetState(State.Alert); break; }
                    MoveTowards(CurrentWaypoint(), patrolSpeed);
                    if (Vector3.Distance(body.position, CurrentWaypoint()) < 0.4f) waypointIndex = (waypointIndex + 1) % Mathf.Max(1, waypoints.Length);
                    break;
                case State.Alert:
                    FaceTowards(PlayerTarget());
                    if (stateTime >= alertSeconds) SetState(sees ? State.Chase : State.Patrol);
                    break;
                case State.Chase:
                    if (Time.time - lastSeenTime > loseSightSeconds) { SetState(State.Retreat); break; }
                    MoveTowards(ClampToZone(PlayerTarget()), chaseSpeed);
                    break;
                case State.Retreat:
                    MoveTowards(NearestWaypoint(), patrolSpeed);
                    if (stateTime >= retreatSeconds && Vector3.Distance(body.position, NearestWaypoint()) < 0.6f) SetState(State.Patrol);
                    else if (stateTime >= retreatSeconds && sees) SetState(State.Alert);
                    break;
                case State.Stunned:
                    if (stateTime >= stunSeconds) SetState(State.Retreat);
                    break;
            }
        }

        void Update()
        {
            // Pequeño balanceo visual del cuerpo (no afecta a la física).
            if (Current != State.Stunned && transform.childCount > 0)
                transform.GetChild(0).localPosition = new Vector3(0f, Mathf.Sin(Time.time * 2.2f) * 0.08f, 0f);
        }

        // ---------- Percepción ----------

        Vector3 PlayerTarget() => player != null ? player.position + player.up * 1.1f : body.position;

        bool CanSeePlayer()
        {
            if (player == null || Current == State.Stunned) return false;
            Vector3 target = PlayerTarget();
            Vector3 toPlayer = target - body.position;
            float distance = toPlayer.magnitude;
            if (distance > detectRange) return false;
            // Kai fuera de la zona de patrulla (por ejemplo, en el techo) no es un objetivo.
            if (!patrolZone.Contains(player.position) && !patrolZone.Contains(target)) return false;
            // Línea de visión: cualquier superficie o caja entre los dos la corta.
            var hits = Physics.RaycastAll(body.position, toPlayer / distance, distance, obstacleLayers, QueryTriggerInteraction.Ignore);
            foreach (var hit in hits)
            {
                if (hit.rigidbody == body) continue;
                if (hit.transform == player || hit.transform.IsChildOf(player)) continue;
                return false;
            }
            return true;
        }

        // ---------- Movimiento ----------

        Vector3 CurrentWaypoint() => waypoints.Length > 0 ? waypoints[waypointIndex % waypoints.Length] : hoverBase;

        Vector3 NearestWaypoint()
        {
            if (waypoints.Length == 0) return hoverBase;
            int best = 0;
            for (int i = 1; i < waypoints.Length; i++)
                if ((waypoints[i] - body.position).sqrMagnitude < (waypoints[best] - body.position).sqrMagnitude) best = i;
            waypointIndex = best;
            return waypoints[best];
        }

        Vector3 ClampToZone(Vector3 p)
        {
            Vector3 min = patrolZone.min + Vector3.one * hullRadius;
            Vector3 max = patrolZone.max - Vector3.one * hullRadius;
            return new Vector3(Mathf.Clamp(p.x, min.x, max.x), Mathf.Clamp(p.y, min.y, max.y), Mathf.Clamp(p.z, min.z, max.z));
        }

        void MoveTowards(Vector3 target, float speed)
        {
            Vector3 position = body.position;
            Vector3 delta = target - position;
            float step = Mathf.Min(delta.magnitude, speed * Time.fixedDeltaTime);
            if (step < 1e-4f) return;
            Vector3 direction = delta.normalized;
            FaceTowards(target);

            // Comprueba el paso antes de darlo; si hay algo delante, se desliza por su superficie.
            if (Blocked(position, direction, step, out RaycastHit hit))
            {
                direction = Vector3.ProjectOnPlane(direction, hit.normal);
                if (direction.sqrMagnitude < 1e-4f || Blocked(position, direction.normalized, step, out _)) return;
                direction.Normalize();
            }
            body.MovePosition(ClampToZone(position + direction * step));
        }

        bool Blocked(Vector3 position, Vector3 direction, float step, out RaycastHit blocking)
        {
            blocking = default;
            var hits = Physics.SphereCastAll(position, hullRadius * 0.95f, direction, step + 0.05f, obstacleLayers, QueryTriggerInteraction.Ignore);
            float best = float.PositiveInfinity;
            foreach (var hit in hits)
            {
                if (hit.rigidbody == body) continue;
                if (player != null && (hit.transform == player || hit.transform.IsChildOf(player))) continue;
                if (hit.distance < best) { best = hit.distance; blocking = hit; }
            }
            return best < float.PositiveInfinity;
        }

        void FaceTowards(Vector3 target)
        {
            Vector3 flat = target - body.position;
            flat.y = 0f;
            if (flat.sqrMagnitude < 1e-4f) return;
            body.MoveRotation(Quaternion.RotateTowards(body.rotation, Quaternion.LookRotation(flat, Vector3.up), 360f * Time.fixedDeltaTime));
        }

        // ---------- Golpes ----------

        void OnTriggerStay(Collider other)
        {
            if (Current != State.Chase && Current != State.Alert && Current != State.Patrol) return;
            if (playerHealth == null || other.attachedRigidbody == null || other.attachedRigidbody.transform != player) return;
            if (!playerHealth.TakeHit(body.position)) return;
            HitPlayer?.Invoke(this);
            SetState(State.Retreat);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (Current == State.Stunned) return;
            if (collision.rigidbody == null || collision.rigidbody.GetComponent<AnchorableBody>() == null) return;
            if (collision.relativeVelocity.magnitude >= stunImpactSpeed) Stun();
        }

        /// <summary>Deja al dron fuera de combate unos segundos (golpe de una caja).</summary>
        public void Stun() => SetState(State.Stunned);

        void SetState(State state)
        {
            Current = state;
            stateTime = 0f;
            Color color = state == State.Patrol || state == State.Retreat ? PatrolColor
                : state == State.Alert ? AlertColor
                : state == State.Chase ? ChaseColor : StunnedColor;
            if (eye != null)
            {
                eye.GetPropertyBlock(block);
                block.SetColor(BaseColorId, color);
                block.SetColor(EmissionId, color * (state == State.Stunned ? 0.2f : 3f));
                eye.SetPropertyBlock(block);
            }
            if (eyeLight != null)
            {
                eyeLight.color = color;
                eyeLight.intensity = state == State.Stunned ? 0f : 3f;
            }
            StateChanged?.Invoke(this, state);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(patrolZone.center, patrolZone.size);
            for (int i = 0; i < waypoints.Length; i++)
                Gizmos.DrawLine(waypoints[i], waypoints[(i + 1) % waypoints.Length]);
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, detectRange);
        }
    }
}
