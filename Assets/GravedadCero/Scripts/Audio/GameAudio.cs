using GravedadCero.Enemies;
using GravedadCero.Gravity;
using GravedadCero.Interaction;
using GravedadCero.Level;
using GravedadCero.Player;
using GravedadCero.Puzzles;
using GravedadCero.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GravedadCero.Audio
{
    /// <summary>
    /// Música y efectos de sonido de todo el juego. Se crea sola al arrancar y sobrevive a los
    /// cambios de escena, así que la música no se corta: al cargar una escena hace un fundido
    /// cruzado hacia su pista (o sigue con la misma si no cambia). Escucha los eventos de los
    /// componentes de la escena (gravedad, anclaje, puertas, placas, interruptores, puntos de
    /// control, registros, drones, golpes, reaparición, pausa y victoria); ninguno de ellos conoce el audio. Usa tiempo no
    /// escalado, así que funciona en pausa.
    /// </summary>
    public class GameAudio : MonoBehaviour
    {
        const float CrossfadeSeconds = 1.2f;
        const float PausedMusicLevel = 0.4f;
        const float VictoryMusicLevel = 0.25f;
        // Los mecanismos que ya empiezan activados (una caja sobre su placa) no suenan al cargar.
        const float SilenceAfterLoad = 0.5f;

        static GameAudio instance;

        AudioCatalog catalog;
        AudioSource[] music;
        int current;
        float[] fade = { 0f, 0f };
        float duck = 1f;
        float duckTarget = 1f;
        AudioSource sfx;
        float loadedAt;

        /// <summary>Pista que suena (o hacia la que se está haciendo el fundido); null en silencio.</summary>
        public static AudioClip CurrentMusic => instance != null ? instance.music[instance.current].clip : null;

        /// <summary>Último efecto reproducido y cuántos van (lo usan las pruebas).</summary>
        public static AudioClip LastSfx { get; private set; }
        public static int SfxCount { get; private set; }

        public static AudioCatalog Catalog => Ensure().catalog;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            Ensure().OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        static GameAudio Ensure()
        {
            if (instance != null) return instance;
            var go = new GameObject("GameAudio");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<GameAudio>();
            instance.Build();
            return instance;
        }

        void Build()
        {
            catalog = Resources.Load<AudioCatalog>("AudioCatalog");
            if (catalog == null) catalog = ScriptableObject.CreateInstance<AudioCatalog>();
            music = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                music[i] = gameObject.AddComponent<AudioSource>();
                music[i].loop = true;
                music[i].playOnAwake = false;
                music[i].ignoreListenerPause = true;
                music[i].spatialBlend = 0f;
                music[i].priority = 0;
            }
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.ignoreListenerPause = true;
            sfx.spatialBlend = 0f;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (instance == this) instance = null;
        }

        void Update()
        {
            float step = Time.unscaledDeltaTime / CrossfadeSeconds;
            for (int i = 0; i < 2; i++)
            {
                fade[i] = Mathf.MoveTowards(fade[i], i == current && music[i].clip != null ? 1f : 0f, step);
                if (fade[i] <= 0f && i != current && music[i].isPlaying) music[i].Stop();
            }
            duck = Mathf.MoveTowards(duck, duckTarget, Time.unscaledDeltaTime * 2f);
            float volume = GameSettings.MusicVolume * duck;
            for (int i = 0; i < 2; i++) music[i].volume = volume * fade[i];
        }

        // ---------- Música ----------

        /// <summary>Hace un fundido cruzado hacia esta pista (null = silencio).</summary>
        public static void PlayMusic(AudioClip clip) => Ensure().CrossfadeTo(clip);

        void CrossfadeTo(AudioClip clip)
        {
            if (music[current].clip == clip) return;
            current = 1 - current;
            var source = music[current];
            source.clip = clip;
            fade[current] = 0f;
            if (clip != null) source.Play();
        }

        // ---------- Efectos ----------

        public static void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            var audio = Ensure();
            if (Time.unscaledTime - audio.loadedAt < SilenceAfterLoad) return;
            audio.sfx.PlayOneShot(clip, volume * GameSettings.SfxVolume);
            LastSfx = clip;
            SfxCount++;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Single) return;
            loadedAt = Time.unscaledTime;
            duckTarget = 1f;
            CrossfadeTo(catalog.MusicFor(scene.name));
            BindScene();
        }

        /// <summary>Se suscribe a los eventos de los componentes de la escena recién cargada.</summary>
        /// <summary>Zumbido 3D en el propio dron y avisos al detectar a Kai o quedar aturdido.</summary>
        void BindDrone(SecurityDrone drone)
        {
            var hum = drone.gameObject.AddComponent<AudioSource>();
            hum.clip = catalog.droneHum;
            hum.loop = true;
            hum.spatialBlend = 1f;
            hum.rolloffMode = AudioRolloffMode.Linear;
            hum.minDistance = 2f;
            hum.maxDistance = 18f;
            hum.volume = 0.35f * GameSettings.SfxVolume;
            if (hum.clip != null) hum.Play();
            drone.StateChanged += (d, state) =>
            {
                if (state == SecurityDrone.State.Alert) Play(catalog.droneAlert, 0.7f);
                if (state == SecurityDrone.State.Stunned) Play(catalog.droneStunned, 0.8f);
                hum.pitch = state == SecurityDrone.State.Chase ? 1.25f : 1f;
                hum.volume = (state == SecurityDrone.State.Stunned ? 0f : 0.35f) * GameSettings.SfxVolume;
            };
        }

        /// <summary>Silbido 3D en la propia ventila cada vez que se enciende.</summary>
        void BindVent(TimedHazard vent)
        {
            var source = vent.gameObject.AddComponent<AudioSource>();
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 3f;
            source.maxDistance = 22f;
            source.playOnAwake = false;
            vent.PhaseChanged += (v, phase) =>
            {
                if (phase == TimedHazard.Phase.On && catalog.steamVent != null && Time.unscaledTime - loadedAt >= SilenceAfterLoad)
                    source.PlayOneShot(catalog.steamVent, 0.55f * GameSettings.SfxVolume);
            };
        }

        void BindScene()
        {
            foreach (var gravity in FindObjectsByType<GravityController>())
                gravity.GravityChanged += (from, to) => Play(catalog.gravityChange, 0.8f);
            foreach (var body in FindObjectsByType<AnchorableBody>())
                body.AnchorChanged += anchored => Play(anchored ? catalog.anchor : catalog.release);
            foreach (var door in FindObjectsByType<DoorController>())
                door.ActivationChanged += open => Play(open ? catalog.doorOpen : catalog.doorClose, 0.9f);
            foreach (var plate in FindObjectsByType<PressurePlate>())
                plate.PressedChanged += pressed => { if (pressed) Play(catalog.platePressed, 0.8f); };
            foreach (var interactor in FindObjectsByType<PlayerInteractor>())
                interactor.Interacted += _ => Play(catalog.switchUsed);
            foreach (var recovery in FindObjectsByType<FallRecovery>())
                recovery.Downed += _ => Play(catalog.respawn, 0.9f);
            foreach (var checkpoint in FindObjectsByType<Checkpoint>())
                checkpoint.Reached += _ => Play(catalog.switchUsed, 0.8f);
            foreach (var log in FindObjectsByType<DataLog>())
                log.Collected += _ => Play(catalog.dataLog);
            foreach (var box in FindObjectsByType<AnchorableBody>())
                box.Respawned += _ => Play(catalog.boxRespawn, 0.8f);
            foreach (var health in FindObjectsByType<PlayerHealth>())
                health.Damaged += _ => Play(catalog.playerHit);
            foreach (var drone in FindObjectsByType<SecurityDrone>()) BindDrone(drone);
            foreach (var vent in FindObjectsByType<TimedHazard>()) BindVent(vent);
            foreach (var pause in FindObjectsByType<PauseController>())
                pause.PauseChanged += paused => duckTarget = paused ? PausedMusicLevel : 1f;
            foreach (var level in FindObjectsByType<LevelManager>())
                level.Completed += () =>
                {
                    duckTarget = VictoryMusicLevel;
                    Play(catalog.victory);
                };
        }
    }
}
