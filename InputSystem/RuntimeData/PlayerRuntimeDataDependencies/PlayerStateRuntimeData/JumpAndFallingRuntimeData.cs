using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;


namespace PlayerData
{
    [System.Serializable]
    public class JumpAndFallingRuntimeData
    {
        JumpAndFallingData JAFData;
        PlayerRuntimeData playerRuntimeData;
        [SerializeField] private float _fallTimeoutDelta;
        public float timer_CoyoteTime = 0.0f;


        public JumpAndFallingRuntimeData(JumpAndFallingData jumpAndFallingData, PlayerRuntimeData playerRuntimeData )
        {
            this.JAFData = jumpAndFallingData;
            this.timer_CooldownJump = 0.0f;
            this.timer_ForceJump = 0.0f;
            this.coyoteTimeActive = false;
            this.state_CanStartCoyoteTime = true;

            this.playerRuntimeData = playerRuntimeData;
            jumpCooldownTimer = new SimpleCooldownTimer(JAFData.jumpCooldown);
            extendJumpTimer = new SimpleTimer();
            forceJumpTimer = new SimpleTimer();
            coyotoeTimeTimer = new SimpleTimer();
        }

        [SerializeField] [ReadOnly] public SimpleCooldownTimer jumpCooldownTimer; 
        [SerializeField] [ReadOnly] public SimpleTimer extendJumpTimer;
        [SerializeField] [ReadOnly] public SimpleTimer forceJumpTimer;
        [SerializeField] [ReadOnly] public SimpleTimer coyotoeTimeTimer;

        public void UseJumpCooldown(ref float verticalVelocity)
        {   
            if(jumpCooldownTimer.TryUseCooldown())
            {
                extendJumpTimer.SetFlag(true, JAFData.maxJumpDuration);
                forceJumpTimer.SetFlag(true, JAFData.forceJumpDuration);

                float jumpHeightMultiplier =  JAFData.initialVerticalVelocity;
                //playerRuntimeData.baseJumpModifier = 0.0f;

                verticalVelocity = Mathf.Sqrt(-2f * (JAFData.initialVerticalVelocity) * JAFData.gravity); 
            }
        }


        
        [Title("Base Jump State:")]
        [SerializeField] [ReadOnly] private bool canJump;
        [SerializeField] [ReadOnly] private float timer_CooldownJump;

        [Title("Force Jump State:")]
        [SerializeField] [ReadOnly] private float timer_ForceJump;
        [SerializeField] [ReadOnly] private bool state_forceJump;
        
        [Title("Extend Jump State:")]
        [SerializeField] [ReadOnly] private bool state_ExtendJump;
        
        [Title("Coyote ime:")]
        [SerializeField] [ReadOnly] private bool coyoteTimeActive;
        [SerializeField] [ReadOnly] public bool state_CanStartCoyoteTime;
        
        



     



        
        // Jump Cooldown
        // -----------------------------------------------------------------------------------------------
        public bool GetCanJump() { return canJump; }
        public void SetCanJump(bool value) { canJump = value; }
        public void SetJumpCooldownTimer(float value) { jumpCooldownTimer.coolDownAmount = value; }
        public void TickAndUpdateJumpCooldown() { CooldownManagement.TickCooldownTimer(ref timer_CooldownJump, ref canJump); }
        public void ResetInternalJumpTimer() { CooldownManagement.ResetCooldownTimer(ref timer_CooldownJump); }


        // Jump Button Down Logic
        // -----------------------------------------------------------------------------------------------
        public bool GetExtendJump() { return extendJumpTimer.flag; }
        public void SetExtendJump(bool value) { extendJumpTimer.flag = value;}
        //public void UpdateExtendJumpTimer() { CooldownManagement.TickBasicTimer(ref timer_ExtendJump, JAFData.maxJumpDuration, ref state_ExtendJump, true); }
        public void ResetExtendJumpTimer() { extendJumpTimer.flag = false; extendJumpTimer.StopTimer(); }

