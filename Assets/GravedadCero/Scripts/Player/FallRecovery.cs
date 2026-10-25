using GravedadCero.Gravity;
using UnityEngine;

namespace GravedadCero.Player
{
    /// <summary>
    /// Red de seguridad: si Kai pasa demasiado tiempo sin tocar una superficie (por ejemplo,
    /// tras cambiar la gravedad hacia el vacío) o se aleja demasiado, vuelve a su punto de
    /// aparición con la gravedad inicial. Versión provisional de los checkpoints del Sprint 4.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(PlayerMotor))]
    public class FallRecovery : MonoBehaviour
    {
        [SerializeField] GravityController gravityController;
        [Tooltip("Segundos en el aire antes de reaparecer.")]
        [SerializeField, Min(0.5f)] float maxAirTime = 4f;
        [Tooltip("Distancia máxima desde el punto de aparición, en metros.")]
        [SerializeField, Min(1f)] float maxDistanceFromSpawn = 150f;

        Rigidbody body;
        PlayerMotor motor;
        Vector3 spawnPosition;
        Quaternion spawnRotation;
        float airTime;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            motor = GetComponent<PlayerMotor>();
        }

        void Start()
        {
            spawnPosition = body.position;
            spawnRotation = body.rotation;
        }

        void FixedUpdate()
        {
            airTime = motor.IsGrounded ? 0f : airTime + Time.fixedDeltaTime;

            bool fellTooLong = airTime > maxAirTime;
            bool tooFar = (body.position - spawnPosition).sqrMagnitude > maxDistanceFromSpawn * maxDistanceFromSpawn;
            if (fellTooLong || tooFar) Respawn();
        }

        public void Respawn()
        {
            if (gravityController != null) gravityController.ResetGravity();

            body.linearVelocity = Vector3.zero;
            body.position = spawnPosition;
            body.rotation = spawnRotation;
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            airTime = 0f;
        }
    }
}
