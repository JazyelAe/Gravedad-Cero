using System;
using UnityEngine;

namespace GravedadCero.Audio
{
    /// <summary>
    /// Qué música suena en cada escena y qué sonido corresponde a cada evento del juego.
    /// Hay un único catálogo en Resources (AudioCatalog) que GameAudio carga al empezar.
    /// </summary>
    [CreateAssetMenu(menuName = "Gravedad Cero/Catálogo de audio", fileName = "AudioCatalog")]
    public class AudioCatalog : ScriptableObject
    {
        [Serializable]
        public struct SceneMusic
        {
            public string scene;
            public AudioClip clip;
        }

        [Header("Música por escena (sin entrada = silencio)")]
        public SceneMusic[] music = new SceneMusic[0];
        public AudioClip victory;

        [Header("Efectos de juego")]
        public AudioClip gravityChange;
        public AudioClip anchor;
        public AudioClip release;
        public AudioClip doorOpen;
        public AudioClip doorClose;
        public AudioClip platePressed;
        public AudioClip switchUsed;
        public AudioClip respawn;

        [Header("Interfaz")]
        public AudioClip uiMove;
        public AudioClip uiClick;

        public AudioClip MusicFor(string scene)
        {
            foreach (var entry in music)
                if (entry.scene == scene) return entry.clip;
            return null;
        }
    }
}