        // Force Jump Logic
        // -----------------------------------------------------------------------------------------------

        public bool GetForceJump() { return forceJumpTimer.flag; }
        public void SetForceJump(bool value) { forceJumpTimer.flag = value; }
        //public void UpdateForceJumpTimer() { CooldownManagement.TickBasicTimer(ref timer_ForceJump, JAFData.forceJumpDuration, ref state_forceJump, true); }
        public void ResetForceJumpTimer() { forceJumpTimer.flag = false; forceJumpTimer.StopTimer(); }


        // Coyote Time Logic
        // -----------------------------------------------------------------------------------------------
        public bool GetState_CoyoteTimeActive() { return coyoteTimeActive; }
        public void SetState_CoyoteTimeActive(bool value) 
        { 
            coyoteTimeActive = value;
            state_CanStartCoyoteTime = false;
        }
        public void TickAndUpdate_CoyoteTime() { CooldownManagement.TickBasicTimer(ref timer_CoyoteTime, JAFData.coyoteTime, ref coyoteTimeActive, true); }
        public void ResetTimer_CoyoteTime() { CooldownManagement.ResetBasicTimer(ref timer_CoyoteTime, ref coyoteTimeActive); }

        


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
        public void HandleOnLandEvents( Transform spawnLoc)
        {   
            
            //if (_fallTimeoutDelta <= 0.0f)
            // reset the fall timeout timer
            _fallTimeoutDelta = JAFData.FallTimeout;
        }
        public float HandleFallingLogic(float _verticalVelocity)
        {
            // Handle the falling Logic
            if (_verticalVelocity < JAFData.terminalVelocity)
                _verticalVelocity += JAFData.gravity * Time.deltaTime;
                _verticalVelocity += (JAFData.gravity * Time.deltaTime) * playerRuntimeData.fallingModifier;
            return _verticalVelocity;
        }


        public float HandleJumpingLogic(float _verticalVelocity)
        {
            float jumpForce = ComputeJumpHeight();

            if (extendJumpTimer.ElapsedTime < JAFData.maxJumpDuration)
            {
                // Apply an initial force when jump is pressed
                _verticalVelocity += jumpForce * Time.deltaTime;
                _verticalVelocity = HandleFallingLogic(_verticalVelocity);
                //if(GetForceJump()) 
                //_verticalVelocity += startforce * jumpHeightMultiplier * Time.deltaTime;
            }
            else
            {
                // Gradually reduce the vertical velocity over time
                _verticalVelocity -= JAFData.gravity * Time.deltaTime;
            }

            return _verticalVelocity;
        }


        public float ComputeJumpHeight()
        {
            // Extend Jump Modifier
            float extendJumpModifier = Mathf.Clamp01( extendJumpTimer.ElapsedTime / JAFData.maxJumpDuration);

            // Jump Curve Modifier
            float normalizedTime = Mathf.Clamp01(extendJumpTimer.ElapsedTime / JAFData.maxJumpDuration);
            float jumpCurveModifier = JAFData.jumpCurve.Evaluate(normalizedTime);


            float baseJumpHeightCompute = JAFData.baseJumpHeight * extendJumpModifier * jumpCurveModifier; 
            baseJumpHeightCompute += baseJumpHeightCompute * playerRuntimeData.baseJumpModifier;
            baseJumpHeightCompute += baseJumpHeightCompute * playerRuntimeData.edgeJumpModifier;
        
            //Debug.Log("baseJumpHeightCompute: " + baseJumpHeightCompute + " extendJumpModifier: " + extendJumpModifier + " jumpCurveModifier: " + jumpCurveModifier + " JAFData.baseJumpHeight: " + JAFData.baseJumpHeight + " timer_ExtendJump: " +  extendJumpTimer.ElapsedTime + "layerRuntimeData.baseJumpModifier: " + playerRuntimeData.baseJumpModifier + " playerRuntimeData.edgeJumpModifier: " + playerRuntimeData.edgeJumpModifier);

            return baseJumpHeightCompute;
        }
        
    }
}