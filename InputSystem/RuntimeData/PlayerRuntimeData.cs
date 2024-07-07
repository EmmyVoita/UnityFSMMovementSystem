using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;
using UnityEngine.UIElements;


using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;


using PlayerStates;


namespace PlayerData
{
    public struct RotationData
    {
        public float rotationSmoothTime;
        public bool lockInputDirZ;

        public RotationData(float rotation_smooth_time)
        {
            this.rotationSmoothTime = rotation_smooth_time;
            this.lockInputDirZ = false;
        }
        public RotationData(float rotation_smooth_time, bool lockInputDirZ)
        {
            this.rotationSmoothTime = rotation_smooth_time;
            this.lockInputDirZ = lockInputDirZ;
        }
    }

    [System.Serializable]
    public class PlayerRuntimeData 
    {
        
        [TabGroup("tab1","Animator Data")] [ReadOnly] public AnimatorRuntimeData animatorData;
        [TabGroup("tab1","General Data")] [ReadOnly] public GeneralRuntimeData generalData;
        [TabGroup("tab1","General Data")] [ReadOnly] public LastSafePositionRuntimeData lastSafePositionRuntimeData;
        [TabGroup("tab1","State Data")] [ReadOnly] public IdleRuntimeData idleRuntimeData;
        [TabGroup("tab1","State Data")] [ReadOnly] public JumpAndFallingRuntimeData jumpAndFallingRuntimeData;
        [TabGroup("tab1","State Data")] [ReadOnly] public SlideRuntimeData slideRuntimeData;
        [TabGroup("tab1","State Data")] [ReadOnly] public WalkRuntimeData walkRuntimeData;
        [TabGroup("tab1","State Data")] [ReadOnly] public GroundSlamDecisionRuntimeData groundSlamDecisionRuntimeData;
        [TabGroup("tab1","State Data")] [ReadOnly] public RechargableResourceRuntimeData SlideAction_resourceRuntimeData;
        [TabGroup("tab1","State Data")] [ReadOnly] public RechargableResourceRuntimeData GroundSlam_resourceRuntimeData;



        public Stack<RotationData> scheduledRotations = new Stack<RotationData>();
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public PlayerDataManagement playerData;
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public float animationBlend = 0.0f;
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public float currentLeanAmount = 0f; 
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public Vector3 target_direction;
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public Vector3 pivot_vector;
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public float _speed;
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public float _targetRotation = 0.0f;
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public float _rotationVelocity;
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public float verticalVelocity;
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public float  baseRotationScalar = 1.0f;
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public float baseSpeedModifier;
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public float baseJumpModifier;
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public float edgeJumpModifier;
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public float fallingModifier;
     
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public Coroutine speedCoroutine;
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public float walkSpeedModifier;
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public bool rotationLock;
        [TabGroup("tab1","PlayerRuntimeData")] [ReadOnly] public MovementState currentState;

        


        public PlayerRuntimeData(  JumpAndFallingData jumpAndFallingData, 
                            SlideActionData slideActionData, 
                            WalkActionData walkActionData, 
                            IdleActionData idleActionData,
                            RechargableResourceData SlideAction_resourceData,
                            GroundSlamDecisionData groundSlamDecisionData,
                            RechargableResourceData GroundSlam_resourceData,
                            LastSafePositionData lastSafePositionData,
                            PlayerDataManagement playerDataManagement, 
                            GameObject playerRoot, 
                            GameObject movementManager,
                            GameObject playerTargetRoot,
                            string rotationTransformName = "PlayerAimHandler")
                            
        {
            this.playerData = playerDataManagement;
            jumpAndFallingRuntimeData = new JumpAndFallingRuntimeData(jumpAndFallingData, this);
            slideRuntimeData = new SlideRuntimeData(slideActionData);
            walkRuntimeData = new WalkRuntimeData(walkActionData);
            groundSlamDecisionRuntimeData = new GroundSlamDecisionRuntimeData(groundSlamDecisionData);
            animatorData = new AnimatorRuntimeData(playerDataManagement, playerDataManagement.animatorRoot);
            generalData = new GeneralRuntimeData(movementManager, playerRoot, playerTargetRoot, rotationTransformName);
            idleRuntimeData = new IdleRuntimeData(idleActionData);
            SlideAction_resourceRuntimeData = new RechargableResourceRuntimeData(SlideAction_resourceData);
            GroundSlam_resourceRuntimeData = new RechargableResourceRuntimeData(GroundSlam_resourceData);
            lastSafePositionRuntimeData = new LastSafePositionRuntimeData(lastSafePositionData, playerDataManagement);


            // OnAwake values
            rotationLock = false;
            baseSpeedModifier = 1.0f;
            baseJumpModifier = 0.0f;
            walkSpeedModifier = 1.0f;
        }


