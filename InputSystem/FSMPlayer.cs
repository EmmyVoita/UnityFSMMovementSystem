using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;


using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;

using PlayerData;


namespace PlayerStates
{
    public enum MovementState
    {
        Idle,
        Walking,
        Jumping,
        Sliding,
        SlidingOnSlope,
        SlidingOnSlopeGravityAccel,
        OnGround,
        Falling, 
        DirectionalPivot,
        EdgeHold,
        GroundSlam,
        GroundSlamDecision,
    }


    public class FSMPlayer : MonoBehaviour
    {
        

        [TabGroup("fsm/Inscribed/SubTabGroup", "Player State")] public IdleState idleState = new IdleState();
        [TabGroup("fsm/Inscribed/SubTabGroup", "Player State")] public WalkState walkState = new WalkState();
        [TabGroup("fsm/Inscribed/SubTabGroup", "Player State")] public JumpingState jumpingState = new JumpingState();
        [TabGroup("fsm/Inscribed/SubTabGroup", "Player State")] public FallingState fallingState = new FallingState();
        [TabGroup("fsm/Inscribed/SubTabGroup", "Player State")] public SlideState slidingState = new SlideState();
        [TabGroup("fsm/Inscribed/SubTabGroup", "Player State")] public GroundSlamState groundSlamState = new GroundSlamState();  
        [TabGroup("fsm/Inscribed/SubTabGroup", "Player State")] public GroundSlamDecisionState groundSlamDecisionState = new GroundSlamDecisionState(); 
        [TabGroup("fsm/Inscribed/SubTabGroup", "Player State")] public EdgeHoldState edgeHoldState = new EdgeHoldState(); 

        public PlayerDataManagement playerData;
        public Vector3 targetPosition;
        public Vector3 previousPosition;
        public Vector3 playerVelocity;
        //public float interpolationFactor = 0.1f;
        //ublic float moveSpeed = 5.0f;














        // Define a delegate type for the event
        public delegate void PlayerEdgeHoldJumpHandler();

        // Define the event using the delegate type
        public event PlayerEdgeHoldJumpHandler OnPlayerEdgeHoldJump;


        // Debug Options
        
        [TabGroup("fsm", "Debug", TextColor = "orange")]
        [TabGroup("fsm", "Debug")] [SerializeField] bool logStateTransitions = false; 
        [TabGroup("fsm", "Debug")] [SerializeField] bool logJumpSpeedChange = false;


        // Inscribed General Player Data

        [TabGroup("fsm", "Inscribed", TextColor = "green")]
            public bool sperateRotationLogic;

            // Add these fields to your class

        
        [TabGroup("fsm/Inscribed/SubTabGroup", "General", TextColor = "green")] [SerializeField] PlayerWeaponContainer playerWeaponContainer; // Adjust this value to control the speed of the blend


        // Inscribed Scriptable Objects
        [TabGroup("fsm/Inscribed/SubTabGroup", "ScriptableObject")] [SerializeField] private float edgeHoldJumpDelay = 0.3f;





        void Start()
        {
            if(playerData == null)
            {
                Debug.LogError("Player Data Management not found!");
            }
            else
            {
                playerData.edgeHoldCheck.OnPlayerEdgeHoldChanged += HandlePlayerEdgeHoldAlignment;
            } 

        }

        void HandlePlayerEdgeHoldAlignment(bool alignToEdge)
        {
            Debug.LogError("Aligning to Edge event invoked recieved");
           // data.jumpAndFallingRuntimeData.SetCanJump(false);
            //data.jumpAndFallingRuntimeData.SetJumpCooldownTimer(edgeHoldJumpDelay);
        }


        void UpdateDynamicVars()
        {
            playerData.playerRuntimeData.OnUpdateDynamicVariables();
            playerData.playerRuntimeData.animatorData.OnUpdateDynamicVariables();
        }


        void LateUpdate()
        {
            //MoveTowardsTarget();
        }

        void MoveTowardsTarget()
        {
            // Calculate the interpolation factor
            float interpolationFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;

            // Smoothly damp the player's position
            Vector3 interpolatedPosition = Vector3.SmoothDamp(playerData.playerRuntimeData.generalData.playerRoot.transform.position, targetPosition, ref playerVelocity, interpolationFactor);

            // Calculate the difference between the interpolated position and the current position
            Vector3 moveDifference = interpolatedPosition - playerData.playerRuntimeData.generalData.playerRoot.transform.position;

            // Move the character controller by the difference
            playerData.playerRuntimeData.generalData.characterController.Move(moveDifference);

            // Calculate the interpolation factor
            //float interpolationFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;

            // Tween the player's position
            //playerData.playerRuntimeData.generalData.playerRoot.transform.DOMove(targetPosition, interpolationFactor).SetEase(Ease.InOutQuad);

            // Calculate the difference between the interpolated position and the current position
            //Vector3 moveDifference = targetPosition - playerData.playerRuntimeData.generalData.playerRoot.transform.position;

            // Move the character controller by the difference
            //playerData.playerRuntimeData.generalData.characterController.Move(moveDifference);
        }


