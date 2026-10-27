using System;
using UnityEngine;

namespace GravedadCero.Level
{
    /// <summary>
    /// Peligro intermitente (por ejemplo, una ventila de vapor): alterna apagado, aviso y
    /// encendido en un ciclo fijo. Solo mientras está encendido su HazardZone es peligrosa
    /// (el collider se activa), así que Kai puede cruzar en los huecos. El aviso ámbar dura
    /// warnSeconds antes de cada encendido, para que el ritmo sea legible. Usa tiempo escalado:
    /// en pausa el ciclo se detiene.
    /// </summary>
    [RequireComponent(typeof(HazardZone))]
    public class TimedHazard : MonoBehaviour
    {
        public enum Phase { Off, Warning, On }

        [SerializeField, Min(0.1f)] float offSeconds = 2.2f;
        [SerializeField, Min(0f)] float warnSeconds = 0.7f;
        [SerializeField, Min(0.1f)] float onSeconds = 1.8f;
        [Tooltip("Desfase del ciclo en segundos, para que varias ventilas no vayan a la vez.")]
        [SerializeField] float phaseOffset;

        [Header("Indicadores")]
        [SerializeField] Renderer[] indicators = new Renderer[0];
        [Tooltip("Volumen visible del vapor: se escala desde la pared al encenderse.")]
        [SerializeField] Transform steam;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
        static readonly Color OffColor = new Color(0.25f, 0.08f, 0.06f);
        static readonly Color WarnColor = new Color(1f, 0.6f, 0.1f);
        static readonly Color OnColor = new Color(1f, 0.25f, 0.15f);

        Collider trigger;
        MaterialPropertyBlock block;
        Vector3 steamScale;
        float elapsed;

        public Phase Current { get; private set; } = Phase.Off;

        /// <summary>Segundos que faltan para el siguiente cambio de fase.</summary>
        public float TimeToNextPhase { get; private set; }

        /// <summary>Se lanza en cada cambio de fase (para el sonido).</summary>
        public event Action<TimedHazard, Phase> PhaseChanged;

        public void Configure(float off, float warn, float on, float offset, Renderer[] indicatorRenderers, Transform steamVolume)
        {
            offSeconds = off;
            warnSeconds = warn;
            onSeconds = on;
            phaseOffset = offset;
            indicators = indicatorRenderers;
            steam = steamVolume;
        }

        void Awake()
        {
            trigger = GetComponent<Collider>();
            block = new MaterialPropertyBlock();
            if (steam != null) steamScale = steam.localScale;
            elapsed = Mathf.Repeat(phaseOffset, Cycle);
            Apply(PhaseAt(elapsed), true);
        }

        float Cycle => offSeconds + warnSeconds + onSeconds;

        Phase PhaseAt(float t) => t < offSeconds ? Phase.Off : t < offSeconds + warnSeconds ? Phase.Warning : Phase.On;

        void Update()
        {
            elapsed = Mathf.Repeat(elapsed + Time.deltaTime, Cycle);
            Phase phase = PhaseAt(elapsed);
            float end = phase == Phase.Off ? offSeconds : phase == Phase.Warning ? offSeconds + warnSeconds : Cycle;
            TimeToNextPhase = end - elapsed;
            if (phase != Current) Apply(phase, false);
            if (Current == Phase.Warning) Tint(Color.Lerp(OffColor, WarnColor, Mathf.PingPong(Time.time * 8f, 1f)), 2f);
        }

        void Apply(Phase phase, bool initial)
        {
            Current = phase;
            trigger.enabled = phase == Phase.On;
            Tint(phase == Phase.On ? OnColor : phase == Phase.Warning ? WarnColor : OffColor, phase == Phase.On ? 3f : phase == Phase.Warning ? 2f : 0.4f);
            if (steam != null)
            {
                steam.gameObject.SetActive(phase == Phase.On);
                steam.localScale = steamScale;
            }
            if (!initial) PhaseChanged?.Invoke(this, phase);
        }

        void Tint(Color color, float emission)
        {
            foreach (var r in indicators)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block);
                block.SetColor(BaseColorId, color);
                block.SetColor(EmissionId, color * emission);
                r.SetPropertyBlock(block);
            }
        }
    }
}
