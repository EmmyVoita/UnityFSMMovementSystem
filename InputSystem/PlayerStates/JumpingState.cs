using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlayerData;
using System;


namespace PlayerStates
{
    [System.Serializable]
    public class JumpingState : PlayerState
    {

        public JumpingState() 
        { 
            doExecute = false; 
            onEnterLock = false;
        }

        public override void CheckTransitions(PlayerRuntimeData data)
        {
            /// <summary>
            ///    State Transitions: Jumping State -> Falling
            ///    
            ///     Transition Conditions:
            ///     1.   if the player is no longer holding the jump button and the timer for foring jump has finished, then transition to the falling state. 
            ///     2.   if the player is exceeds the max jump button input time, then transition to the falling state 
            ///     3.   if the player hits a ceiling, then transition to the falling state
            /// 

            data.playerData.UnLockInputBuffer();

            //Debug.Log(playerData.GetHitCeiling());
            // Falling Transition:
            if ((!data.jumpAndFallingRuntimeData.GetForceJump() && !data.jumpAndFallingRuntimeData.GetExtendJump()) && !data.jumpAndFallingRuntimeData.GetState_CoyoteTimeActive())
            {
                data.jumpAndFallingRuntimeData.state_CanStartCoyoteTime = false;
                data.jumpAndFallingRuntimeData.ResetExtendJumpTimer();
                data.jumpAndFallingRuntimeData.ResetForceJumpTimer();

                data.playerData.edgeHoldCheck.shouldCheckEdgeHold = true;

                data.currentState = MovementState.Falling;
                OnExit(data, () => { });
            }

            else if (data.playerData.ceilingCheck.playerState_HitCeiling == true)
            {
                data.jumpAndFallingRuntimeData.state_CanStartCoyoteTime = false;
                data.jumpAndFallingRuntimeData.ResetExtendJumpTimer();
                data.jumpAndFallingRuntimeData.ResetForceJumpTimer();
                data.verticalVelocity = 0;

                //playerData.checkEdgeHold = true;

                data.currentState = MovementState.Falling;
                OnExit(data, () => { });
            }
            else
            {
                doExecute = true;
            }
        }

        public override void Execute(PlayerRuntimeData data)
        {
            JumpAndFallingData jumpAndFallData = data.playerData.JAFData;
       
            // Update animator 
            if (data.animatorData._hasAnimator) data.animatorData.animator.SetBool(data.animatorData.GetAnimatorIDJump(), true);

            // Implement Jumping Logic
            data.verticalVelocity = data.jumpAndFallingRuntimeData.HandleJumpingLogic(data.verticalVelocity);

            // Push the rotation to the stack
            data.scheduledRotations.Push(new RotationData(jumpAndFallData.rotationSmoothTime));

            // Update Player Speed
            /*if (JAFData.updateSpeedWhileInAir)
            {
                float beforeSpeed = _speed;

                if (playerData.inputVector.magnitude > 0)
                    MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod, JAFData.targetSpeed_Input, JAFData.speedOffset, playerData.currentHorizontalSpeed, JAFData.speedChangeRate);
                else
                    MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod, JAFData.targetSpeed, JAFData.speedOffset, playerData.currentHorizontalSpeed, JAFData.speedChangeRate);

                if (logJumpSpeedChange)
                {
                    Debug.Log("Updating Speed [HandleJumpingState()] Before: " + beforeSpeed + " After: " + _speed);
                    //print all relevant JAF data varaibles
                    Debug.Log("JAFData.targetSpeed: " + JAFData.targetSpeed + " JAFData.speedOffset: " + JAFData.speedOffset + " JAFData.speedChangeRate: " + JAFData.speedChangeRate);   
                }
            }*/



            // Update Player Speed
            if (jumpAndFallData.updateSpeedWhileInAir)
            {
                Vector3 playerAdjustedHorizontalVelocity = data.generalData.playerRoot.transform.forward *  data._speed;
                if (data.playerData.slopeCheck.playerOnSlope) playerAdjustedHorizontalVelocity = data.playerData.slopeCheck.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);
                // If the player is not holding horizontal input, don't add additional velocity
                if (data.playerData.inputVector.magnitude <= 0.01f)
                {
                     data._speed = 0;
                }
                else
                {
                    // If the player is jumping from standstill, accelerate to the movement speed
                    if (playerAdjustedHorizontalVelocity.magnitude <= jumpAndFallData.targetSpeed )
                    {
                        
                        MovementHelpers.UpdateSpeed(ref data._speed, jumpAndFallData.speedUpdateMethod_Accelerate, jumpAndFallData.targetSpeed , jumpAndFallData.speedOffset,  playerAdjustedHorizontalVelocity.magnitude, jumpAndFallData.speedChangeRate_Accelerate );
                        //SlowToTargetSpeed(ref _speed, JAFData.targetSpeed_Input, JAFData.speedOffset,  playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
                    }
                    else
                    {
                    
                        // If the player's movement speed is greater than the target speed, slow down
                        MovementHelpers.UpdateSpeed(ref  data._speed,jumpAndFallData.speedUpdateMethod_Decelerate, jumpAndFallData.targetSpeed , jumpAndFallData.speedOffset,  playerAdjustedHorizontalVelocity.magnitude, jumpAndFallData.speedChangeRate_Decelerate);
                        //MovementHelpers.AccelerateToTargetSpeed(ref _speed, JAFData.targetSpeed_Input, JAFData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
                    }
                }

                //Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
            // if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

                //MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod, JAFData.targetSpeed_Input, JAFData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
            }

            // Tick Timers
            //data.jumpAndFallingRuntimeData.UpdateForceJumpTimer();
            //data.jumpAndFallingRuntimeData.UpdateExtendJumpTimer();
            if(data.playerData.jump == 0)
            {
                data.jumpAndFallingRuntimeData.SetExtendJump(false);
                data.jumpAndFallingRuntimeData.extendJumpTimer.StopTimer();
            } 
        }  


        public override void OnEnter(PlayerRuntimeData data)
        {
            base.OnEnter(data);
            data.jumpAndFallingRuntimeData.UseJumpCooldown(ref data.verticalVelocity);
            foreach(VisualEffectStruct vfx in data.playerData.JAFData.onJumpVFX)
            {
                VFXEventController.Instance.SpawnSimpleVFXGeneral(vfx, data.generalData.playerRoot.transform, data.generalData.playerRoot.transform);
            }
            
        }

        public override void OnExit(PlayerRuntimeData data, Action action)
        {
            base.OnExit(data, () => {});
            data.jumpAndFallingRuntimeData.ResetExtendJumpTimer();
            data.jumpAndFallingRuntimeData.ResetForceJumpTimer();
            //if (data.animatorData._hasAnimator) data.animatorData.animator.SetBool(data.animatorData.GetAnimatorIDJump(), false);
            action();
        }




        void HandleJumpingState()
        {
           

            
        }

        

    }

    }

