using UnityEngine;

namespace GravedadCero.UI
{
    /// <summary>Hace "respirar" la intensidad de una luz (ambiente del menú).</summary>
    [RequireComponent(typeof(Light))]
    public class LightPulse : MonoBehaviour
    {
        [SerializeField] float amplitude = 0.25f;
        [SerializeField] float speed = 1.2f;

        Light pulsingLight;
        float baseIntensity;

        void Awake()
        {
            pulsingLight = GetComponent<Light>();
            baseIntensity = pulsingLight.intensity;
        }

        void Update()
        {
            pulsingLight.intensity = baseIntensity * (1f + amplitude * Mathf.Sin(Time.unscaledTime * speed));
        }
    }
}
