using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;
    public float FootstepInterval = 0.5f; // Adjust this to control the footstep rate
    private CharacterController characterController;
    private float lastFootstepTime;

    private void Awake()
    {
        characterController = GetComponentInParent<CharacterController>();
        lastFootstepTime = Time.time;
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                // Check if enough time has passed since the last footstep
                if (Time.time - lastFootstepTime >= FootstepInterval)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(characterController.center), FootstepAudioVolume);
                    lastFootstepTime = Time.time; // Update last footstep time
                }
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(characterController.center), FootstepAudioVolume);
        }
    }
}
