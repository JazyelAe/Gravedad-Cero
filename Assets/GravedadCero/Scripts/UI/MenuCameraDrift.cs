using UnityEngine;

namespace GravedadCero.UI
{
    /// <summary>
    /// Cámara del menú principal: se balancea muy despacio para dar vida al fondo y, en la
    /// pantalla de personaje, se acerca al modelo. Usa tiempo no escalado.
    /// </summary>
    public class MenuCameraDrift : MonoBehaviour
    {
        [SerializeField] Vector3 overviewPosition = new Vector3(0.2f, 1.7f, -4.2f);
        [SerializeField] Vector3 overviewLookAt = new Vector3(1.3f, 1.15f, 1.5f);
        [SerializeField] Vector3 focusPosition = new Vector3(1.2f, 1.45f, -1.6f);
        [SerializeField] Vector3 focusLookAt = new Vector3(1.9f, 1.05f, 1.2f);
        [SerializeField] float swayAmount = 0.08f;
        [SerializeField] float blendSpeed = 1.6f;

        bool focus;
        float blend;

        public void SetFocus(bool value) => focus = value;

        void LateUpdate()
        {
            blend = Mathf.MoveTowards(blend, focus ? 1f : 0f, Time.unscaledDeltaTime * blendSpeed);
            float e = blend * blend * (3f - 2f * blend);
            float t = Time.unscaledTime;
            Vector3 sway = new Vector3(Mathf.Sin(t * 0.37f), Mathf.Sin(t * 0.23f) * 0.5f, 0f) * swayAmount;
            Vector3 position = Vector3.Lerp(overviewPosition, focusPosition, e) + sway;
            Vector3 lookAt = Vector3.Lerp(overviewLookAt, focusLookAt, e);
            // En pantallas más estrechas que 16:9 se mira un poco más a la izquierda, para
            // que el personaje no quede detrás del panel de la izquierda.
            var cam = GetComponent<Camera>();
            float aspect = cam != null ? cam.aspect : 16f / 9f;
            lookAt.x -= Mathf.Max(0f, 16f / 9f - aspect) * (0.6f + 0.9f * e);
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(lookAt - position, Vector3.up));
        }
    }
}
