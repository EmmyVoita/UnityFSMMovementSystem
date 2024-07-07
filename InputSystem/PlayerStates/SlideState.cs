using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using PlayerData;


namespace PlayerStates
{
    [System.Serializable]
    public class SlideState : PlayerState
    {
        public SlideState() 
        { 
            doExecute = false; 
            onEnterLock = false;
        }
        
        public override void CheckTransitions(PlayerRuntimeData data)
        {
            /// <summary>
            ///  State Transitions: Sliding State -> Idle || Jumping || Falling || SlidingOnSlope
            ///  State Transition Priority: Falling > Jumping > SlidingOnSlope > Idle
            ///
            ///
            ///       Transition Conditions:
            ///
            ///       Idle:
            ///         *   if the slide ends naturally (i.e. the slide duration ends), then transition back to the idle state. 
            ///       Jumping:
            ///       *   if the player presses the jump key, then transition to the jumping state.
            ///      Falling:
            ///        *   if the player is no longer on the ground, then transition to the falling state.   
            ///      SlidingOnSlope:
            ///        *   if the player is on a slope, then transition to the sliding on slope state where they will continue to slide down the slope.
            /// <summary>

            // Falling Transition:
            if (!data.playerData.groundCheck.playerGrounded && !data.jumpAndFallingRuntimeData.GetState_CoyoteTimeActive())  { 
                data.currentState = MovementState.Falling;
                OnExit(data, () => {}); 
            }

            // Jumping Transition:
            else if (data.playerData.jump == 1 && data.jumpAndFallingRuntimeData.jumpCooldownTimer.CoolDownComplete) { 
                data.currentState = MovementState.Jumping; 
                OnExit(data, () => {});
            }

            // SlidingOnSlope Transition:
            /*else if (onSlope && SAData.state_movingDownSlope)
            {
                if (logStateTransitions) Debug.Log("Sliding State -> SlidingOnSlope");
                currentState = MovementState.SlidingOnSlope;
            }*/

            // Idle Transition:
            else if (!data.slideRuntimeData.GetState_IsSliding() && !data.slideRuntimeData.GetState_IsInputLocked())
            {
                data.currentState = MovementState.Idle;
                OnExit(data, () => {});
            }

            else if(data.slideRuntimeData.GetState_IsSliding())
            {
                doExecute = true;
            }

            else
            {
                doExecute = true;
            }

            // Handle Slide Logic
            //else if (data.slideRuntimeData.GetState_IsSliding()) Slide();

            // Update animator 
            if (data.animatorData._hasAnimator) data.animatorData.animator.SetBool(data.animatorData.GetAnimatorIDIsSliding(), true);
            
        }

        public override void Execute(PlayerRuntimeData data)
        {
            
            SlideActionData slideActionData = data.playerData.SAData;

            // lock the input buffer for an initial duration of the slide
            bool shouldLockInput = data.slideRuntimeData.GetState_IsInputLocked();
            if (!shouldLockInput) data.playerData.UnLockInputBuffer();
            else data.playerData.LockInputBuffer();


            Vector3 projected_velocity = Vector3.Project(data.generalData.characterController.velocity, data.generalData.playerRoot.transform.forward);
            float currentForwardSpeed = projected_velocity.magnitude;

            Vector3 playerAdjustedHorizontalVelocity = data.generalData.playerRoot.transform.forward *  data._speed;
            if (data.playerData.slopeCheck.playerOnSlope) playerAdjustedHorizontalVelocity = data.playerData.slopeCheck.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

            // Update Player Speed
            MovementHelpers.UpdateSpeed(ref data._speed, slideActionData.speedUpdateMethod, slideActionData.baseSlideSpeed * data.baseSpeedModifier, slideActionData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, slideActionData.speedChangeRate);

            // Push the rotation to the stack
            data.scheduledRotations.Push(new RotationData(slideActionData.slideRotationSmoothTime, true));

            // Tick Timers
            //SAData.TickAndUpdate_StateIsSliding();
            //SAData.TickAndUpdate_StateIsInputLocked();
        }

        public override void OnEnter(PlayerRuntimeData data)
        {
           
            
            data.slideRuntimeData.UseSlideCooldown();
            data.slideRuntimeData.SetStartRotation(CharacterRotation.NormalizeAngle(data.generalData.playerRoot.transform.rotation.eulerAngles.y));
            
            foreach(VisualEffectStruct vfx in data.playerData.onSlideVFX)
            {
                VFXEventController.Instance.SpawnSimpleVFXGeneral(vfx, data.generalData.playerRoot.transform, data.generalData.playerRoot.transform);
            }

            base.OnEnter(data);
        }

        public override void OnExit(PlayerRuntimeData data, Action action)
        {
            

            data.playerData.UnLockInputBuffer();

            data.slideRuntimeData.ResetInternalSlideTimer();
            data.slideRuntimeData.ResetTimer_StateIsSliding();
            data.slideRuntimeData.ResetTimer_StateIsInputLocked();

            base.OnExit(data, () => {});
            action();
        }

        private void ComputeLeanAmount()
        {
           
        }
    }
}