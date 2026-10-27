using System;
using UnityEngine;

namespace GravedadCero.Player
{
    /// <summary>
    /// Integridad del traje de Kai: aguanta unos pocos golpes (de los drones de seguridad).
    /// Cada golpe empuja a Kai lejos del atacante y le da un momento de invulnerabilidad.
    /// Sin integridad, Kai reaparece en el último punto de control con el traje reparado.
    /// Las zonas peligrosas no pasan por aquí: siguen devolviendo a Kai al instante.
    /// </summary>
    [RequireComponent(typeof(PlayerMotor), typeof(FallRecovery))]
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField, Min(1)] int maxIntegrity = 3;
        [SerializeField, Min(0f)] float invulnerableSeconds = 1.5f;
        [SerializeField, Min(0f)] float knockbackSpeed = 9f;
        [SerializeField, Min(0f)] float knockbackLift = 4f;
        [SerializeField, Min(0f)] float stunSeconds = 0.35f;

        PlayerMotor motor;
        FallRecovery recovery;
        float invulnerableUntil = float.NegativeInfinity;

        public int MaxIntegrity => maxIntegrity;
        public int Integrity { get; private set; }
        public bool IsInvulnerable => Time.time < invulnerableUntil;

        /// <summary>Se lanza con la integridad que queda tras cada golpe recibido.</summary>
        public event Action<int> Damaged;

        /// <summary>Se lanza cuando la integridad cambia (golpe o reparación).</summary>
        public event Action<int> IntegrityChanged;

        void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            recovery = GetComponent<FallRecovery>();
            Integrity = maxIntegrity;
        }

        void OnEnable() => recovery.Respawned += Repair;

        void OnDisable() => recovery.Respawned -= Repair;

        /// <summary>
        /// Recibe un golpe desde sourcePosition. Devuelve false si Kai era invulnerable.
        /// Con la última unidad de integridad, Kai cae (pantalla de derrota o reaparición).
        /// </summary>
        public bool TakeHit(Vector3 sourcePosition)
        {
            if (IsInvulnerable || Integrity <= 0 || recovery.IsDown) return false;
            Integrity--;
            invulnerableUntil = Time.time + invulnerableSeconds;
            Damaged?.Invoke(Integrity);
            IntegrityChanged?.Invoke(Integrity);

            if (Integrity <= 0)
            {
                recovery.Fail(FallRecovery.Cause.Integrity); // al reaparecer, Respawned repara el traje
                return true;
            }

            // Empujón en el plano de la superficie, alejándose del atacante, y un poco hacia "arriba".
            Vector3 up = motor.Up;
            Vector3 away = Vector3.ProjectOnPlane(transform.position - sourcePosition, up);
            if (away.sqrMagnitude < 1e-4f) away = -transform.forward;
            motor.Knockback(away.normalized * knockbackSpeed + up * knockbackLift, stunSeconds);
            return true;
        }

        void Repair()
        {
            Integrity = maxIntegrity;
            invulnerableUntil = Time.time + invulnerableSeconds;
            IntegrityChanged?.Invoke(Integrity);
        }
    }
}
