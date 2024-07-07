using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlayerData;
using System;


namespace PlayerStates
{
    [System.Serializable]
    public class EdgeHoldState : PlayerState
    {

        public EdgeHoldState() 
        { 
            doExecute = false; 
            onEnterLock = false;
        }

        public override void CheckTransitions(PlayerRuntimeData data)
        {
            //AlignPlayerToEdge();
            /// <summary>
            /// 
            /// State Transitions: EdgeHold -> Jumping 
            /// State Transition Priority: Falling > Jumping 
            /// Transition Conditions:
           

            

            

            // Jumping Transition:
            if (data.playerData.jump == 1 && data.jumpAndFallingRuntimeData.jumpCooldownTimer.CoolDownComplete)
            {
                
                //OnPlayerEdgeHoldJump?.Invoke();

                // Use the Jump Cooldown
                data.currentState = MovementState.Jumping;
                OnExit(data, () => 
                {
                    data.playerData.edgeHoldCheck.edgeHoldState = false;
                    data.playerData.edgeHoldCheck.shouldCheckEdgeHold = false;
                    data.animatorData.animator.SetBool(data.animatorData.GetAnimatorIDEdgeHold(), false);
                });
            }
            
            else
            {
                doExecute = true;
            }

            
            //
        }

        public override void Execute(PlayerRuntimeData data)
        {
            data.playerData.edgeHoldCheck.AlignPlayerToEdge();
            doExecute = false;
        }  





        public override void OnEnter(PlayerRuntimeData data)
        {
            base.OnEnter(data);

            // update animator if using character
            if (data.animatorData._hasAnimator)
            {
                data.animatorData.animator.SetFloat(data.animatorData.GetAnimatorIDSpeed(), 0);
                //animator.SetFloat(_animIDMotionSpeed, playerData.currentHorizontalSpeed_Projected);
                data.animatorData.animator.SetFloat(data.animatorData.GetAnimatorIDMotionSpeed(), 1);
                data.animatorData.animator.SetBool(data.animatorData.GetAnimatorIDEdgeHold(), true);
            }

            data._speed = 0;
            data.verticalVelocity = 0.0f;
            data.jumpAndFallingRuntimeData.jumpCooldownTimer.ResetCooldown();
            //data.jumpAndFallingRuntimeData.jumpCooldownTimer.StartCoolDown();
            data.jumpAndFallingRuntimeData.jumpCooldownTimer.SetCooldown(0.2f);
            
        }

        public override void OnExit(PlayerRuntimeData data, Action action)
        {
            base.OnExit(data, () => {});
            data.edgeJumpModifier = 0.4f;
            //if (data.animatorData._hasAnimator) data.animatorData.animator.SetBool(data.animatorData.GetAnimatorIDJump(), false);
            action();
        }




       


        

    }

}