        void Update()
        {
            UpdateDynamicVars();

            switch (playerData.playerRuntimeData.currentState)
            {
                case MovementState.Idle:
                    if(!idleState.onEnterLock) idleState.OnEnter(playerData.playerRuntimeData);
                    idleState.CheckTransitions(playerData.playerRuntimeData);
                    if(idleState.doExecute) idleState.Execute(playerData.playerRuntimeData);
                    break;
                case MovementState.Walking:
                    if(!walkState.onEnterLock) walkState.OnEnter(playerData.playerRuntimeData);
                    walkState.CheckTransitions(playerData.playerRuntimeData);
                    if(walkState.doExecute) walkState.Execute(playerData.playerRuntimeData);
                    break;
              
                case MovementState.Falling:
                    if(!fallingState.onEnterLock) fallingState.OnEnter(playerData.playerRuntimeData);
                    fallingState.CheckTransitions(playerData.playerRuntimeData);
                    if(fallingState.doExecute) fallingState.Execute(playerData.playerRuntimeData);
                    break;
                case MovementState.Jumping:
                    if(!jumpingState.onEnterLock) jumpingState.OnEnter(playerData.playerRuntimeData);
                    jumpingState.CheckTransitions(playerData.playerRuntimeData);
                    if(jumpingState.doExecute) jumpingState.Execute(playerData.playerRuntimeData);
                    break;
               
                case MovementState.Sliding:
                    if(!slidingState.onEnterLock) slidingState.OnEnter(playerData.playerRuntimeData);
                    slidingState.CheckTransitions(playerData.playerRuntimeData);
                    if(slidingState.doExecute) slidingState.Execute(playerData.playerRuntimeData);
                    break;
                case MovementState.GroundSlam:
                    if(!groundSlamState.onEnterLock) groundSlamState.OnEnter(playerData.playerRuntimeData);
                    groundSlamState.CheckTransitions(playerData.playerRuntimeData);
                    if(groundSlamState.doExecute) groundSlamState.Execute(playerData.playerRuntimeData);
                    break;
                case MovementState.GroundSlamDecision:
                    if(!groundSlamDecisionState.onEnterLock) groundSlamDecisionState.OnEnter(playerData.playerRuntimeData);
                    groundSlamDecisionState.CheckTransitions(playerData.playerRuntimeData);
                    if(groundSlamDecisionState.doExecute) groundSlamDecisionState.Execute(playerData.playerRuntimeData);
                    break;
                 case MovementState.EdgeHold:
                    if(!edgeHoldState.onEnterLock) edgeHoldState.OnEnter(playerData.playerRuntimeData);
                    edgeHoldState.CheckTransitions(playerData.playerRuntimeData);
                    if(edgeHoldState.doExecute) edgeHoldState.Execute(playerData.playerRuntimeData);
                    break;
            }
            

            if (!playerData.playerRuntimeData.rotationLock) playerData.playerRuntimeData.ApplyRotations();

            Vector3 verticalVelocity = new Vector3(0.0f, playerData.playerRuntimeData.verticalVelocity, 0.0f);
            Debug.DrawRay(playerData.playerRuntimeData.generalData.playerRoot.transform.position, verticalVelocity, Color.magenta);

            playerVelocity = playerData.playerRuntimeData.generalData.playerRoot.transform.forward * playerData.playerRuntimeData._speed;
            
            if (playerData.slopeCheck.playerOnSlope)
            {
                playerVelocity = playerData.slopeCheck.AdjustVelocityToSlope(playerVelocity);
            }
            
            //playerVelocity += verticalVelocity;

            if(playerData.slopeCheck.slopeAngle > playerData.slopeCheck.max_SlopeAngle)
            {
                playerVelocity += playerData.slopeCheck.AdjustVelocityToSlopeVertical(verticalVelocity);
            }
            else
            {
                playerVelocity += verticalVelocity;
            }
            

            Debug.DrawRay(playerData.playerRuntimeData.generalData.playerRoot.transform.position, playerVelocity, Color.cyan);

             // Calculate the distance the player should move in this frame
            Vector3 moveDistance = playerVelocity * Time.deltaTime;

            // Calculate the target position by adding the move distance to the current position
            //targetPosition = playerData.playerRuntimeData.generalData.playerRoot.transform.position + moveDistance;
            //previousPosition = playerData.playerRuntimeData.generalData.playerRoot.transform.position;


            playerData.playerRuntimeData.generalData.characterController.Move(moveDistance);
    
        }

    
     
    }
}