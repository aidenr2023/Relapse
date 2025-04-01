    using UnityEngine;

    public class PlayerMovementAnimationHelper : MonoBehaviour
    {
        [SerializeField] private PlayerMovementV2 playerMovementV2;
        
        public void EnableSprintingAnimationPlaying()
        {
            playerMovementV2.SetSprintAnimationPlaying(true);
        }
        
        public void DisableSprintingAnimationPlaying()
        {
            playerMovementV2.SetSprintAnimationPlaying(false);
        }
    }