        public void OnStart()
        {
            // Set the starting state
            currentState = MovementState.Idle;
            animatorData.OnStart();
        }

        public void OnUpdateDynamicVariables()
        {
            // Tick and Update Cooldowns:
            if(!jumpAndFallingRuntimeData.GetCanJump())  jumpAndFallingRuntimeData.TickAndUpdateJumpCooldown();

            if (jumpAndFallingRuntimeData.GetState_CoyoteTimeActive())
            {
                jumpAndFallingRuntimeData.TickAndUpdate_CoyoteTime();
                if (jumpAndFallingRuntimeData.GetState_CoyoteTimeActive() == false)
                {
                    // Reset timer
                    jumpAndFallingRuntimeData.ResetTimer_CoyoteTime();
                }
            }

            // Recharge Resources:
            SlideAction_resourceRuntimeData.RechargeResource();
            GroundSlam_resourceRuntimeData.RechargeResource();
            lastSafePositionRuntimeData.OnUpdate();
        }

        public void ApplyRotations()
        {
            while (scheduledRotations.Count > 0)
            {
                HandleCharacterRotation(scheduledRotations.Pop());
            }
        }

        private void HandleCharacterRotation(RotationData rotationData)
        {
            if (playerData.inputVector != Vector2.zero)
            {
                Quaternion targetRotation;

                if (rotationData.lockInputDirZ)
                {
                    float rawRotation = Mathf.Atan2(playerData.inputDirection.x, playerData.inputDirection.z) * Mathf.Rad2Deg + generalData.mainCamera.transform.eulerAngles.y;
                    Quaternion rawQuaternion = Quaternion.Euler(0.0f, rawRotation, 0.0f);

                    targetRotation = CharacterRotation.ClampRotation(rawQuaternion, slideRuntimeData.GetStartRotation(), -playerData.SAData.maxAngle, playerData.SAData.maxAngle);
                }
                else
                {
                    float rawRotation = Mathf.Atan2(playerData.inputDirection.x, playerData.inputDirection.z) * Mathf.Rad2Deg + generalData.mainCamera.transform.eulerAngles.y;
                    targetRotation = Quaternion.Euler(0.0f, rawRotation, 0.0f);
                }

                /*if(sperateRotationLogic)
                {
                    float rotation = Mathf.SmoothDampAngle(rotationTransform.transform.eulerAngles.y, targetRotation.eulerAngles.y, ref _rotationVelocity, rotationData.rotationSmoothTime * baseRotationScalar);
                    rotationTransform.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }
                else
                {
                    float rotation = Mathf.SmoothDampAngle(playerRoot.transform.eulerAngles.y, targetRotation.eulerAngles.y, ref _rotationVelocity, rotationData.rotationSmoothTime * baseRotationScalar);
                    playerRoot.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }*/

                float rotation = Mathf.SmoothDampAngle(generalData.playerRoot.transform.eulerAngles.y, targetRotation.eulerAngles.y, ref _rotationVelocity, rotationData.rotationSmoothTime * baseRotationScalar);
                generalData.playerRoot.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                target_direction = Quaternion.Euler(0.0f, targetRotation.eulerAngles.y, 0.0f) * Vector3.forward;
            }
        }

        public void ModifySpeed(float targetValue, float duration, ref Coroutine coroutine)
        {
            // If a speed coroutine is already running, stop it
            if (coroutine != null)
            {
                CoroutineStarter.Instance.StopCoroutine(coroutine);
            }

            coroutine = CoroutineStarter.Instance.StartCoroutine(ModifySpeedCoroutine(targetValue, duration));
        }

        private IEnumerator ModifySpeedCoroutine(float targetValue, float duration)
        {
            float originalSpeed = baseSpeedModifier;
            float fifthDuration = duration / 5f;
            float fourfifthDuration = (duration*4) / 5f;


            // Increase speed to targetValue over halfDuration
            for (float t = 0; t < fifthDuration; t += Time.fixedDeltaTime)
            {
                baseSpeedModifier = Mathf.Lerp(originalSpeed, targetValue, t / fifthDuration);
                yield return null;
            }

            baseSpeedModifier = targetValue;

            // Decrease speed back to originalSpeed over halfDuration
            for (float t = 0; t < fourfifthDuration; t += Time.fixedDeltaTime)
            {
                baseSpeedModifier = Mathf.Lerp(targetValue, originalSpeed, t / fourfifthDuration);
                yield return null;
            }

            baseSpeedModifier = originalSpeed;
        }
        
       
    }
}