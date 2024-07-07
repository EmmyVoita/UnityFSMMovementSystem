using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using PlayerData;


namespace PlayerStates
{
    [System.Serializable]
    public class WalkState : PlayerState
    {
        [SerializeField] private bool walkToIdleTimerCheck;
        public WalkState() 
        { 
            doExecute = false; 
            onEnterLock = false;
        }

        public override void CheckTransitions(PlayerRuntimeData data)
        {
            /// <summary>
            /// 
            /// State Transitions: Walking State -> Idle || Jumping || Sliding || Falling || DirectionalPivot
            /// State Transition Priority:  Falling > Sliding > Jumping > DirectionalPivot > Idle
            ///     
            /// Transition Conditions:
            ///     
            /// 1. If the player's speed is below the defined threshold for transitioning to idle, change the state to Idle.
            /// 2. If the player presses the jump key and they are eligible to jump, change the state to Jumping.
            /// 3. If the player presses the slide key and they are eligible to slide, change the state to Sliding.
            /// 4. If the player is no longer on the ground, switch to the Falling state.
            /// 5. Parse the input buffer to check if the player is attempting a directional pivot. If so, transition to the Directional Pivot state.
            /// 
            /// </summary>
        
           
        
            // Falling Transition:
            if (!data.playerData.groundCheck.playerGrounded && !data.jumpAndFallingRuntimeData.GetState_CoyoteTimeActive())  { 
                data.currentState = MovementState.Falling;
                OnExit(data, () => {}); 
            }

            // Sliding Transition:
            /*else if (data.playerData.slide == 1 && SAData.GetCanSlide() && SARRData.TryUseResource(SAData.slideResourceCost)) {
                currentState = MovementState.Sliding;
                OnExit(data, () => null);
            }*/

            else if (data.playerData.slide == 1 && data.slideRuntimeData.GetCanSlide() && data.SlideAction_resourceRuntimeData.TryUseResource(data.playerData.SAData.slideResourceCost)) {
                data.currentState = MovementState.Sliding;
                OnExit(data, () => {});
            }

            // Jumping Transition:
            else if (data.playerData.jump == 1 && data.jumpAndFallingRuntimeData.jumpCooldownTimer.CoolDownComplete) { 
                data.currentState = MovementState.Jumping; 
                OnExit(data, () => {});
            }

            // DirectionalPivot Transition:
            /*else if (playerData.pivotCheck.ParseInputBufferForPivot() && playerData.currentHorizontalSpeed_Projected >= DPData.speedThreshold 
                        && playerWeaponContainer.currentWeaponState != PlayerWeaponContainer.WeaponState.HipFire
                        && playerWeaponContainer.currentWeaponState != PlayerWeaponContainer.WeaponState.AimDownSights)
            {
                if (logStateTransitions) Debug.Log("Walking State -> DirectionalPivot");
                rotationLock = true;

                DPData.SetInitialDirection(playerData.characterVelocityNormalized);
                DPData.SetState_IsRotationLocked(true);

                if (_hasAnimator) animator.SetTrigger(_animIDDirectionalPivot);
                
                // The input direction is nearly opposite to the velocity direction.
                // This means the player is pressing in the opposite direction they are moving.
                pivot_vector = playerData.inputDirection;
                DPData.SetLockoutTime(playerData.currentHorizontalSpeed_Projected * DPData.speedFactor);
                currentState = MovementState.DirectionalPivot;
                
            }*/

            // Idle Transition:
            else if (data.playerData.inputVector.magnitude == 0 ){
                data.currentState = MovementState.Idle;
                OnExit(data, () => {});
            }
            else
            {
                doExecute = true;
            }
            
        }

        public override void Execute(PlayerRuntimeData data)
        {
            // Implement Walking Logic:
            ComputeLeanAmount(data);
            WalkActionData walkActionData = data.playerData.WAData;

            data.scheduledRotations.Push(new RotationData(walkActionData.rotationSmoothTime));

            Vector3 playerAdjustedHorizontalVelocity = data.playerData.playerRoot.transform.forward * data._speed;
            if (data.playerData.slopeCheck.playerOnSlope) playerAdjustedHorizontalVelocity = data.playerData.slopeCheck.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

            // Update Player Speed
            MovementHelpers.UpdateSpeed(ref data._speed, walkActionData.speedUpdateMethod, walkActionData.playerSpeed * data.walkSpeedModifier, walkActionData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, walkActionData.speedChangeRate);

            data.animationBlend = Mathf.Lerp(data.animationBlend, walkActionData.playerSpeed, Time.deltaTime * walkActionData.speedChangeRate);
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
        }

        public override void OnExit(PlayerRuntimeData data, Action action)
        {
            base.OnExit(data, () => {});
            action();
        }

        private void ComputeLeanAmount(PlayerRuntimeData data)
        {
            Vector3 inputMovementVector3D = data.generalData.playerRoot.transform.TransformDirection(data.playerData.inputDirection);
            Vector3 cameraForwardVector3D = Camera.main.transform.forward;

            // Convert 3D vectors to 2D
            Vector2 inputMovementVector = new Vector2(inputMovementVector3D.x, inputMovementVector3D.z);
            Vector2 cameraForwardVector = new Vector2(cameraForwardVector3D.x, cameraForwardVector3D.z);

            // Compute the dot product of the vectors and use it to compute the angle
            float dotProduct = 1 - Vector2.Dot(cameraForwardVector, inputMovementVector);

            // Compute the cross product of the vectors
            bool isRight = IsRight(cameraForwardVector, inputMovementVector);

            if(!isRight) dotProduct = -dotProduct;

            // Lerp from the current lean amount to the target lean amount
            float targetLeanAmount = dotProduct * data.playerData.playerDirectionalLeanInfluence;
            data.currentLeanAmount = Mathf.Lerp(data.currentLeanAmount, targetLeanAmount, Time.deltaTime);

            if (data.animatorData._hasAnimator) data.animatorData.animator.SetFloat(data.animatorData.GetAnimatorIDLeanAmount(), data.currentLeanAmount);
        }

        private bool IsRight(Vector2 firstVector, Vector2 secondVector)
        {
            float crossProduct = firstVector.x * secondVector.y - firstVector.y * secondVector.x;
            return crossProduct < 0;
        }



    }
}