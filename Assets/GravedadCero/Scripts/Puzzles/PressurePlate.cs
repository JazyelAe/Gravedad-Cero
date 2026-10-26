using System;
using System.Collections.Generic;
using GravedadCero.Interaction;
using UnityEngine;

namespace GravedadCero.Puzzles
{
    /// <summary>
    /// Placa que activa sus objetivos mientras haya algo encima. Usa un collider trigger:
    /// su "encima" es el volumen del trigger, así que funciona igual en piso, pared o techo.
    /// Por defecto solo cuentan los objetos anclables (cajas); el jugador puede contarse
    /// activando acceptPlayer. Una caja anclada dentro del volumen sigue contando.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PressurePlate : MonoBehaviour
    {
        [SerializeField] Activatable[] targets = new Activatable[0];
        [Tooltip("Si es true, Kai (etiqueta Player) también presiona la placa.")]
        [SerializeField] bool acceptPlayer;

        [Header("Indicador")]
        [SerializeField] Renderer indicator;
        [SerializeField] Color offColor = new Color(1f, 0.6f, 0.1f);
        [SerializeField] Color onColor = new Color(0.1f, 0.9f, 1f);

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        readonly HashSet<Rigidbody> bodiesOnPlate = new HashSet<Rigidbody>();
        MaterialPropertyBlock propertyBlock;

        public bool IsPressed { get; private set; }

        /// <summary>Se lanza cuando la placa pasa a estar presionada o libre.</summary>
        public event Action<bool> PressedChanged;

        void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            propertyBlock = new MaterialPropertyBlock();
        }

        void Start()
        {
            RefreshIndicator();
        }

        void OnTriggerEnter(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body == null || !Counts(body)) return;
            bodiesOnPlate.Add(body);
            Evaluate();
        }

        void OnTriggerExit(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body == null) return;
            bodiesOnPlate.Remove(body);
            Evaluate();
        }

        void FixedUpdate()
        {
            // Un objeto destruido o desactivado dentro del trigger no lanza OnTriggerExit.
            if (bodiesOnPlate.RemoveWhere(b => b == null || !b.gameObject.activeInHierarchy) > 0) Evaluate();
        }

        bool Counts(Rigidbody body)
        {
            if (body.GetComponent<AnchorableBody>() != null) return true;
            return acceptPlayer && body.CompareTag("Player");
        }

        void Evaluate()
        {
            bool pressed = bodiesOnPlate.Count > 0;
            if (pressed == IsPressed) return;

            IsPressed = pressed;
            foreach (Activatable target in targets)
                if (target != null) target.SetActivated(pressed);
            RefreshIndicator();
            PressedChanged?.Invoke(pressed);
        }

        void RefreshIndicator()
        {
            if (indicator == null) return;
            indicator.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, IsPressed ? onColor : offColor);
            indicator.SetPropertyBlock(propertyBlock);
        }
    }
}
