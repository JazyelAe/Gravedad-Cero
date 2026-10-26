using GravedadCero.Save;
using UnityEngine;

namespace GravedadCero.Player
{
    /// <summary>
    /// Aplica al modelo de Kai la skin guardada en GameProgress al cargar la escena.
    /// El menú principal lo usa también para la vista previa.
    /// </summary>
    public class KaiSkinApplier : MonoBehaviour
    {
        [SerializeField] SkinCatalog catalog;
        [SerializeField] Renderer target;

        public SkinCatalog Catalog => catalog;
        public int CurrentIndex { get; private set; }

        void Awake()
        {
            Apply(GameProgress.SelectedSkin);
        }

        public void Apply(int index)
        {
            if (catalog == null || target == null || catalog.Count == 0) return;
            CurrentIndex = catalog.Wrap(index);
            Material material = catalog.Get(CurrentIndex).material;
            if (material != null) target.sharedMaterial = material;
        }
    }
}
