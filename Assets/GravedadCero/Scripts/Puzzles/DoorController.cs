using UnityEngine;

namespace GravedadCero.Puzzles
{
    /// <summary>
    /// Puerta corrediza: mueve una hoja (hijo de este objeto) entre su posición cerrada y
    /// cerrada + openOffset. El desplazamiento es local, así que la misma puerta funciona
    /// montada en el piso, en una pared o en el techo. Al cerrarse se detiene si algo con
    /// física (Kai, una caja) está en su camino, en lugar de empujarlo a través de la pared.
    /// </summary>
    public class DoorController : Activatable
    {
        [Tooltip("Hoja móvil de la puerta. Debe ser hijo de este objeto y tener un BoxCollider.")]
        [SerializeField] Transform panel;
        [Tooltip("Desplazamiento de la hoja al abrirse, en el espacio local de la puerta.")]
        [SerializeField] Vector3 openOffset = new Vector3(0f, 3.2f, 0f);
        [Tooltip("Velocidad de la hoja en m/s.")]
        [SerializeField, Min(0.1f)] float speed = 2f;

        readonly Collider[] blockers = new Collider[8];
        Rigidbody panelBody;
        BoxCollider panelCollider;
        Vector3 closedLocalPosition;
        Vector3 targetLocalPosition;
        // Posición física de la hoja. No se usa panel.localPosition porque, con interpolación,
        // el Transform muestra una posición suavizada entre pasos de física.
        Vector3 currentLocalPosition;

        public bool IsMoving => currentLocalPosition != targetLocalPosition;

        protected override void Awake()
        {
            base.Awake();
            panelCollider = panel.GetComponent<BoxCollider>();

            // Hoja cinemática: la movemos nosotros y la física la trata como un objeto en movimiento.
            if (!panel.TryGetComponent(out panelBody)) panelBody = panel.gameObject.AddComponent<Rigidbody>();
            panelBody.isKinematic = true;
            panelBody.interpolation = RigidbodyInterpolation.Interpolate;

            closedLocalPosition = panel.localPosition;
            targetLocalPosition = closedLocalPosition;
            currentLocalPosition = closedLocalPosition;
        }

        protected override void OnActivationChanged(bool activated, bool instant)
        {
            targetLocalPosition = activated ? closedLocalPosition + openOffset : closedLocalPosition;
            if (!instant) return;

            currentLocalPosition = targetLocalPosition;
            panel.localPosition = targetLocalPosition;
            panelBody.position = panel.position;
        }

        void FixedUpdate()
        {
            if (!IsMoving) return;

            Vector3 next = Vector3.MoveTowards(currentLocalPosition, targetLocalPosition, speed * Time.fixedDeltaTime);
            bool closing = (next - closedLocalPosition).sqrMagnitude < (currentLocalPosition - closedLocalPosition).sqrMagnitude;
            if (closing && IsBlocked(transform.TransformPoint(next))) return;

            currentLocalPosition = next;
            panelBody.MovePosition(transform.TransformPoint(next));
        }

        /// <summary>¿La hoja, colocada en <paramref name="panelWorldPosition"/>, tocaría algún cuerpo dinámico?</summary>
        bool IsBlocked(Vector3 panelWorldPosition)
        {
            if (panelCollider == null) return false;

            Quaternion rotation = panel.rotation;
            Vector3 center = panelWorldPosition + rotation * Vector3.Scale(panelCollider.center, panel.lossyScale);
            // Un poco más pequeña que la hoja, para que rozarla de costado no la bloquee.
            Vector3 halfExtents = Vector3.Scale(panelCollider.size, panel.lossyScale) * 0.5f - Vector3.one * 0.02f;
            int count = Physics.OverlapBoxNonAlloc(center, halfExtents, blockers, rotation, ~0, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Rigidbody other = blockers[i].attachedRigidbody;
                // Solo frenan la puerta los objetos con física dinámica; el piso y el marco no.
                if (other != null && other != panelBody && !other.isKinematic) return true;
            }
            return false;
        }
    }
}
