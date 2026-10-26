using System;
using GravedadCero.Player;
using UnityEngine;

namespace GravedadCero.Level
{
    /// <summary>
    /// Punto de control: cuando Kai entra en su volumen (por cualquier superficie), pasa a
    /// reaparecer aquí al caer o tocar una zona de peligro. El punto de reaparición es este
    /// transform, apoyado en el suelo y con la gravedad normal. Retroceso sigue reiniciando
    /// la sección entera. Solo se activa una vez.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        public bool IsReached { get; private set; }

        /// <summary>Se lanza la primera vez que Kai lo alcanza.</summary>
        public event Action<Checkpoint> Reached;

        void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (IsReached || !other.CompareTag("Player")) return;
            var recovery = other.GetComponentInParent<FallRecovery>();
            if (recovery == null) return;
            IsReached = true;
            recovery.SetSpawn(transform.position, transform.rotation);
            Reached?.Invoke(this);
        }
    }
}
