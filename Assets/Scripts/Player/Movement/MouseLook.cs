using UnityEngine;
using HorrorGame.Core;

namespace HorrorGame.Player
{
    /// <summary>
    /// Controla a rotação da câmera em primeira pessoa com o mouse
    /// </summary>
    public class MouseLook : MonoBehaviour
    {
        [Header("Mouse Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private bool invertY = false;

        [Header("Camera Limits")]
        [SerializeField] private float minVerticalAngle = -90f;
        [SerializeField] private float maxVerticalAngle = 90f;

        [Header("Smoothing")]
        [SerializeField] private bool smoothRotation = true;
        [SerializeField] private float smoothSpeed = 10f;

        [Header("Camera Shake")]
        [SerializeField] private bool enableCameraShake = true;
        [SerializeField] private float shakeDecay = 5f;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private float xRotation = 0f;
        private float yRotation = 0f;
        private float targetXRotation = 0f;
        private float targetYRotation = 0f;

        // Camera shake
        private float shakeIntensity = 0f;
        private float shakeDuration = 0f;

        private void Awake()
        {
            if (cameraTransform == null)
            {
                cameraTransform = GetComponentInChildren<Camera>().transform;
            }
        }

        private void Start()
        {
            // Cursor setup (importante para FPS)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
                return;

            HandleMouseInput();
            ApplyRotation();
            UpdateCameraShake();
        }

        private void HandleMouseInput()
        {
            // Recebe input do mouse
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Inverte Y se necessário
            if (invertY)
                mouseY = -mouseY;

            // Rotação horizontal (corpo do player)
            targetYRotation += mouseX;

            // Rotação vertical (câmera)
            targetXRotation -= mouseY;
            targetXRotation = Mathf.Clamp(targetXRotation, minVerticalAngle, maxVerticalAngle);
        }

        private void ApplyRotation()
        {
            if (smoothRotation)
            {
                // Suaviza a rotação
                xRotation = Mathf.Lerp(xRotation, targetXRotation, smoothSpeed * Time.deltaTime);
                yRotation = Mathf.Lerp(yRotation, targetYRotation, smoothSpeed * Time.deltaTime);
            }
            else
            {
                xRotation = targetXRotation;
                yRotation = targetYRotation;
            }

            // Aplica rotação
            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            }

            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }

        private void UpdateCameraShake()
        {
            if (!enableCameraShake || cameraTransform == null) return;

            if (shakeDuration > 0f)
            {
                shakeDuration -= Time.deltaTime;

                // Gera shake aleatório
                Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
                shakeOffset.z = 0f; // Não move a câmera para frente/trás

                // Aplica shake
                cameraTransform.localPosition += shakeOffset;

                // Decai a intensidade
                shakeIntensity = Mathf.Lerp(shakeIntensity, 0f, shakeDecay * Time.deltaTime);
            }
        }

        /// <summary>
        /// Ativa shake na câmera (útil para quando o player leva dano ou explosões)
        /// </summary>
        public void ShakeCamera(float intensity, float duration)
        {
            shakeIntensity = intensity;
            shakeDuration = duration;
        }

        /// <summary>
        /// Define a sensibilidade do mouse
        /// </summary>
        public void SetSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }

        /// <summary>
        /// Inverte o eixo Y
        /// </summary>
        public void SetInvertY(bool invert)
        {
            invertY = invert;
        }
    }
}
