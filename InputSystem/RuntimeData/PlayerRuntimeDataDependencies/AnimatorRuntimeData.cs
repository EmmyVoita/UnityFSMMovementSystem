using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;


namespace PlayerData
{
    [System.Serializable]
    public class AnimatorRuntimeData 
    {
        [SerializeField] [ReadOnly] private PlayerDataManagement playerData;
        [SerializeField] [ReadOnly] private GameObject animatorRoot;
        [SerializeField] [ReadOnly] public Animator animator;
        [SerializeField] [ReadOnly] public bool _hasAnimator;
        [SerializeField] [ReadOnly] int _animIDSpeed;
        [SerializeField] [ReadOnly] int _animIDGrounded;
        [SerializeField] [ReadOnly] int _animIDEdgeHold;
        [SerializeField] [ReadOnly] int _animIDJump;
        [SerializeField] [ReadOnly] int _animIDFreeFall;
        [SerializeField] [ReadOnly] int _animIDMotionSpeed;
        [SerializeField] [ReadOnly] int _animIDIsSliding;
        [SerializeField] [ReadOnly] int _animIDDirectionalPivot;
        [SerializeField] [ReadOnly] int _animIDCoyoteTime;
        [SerializeField] [ReadOnly] int _animIDLeanAmount;
        [SerializeField] [ReadOnly] int _animIDMovingLeft;
        [SerializeField] [ReadOnly] int _animIDMovingForward;
        [SerializeField] [ReadOnly] int _animIDGroundSlam; 

        [SerializeField] private float currentMovingLeft = 0f;
        [SerializeField] private float currentMovingForward = 0f;

        public AnimatorRuntimeData(PlayerDataManagement playerData, GameObject animatorRoot)
        {
            this.playerData = playerData;
            this.animatorRoot = animatorRoot;
            _hasAnimator = animatorRoot.TryGetComponent(out animator);
            AssignAnimationIDs();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDEdgeHold = Animator.StringToHash("EdgeHold");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDIsSliding = Animator.StringToHash("IsSliding");
            _animIDDirectionalPivot = Animator.StringToHash("DirectionalPivot");
            _animIDCoyoteTime = Animator.StringToHash("CoyoteTime");
            _animIDLeanAmount = Animator.StringToHash("LeanAmount");
            _animIDMovingLeft = Animator.StringToHash("MovingLeft");
            _animIDMovingForward = Animator.StringToHash("MovingForward");
            _animIDGroundSlam = Animator.StringToHash("InAirSlam");
        }

        public void OnStart()
        {
            if (_hasAnimator) animator.SetFloat(_animIDLeanAmount, 0.5f);
        }

        public void OnUpdateDynamicVariables()
        {
            if(_hasAnimator)
            {
                animator.SetBool(_animIDGrounded, playerData.groundCheck.playerGrounded);
                animator.SetFloat(_animIDMotionSpeed, 1);
                animator.SetBool(_animIDCoyoteTime, playerData.playerRuntimeData.jumpAndFallingRuntimeData.GetState_CoyoteTimeActive());

                currentMovingLeft = Mathf.Lerp(currentMovingLeft, playerData.inputDirection.x, playerData.blendSpeed * Time.deltaTime);
                animator.SetFloat(_animIDMovingLeft, currentMovingLeft);

                currentMovingForward = Mathf.Lerp(currentMovingForward, playerData.inputDirection.z, playerData.blendSpeed * Time.deltaTime);
                animator.SetFloat(_animIDMovingForward, currentMovingForward);
            }
        }

        public int GetAnimatorIDSpeed() { return _animIDSpeed; }
        public int GetAnimatorIDGrounded() { return _animIDGrounded; }
        public int GetAnimatorIDEdgeHold() { return _animIDEdgeHold; }
        public int GetAnimatorIDJump() { return _animIDJump; }
        public int GetAnimatorIDFreeFall() { return _animIDFreeFall; }
        public int GetAnimatorIDMotionSpeed() { return _animIDMotionSpeed; }
        public int GetAnimatorIDIsSliding() { return _animIDIsSliding; }
        public int GetAnimatorIDDirectionalPivot() { return _animIDDirectionalPivot; }
        public int GetAnimatorIDCoyoteTime() { return _animIDCoyoteTime; }
        public int GetAnimatorIDLeanAmount() { return _animIDLeanAmount; }
        public int GetAnimatorIDMovingLeft() { return _animIDMovingLeft; }
        public int GetAnimatorIDMovingForward() { return _animIDMovingForward; }
        public int GetAnimatorIDGroundSlam() { return _animIDGroundSlam; }
    }
}
