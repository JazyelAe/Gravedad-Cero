using System;
using UnityEngine;

namespace GravedadCero.Player
{
    /// <summary>
    /// Lista de skins disponibles para Kai. Cada skin es un material para el mismo modelo,
    /// así que añadir una nueva solo requiere crear el material y agregarlo aquí.
    /// </summary>
    [CreateAssetMenu(menuName = "Gravedad Cero/Skin Catalog", fileName = "SkinCatalog")]
    public class SkinCatalog : ScriptableObject
    {
        [Serializable]
        public struct Skin
        {
            public string displayName;
            public Material material;
        }

        [SerializeField] Skin[] skins = new Skin[0];

        public int Count => skins.Length;

        public Skin Get(int index) => skins[Mathf.Clamp(index, 0, Mathf.Max(0, skins.Length - 1))];

        /// <summary>Convierte cualquier índice (incluso negativo) en uno válido, dando la vuelta.</summary>
        public int Wrap(int index) => skins.Length == 0 ? 0 : ((index % skins.Length) + skins.Length) % skins.Length;
    }
}
