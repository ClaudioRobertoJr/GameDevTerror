using UnityEngine;
using HorrorGame.Player;
using HorrorGame.Core;

namespace HorrorGame.Environment
{
    /// <summary>
    /// Item coletável genérico (chaves, munição, kits médicos, etc)
    /// </summary>
    public class Collectible : MonoBehaviour, IInteractable
    {
        [Header("Collectible Settings")]
        [SerializeField] private CollectibleType type = CollectibleType.HealthKit;
        [SerializeField] private string itemName = "Item";
        [SerializeField] private float value = 50f;
        [SerializeField] private bool autoCollectOnTrigger = false;

        [Header("Visual")]
        [SerializeField] private bool rotateObject = true;
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private bool bobUpDown = true;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobAmount = 0.2f;

        [Header("Audio")]
        [SerializeField] private string collectSound = "item_collected";

        [Header("Effects")]
        [SerializeField] private GameObject collectEffect;

        private Vector3 startPosition;
        private float bobTimer = 0f;

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            if (rotateObject)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }

            if (bobUpDown)
            {
                bobTimer += Time.deltaTime * bobSpeed;
                float yOffset = Mathf.Sin(bobTimer) * bobAmount;
                transform.position = startPosition + Vector3.up * yOffset;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (autoCollectOnTrigger && other.CompareTag("Player"))
            {
                Interact(other.gameObject);
            }
        }

        #region IInteractable Implementation

        public string GetInteractionPrompt()
        {
            return $"Collect {itemName} [E]";
        }

        public bool CanInteract()
        {
            return true;
        }

        public void Interact(GameObject player)
        {
            ApplyEffect(player);
            Collect();
        }

        #endregion

        private void ApplyEffect(GameObject player)
        {
            switch (type)
            {
                case CollectibleType.HealthKit:
                    PlayerHealth health = player.GetComponent<PlayerHealth>();
                    if (health != null)
                    {
                        health.Heal(value);
                        GameEvents.OnShowMessage?.Invoke($"Healed +{value}", 2f);
                    }
                    break;

                case CollectibleType.Key:
                    // TODO: Adicionar ao inventário
                    GameEvents.OnShowMessage?.Invoke($"Collected {itemName}", 2f);
                    break;

                case CollectibleType.Ammo:
                    // TODO: Adicionar munição
                    GameEvents.OnShowMessage?.Invoke($"Ammo +{value}", 2f);
                    break;

                case CollectibleType.Note:
                    // TODO: Abrir UI de leitura
                    GameEvents.OnShowMessage?.Invoke($"Found {itemName}", 2f);
                    break;
            }
        }

        private void Collect()
        {
            // Toca som
            if (!string.IsNullOrEmpty(collectSound))
            {
                GameEvents.OnPlaySound3D?.Invoke(collectSound, transform.position);
            }

            // Spawna efeito visual
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }

            // Notifica evento
            GameEvents.OnItemCollected?.Invoke(gameObject);

            Debug.Log($"[Collectible] {itemName} collected");

            // Destrói objeto
            Destroy(gameObject);
        }
    }

    public enum CollectibleType
    {
        HealthKit,
        Ammo,
        Key,
        Note,
        Battery,
        Other
    }
}
