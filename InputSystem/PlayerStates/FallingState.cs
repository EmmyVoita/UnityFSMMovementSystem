using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlayerData;
using System;

namespace PlayerStates
{
    [System.Serializable]
    public class FallingState : PlayerState
    {
        public FallingState() 
        { 
            doExecute = false; 
            onEnterLock = false;
        }
        public override void CheckTransitions(PlayerRuntimeData data)
        {
            ///Summary
            /// State Transitions: Falling State -> Idle || EdgeHold
            /// State Transition Priority: EdgeHold > Idle 
            ///
            /// Transition Conditions:
            ///
            ///    1.   if player is on the ground to transition back to OnGround state
            ///
    
            data.playerData.UnLockInputBuffer();

            if (data.jumpAndFallingRuntimeData.state_CanStartCoyoteTime == true)
            {
                data.jumpAndFallingRuntimeData.SetState_CoyoteTimeActive(true);
                data.currentState = MovementState.Idle;
                OnExit(data, () => { });
            }

            else if (data.jumpAndFallingRuntimeData.GetState_CoyoteTimeActive())
            {
                data.jumpAndFallingRuntimeData.TickAndUpdate_CoyoteTime();
                if (data.jumpAndFallingRuntimeData.GetState_CoyoteTimeActive() == false)
                {
                    // Reset timer
                    data.jumpAndFallingRuntimeData.ResetTimer_CoyoteTime();
                }
                else
                {
                    data.currentState = MovementState.Idle;
                    OnExit(data, () => { });
                }
            }

            // EdgeHold Transition:
            else if (data.playerData.edgeHoldCheck.edgeHoldState)
            {
                data.jumpAndFallingRuntimeData.state_CanStartCoyoteTime = true;
      
                data.currentState = MovementState.EdgeHold;
                OnExit(data, () => 
                { 
                    //doExecute = false;
                    data.jumpAndFallingRuntimeData.SetCanJump(true);
                });
            }

            // Idle Transition:
            else if (data.playerData.groundCheck.playerGrounded)
            {
                data.currentState = MovementState.Idle;

                OnExit(data, () => 
                {
                    data.verticalVelocity = 0.0f;
                    data.jumpAndFallingRuntimeData.state_CanStartCoyoteTime = true;
                    data.jumpAndFallingRuntimeData.HandleOnLandEvents( data.generalData.playerRoot.transform);
                    data.playerData.edgeHoldCheck.shouldCheckEdgeHold = false;

                    foreach(VisualEffectStruct vfx in data.playerData.onLandVFX)
                    {
                        VFXEventController.Instance.SpawnSimpleVFXGeneral(vfx, data.generalData.playerRoot.transform, data.generalData.playerRoot.transform);
                    }
                });
            }

            else if(data.playerData.groundSlam == 1 && data.GroundSlam_resourceRuntimeData.TryUseResource(data.playerData.GSDData.groundSlamResourceCost))
            {
                data.currentState = MovementState.GroundSlam;
                OnExit(data, () => {});
            }

            else
            {
                doExecute = true;
            }

            

            
        }

        public override void Execute(PlayerRuntimeData data)
        {
            JumpAndFallingData jumpAndFallData = data.playerData.JAFData;
            //onSlidePS.Stop();
            
            // Update animator 
            if (data.animatorData._hasAnimator) data.animatorData.animator.SetBool(data.animatorData.GetAnimatorIDFreeFall(), true);
            // Handle the falling Logic
        
            //if(playerData.slopeAngle > playerData.max_SlopeAngle)
            //data.verticalVelocity = JAFData.HandleFallingLogic(playerData.AdjustVelocityToSlopeVertical(new Vector3(0,data.verticalVelocity,0)).magnitude);
            //else
            data.verticalVelocity = data.jumpAndFallingRuntimeData.HandleFallingLogic(data.verticalVelocity);


            // Push the rotation to the stack
            data.scheduledRotations.Push(new RotationData(jumpAndFallData.rotationSmoothTime));

            // Update Player Speed
            /*if (JAFData.updateSpeedWhileInAir)
            {

                Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
                if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);


                MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod, JAFData.targetSpeed_Input, JAFData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
            }*/

            if (jumpAndFallData.updateSpeedWhileInAir)
            {
                Vector3 playerAdjustedHorizontalVelocity = data.generalData.playerRoot.transform.forward * data._speed;
                if (data.playerData.slopeCheck.playerOnSlope)  playerAdjustedHorizontalVelocity = data.playerData.slopeCheck.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);
                // If the player is not holding horizontal input, don't add additional velocity
                if (data.playerData.inputVector.magnitude <= 0.01f)
                {
                    //_speed = 0;
                }
                else
                {
                    // If the player is jumping from standstill, accelerate to the movement speed
                    if (playerAdjustedHorizontalVelocity.magnitude <= jumpAndFallData.targetSpeed )
                    {
                        MovementHelpers.UpdateSpeed(ref data._speed, jumpAndFallData.speedUpdateMethod_Accelerate, jumpAndFallData.targetSpeed , jumpAndFallData.speedOffset,  playerAdjustedHorizontalVelocity.magnitude, jumpAndFallData.speedChangeRate_Accelerate);
                        //MovementHelpers.SlowToTargetSpeed(ref _speed, JAFData.targetSpeed_Input, JAFData.speedOffset,  playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
                    }
                    else
                    {
                        // If the player's movement speed is greater than the target speed, slow down
                        //MovementHelpers.AccelerateToTargetSpeed(ref _speed, JAFData.targetSpeed_Input, JAFData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
                        MovementHelpers.UpdateSpeed(ref data._speed, jumpAndFallData.speedUpdateMethod_Decelerate, jumpAndFallData.targetSpeed , jumpAndFallData.speedOffset,  playerAdjustedHorizontalVelocity.magnitude, jumpAndFallData.speedChangeRate_Decelerate);
                    
                    }
                }

                //Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
            // if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

                //MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod, JAFData.targetSpeed_Input, JAFData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
            }
            //Debug.LogError("Falling State");
            doExecute = false;
        }

        public override void OnEnter(PlayerRuntimeData data)
        {
            
            base.OnEnter(data);
            data.edgeJumpModifier = 0.0f;
            data.baseJumpModifier = 0.0f;
        }

        public override void OnExit(PlayerRuntimeData data, Action action)
        {
            base.OnExit(data, () => {});
            if (data.animatorData._hasAnimator) data.animatorData.animator.SetBool(data.animatorData.GetAnimatorIDFreeFall(), false);
            action();
        }

    }
}