using UnityEngine;
using HorrorGame.Core;

namespace HorrorGame.Player
{
    /// <summary>
    /// Controlador de movimento em primeira pessoa.
    /// Suporta andar, correr, agachar e pular com sistema de stamina.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float crouchSpeed = 1.5f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 8f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -15f;

        [Header("Crouch Settings")]
        [SerializeField] private float standHeight = 2f;
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float crouchTransitionSpeed = 10f;

        [Header("Stamina Settings")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 20f;
        [SerializeField] private float staminaRegenRate = 15f;
        [SerializeField] private float staminaRegenDelay = 2f;

        [Header("Input")]
        [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;

        [Header("Head Bob")]
        [SerializeField] private bool enableHeadBob = true;
        [SerializeField] private float bobFrequency = 2f;
        [SerializeField] private float bobHorizontalAmount = 0.05f;
        [SerializeField] private float bobVerticalAmount = 0.1f;

        // Components
        private CharacterController controller;
        private Camera playerCamera;

        // Movement
        private Vector3 velocity;
        private Vector3 currentVelocity;
        private bool isRunning = false;
        private bool isCrouching = false;
        private float targetHeight;
        private float currentHeight;

        // Stamina
        private float currentStamina;
        private float staminaRegenTimer;

        // Head bob
        private float bobTimer;
        private Vector3 cameraOriginalPosition;

        public bool IsMoving => controller.velocity.magnitude > 0.1f;
        public bool IsGrounded => controller.isGrounded;
        public float CurrentStamina => currentStamina;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            playerCamera = GetComponentInChildren<Camera>();

            if (playerCamera != null)
            {
                cameraOriginalPosition = playerCamera.transform.localPosition;
            }

            currentStamina = maxStamina;
            currentHeight = standHeight;
            targetHeight = standHeight;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
                return;

            HandleMovement();
            HandleCrouch();
            HandleJump();
            HandleGravity();
            HandleStamina();
            HandleHeadBob();
        }

        private void HandleMovement()
        {
            // Input
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

            // Determina velocidade baseada no estado
            isRunning = Input.GetKey(runKey) && !isCrouching && currentStamina > 0f;

            float targetSpeed = walkSpeed;
            if (isRunning)
                targetSpeed = runSpeed;
            else if (isCrouching)
                targetSpeed = crouchSpeed;

            // Calcula movimento relativo à câmera
            Vector3 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.z;
            Vector3 targetVelocity = moveDirection * targetSpeed;

            // Suaviza aceleração/desaceleração
            float accel = inputDirection.magnitude > 0.1f ? acceleration : deceleration;
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, accel * Time.deltaTime);

            // Aplica movimento
            controller.Move(currentVelocity * Time.deltaTime);
        }

        private void HandleCrouch()
        {
            // Toggle crouch
            if (Input.GetKeyDown(crouchKey))
            {
                isCrouching = !isCrouching;
            }

            // Define altura alvo
            targetHeight = isCrouching ? crouchHeight : standHeight;

            // Suaviza transição de altura
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
            controller.height = currentHeight;

            // Ajusta centro do controller
            controller.center = Vector3.up * (currentHeight / 2f);
        }

        private void HandleJump()
        {
            if (Input.GetKeyDown(jumpKey) && controller.isGrounded && !isCrouching)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        private void HandleGravity()
        {
            if (controller.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Pequeno valor negativo para manter no chão
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleStamina()
        {
            bool wasRunning = isRunning;

            // Drena stamina ao correr
            if (isRunning && IsMoving)
            {
                currentStamina -= staminaDrainRate * Time.deltaTime;
                currentStamina = Mathf.Max(currentStamina, 0f);
                staminaRegenTimer = staminaRegenDelay;

                // Para de correr se stamina acabar
                if (currentStamina <= 0f)
                {
                    isRunning = false;
                }
            }
            else
            {
                // Regenera stamina após delay
                if (staminaRegenTimer > 0f)
                {
                    staminaRegenTimer -= Time.deltaTime;
                }
                else
                {
                    currentStamina += staminaRegenRate * Time.deltaTime;
                    currentStamina = Mathf.Min(currentStamina, maxStamina);
                }
            }

            // Notifica mudança de stamina
            GameEvents.OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        private void HandleHeadBob()
        {
            if (!enableHeadBob || playerCamera == null || !IsMoving || !IsGrounded)
            {
                // Retorna à posição original
                if (playerCamera != null)
                {
                    playerCamera.transform.localPosition = Vector3.Lerp(
                        playerCamera.transform.localPosition,
                        cameraOriginalPosition,
                        Time.deltaTime * 5f
                    );
                }
                bobTimer = 0f;
                return;
            }

            // Calcula head bob
            bobTimer += Time.deltaTime * (isRunning ? bobFrequency * 1.5f : bobFrequency);

            float bobX = Mathf.Cos(bobTimer) * bobHorizontalAmount;
            float bobY = Mathf.Sin(bobTimer * 2f) * bobVerticalAmount;

            Vector3 bobOffset = new Vector3(bobX, bobY, 0f);
            playerCamera.transform.localPosition = cameraOriginalPosition + bobOffset;
        }

        /// <summary>
        /// Teleporta o player para uma nova posição
        /// </summary>
        public void Teleport(Vector3 position)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
            velocity = Vector3.zero;
        }

        /// <summary>
        /// Adiciona força ao player (útil para knockback)
        /// </summary>
        public void AddForce(Vector3 force)
        {
            velocity += force;
        }
    }
}
