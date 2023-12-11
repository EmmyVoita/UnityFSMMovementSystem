using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/FSM/SlideAction")]
public class SlideActionData : ScriptableObject
{
    [Header("Inscribed")]
    public SpeedUpdateMethod speedUpdateMethod;
    public float baseSlideSpeed = 10.0f;
    
    public float slideResourceCost = 0.5f;

    [Header(" - GravityAccelerationDownSlope")]
    public bool useCustomGravity;
    public float gravity = 10.0f;
    public float currentMaxslideSpeed = 10.0f;
    public float maxGravityAdjustedSlideSpeed = 30.0f;
    public float gravityAccelerateSpeedChangeRate = 3.0f;


    [Header(" - SpeedChangeRateCurve")]
    public float speedChangeRate = 3.0f;
    public float speedOffset = 0.1f;


    [Header(" - PlayerRotation")]
    public float slideRotationSmoothTime = 0.4f;
    public float maxAngle = 15.0f;

 
    [Header("Cooldown_Slide")]
    public float slideCooldown = 1.0f;
    [SerializeField]
    private float timer_CooldownSlide = 0;


    [Header("State_CanSlide")]
    [SerializeField]
    private bool canSlide = true;

   
    [Header("State_IsSliding")]
    public float slideDuration = 0.3f;
    [SerializeField]
    private float timer_IsSliding = 0;
    [SerializeField]
    private bool state_IsSliding;


    [Header("State_InputLocked")]
    public float inputLockTime = 0.4f;
    [SerializeField]
    private float timer_IsInputLocked = 0;
    [SerializeField]
    private bool state_IsInputLocked;


    [Header("State_CanLockInput")]
    [SerializeField]
    private bool canLockInput;


    [Header("Action Specific Dynamic")]
    public bool state_movingDownSlope;
    [SerializeField]
    private float startRotation;


    // Input Lockout
    // -----------------------------------------------------------------------------------------------
    public void SetCanLockInput(bool value) { canLockInput = value; }
    public bool GetCanLockInput() { return canLockInput; }

    // Slide Cooldown
    // -----------------------------------------------------------------------------------------------
    public bool GetCanSlide() { return canSlide; }
    public void SetCanSlide(bool value) { canSlide = value; }
    public void UseSlideCooldown()
    {
        CooldownManagement.UseCooldown(ref timer_CooldownSlide, ref canSlide, slideCooldown);
        SetCanLockInput(true);
        SetState_IsInputLocked(true);
        SetState_IsSliding(true);
    }
    public void TickAndUpdateSlideCooldown() { CooldownManagement.TickCooldownTimer(ref timer_CooldownSlide, ref canSlide); }
    public void ResetInternalSlideTimer() { CooldownManagement.ResetCooldownTimer(ref timer_IsSliding); }

    // Is Sliding State Check
    // -----------------------------------------------------------------------------------------------

    public bool GetState_IsSliding() { return state_IsSliding; }
    public void SetState_IsSliding(bool value) { state_IsSliding = value; }
    public void TickAndUpdate_StateIsSliding() { CooldownManagement.TickBasicTimer(ref timer_IsSliding, slideDuration, ref state_IsSliding, true); }
    public void ResetTimer_StateIsSliding() { CooldownManagement.ResetBasicTimer(ref timer_IsSliding, ref state_IsSliding); }

    // Is InputLocked State Check
    // -----------------------------------------------------------------------------------------------

    public void SetState_IsInputLocked(bool value) { state_IsInputLocked = value; }
    public bool GetState_IsInputLocked() { return state_IsInputLocked; }
    public void TickAndUpdate_StateIsInputLocked() { CooldownManagement.TickBasicTimer(ref timer_IsInputLocked, inputLockTime, ref state_IsInputLocked, true); }
    public void ResetTimer_StateIsInputLockedg() { CooldownManagement.ResetBasicTimer(ref timer_IsInputLocked, ref state_IsInputLocked); }


    // Movement Lockout
    // ------------------------------------------------------------------------------------- ----------

    public void SetStartRotation(float rotation) { startRotation = rotation; }
    public float GetStartRotation() { return startRotation; }


    // Speed Update 
    // -----------------------------------------------------------------------------------------------
    public void ResetMaxSlideSpeed()
    {
        currentMaxslideSpeed = baseSlideSpeed;
    }

    public void TryUpdataMaxSlideSpeed(float gravityComponent)
    {
        currentMaxslideSpeed = Mathf.Min(maxGravityAdjustedSlideSpeed, currentMaxslideSpeed + gravityComponent);
    }
}
