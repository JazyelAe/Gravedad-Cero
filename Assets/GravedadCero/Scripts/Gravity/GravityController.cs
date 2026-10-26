using System;
using UnityEngine;

namespace GravedadCero.Gravity
{
    /// <summary>
    /// Fuente única de la dirección de gravedad del nivel. Calcula los giros del Módulo
    /// Gravitacional (izquierda, derecha, invertir), aplica una espera entre cambios y
    /// reparte la nueva dirección a todos los GravityBody de la escena.
    /// La dirección siempre se redondea a uno de los seis ejes del mundo.
    /// </summary>
    public class GravityController : MonoBehaviour
    {
        [SerializeField] Vector3 initialDirection = Vector3.down;
        [Tooltip("Segundos mínimos entre dos cambios de gravedad.")]
        [SerializeField, Min(0f)] float changeCooldown = 0.5f;

        float lastChangeTime = float.NegativeInfinity;

        /// <summary>Dirección actual de la gravedad (normalizada, alineada a un eje).</summary>
        public Vector3 Direction { get; private set; } = Vector3.down;

        public bool CanChange => Time.time >= lastChangeTime + changeCooldown;

        /// <summary>Se lanza tras cada cambio con (dirección anterior, dirección nueva).</summary>
        public event Action<Vector3, Vector3> GravityChanged;

        void Awake()
        {
            Direction = SnapToAxis(initialDirection, Vector3.down);
        }

        void Start()
        {
            // Todos los GravityBody ya ejecutaron su Awake: les damos la dirección inicial.
            ApplyToBodies();
        }

        /// <summary>La gravedad pasa a apuntar a la izquierda de <paramref name="reference"/>.</summary>
        public bool RotateLeft(Transform reference) => RotateToward(-reference.right);

        /// <summary>La gravedad pasa a apuntar a la derecha de <paramref name="reference"/>.</summary>
        public bool RotateRight(Transform reference) => RotateToward(reference.right);

        /// <summary>Invierte la gravedad 180°.</summary>
        public bool Flip() => TryChange(-Direction);

        /// <summary>Vuelve a la dirección inicial sin esperar el tiempo entre cambios.</summary>
        public void ResetGravity()
        {
            Change(SnapToAxis(initialDirection, Vector3.down));
        }

        /// <summary>Gira la gravedad 90° hacia el eje más cercano a <paramref name="worldDirection"/>.</summary>
        bool RotateToward(Vector3 worldDirection)
        {
            // Solo valen direcciones perpendiculares a la gravedad actual.
            Vector3 perpendicular = Vector3.ProjectOnPlane(worldDirection, Direction);
            if (perpendicular.sqrMagnitude < 0.01f) return false;
            return TryChange(SnapToAxis(perpendicular, Direction));
        }

        bool TryChange(Vector3 newDirection)
        {
            // Con el juego en pausa (timeScale 0) el Módulo Gravitacional no responde.
            if (!CanChange || Time.timeScale <= 0f) return false;
            Change(newDirection);
            return true;
        }

        void Change(Vector3 newDirection)
        {
            Vector3 previous = Direction;
            Direction = newDirection;
            lastChangeTime = Time.time;
            ApplyToBodies();
            if (previous != newDirection) GravityChanged?.Invoke(previous, newDirection);
        }

        void ApplyToBodies()
        {
            foreach (GravityBody body in FindObjectsByType<GravityBody>())
                body.SetGravityDirection(Direction);
        }

        /// <summary>Devuelve el eje del mundo (±X, ±Y, ±Z) más parecido a <paramref name="v"/>.</summary>
        public static Vector3 SnapToAxis(Vector3 v, Vector3 fallback)
        {
            if (v.sqrMagnitude < 1e-6f) return fallback;
            Vector3 a = new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
            if (a.x >= a.y && a.x >= a.z) return new Vector3(Mathf.Sign(v.x), 0f, 0f);
            if (a.y >= a.z) return new Vector3(0f, Mathf.Sign(v.y), 0f);
            return new Vector3(0f, 0f, Mathf.Sign(v.z));
        }

        /// <summary>
        /// Gira <paramref name="rotation"/> como máximo <paramref name="maxDegrees"/> para que su
        /// "arriba" se acerque a <paramref name="targetUp"/>. En un giro de 180° el eje que elige
        /// Unity sería arbitrario, así que se usa el "adelante" del objeto: rueda sobre sí mismo
        /// sin cambiar hacia dónde mira.
        /// </summary>
        public static Quaternion RotateUpTowards(Quaternion rotation, Vector3 targetUp, float maxDegrees)
        {
            Vector3 currentUp = rotation * Vector3.up;
            Quaternion delta = Vector3.Dot(currentUp, targetUp) < -0.9999f
                ? Quaternion.AngleAxis(180f, rotation * Vector3.forward)
                : Quaternion.FromToRotation(currentUp, targetUp);
            return Quaternion.Normalize(Quaternion.RotateTowards(rotation, delta * rotation, maxDegrees));
        }
    }
}
