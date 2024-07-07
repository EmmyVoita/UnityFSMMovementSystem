using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerData;
using System;

namespace PlayerStates
{
    [System.Serializable]
    public class GroundSlamDecisionState : PlayerState
    {
        public GroundSlamDecisionState() 
        { 
            doExecute = false; 
            onEnterLock = false;
        }

        public override void CheckTransitions(PlayerRuntimeData data)
        {
            ///Summary:
            /// Idle State -> Jumping || Walking || Falling
            /// State Transition Priority: Falling > Jumping > Walking
            /// 
            /// Transition Conditions:
            /// 1. if player presses the jump key and they can jump, transition to the Jumping state
            /// 2. if player input vector is not 0, transition to the walking state
            /// 3. if the player is no longer on the ground, transition to the falling state
            /// 
            /// Important Note: Idle is treated As the root/start state. Reset all timers and flags related to other states.
            /// When some state completes, it will transition back to the idle state.

            // Falling Transition:
            if (!data.playerData.groundCheck.playerGrounded && !data.jumpAndFallingRuntimeData.GetState_CoyoteTimeActive())  { 
                data.currentState = MovementState.Falling;
                OnExit(data, () => {}); 
            }
            
            // Jumping Transition:
            else if (data.playerData.jump == 1 && data.jumpAndFallingRuntimeData.GetCanJump()) { 
                data.currentState = MovementState.Jumping; 
                OnExit(data, () => {

                    
                    foreach(VisualEffectStruct vfx in data.playerData.onGroundSlamToJumpVFX)
                    {
                        VFXEventController.Instance.SpawnSimpleVFXGeneral(vfx, data.generalData.playerRoot.transform, data.generalData.playerRoot.transform);
                    }
                });
            }

            else if (data.playerData.slide == 1 && data.slideRuntimeData.GetCanSlide() && data.SlideAction_resourceRuntimeData.TryUseResource(data.playerData.SAData.slideResourceCost)) {
                data.currentState = MovementState.Sliding;
                OnExit(data, () => {
                    
                    foreach(VisualEffectStruct vfx in data.playerData.onGroundSlamToSlideVFX)
                    {
                        VFXEventController.Instance.SpawnSimpleVFXGeneral(vfx, data.generalData.playerRoot.transform, data.generalData.playerRoot.transform);
                    }
                });
            }

            // Walking Transition:
            /*else if (data.playerData.inputVector.magnitude > 0) { 
                data.currentState = MovementState.Walking;    
                OnExit(data, () => 
                {
                    foreach(VisualEffectStruct vfx in data.playerData.onWalkVFX)
                    {
                        VFXEventController.Instance.SpawnSimpleVFXGeneral(vfx, data.generalData.playerRoot.transform, data.generalData.playerRoot.transform);
                    }
                });
            }*/
            
            else
            {
                doExecute = true;
            }
        }

        public override void Execute(PlayerRuntimeData data)
        {
            GroundSlamDecisionData groundSlamDecisionData = data.playerData.GSDData;
            //data.walkSpeedModifier = 0.0f;

            // Update Player Speed
            Vector3 playerAdjustedHorizontalVelocity = data.playerData.playerRoot.transform.forward * data._speed;
            if (data.playerData.slopeCheck.playerOnSlope) playerAdjustedHorizontalVelocity = data.playerData.slopeCheck.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

            MovementHelpers.UpdateSpeed(ref data._speed, groundSlamDecisionData.speedUpdateMethod, groundSlamDecisionData.targetSpeed * data.baseSpeedModifier, groundSlamDecisionData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, groundSlamDecisionData.speedChangeRate);

            data.animationBlend = Mathf.Lerp(data.animationBlend, data.playerData.currentHorizontalSpeed_Projected, Time.deltaTime * groundSlamDecisionData.speedChangeRate);
            if (data.animationBlend < 0.01f) data.animationBlend = 0f;

            // Update Animator
            if (data.animatorData._hasAnimator)
            {
                data.animatorData.animator.SetFloat(data.animatorData.GetAnimatorIDSpeed(), data.animationBlend);
                data.animatorData.animator.SetFloat(data.animatorData.GetAnimatorIDMotionSpeed(), 1);
            }
        }

        public override void OnEnter(PlayerRuntimeData data)
        {
            base.OnEnter(data);
            //data.baseSpeedModifier = 0.0f;

            data.slideRuntimeData.ResetInternalSlideTimer();
            data.jumpAndFallingRuntimeData.SetExtendJump(false);
            data.jumpAndFallingRuntimeData.SetForceJump(false);

            data.rotationLock = false;
        
            // update animator if using character
            data.animatorData.animator.SetBool(data.animatorData.GetAnimatorIDJump(), false);
            data.animatorData.animator.SetBool(data.animatorData.GetAnimatorIDFreeFall(), false);
            data.animatorData.animator.SetBool(data.animatorData.GetAnimatorIDIsSliding(), false);

            
            foreach(VisualEffectStruct vfx in data.playerData.onGroundSlamVFX)
            {
                VFXEventController.Instance.SpawnSimpleVFXGeneral(vfx, data.generalData.playerRoot.transform, data.generalData.playerRoot.transform);
            }


            PlayerInfo.Instance.CreateUISlider(data);
            data.groundSlamDecisionRuntimeData.StartChargeGeneration(data);
            data.verticalVelocity = 0.0f;
        }

        public override void OnExit(PlayerRuntimeData data, Action action)
        {
            base.OnExit(data, () => {});
            data.baseSpeedModifier = 1.0f;
            data.baseJumpModifier = 1.0f;
            data.ModifySpeed(Mathf.Max(1, data.playerData.GSDData.exitSpeedBaseMultiplier * data.groundSlamDecisionRuntimeData.chargeAmount),2.5f, ref data.speedCoroutine);
            data.baseJumpModifier = data.groundSlamDecisionRuntimeData.chargeAmount;
            data.groundSlamDecisionRuntimeData.StopChargeGeneration();
            data.groundSlamDecisionRuntimeData.ResetChargeAmount();
            //data.ModifySpeed(2.5f,2.5f, ref data.jumpCoroutine);
            action();
        }
    }
}

