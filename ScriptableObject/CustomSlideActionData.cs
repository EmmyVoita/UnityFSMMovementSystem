using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/FSM/CustomSlideAction")]
public class CustomSlideActionData : ScriptableObject
{
    [Header("Inscribed")]
    public float slideCooldown = 1.0f;
    public float slideSpeed = 10.0f;
    public float controlsLockoutTime = 0.4f;
    public float slideDuration = 0.3f;

    [Header(" - SpeedChangeRateCurve")]
    public float speedChangeRate = 1.0f;
    public float speedOffset = 0.1f;

    [Header(" - PlayerRotation")]
    public float slideRotationSmoothTime = 0.4f;
    public float maxAngle = 15.0f;

    [Header(" - InputLocking")]
    public float inputLockTime = 0.4f;

    [Header("Dynamic")]
    [SerializeField]
    private float startRotation;
    [SerializeField]
    private float internalLockControlsTimer = 0;
    [SerializeField]
    private float internalSlideCooldownTimer = 0;
    [SerializeField]
    private float internalSlideTimer = 0;
    [SerializeField]
    private bool canSlide = true;
    [SerializeField]
    private float internalInputLockTimer = 0;
    [SerializeField]
    private bool canLock = false;




    public void SetCanSlide(bool value) { canSlide = value; }
    public void SetCanLock(bool value) { canLock = value; }
    public bool GetCanSlide() { return canSlide; }

    public void UseCooldown()
    {
        internalSlideCooldownTimer = slideCooldown;
        SetCanSlide(false);
        SetCanLock(true);
    }

    public bool IsSlidingCheck()
    {
        return internalSlideTimer >= slideDuration ? false : true;
    }

    public bool IsMovementLockoutCheck()
    {
        return internalLockControlsTimer >= controlsLockoutTime ? false : true;
    }

    public void IncrementInternalSlideTimer()
    {
        internalSlideTimer += Time.deltaTime;
    }

    public bool ManageInternalInputLockTimer()
    {
        if (canLock)
        {
            internalInputLockTimer += Time.deltaTime;
            if (internalInputLockTimer >= inputLockTime)
            {
                internalInputLockTimer = 0;
                canLock = false;
                return false;
            }
            return true;
        }
        return false;
    }

    public void IncrementInternalMovementLockOutTimer()
    {
        internalLockControlsTimer += Time.deltaTime;
    }

    public void DeincrementInternalSlideCooldownTimer()
    {
        internalSlideCooldownTimer -= Time.deltaTime;
        if (internalSlideCooldownTimer <= 0.0f)
        {
            SetCanSlide(true);
            internalSlideCooldownTimer = 0.0f;
        }
    }

    public void ResetInternalSlideTimer()
    {
        internalSlideTimer = 0;
    }

    public void ResetInternalMovementLockOutTimer()
    {
        internalLockControlsTimer = 0;
    }

    public void SetStartRotation(float rotation)
    {
        startRotation = rotation;
    }

    public float GetStartRotation() { return startRotation; }

}
