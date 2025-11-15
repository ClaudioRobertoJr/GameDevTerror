using UnityEngine;
using HorrorGame.Core;
using HorrorGame.Player;

namespace HorrorGame.Utilities
{
    /// <summary>
    /// Checkpoint que salva a posição do player
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint Settings")]
        [SerializeField] private bool activateOnTrigger = true;
        [SerializeField] private bool saveOnActivate = true;
        [SerializeField] private bool healPlayerOnActivate = true;
        [SerializeField] private float healAmount = 50f;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject activeVisual;
        [SerializeField] private GameObject inactiveVisual;

        [Header("Audio")]
        [SerializeField] private string activationSound = "checkpoint_activated";

        private bool isActivated = false;

        private void Start()
        {
            UpdateVisuals();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!activateOnTrigger || isActivated) return;

            if (other.CompareTag("Player"))
            {
                ActivateCheckpoint(other.gameObject);
            }
        }

        public void ActivateCheckpoint(GameObject player)
        {
            if (isActivated) return;

            isActivated = true;

            // Notifica eventos
            GameEvents.OnCheckpointReached?.Invoke(transform.position);

            // Cura o player se configurado
            if (healPlayerOnActivate)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Heal(healAmount);
                }
            }

            // Toca som
            if (!string.IsNullOrEmpty(activationSound))
            {
                GameEvents.OnPlaySound3D?.Invoke(activationSound, transform.position);
            }

            // Mostra mensagem
            GameEvents.OnShowMessage?.Invoke("Checkpoint Reached", 3f);

            UpdateVisuals();

            Debug.Log($"[Checkpoint] Activated at {transform.position}");
        }

        private void UpdateVisuals()
        {
            if (activeVisual != null)
                activeVisual.SetActive(isActivated);

            if (inactiveVisual != null)
                inactiveVisual.SetActive(!isActivated);
        }

        public Vector3 GetRespawnPosition()
        {
            return transform.position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isActivated ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
        }
    }
}
