using System;
using UnityEngine;

namespace GravedadCero.Interaction
{
    /// <summary>
    /// Busca el objeto interactuable más conveniente alrededor de Kai (cerca, delante de él y
    /// sin paredes en medio) y lo usa cuando se le pide. Todo se mide respecto al "arriba"
    /// de Kai, así que funciona igual en el piso, en paredes y en techos.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] float range = 2f;
        [Tooltip("Altura, sobre los pies de Kai, desde la que se mide el alcance.")]
        [SerializeField] float originHeight = 1.2f;
        [Tooltip("Qué tan de frente debe estar el objeto: 1 = justo delante, 0 = a un lado.")]
        [SerializeField, Range(-1f, 1f)] float minFacingDot = 0.2f;
        [SerializeField] LayerMask interactableLayers = ~0;
        [SerializeField] LayerMask obstacleLayers = ~0;

        readonly Collider[] candidates = new Collider[16];

        /// <summary>Objeto que se usaría al pulsar E, o null.</summary>
        public IInteractable Current { get; private set; }

        /// <summary>Se lanza cuando cambia el objeto enfocado. La interfaz lo usará en el Sprint 11.</summary>
        public event Action<IInteractable> FocusChanged;

        Vector3 Origin => transform.position + transform.up * originHeight;

        void Update()
        {
            IInteractable best = FindBest();
            if (best == Current) return;

            Current = best;
            FocusChanged?.Invoke(Current);
            if (Current != null) Debug.Log($"[E] {Current.Prompt}");
        }

        public void TryInteract()
        {
            if (Current == null || !Current.CanInteract(gameObject)) return;

            Current.Interact(gameObject);
            // Provisional hasta el Sprint 11: muestra el nuevo texto (por ejemplo, "Desactivar").
            if (Current.CanInteract(gameObject)) Debug.Log($"[E] {Current.Prompt}");
        }

        IInteractable FindBest()
        {
            Vector3 origin = Origin;
            Vector3 up = transform.up;
            int count = Physics.OverlapSphereNonAlloc(origin, range, candidates, interactableLayers,
                QueryTriggerInteraction.Collide);

            IInteractable best = null;
            float bestScore = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                IInteractable candidate = candidates[i].GetComponentInParent<IInteractable>();
                if (candidate == null || !candidate.CanInteract(gameObject)) continue;

                Vector3 toTarget = candidate.InteractionPoint - origin;
                float distance = toTarget.magnitude;
                if (distance > range) continue;

                // Orientación medida en el plano de la superficie: mirar hacia arriba o abajo no cuenta.
                Vector3 flat = Vector3.ProjectOnPlane(toTarget, up);
                float facing = flat.sqrMagnitude < 1e-4f ? 1f : Vector3.Dot(transform.forward, flat.normalized);
                if (facing < minFacingDot) continue;
                if (!HasLineOfSight(origin, candidate)) continue;

                // Menor puntuación = mejor: favorece lo cercano y lo que está de frente.
                float score = distance * (1.5f - facing);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }
            return best;
        }

        bool HasLineOfSight(Vector3 origin, IInteractable candidate)
        {
            if (!Physics.Linecast(origin, candidate.InteractionPoint, out RaycastHit hit, obstacleLayers,
                    QueryTriggerInteraction.Ignore))
                return true;

            // Chocar con el propio objeto (o con Kai) no cuenta como obstáculo.
            return hit.collider.GetComponentInParent<IInteractable>() == candidate
                || hit.collider.transform.IsChildOf(transform);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Origin, range);
            if (Current != null) Gizmos.DrawLine(Origin, Current.InteractionPoint);
        }
    }
}
