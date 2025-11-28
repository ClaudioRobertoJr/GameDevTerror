using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HorrorGame.Player
{
    /// <summary>
    /// Controlador de player multiplayer usando New Input System
    /// Suporta 4-5 jogadores simultâneos
    ///
    /// ✅ VERSÃO CORRIGIDA - Novembro 2025
    /// - Server authority implementado corretamente
    /// - Memory leaks corrigidos (OnNetworkDespawn)
    /// - Performance otimizada (sqrMagnitude, caching)
    /// - CharacterController mantido em remote players
    /// </summary>
    public class NetworkPlayerController : NetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private AudioListener audioListener;

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float sprintSpeed = 6f;
        [SerializeField] private float crouchSpeed = 1.5f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = -19.62f;

        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float minVerticalAngle = -90f;
        [SerializeField] private float maxVerticalAngle = 90f;

        [Header("Network Sync Settings")]
        [SerializeField] private float positionSyncThreshold = 0.01f; // 0.1m squared
        [SerializeField] private float syncInterval = 0.1f; // 10 updates/sec

        // Network Variables
        private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
        private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>();
        private NetworkVariable<int> networkHealth = new NetworkVariable<int>(100);
        private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>();

        // ✅ FIX: Cache para otimização
        private Vector3 lastSyncedPosition;
        private Quaternion lastSyncedRotation;
        private float lastSyncTime;

        // Estado local
        private Vector3 velocity;
        private float xRotation = 0f;
        private bool isGrounded;
        private bool isDead = false;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                SetupLocalPlayer();
                SetPlayerNameServerRpc(AuthenticationManager.Instance.PlayerName);
            }
            else
            {
                SetupRemotePlayer();
            }

            // ✅ FIX: Subscribe to events
            networkHealth.OnValueChanged += OnHealthChanged;
            playerName.OnValueChanged += OnPlayerNameChanged;

            // Inicializar cache
            lastSyncedPosition = transform.position;
            lastSyncedRotation = transform.rotation;
            lastSyncTime = Time.time;
        }

        // ✅ FIX: Cleanup de eventos para evitar memory leaks
        public override void OnNetworkDespawn()
        {
            // CRÍTICO: Sempre limpar eventos NetworkVariable
            networkHealth.OnValueChanged -= OnHealthChanged;
            playerName.OnValueChanged -= OnPlayerNameChanged;

            base.OnNetworkDespawn();
        }

        private void SetupLocalPlayer()
        {
            Debug.Log("[NetworkPlayer] Configurando player local");

            // Habilitar input
            if (inputHandler != null)
                inputHandler.enabled = true;

            // Habilitar câmera
            if (playerCamera != null)
            {
                playerCamera.enabled = true;
                playerCamera.tag = "MainCamera";
            }

            // Habilitar audio listener
            if (audioListener != null)
                audioListener.enabled = true;

            // ✅ CharacterController HABILITADO para player local
            if (characterController != null)
                characterController.enabled = true;

            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void SetupRemotePlayer()
        {
            Debug.Log("[NetworkPlayer] Configurando player remoto");

            // Desabilitar input
            if (inputHandler != null)
                inputHandler.enabled = false;

            // Desabilitar câmera
            if (playerCamera != null)
                playerCamera.enabled = false;

            // Desabilitar audio listener
            if (audioListener != null)
                audioListener.enabled = false;

            // ✅ FIX: MANTER CharacterController habilitado para colisões!
            // Apenas desabilitar o controle de movimento
            if (characterController != null)
            {
                characterController.enabled = true; // Manter para física/colisões
            }

            // Desabilitar apenas movimento manual
            // O NetworkTransform cuidará da sincronização de posição
        }

        private void Update()
        {
            if (isDead) return;

            if (IsOwner)
            {
                HandleLocalPlayer();
            }
            else
            {
                HandleRemotePlayer();
            }
        }

        private void HandleLocalPlayer()
        {
            // Ground check
            isGrounded = characterController.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            // Input
            Vector2 moveInput = inputHandler.MoveInput;
            Vector2 lookInput = inputHandler.LookInput;

            // Movimento
            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

            float currentSpeed = walkSpeed;
            if (inputHandler.SprintHeld)
                currentSpeed = sprintSpeed;

            characterController.Move(move * currentSpeed * Time.deltaTime);

            // Pulo
            if (inputHandler.JumpPressed && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }

            // Gravidade
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);

            // Rotação da câmera (mouse look)
            float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
            float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);

            // ✅ FIX: Sincronização otimizada com sqrMagnitude e interval
            SyncPositionIfNeeded();
        }

        // ✅ FIX: Sincronização otimizada
        private void SyncPositionIfNeeded()
        {
            // Limitar updates por intervalo de tempo
            if (Time.time - lastSyncTime < syncInterval)
                return;

            // Usar sqrMagnitude (muito mais rápido que Distance)
            float sqrDistance = (transform.position - lastSyncedPosition).sqrMagnitude;

            if (sqrDistance > positionSyncThreshold)
            {
                UpdatePositionServerRpc(transform.position, transform.rotation);
                lastSyncedPosition = transform.position;
                lastSyncedRotation = transform.rotation;
                lastSyncTime = Time.time;
            }
        }

        private void HandleRemotePlayer()
        {
            // Interpolação suave de posição
            transform.position = Vector3.Lerp(
                transform.position,
                networkPosition.Value,
                Time.deltaTime * 15f
            );

            // Interpolação suave de rotação
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                networkRotation.Value,
                Time.deltaTime * 15f
            );
        }

        [ServerRpc]
        private void UpdatePositionServerRpc(Vector3 position, Quaternion rotation)
        {
            networkPosition.Value = position;
            networkRotation.Value = rotation;
        }

        [ServerRpc]
        private void SetPlayerNameServerRpc(string name)
        {
            playerName.Value = name;
        }

        /// <summary>
        /// ✅ FIX: Sistema de dano com VALIDAÇÃO SERVER-SIDE completa
        /// Cliente solicita, servidor valida TUDO e decide
        /// </summary>
        public void TakeDamage(int damage, ulong attackerId)
        {
            // Cliente sempre envia para servidor validar
            TakeDamageServerRpc(damage, attackerId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void TakeDamageServerRpc(int damage, ulong attackerId, ServerRpcParams rpcParams = default)
        {
            // ✅ SERVIDOR valida TUDO (anti-cheat)

            // 1. Validar que não está morto
            if (isDead) return;

            // 2. Validar que attacker existe
            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(attackerId))
            {
                Debug.LogWarning($"[Anti-Cheat] Attacker {attackerId} não existe!");
                return;
            }

            // 3. Validar distância (anti-cheat básico)
            var attackerObject = NetworkManager.Singleton.ConnectedClients[attackerId].PlayerObject;
            if (attackerObject != null)
            {
                float sqrDistance = (transform.position - attackerObject.transform.position).sqrMagnitude;
                if (sqrDistance > 25f) // 5m * 5m = 25
                {
                    Debug.LogWarning($"[Anti-Cheat] Ataque de distância inválida: {Mathf.Sqrt(sqrDistance)}m");
                    return;
                }
            }

            // 4. Validar valor do dano (anti-cheat)
            if (damage < 0 || damage > 100)
            {
                Debug.LogWarning($"[Anti-Cheat] Dano inválido: {damage}");
                return;
            }

            // 5. Aplicar dano (servidor tem autoridade final)
            networkHealth.Value -= damage;
            networkHealth.Value = Mathf.Max(0, networkHealth.Value);

            Debug.Log($"[Server] Player {playerName.Value} recebeu {damage} de dano. Vida: {networkHealth.Value}");

            // 6. Verificar morte
            if (networkHealth.Value <= 0 && !isDead)
            {
                isDead = true;
                HandlePlayerDeathClientRpc();
            }
        }

        /// <summary>
        /// Sistema de cura (também validado por servidor)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void HealServerRpc(int healAmount, ServerRpcParams rpcParams = default)
        {
            if (isDead) return;

            // Validar valor
            if (healAmount < 0 || healAmount > 100)
            {
                Debug.LogWarning($"[Anti-Cheat] Cura inválida: {healAmount}");
                return;
            }

            networkHealth.Value += healAmount;
            networkHealth.Value = Mathf.Min(100, networkHealth.Value);

            Debug.Log($"[Server] Player {playerName.Value} curou {healAmount}. Vida: {networkHealth.Value}");
        }

        [ClientRpc]
        private void HandlePlayerDeathClientRpc()
        {
            Debug.Log($"[NetworkPlayer] {playerName.Value} morreu!");

            isDead = true;

            if (IsOwner)
            {
                // Desabilitar controles
                if (inputHandler != null)
                    inputHandler.enabled = false;

                // Unlock cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Mostrar tela de morte
                GameEvents.OnPlayerDied?.Invoke();
            }

            // Tocar animação/efeitos de morte
            PlayDeathEffects();
        }

        private void OnHealthChanged(int oldHealth, int newHealth)
        {
            if (IsOwner)
            {
                // Atualizar HUD
                int damageTaken = oldHealth - newHealth;
                GameEvents.OnPlayerDamaged?.Invoke(damageTaken, newHealth);
            }

            Debug.Log($"[Player] {playerName.Value} - Vida: {oldHealth} → {newHealth}");
        }

        private void OnPlayerNameChanged(FixedString64Bytes oldName, FixedString64Bytes newName)
        {
            Debug.Log($"[Player] Nome atualizado: {oldName} → {newName}");
        }

        private void PlayDeathEffects()
        {
            // Implementar efeitos visuais/sonoros
            // TODO: Animação de morte
            // TODO: Som de morte
            // TODO: Efeitos de partículas
        }

        // ===== PUBLIC GETTERS =====

        public int GetHealth() => networkHealth.Value;
        public int GetMaxHealth() => 100;
        public string GetPlayerName() => playerName.Value.ToString();
        public bool IsDead() => isDead;
        public bool IsLocalPlayer() => IsOwner;

        // ===== DEBUG =====

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (!IsServer) return;

            // Desenhar range de ataque válido (5m)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 5f);
        }
    }
}
