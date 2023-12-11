using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/FSM/JumpAndFallAction")]
public class JumpAndFallingData : ScriptableObject
{
    // Jump Action
    [Header("Jump Curve Settings:")]
    public float jumpHeight = 1.2f;
    public AnimationCurve jumpCurve;
    public float startforce;


    [Header("Falling Action")]
    public float gravity = -15.0f;
    public float FallTimeout = 0.15f;
    [SerializeField]
    private float _terminalVelocity = 53.0f;

    [Header("Coyote Time")]
    public float coyoteTime = 0.1f;
    public float timer_CoyoteTime = 0.0f;
    [SerializeField]
    private bool coyoteTimeActive = false;
    public bool state_CanStartCoyoteTime = true;

    [Header("InternalTimers")]
    [SerializeField]
    private float _fallTimeoutDelta;


    [Header("Cooldown_Jump")]
    public float jumpCooldown = 0.50f;
    [SerializeField]
    private float timer_CooldownJump = 0.0f;


    [Header("State_CanJump")]
    [SerializeField]
    private bool canJump;


    [Header("State_ForceJump")]
    public float forceJumpDuration = 0.1f;
    [SerializeField]
    private float timer_ForceJump = 0.0f;
    [SerializeField]
    private bool state_forceJump;


    [Header("State_ExtendJump")]
    public float maxJumpDuration = 0.5f;
    [SerializeField]
    private float timer_ExtendJump = 0.0f;
    [SerializeField]
    private bool state_ExtendJump;


    [Header("Update Speed while in air?")]
   
    public bool updateSpeedWhileInAir = false;
    public SpeedUpdateMethod speedUpdateMethod;
    public float rotationSmoothTime = 0.1f;
    public float targetSpeed = 0.0f;
    public float targetSpeed_Input = 4.0f;
    public float speedOffset = 0.0f;
    public float speedChangeRate = 3.0f;



    // Jump Cooldown
    // -----------------------------------------------------------------------------------------------
    public bool GetCanJump() { return canJump; }
    public void SetCanJump(bool value) { canJump = value; }
    public void UseJumpCooldown(ref float _verticalVelocity)
    {   
        CooldownManagement.UseCooldown(ref timer_CooldownJump, ref canJump, jumpCooldown); 
        SetExtendJump(true);
        SetForceJump(true);
        _verticalVelocity = Mathf.Sqrt(-2f * startforce * gravity); 
    }
    public void TickAndUpdateJumpCooldown() { CooldownManagement.TickCooldownTimer(ref timer_CooldownJump, ref canJump); }
    public void ResetInternalJumpTimer() { CooldownManagement.ResetCooldownTimer(ref timer_CooldownJump); }

    // Jump Button Down Logic
    // -----------------------------------------------------------------------------------------------
    public bool GetExtendJump() { return state_ExtendJump; }
    public void SetExtendJump(bool value) { state_ExtendJump = value;}
    public void UpdateExtendJumpTimer() { CooldownManagement.TickBasicTimer(ref timer_ExtendJump, maxJumpDuration, ref state_ExtendJump, true); }
    public void ResetExtendJumpTimer() { CooldownManagement.ResetBasicTimer(ref timer_ExtendJump, ref state_ExtendJump); }

    // Force Jump Logic
    // -----------------------------------------------------------------------------------------------

    public bool GetForceJump() { return state_forceJump; }
    public void SetForceJump(bool value) { state_forceJump = value; }
    public void UpdateForceJumpTimer() { CooldownManagement.TickBasicTimer(ref timer_ForceJump, forceJumpDuration, ref state_forceJump, true); }
    public void ResetForceJumpTimer() { CooldownManagement.ResetBasicTimer(ref timer_ForceJump, ref state_forceJump); }
    

    // 
    // -----------------------------------------------------------------------------------------------


    public void HandleTimeOuts(bool grounded)
    {
        if (grounded)
        {
            // jump timeout
            if (timer_CooldownJump >= 0.0f)
            {
                timer_CooldownJump -= Time.deltaTime;
            }
        }
        else
        {
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
        }
    }
    
    public void HandleOnLandEvents(ref ParticleSystem onLand)
    {
        //if (_fallTimeoutDelta <= 0.0f)
            onLand.Play();
        // reset the fall timeout timer
        _fallTimeoutDelta = FallTimeout;
    }

    public float HandleFallingLogic(float _verticalVelocity)
    {
        // Handle the falling Logic
        if (_verticalVelocity < _terminalVelocity)
            _verticalVelocity += gravity * Time.deltaTime;
        return _verticalVelocity;
    }



    public float HandleJumpingLogic(float _verticalVelocity)
    {
        float jumpHeightMultiplier = Mathf.Clamp01(timer_ExtendJump / maxJumpDuration);
        float modifiedJumpHeight = jumpHeight * jumpHeightMultiplier;

        float normalizedTime = Mathf.Clamp01(timer_ExtendJump / maxJumpDuration);
        float jumpForce = jumpCurve.Evaluate(normalizedTime) * modifiedJumpHeight;

        if (timer_ExtendJump < maxJumpDuration)
        {
            // Apply an initial force when jump is pressed
            _verticalVelocity += jumpForce * Time.deltaTime;
            _verticalVelocity = HandleFallingLogic(_verticalVelocity);
            //Debug.Log("_verticalVelocity 1:" + _verticalVelocity);
            //if(GetForceJump()) 
            //_verticalVelocity += startforce * jumpHeightMultiplier * Time.deltaTime;
            //Debug.Log("_verticalVelocity 2:" + _verticalVelocity);
        }
        else
        {
            // Gradually reduce the vertical velocity over time
            _verticalVelocity -= gravity * Time.deltaTime;
        }

        return _verticalVelocity;
    }


    // --
    public bool GetState_CoyoteTimeActive() { return coyoteTimeActive; }
    public void SetState_CoyoteTimeActive(bool value) 
    { 
        coyoteTimeActive = value;
        state_CanStartCoyoteTime = false;
    }
    public void TickAndUpdate_CoyoteTime() { CooldownManagement.TickBasicTimer(ref timer_CoyoteTime, coyoteTime, ref coyoteTimeActive, true); }
    public void ResetTimer_CoyoteTime() 
    {
        CooldownManagement.ResetBasicTimer(ref timer_CoyoteTime, ref coyoteTimeActive); 
    }



}
