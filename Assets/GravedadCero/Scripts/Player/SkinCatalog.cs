using System;
using UnityEngine;

namespace GravedadCero.Player
{
    /// <summary>
    /// Personaje jugable y sus skins. Cada skin es un material para el mismo modelo, así que
    /// añadir una nueva solo requiere crear el material y agregarlo aquí. Los textos y el
    /// color de muestra se usan en la pantalla de selección del menú.
    /// </summary>
    [CreateAssetMenu(menuName = "Gravedad Cero/Skin Catalog", fileName = "SkinCatalog")]
    public class SkinCatalog : ScriptableObject
    {
        [Serializable]
        public struct Skin
        {
            public string displayName;
            public Material material;
            [Tooltip("Color representativo del traje para la muestra del menú.")]
            public Color swatch;
        }

        [Header("Personaje")]
        [SerializeField] string characterName = "Kai";
        [SerializeField] string role = "Ingeniero de mantenimiento";
        [TextArea(2, 4)]
        [SerializeField] string description = "";

        [SerializeField] Skin[] skins = new Skin[0];

        public string CharacterName => characterName;
        public string Role => role;
        public string Description => description;
        public int Count => skins.Length;

        public Skin Get(int index) => skins[Mathf.Clamp(index, 0, Mathf.Max(0, skins.Length - 1))];

        /// <summary>Convierte cualquier índice (incluso negativo) en uno válido, dando la vuelta.</summary>
        public int Wrap(int index) => skins.Length == 0 ? 0 : ((index % skins.Length) + skins.Length) % skins.Length;
    }
}
