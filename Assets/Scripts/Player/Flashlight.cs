using UnityEngine;
using HorrorGame.Core;

namespace HorrorGame.Player
{
    /// <summary>
    /// Sistema de lanterna com bateria
    /// Elemento essencial em jogos de terror
    /// </summary>
    public class Flashlight : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Light flashlightLight;
        [SerializeField] private GameObject flashlightModel;

        [Header("Settings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F;
        [SerializeField] private float maxIntensity = 2f;
        [SerializeField] private float minIntensity = 0.3f;

        [Header("Battery")]
        [SerializeField] private bool useBattery = true;
        [SerializeField] private float maxBatteryLife = 100f;
        [SerializeField] private float batteryDrainRate = 5f;
        [SerializeField] private float batteryRechargeRate = 10f;
        [SerializeField] private float batteryRechargeDelay = 3f;

        [Header("Flicker Effect")]
        [SerializeField] private bool flickerWhenLowBattery = true;
        [SerializeField] private float lowBatteryThreshold = 20f;
        [SerializeField] private float flickerSpeed = 0.1f;

        [Header("Audio")]
        [SerializeField] private string toggleOnSound = "flashlight_on";
        [SerializeField] private string toggleOffSound = "flashlight_off";
        [SerializeField] private string lowBatterySound = "flashlight_low_battery";

        private bool isOn = false;
        private float currentBattery;
        private float rechargeTimer = 0f;
        private float flickerTimer = 0f;
        private bool hasPlayedLowBatterySound = false;

        public bool IsOn => isOn;
        public float BatteryPercent => currentBattery / maxBatteryLife;

        private void Awake()
        {
            if (flashlightLight == null)
            {
                flashlightLight = GetComponent<Light>();
            }

            currentBattery = maxBatteryLife;
        }

        private void Start()
        {
            TurnOff();
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
                return;

            HandleInput();
            UpdateBattery();
            UpdateFlicker();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                Toggle();
            }
        }

        private void UpdateBattery()
        {
            if (!useBattery) return;

            if (isOn)
            {
                // Drena bateria
                currentBattery -= batteryDrainRate * Time.deltaTime;
                currentBattery = Mathf.Max(currentBattery, 0f);
                rechargeTimer = batteryRechargeDelay;

                // Desliga se bateria acabar
                if (currentBattery <= 0f)
                {
                    TurnOff();
                }

                // Som de bateria fraca
                if (currentBattery <= lowBatteryThreshold && !hasPlayedLowBatterySound)
                {
                    if (!string.IsNullOrEmpty(lowBatterySound))
                    {
                        GameEvents.OnPlaySound2D?.Invoke(lowBatterySound);
                    }
                    hasPlayedLowBatterySound = true;
                }
            }
            else
            {
                // Recarrega bateria após delay
                if (rechargeTimer > 0f)
                {
                    rechargeTimer -= Time.deltaTime;
                }
                else
                {
                    currentBattery += batteryRechargeRate * Time.deltaTime;
                    currentBattery = Mathf.Min(currentBattery, maxBatteryLife);

                    if (currentBattery > lowBatteryThreshold)
                    {
                        hasPlayedLowBatterySound = false;
                    }
                }
            }

            // Atualiza intensidade baseada na bateria
            UpdateIntensity();
        }

        private void UpdateIntensity()
        {
            if (flashlightLight == null || !isOn) return;

            // Intensidade diminui com bateria baixa
            float batteryPercent = currentBattery / maxBatteryLife;
            float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, batteryPercent);

            flashlightLight.intensity = targetIntensity;
        }

        private void UpdateFlicker()
        {
            if (!flickerWhenLowBattery || !isOn || flashlightLight == null) return;

            if (currentBattery <= lowBatteryThreshold)
            {
                flickerTimer += Time.deltaTime;

                if (flickerTimer >= flickerSpeed)
                {
                    flickerTimer = 0f;
                    flashlightLight.enabled = !flashlightLight.enabled;
                }
            }
            else
            {
                flashlightLight.enabled = true;
            }
        }

        #region Public Methods

        public void Toggle()
        {
            if (isOn)
            {
                TurnOff();
            }
            else
            {
                TurnOn();
            }
        }

        public void TurnOn()
        {
            if (isOn || (useBattery && currentBattery <= 0f)) return;

            isOn = true;

            if (flashlightLight != null)
                flashlightLight.enabled = true;

            if (flashlightModel != null)
                flashlightModel.SetActive(true);

            if (!string.IsNullOrEmpty(toggleOnSound))
            {
                GameEvents.OnPlaySound2D?.Invoke(toggleOnSound);
            }

            Debug.Log("[Flashlight] Turned on");
        }

        public void TurnOff()
        {
            if (!isOn) return;

            isOn = false;

            if (flashlightLight != null)
                flashlightLight.enabled = false;

            if (flashlightModel != null)
                flashlightModel.SetActive(false);

            if (!string.IsNullOrEmpty(toggleOffSound))
            {
                GameEvents.OnPlaySound2D?.Invoke(toggleOffSound);
            }

            Debug.Log("[Flashlight] Turned off");
        }

        public void RechargeBattery(float amount)
        {
            currentBattery += amount;
            currentBattery = Mathf.Min(currentBattery, maxBatteryLife);

            if (currentBattery > lowBatteryThreshold)
            {
                hasPlayedLowBatterySound = false;
            }
        }

        public void FullyRecharge()
        {
            RechargeBattery(maxBatteryLife);
        }

        #endregion
    }
}
