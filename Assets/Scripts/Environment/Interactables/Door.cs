using UnityEngine;
using HorrorGame.Player;
using HorrorGame.Core;

namespace HorrorGame.Environment
{
    /// <summary>
    /// Porta interagível que pode ser aberta/fechada
    /// </summary>
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("Door Settings")]
        [SerializeField] private bool isLocked = false;
        [SerializeField] private string requiredKeyID = "";
        [SerializeField] private bool canClose = true;

        [Header("Animation")]
        [SerializeField] private Transform doorTransform;
        [SerializeField] private Vector3 openRotation = new Vector3(0, 90, 0);
        [SerializeField] private float openSpeed = 2f;
        [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Audio")]
        [SerializeField] private string openSoundName = "door_open";
        [SerializeField] private string closeSoundName = "door_close";
        [SerializeField] private string lockedSoundName = "door_locked";

        private bool isOpen = false;
        private bool isAnimating = false;
        private Quaternion closedRotation;
        private Quaternion targetRotation;
        private float animationProgress = 0f;

        private void Start()
        {
            if (doorTransform == null)
                doorTransform = transform;

            closedRotation = doorTransform.localRotation;
            targetRotation = closedRotation;
        }

        private void Update()
        {
            if (isAnimating)
            {
                AnimateDoor();
            }
        }

        #region IInteractable Implementation

        public string GetInteractionPrompt()
        {
            if (isLocked)
                return "Locked";

            return isOpen ? "Close Door [E]" : "Open Door [E]";
        }

        public bool CanInteract()
        {
            return !isAnimating;
        }

        public void Interact(GameObject player)
        {
            if (isLocked)
            {
                PlayLockedSound();
                return;
            }

            if (isOpen && canClose)
            {
                CloseDoor();
            }
            else if (!isOpen)
            {
                OpenDoor();
            }
        }

        #endregion

        #region Door Control

        public void OpenDoor()
        {
            if (isOpen || isAnimating || isLocked) return;

            isOpen = true;
            isAnimating = true;
            animationProgress = 0f;
            targetRotation = closedRotation * Quaternion.Euler(openRotation);

            PlayOpenSound();
            GameEvents.OnDoorToggled?.Invoke(gameObject, true);

            Debug.Log($"[Door] Opening door: {name}");
        }

        public void CloseDoor()
        {
            if (!isOpen || isAnimating || !canClose) return;

            isOpen = false;
            isAnimating = true;
            animationProgress = 0f;
            targetRotation = closedRotation;

            PlayCloseSound();
            GameEvents.OnDoorToggled?.Invoke(gameObject, false);

            Debug.Log($"[Door] Closing door: {name}");
        }

        private void AnimateDoor()
        {
            animationProgress += Time.deltaTime * openSpeed;

            if (animationProgress >= 1f)
            {
                animationProgress = 1f;
                isAnimating = false;
            }

            float curveValue = openCurve.Evaluate(animationProgress);
            Quaternion startRotation = isOpen ? closedRotation : closedRotation * Quaternion.Euler(openRotation);
            doorTransform.localRotation = Quaternion.Lerp(startRotation, targetRotation, curveValue);
        }

        #endregion

        #region Lock System

        public void Lock()
        {
            isLocked = true;
            Debug.Log($"[Door] Door locked: {name}");
        }

        public void Unlock()
        {
            isLocked = false;
            Debug.Log($"[Door] Door unlocked: {name}");
        }

        public void SetRequiredKey(string keyID)
        {
            requiredKeyID = keyID;
        }

        public bool TryUnlockWithKey(string keyID)
        {
            if (keyID == requiredKeyID)
            {
                Unlock();
                return true;
            }
            return false;
        }

        #endregion

        #region Audio

        private void PlayOpenSound()
        {
            if (!string.IsNullOrEmpty(openSoundName))
            {
                GameEvents.OnPlaySound3D?.Invoke(openSoundName, transform.position);
            }
        }

        private void PlayCloseSound()
        {
            if (!string.IsNullOrEmpty(closeSoundName))
            {
                GameEvents.OnPlaySound3D?.Invoke(closeSoundName, transform.position);
            }
        }

        private void PlayLockedSound()
        {
            if (!string.IsNullOrEmpty(lockedSoundName))
            {
                GameEvents.OnPlaySound3D?.Invoke(lockedSoundName, transform.position);
            }

            Debug.Log($"[Door] Door is locked: {name}");
        }

        #endregion
    }
}
