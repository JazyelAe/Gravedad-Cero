using GravedadCero.Interaction;
using UnityEngine;

namespace GravedadCero.Puzzles
{
    /// <summary>
    /// Interruptor que Kai usa con E. En modo Palanca alterna el estado de sus objetivos cada
    /// vez; en modo Un solo uso los activa una vez y queda bloqueado. Varios interruptores
    /// pueden controlar el mismo mecanismo: el indicador muestra siempre el estado real
    /// del primer objetivo, no el del propio interruptor.
    /// </summary>
    public class SwitchController : MonoBehaviour, IInteractable
    {
        public enum SwitchMode { Lever, OneShot }

        [SerializeField] Activatable[] targets = new Activatable[0];
        [SerializeField] SwitchMode mode = SwitchMode.Lever;
        [SerializeField] string activatePrompt = "Activar";
        [SerializeField] string deactivatePrompt = "Desactivar";

        [Header("Indicador")]
        [Tooltip("Renderer cuyo color muestra el estado (ámbar = apagado, cian = encendido).")]
        [SerializeField] Renderer indicator;
        [SerializeField] Color offColor = new Color(1f, 0.6f, 0.1f);
        [SerializeField] Color onColor = new Color(0.1f, 0.9f, 1f);

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        MaterialPropertyBlock propertyBlock;
        bool used;

        bool TargetsOn => targets.Length > 0 && targets[0] != null && targets[0].IsActivated;

        public string Prompt => TargetsOn ? deactivatePrompt : activatePrompt;

        public Vector3 InteractionPoint => transform.position;

        public bool CanInteract(GameObject interactor) => targets.Length > 0 && !(mode == SwitchMode.OneShot && used);

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor)) return;

            foreach (Activatable target in targets)
            {
                if (target == null) continue;
                if (mode == SwitchMode.Lever) target.Toggle();
                else target.SetActivated(true);
            }
            used = true;
            RefreshIndicator();
        }

        void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        void OnEnable()
        {
            if (targets.Length > 0 && targets[0] != null) targets[0].ActivationChanged += OnTargetChanged;
        }

        void OnDisable()
        {
            if (targets.Length > 0 && targets[0] != null) targets[0].ActivationChanged -= OnTargetChanged;
        }

        void Start()
        {
            RefreshIndicator();
        }

        void OnTargetChanged(bool activated)
        {
            RefreshIndicator();
        }

        void RefreshIndicator()
        {
            if (indicator == null) return;
            indicator.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, TargetsOn ? onColor : offColor);
            indicator.SetPropertyBlock(propertyBlock);
        }
    }
}
