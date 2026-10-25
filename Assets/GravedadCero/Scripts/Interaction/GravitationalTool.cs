using System.Collections.Generic;
using UnityEngine;

namespace GravedadCero.Interaction
{
    /// <summary>
    /// Función de anclaje del Módulo Gravitacional. Apunta con el centro de la cámara: si
    /// hay un AnchorableBody visible y a menos de maxDistance de Kai, el clic lo ancla; un
    /// segundo clic sobre una caja anclada la suelta. Solo puede haber maxAnchored cajas
    /// ancladas a la vez; anclar una más suelta la más antigua.
    /// </summary>
    public class GravitationalTool : MonoBehaviour
    {
        [SerializeField] Camera viewCamera;
        [Tooltip("Distancia máxima entre Kai y el objeto, en metros.")]
        [SerializeField, Min(0.5f)] float maxDistance = 8f;
        [Tooltip("Cuántos objetos puede mantener anclados a la vez.")]
        [SerializeField, Min(1)] int maxAnchored = 1;
        [Tooltip("Capas que bloquean o reciben el rayo (paredes y objetos anclables).")]
        [SerializeField] LayerMask targetLayers = ~0;
        [Tooltip("Altura, sobre los pies de Kai, desde la que se mide distancia y visibilidad.")]
        [SerializeField] float originHeight = 1.2f;

        readonly RaycastHit[] hits = new RaycastHit[16];
        readonly List<AnchorableBody> anchored = new List<AnchorableBody>();

        /// <summary>Objeto anclable bajo la mira y al alcance, o null.</summary>
        public AnchorableBody Target { get; private set; }

        Vector3 Origin => transform.position + transform.up * originHeight;

        void Update()
        {
            anchored.RemoveAll(a => a == null || !a.IsAnchored);
            Target = FindTarget();
        }

        /// <summary>Ancla o suelta el objeto bajo la mira. Devuelve true si hizo algo.</summary>
        public bool ToggleAnchor()
        {
            AnchorableBody target = FindTarget();
            if (target == null) return false;

            if (target.IsAnchored)
            {
                target.Release();
                anchored.Remove(target);
                return true;
            }

            anchored.RemoveAll(a => a == null || !a.IsAnchored);
            while (anchored.Count >= maxAnchored)
            {
                anchored[0].Release();
                anchored.RemoveAt(0);
            }
            target.Anchor();
            anchored.Add(target);
            return true;
        }

        public AnchorableBody FindTarget()
        {
            if (viewCamera == null) return null;

            // El rayo debe poder pasar por detrás de Kai: la cámara está a varios metros.
            Ray ray = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            float rayLength = Vector3.Distance(viewCamera.transform.position, Origin) + maxDistance;
            int count = Physics.RaycastNonAlloc(ray, hits, rayLength, targetLayers, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, 0, count, HitDistanceComparer.Instance);

            for (int i = 0; i < count; i++)
            {
                Collider hitCollider = hits[i].collider;
                if (hitCollider.transform.IsChildOf(transform)) continue; // el propio Kai

                // El primer objeto que no es Kai decide: si es una pared, bloquea la selección.
                AnchorableBody candidate = hitCollider.GetComponentInParent<AnchorableBody>();
                if (candidate == null) return null;
                if (Vector3.Distance(Origin, hits[i].point) > maxDistance) return null;
                return HasLineOfSight(candidate, hits[i].point) ? candidate : null;
            }
            return null;
        }

        /// <summary>Comprueba que Kai también ve el punto, no solo la cámara.</summary>
        bool HasLineOfSight(AnchorableBody candidate, Vector3 point)
        {
            Vector3 origin = Origin;
            Vector3 toPoint = point - origin;
            int count = Physics.RaycastNonAlloc(origin, toPoint.normalized, hits, toPoint.magnitude, targetLayers,
                QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Transform t = hits[i].collider.transform;
                if (t.IsChildOf(transform) || t.IsChildOf(candidate.transform)) continue;
                return false;
            }
            return true;
        }

        sealed class HitDistanceComparer : IComparer<RaycastHit>
        {
            public static readonly HitDistanceComparer Instance = new HitDistanceComparer();
            public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
        }
    }
}
