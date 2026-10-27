using System;
using GravedadCero.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GravedadCero.Level
{
    /// <summary>
    /// Registro de datos escondido: un objeto luminoso que Kai recoge al tocarlo. Se guarda en
    /// GameProgress con el id "escena/nombre", así que no reaparece en partidas siguientes.
    /// El menú muestra cuántos hay encontrados en cada sección.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DataLog : MonoBehaviour
    {
        [SerializeField] string logName = "registro";
        [Tooltip("Texto que se muestra al recogerlo.")]
        [SerializeField, TextArea(1, 3)] string message = "";
        [Tooltip("Parte visible: gira y se oculta al recogerlo.")]
        [SerializeField] Transform visual;

        public string Id => SceneManager.GetActiveScene().name + "/" + logName;
        public string Message => message;
        public bool IsCollected { get; private set; }

        /// <summary>Se lanza al recogerlo.</summary>
        public event Action<DataLog> Collected;

        public void Configure(string nameInScene, string text, Transform visualRoot)
        {
            logName = nameInScene;
            message = text;
            visual = visualRoot;
        }

        void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        void Start()
        {
            if (GameProgress.HasLog(Id)) Hide();
        }

        void Update()
        {
            if (visual != null && !IsCollected) visual.Rotate(0f, 90f * Time.deltaTime, 0f, Space.Self);
        }

        void OnTriggerEnter(Collider other)
        {
            if (IsCollected || !other.CompareTag("Player")) return;
            GameProgress.CollectLog(Id);
            Hide();
            Collected?.Invoke(this);
        }

        void Hide()
        {
            IsCollected = true;
            if (visual != null) visual.gameObject.SetActive(false);
        }
    }
}
