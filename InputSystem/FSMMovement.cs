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
public class FSMMovement : MonoBehaviour
{
}
/*
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


[RequireComponent(typeof(PlayerDataManagement)), RequireComponent(typeof(PlayerCameraManagement))]
public class FSMMovement : MonoBehaviour
{

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
    [TabGroup("fsm/Inscribed/SubTabGroup", "General", TextColor = "green")] [SerializeField] public GameObject playerRoot;
    [TabGroup("fsm/Inscribed/SubTabGroup", "General", TextColor = "green")] [SerializeField] public GameObject rotationTransform;
    [TabGroup("fsm/Inscribed/SubTabGroup", "General", TextColor = "green")] [SerializeField] public bool sperateRotationLogic = false;
    [TabGroup("fsm/Inscribed/SubTabGroup", "General", TextColor = "green")] [SerializeField] public float walkSpeedModifier = 1.0f;
        // Add these fields to your class
     [TabGroup("fsm/Inscribed/SubTabGroup", "General", TextColor = "green")] [SerializeField ]private float currentMovingLeft = 0f;
     [TabGroup("fsm/Inscribed/SubTabGroup", "General", TextColor = "green")] [SerializeField] private float currentMovingForward = 0f;
    [TabGroup("fsm/Inscribed/SubTabGroup", "General", TextColor = "green")] [SerializeField] private float blendSpeed = 5f; // Adjust this value to control the speed of the blend
    [TabGroup("fsm/Inscribed/SubTabGroup", "General", TextColor = "green")] [SerializeField] PlayerWeaponContainer playerWeaponContainer; // Adjust this value to control the speed of the blend


    // Inscribed Scriptable Objects
    [TabGroup("fsm/Inscribed/SubTabGroup", "ScriptableObject")] [SerializeField] private float edgeHoldJumpDelay = 0.3f;
    [TabGroup("fsm/Inscribed/SubTabGroup", "ScriptableObject")] [SerializeField] private float playerDirectionalLeanInfluence = 0.5f;
    [TabGroup("fsm/Inscribed/SubTabGroup", "ScriptableObject")] [SerializeField] [ReadOnly] private float currentLeanAmount = 0f; // Add this line at the top of your class



    // Inscribed VFX Data

    [TabGroup("fsm/Inscribed/SubTabGroup", "VFX", TextColor = "purple")] [SerializeField] VFXListHolder vfxListHolder;
    [TabGroup("fsm/Inscribed/SubTabGroup", "VFX")] [SerializeField] int onWalkPSIndex;
    [TabGroup("fsm/Inscribed/SubTabGroup", "VFX")] [SerializeField] List<int> onLandPSIndices = new List<int>();
    [TabGroup("fsm/Inscribed/SubTabGroup", "VFX")] [SerializeField] int onSlidePSIndex;


    // Dynamic General Player Data
    [TabGroup("fsm/Dynamic/SubTabGroup", "General", TextColor = "green")] [SerializeField] [ReadOnly] private Stack<RotationData> scheduledRotations = new Stack<RotationData>();
    [TabGroup("fsm/Dynamic/SubTabGroup", "General", TextColor = "green")] [SerializeField] [ReadOnly] public bool rotationLock;
    
    [TabGroup("fsm/Dynamic/SubTabGroup", "General")] [SerializeField] [ReadOnly] CharacterController characterController;
    [TabGroup("fsm/Dynamic/SubTabGroup", "General")] [SerializeField] [ReadOnly] PlayerDataManagement playerData;
    [TabGroup("fsm/Dynamic/SubTabGroup", "General")] [SerializeField] [ReadOnly] PlayerCameraManagement playerCameraData;
    [TabGroup("fsm/Dynamic/SubTabGroup", "General")] [SerializeField] [ReadOnly] GameObject main_camera;
    [TabGroup("fsm/Dynamic/SubTabGroup", "General")] [SerializeField] [ReadOnly] public float  baseRotationScalar = 1.0f;
    [TabGroup("fsm/Dynamic/SubTabGroup", "General")] [SerializeField] [ReadOnly] public Vector3 target_direction;
    [TabGroup("fsm/Dynamic/SubTabGroup", "General")] [SerializeField] [ReadOnly] Vector3 pivot_vector;
    [TabGroup("fsm/Dynamic/SubTabGroup", "General")] [SerializeField] [ReadOnly] float _speed;
    [TabGroup("fsm/Dynamic/SubTabGroup", "General")] [SerializeField]  public float _animationBlend;
    [TabGroup("fsm/Dynamic/SubTabGroup", "General")] [SerializeField]  float _targetRotation = 0.0f;
    [TabGroup("fsm/Dynamic/SubTabGroup", "General")] [SerializeField] [ReadOnly] float _rotationVelocity;
    [TabGroup("fsm/Dynamic/SubTabGroup", "General")] [SerializeField] [ReadOnly] float _verticalVelocity;


    // Dynamic Player FSM Data

    [TabGroup("fsm", "Dynamic", TextColor = "blue")]
    [TabGroup("fsm/Dynamic/SubTabGroup", "StateTransitionData", TextColor = "cyan")] [SerializeField] [ReadOnly] public MovementState currentState;
    [TabGroup("fsm/Dynamic/SubTabGroup", "StateTransitionData")] [SerializeField] [ReadOnly] bool grounded;
    [TabGroup("fsm/Dynamic/SubTabGroup", "StateTransitionData")] [SerializeField] [ReadOnly] bool onSlope;



    // Dynamic Animator Data
    [TabGroup("fsm/Dynamic/SubTabGroup", "Animator", TextColor = "orange")] [SerializeField] [ReadOnly] Animator animator;
    [TabGroup("fsm/Dynamic/SubTabGroup", "Animator")] [SerializeField] [ReadOnly] bool _hasAnimator;
    [TabGroup("fsm/Dynamic/SubTabGroup", "Animator")] [SerializeField] [ReadOnly] int _animIDSpeed;
    [TabGroup("fsm/Dynamic/SubTabGroup", "Animator")] [SerializeField] [ReadOnly] int _animIDGrounded;
    [TabGroup("fsm/Dynamic/SubTabGroup", "Animator")] [SerializeField] [ReadOnly] int _animIDEdgeHold;
    [TabGroup("fsm/Dynamic/SubTabGroup", "Animator")] [SerializeField] [ReadOnly] int _animIDJump;
    [TabGroup("fsm/Dynamic/SubTabGroup", "Animator")] [SerializeField] [ReadOnly] int _animIDFreeFall;
    [TabGroup("fsm/Dynamic/SubTabGroup", "Animator")] [SerializeField] [ReadOnly] int _animIDMotionSpeed;
    [TabGroup("fsm/Dynamic/SubTabGroup", "Animator")] [SerializeField] [ReadOnly] int _animIDIsSliding;
    [TabGroup("fsm/Dynamic/SubTabGroup", "Animator")] [SerializeField] [ReadOnly] int _animIDDirectionalPivot;
    [TabGroup("fsm/Dynamic/SubTabGroup", "Animator")] [SerializeField] [ReadOnly] int _animIDCoyoteTime;
    [TabGroup("fsm/Dynamic/SubTabGroup", "Animator")] [SerializeField] [ReadOnly] int _animIDLeanAmount;
    [TabGroup("fsm/Dynamic/SubTabGroup", "Animator")] [SerializeField] [ReadOnly] int _animIDMovingLeft;
    [TabGroup("fsm/Dynamic/SubTabGroup", "Animator")] [SerializeField] [ReadOnly] int _animIDMovingForward;
    [TabGroup("fsm/Dynamic/SubTabGroup", "Animator")] [SerializeField] [ReadOnly] int _animIDGroundSlam; 







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
    }

    private void Awake()
    {
        characterController = playerRoot.GetComponent<CharacterController>();
        playerData = GetComponent<PlayerDataManagement>();
        playerCameraData = GetComponent<PlayerCameraManagement>();

        // Get the main camera
        if (main_camera == null)
            main_camera = GameObject.FindGameObjectWithTag("MainCamera");

        rotationLock = false;
    }


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

        // Set the starting state
        currentState = MovementState.Idle;
        if(sperateRotationLogic)
        {
            _hasAnimator = rotationTransform.TryGetComponent(out animator);
        }
        else
        {
            _hasAnimator = playerRoot.TryGetComponent(out animator);
        }
        
        AssignAnimationIDs();

        if (_hasAnimator) animator.SetFloat(_animIDLeanAmount, 0.5f);
        
    }

    void UpdateDynamicVars()
    {
        // Tick and Update Cooldowns:
        if(!SAData.GetCanSlide())  SAData.TickAndUpdateSlideCooldown();
        if(!JAFData.GetCanJump())  JAFData.TickAndUpdateJumpCooldown();

        if (JAFData.GetState_CoyoteTimeActive())
        {
            JAFData.TickAndUpdate_CoyoteTime();
            if (JAFData.GetState_CoyoteTimeActive() == false)
            {
                // Reset timer
                JAFData.ResetTimer_CoyoteTime();
            }
        }

        // Recharge Resources:
        SARRData.RechargeResource();

        // Update Animator Vars:
        if(_hasAnimator)
        {
            animator.SetBool(_animIDGrounded, grounded);
            animator.SetFloat(_animIDMotionSpeed, 1);
            animator.SetBool(_animIDCoyoteTime, JAFData.GetState_CoyoteTimeActive());

            currentMovingLeft = Mathf.Lerp(currentMovingLeft, playerData.inputDirection.x, blendSpeed * Time.deltaTime);
            animator.SetFloat(_animIDMovingLeft, currentMovingLeft);

            currentMovingForward = Mathf.Lerp(currentMovingForward, playerData.inputDirection.z, blendSpeed * Time.deltaTime);
            animator.SetFloat(_animIDMovingForward, currentMovingForward);
        }




        // Fetch Frequently Used Player States:
        grounded = playerData.groundCheck.playerGrounded;
        onSlope = playerData.slopeCheck.playerOnSlope;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDynamicVars();


        switch (currentState)
        {
            case MovementState.Idle:
                HandleIdleState();
                break;
            case MovementState.Sliding:
                HandleSlidingState();
                break;
            case MovementState.SlidingOnSlope:
                HandleSlidingOnSlopeState();
                break;
            case MovementState.Jumping:
                HandleJumpingState();
                break;
            case MovementState.Falling:
                HandleFallingState();
                break;
            case MovementState.Walking:
                HandleWalkingState();
                break;
            case MovementState.DirectionalPivot:
                HandleDirectionalPivot();
                break;
            case MovementState.SlidingOnSlopeGravityAccel:
                HandleSlidingOnSlopeGravityAccelState();
                break;
            case MovementState.EdgeHold:
                HandleEdgeHold();
                break;
            case MovementState.GroundSlam:
                HandleGroundSlam();
                break;
                // Add other state handlers as needed
        }

    
        //Debug.DrawRay(playerRoot.transform.position, playerData.characterVelocityNormalized * 5, Color.cyan);
       // Debug.DrawRay(playerRoot.transform.position, playerCameraData.camInputDir * 5, Color.green);
        //Debug.DrawRay(playerRoot.transform.position, playerRoot.transform.forward * 10, Color.red);
        //Debug.DrawRay(transform.position, playerRoot.transform.forward * 10, Color.red);
        //Debug.DrawRay(transform.position, transform.forward * 5, Color.red);

        


        if (!rotationLock) ApplyRotations();

        Vector3 verticalVelocity = new Vector3(0.0f, _verticalVelocity, 0.0f);
        Debug.DrawRay(playerRoot.transform.position, verticalVelocity, Color.magenta);

        Vector3 playerVelocity = playerRoot.transform.forward * _speed;
        
        if(onSlope) 
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
        

        Debug.DrawRay(playerRoot.transform.position, playerVelocity, Color.cyan);

        characterController.Move(playerVelocity * Time.deltaTime);
  
    }

    private bool IsRight(Vector2 firstVector, Vector2 secondVector)
    {
        float crossProduct = firstVector.x * secondVector.y - firstVector.y * secondVector.x;
        return crossProduct < 0;
    }

    /*private void ComputeLeanAmount()
    {
        Vector3 inputMovementVector3D = playerRoot.transform.TransformDirection(playerData.inputDirection);
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
        float targetLeanAmount = dotProduct * playerDirectionalLeanInfluence;
        currentLeanAmount = Mathf.Lerp(currentLeanAmount, targetLeanAmount, Time.deltaTime);

        if (_hasAnimator) animator.SetFloat(_animIDLeanAmount, currentLeanAmount);
    }*/

    /*void ApplyRotations()
    {
        //Debug.Log("NumScheduledRotations: " + scheduledRotations.Count + " RotationLock: " + rotationLock + "angle: " + scheduledRotations.Peek().rotationSmoothTime);

        while (scheduledRotations.Count > 0)
        {
            HandleCharacterRotation(scheduledRotations.Pop());
        }
    }
  

    // ----------------------------------------- Idle State -----------------------------------------

    /*void HandleIdleState()
    {
        // Idle State -> Jumping || Walking || Falling

        /*
           State Transitions: Idle State -> Jumping || Walking || Falling
           State Transition Priority: Falling > Jumping > Walking 
            
           Transition Conditions:

           1.   if player presses the jump key and they can jump, transition to the Jumping state
           2.   if player input vector is not 0, transition to the walking state
           3.   if the player is no longer on the ground, transition to the falling state

           Important Note: Idle is treated As the root/start state. Reset all timers and flags related to other states.
           When some state completes, it will transition back to the idle state.

       */

        
       /* SAData.ResetInternalSlideTimer();
        JAFData.SetExtendJump(false);
        JAFData.SetForceJump(false);
        //JAFData.ResetExtendJumpTimer();
        //JAFData.ResetForceJumpTimer();

        // reset the fall timeout timer

        //onSlidePS.Stop();
        rotationLock = false;
        

        // update animator if using character
        animator.SetBool(_animIDJump, false);
        animator.SetBool(_animIDFreeFall, false);
        animator.SetBool(_animIDIsSliding, false);

        _verticalVelocity = 0.0f;
        //_customRotation = false;



        // Falling Transition:
        if (!grounded && !JAFData.GetState_CoyoteTimeActive())
        {
            if (logStateTransitions) Debug.Log("Idle State -> Falling");
            currentState = MovementState.Falling;
        }

        // Jumping Transition:
        else if (playerData.jump == 1 && JAFData.GetCanJump())
        {
            if(logStateTransitions) Debug.Log("Idle State -> Jumping");
            JAFData.UseJumpCooldown(ref _verticalVelocity);
            currentState = MovementState.Jumping;
        }

        // Walking Transition:
        else if (playerData.inputVector.magnitude > 0)
        {
            VFXEventController.Instance.SpawnSimpleVFXGeneral(vfxListHolder.vfxStructs[onWalkPSIndex], playerRoot.transform, playerRoot.transform);
            if (logStateTransitions) Debug.Log("Idle State -> Walking");
            currentState = MovementState.Walking;
        }

        // Implement Idle Logic
        Idle();
    }

    void Idle()
    {
        // Update Player Speed
        Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
        if (onSlope) playerAdjustedHorizontalVelocity = playerData.slopeCheck.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);



        MovementHelpers.UpdateSpeed(ref _speed, IAData.speedUpdateMethod, IAData.targetSpeed, IAData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, IAData.speedChangeRate);

        _animationBlend = Mathf.Lerp(_animationBlend, playerData.currentHorizontalSpeed_Projected, Time.deltaTime * IAData.speedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // update animator if using character
        if (_hasAnimator)
        {
            animator.SetFloat(_animIDSpeed, _animationBlend);
            //animator.SetFloat(_animIDMotionSpeed, playerData.currentHorizontalSpeed_Projected);
            animator.SetFloat(_animIDMotionSpeed, 1);
        }
    }*/

    // ----------------------------------------- Walking State -----------------------------------------

    //void HandleWalkingState()
    //{
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
       /* if (!grounded && !JAFData.GetState_CoyoteTimeActive())
        {
            if (logStateTransitions) Debug.Log("Walking State -> Falling");
            currentState = MovementState.Falling;
        }

        // Sliding Transition:
        else if (playerData.slide == 1 && SAData.GetCanSlide())
        {
            // Check if the player has the resource to slide. If they do, then use the slide cooldown.
            bool hasResource = SARRData.TryUseResource(SAData.slideResourceCost);
            if (hasResource)
            {
                if (logStateTransitions) Debug.Log("Walking State -> Sliding");
                playerData.LockInputBuffer();
                SAData.UseSlideCooldown();
                SAData.SetStartRotation(NormalizeAngle(playerRoot.transform.rotation.eulerAngles.y));
                //onSlidePS.Play();
                VFXEventController.Instance.SpawnSimpleVFXGeneral(vfxListHolder.vfxStructs[onSlidePSIndex], playerRoot.transform, playerRoot.transform);

                currentState = MovementState.Sliding;
            }
        }

        // Jumping Transition:
        else if (playerData.jump == 1 && JAFData.GetCanJump())
        {
            if(logStateTransitions) Debug.Log("Walking State -> Jumping");
            // Use the Jump Cooldown
            JAFData.UseJumpCooldown(ref _verticalVelocity);
            currentState = MovementState.Jumping;
        }

        // DirectionalPivot Transition:
        else if (playerData.pivotCheck.ParseInputBufferForPivot() && playerData.currentHorizontalSpeed_Projected >= DPData.speedThreshold 
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
            
        }

        // Idle Transition:
        else if (playerData.inputVector.magnitude == 0 || playerData.currentHorizontalSpeed_Projected < IAData.stateTransitionIdleThresholdSpeed)
        {
            if (logStateTransitions) Debug.Log("Walking State -> Idle");
            currentState = MovementState.Idle;
        }

        // Implement Walking Logic:
        ComputeLeanAmount();
        Walk();
    }


    void Walk()
    {
        //onWalkPSIndex

        scheduledRotations.Push(new RotationData(WAData.rotationSmoothTime));


        //Debug.Log("Current Horizontal Speed: " + playerData.currentHorizontalSpeed_Projected);

        Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
        if (onSlope) playerAdjustedHorizontalVelocity = playerData.slopeCheck.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

       

        // Update Player Speed
        MovementHelpers.UpdateSpeed(ref _speed, WAData.speedUpdateMethod, WAData.playerSpeed * walkSpeedModifier, WAData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, WAData.speedChangeRate);
       
        _animationBlend = Mathf.Lerp(_animationBlend, WAData.playerSpeed, Time.deltaTime * WAData.speedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // update animator if using character
        if (_hasAnimator)
        {
            animator.SetFloat(_animIDSpeed, _animationBlend);
            //animator.SetFloat(_animIDMotionSpeed, playerData.currentHorizontalSpeed_Projected);
            animator.SetFloat(_animIDMotionSpeed, 1);
        }
    }
    */

    // ----------------------------------------- Directional Pivot State -----------------------------------------

    /*void HandleDirectionalPivot()
    {
        // DirectionalPivot -> Idle

        // Idle Transition:
        if (!DPData.IsDirectionalPivotCheck())
        {
            if (logStateTransitions) Debug.Log("DirectionalPivot State -> Idle");
            // Reset any flags or timers related to Directional Pivot
            DPData.ResetInternalLockoutTimer();
            DPData.IncrementInternalLockoutTimer();
            rotationLock = false;

            currentState = MovementState.Idle;
        }

        // Implement Directional Pivot Logic
        rotationLock = DPData.GetState_IsRotationLocked();
       
        DPData.IncrementInternalLockoutTimer();

        Vector3 playerAdjustedHorizontalVelocity = DPData.GetInitalDirection() * _speed;
        if (onSlope) playerAdjustedHorizontalVelocity = playerData.slopeCheck.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);


        

        // Update Player Speed
        MovementHelpers.UpdateSpeed(ref _speed, DPData.speedUpdateMethod, DPData.playerSpeed, DPData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, DPData.speedChangeRate);

        //playerData.inputDirection = pivot_vector;

        // Push the rotation to the stack
        scheduledRotations.Push(new RotationData(DPData.rotationSmoothTime, true));
        ApplyRotations();

        // Tick Timers
        DPData.TickAndUpdate_StateIsRotationLocked();
        //rotationLock = false;
    }

    // ----------------------------------------- Dash Cancel State -----------------------------------------

    void HandleJumpingState()
    {
        /*
            State Transitions: Jumping State -> Falling

            Transition Conditions:

            1.   if the player is no longer holding the jump button and the timer for foring jump has finished, then transition to the falling state. 
            2.   if the player is exceeds the max jump button input time, then transition to the falling state 
                
        */


        /*playerData.UnLockInputBuffer();

        //Debug.Log(playerData.GetHitCeiling());
        // Falling Transition:
        if ((!JAFData.GetForceJump() && !JAFData.GetExtendJump()) && !JAFData.GetState_CoyoteTimeActive())
        {
            JAFData.state_CanStartCoyoteTime = false;
            if (logStateTransitions) Debug.Log("Jumping State -> Falling");
            JAFData.ResetExtendJumpTimer();
            JAFData.ResetForceJumpTimer();

            //playerData.checkEdgeHold = true;

            currentState = MovementState.Falling;
        }

        if (playerData.ceilingCheck.playerState_HitCeiling == true)
        {
            JAFData.state_CanStartCoyoteTime = false;
            if (logStateTransitions) Debug.Log("Jumping State -> Falling");
            JAFData.ResetExtendJumpTimer();
            JAFData.ResetForceJumpTimer();
            _verticalVelocity = 0;

            //playerData.checkEdgeHold = true;

            currentState = MovementState.Falling;
        }

        // Update animator 
        if (_hasAnimator) animator.SetBool(_animIDJump, true);

        // Implement Jumping Logic
        _verticalVelocity = JAFData.HandleJumpingLogic(_verticalVelocity);

        // Push the rotation to the stack
        scheduledRotations.Push(new RotationData(JAFData.rotationSmoothTime));

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
      /*  if (JAFData.updateSpeedWhileInAir)
        {
            Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
            if (onSlope) playerAdjustedHorizontalVelocity = playerData.slopeCheck.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);
            // If the player is not holding horizontal input, don't add additional velocity
            if (playerData.inputVector.magnitude <= 0.01f)
            {
                _speed = 0;
            }
            else
            {
                // If the player is jumping from standstill, accelerate to the movement speed
                if (playerAdjustedHorizontalVelocity.magnitude <= JAFData.targetSpeed )
                {
                    
                     MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod_Accelerate, JAFData.targetSpeed , JAFData.speedOffset,  playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate_Accelerate );
                    //SlowToTargetSpeed(ref _speed, JAFData.targetSpeed_Input, JAFData.speedOffset,  playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
                }
                else
                {
                   
                    // If the player's movement speed is greater than the target speed, slow down
                    MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod_Decelerate, JAFData.targetSpeed , JAFData.speedOffset,  playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate_Decelerate);
                    //MovementHelpers.AccelerateToTargetSpeed(ref _speed, JAFData.targetSpeed_Input, JAFData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
                }
            }

            //Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
           // if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

            //MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod, JAFData.targetSpeed_Input, JAFData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
        }

        // Tick Timers
        JAFData.UpdateForceJumpTimer();
        JAFData.UpdateExtendJumpTimer();
        if(playerData.jump == 0) JAFData.SetExtendJump(false);
    }

    void HandleFallingState()
    {
        ///Summary
        /// State Transitions: Falling State -> Idle || EdgeHold
        /// State Transition Priority: EdgeHold > Idle 
        ///
        /// Transition Conditions:
        ///
        ///    1.   if player is on the ground to transition back to OnGround state
        ///
        ///
        ///
        ///
        ///
            
        
        
        //onSlidePS.Stop();
        playerData.UnLockInputBuffer();

        if (JAFData.state_CanStartCoyoteTime == true)
        {
            JAFData.SetState_CoyoteTimeActive(true);
            currentState = MovementState.Idle;
        }

        if (JAFData.GetState_CoyoteTimeActive())
        {
            JAFData.TickAndUpdate_CoyoteTime();
            if (JAFData.GetState_CoyoteTimeActive() == false)
            {
                // Reset timer
                JAFData.ResetTimer_CoyoteTime();
            }
            else
            {
                currentState = MovementState.Idle;
            }
        }

        // EdgeHold Transition:
        if (playerData.edgeHoldCheck.edgeHoldState)
        {
            JAFData.state_CanStartCoyoteTime = true;
            if (logStateTransitions) Debug.Log("Falling State -> EdgeHold");
            Debug.Log("EdgeHold Event");
            _verticalVelocity = 0.0f;
            _speed = 0.0f;
            ////////////////

            //
            //JAFData.SetCanJump(true);

            //playerRoot.transform.rotation = Quaternion.FromToRotation (Vector3.up, playerData.edgeIntersectNormal);
            currentState = MovementState.EdgeHold;
        }

        // Idle Transition:
        if (grounded)
        {
            JAFData.state_CanStartCoyoteTime = true;
            if (logStateTransitions) Debug.Log("Falling State -> Idle");
            Debug.Log("On Land Event");
            if(onLandPSIndices == null) Debug.Log("On Land PS is null");
            else JAFData.HandleOnLandEvents( playerRoot.transform);

            foreach(int index in onLandPSIndices)
            {
                VFXEventController.Instance.SpawnSimpleVFXGeneral(vfxListHolder.vfxStructs[index], playerRoot.transform, playerRoot.transform);
            }


            
           _verticalVelocity = 0.0f;
           currentState = MovementState.Idle;

           //playerData.edgeHoldCheck. //checkEdgeHold = false;
        }

        if(playerData.groundSlam == 1)
        {
            JAFData.state_CanStartCoyoteTime = true;
            if (logStateTransitions) Debug.Log("Falling State -> GroundSlam");
            animator.SetBool(_animIDGroundSlam, true);
            Debug.Log("GroundSlam Event");
            currentState = MovementState.GroundSlam;
        }

        // Update animator 
        if (_hasAnimator) animator.SetBool(_animIDFreeFall, true);

        // Handle the falling Logic
       
        //if(playerData.slopeAngle > playerData.max_SlopeAngle)
        //_verticalVelocity = JAFData.HandleFallingLogic(playerData.AdjustVelocityToSlopeVertical(new Vector3(0,_verticalVelocity,0)).magnitude);
        //else
        _verticalVelocity = JAFData.HandleFallingLogic(_verticalVelocity);


        // Push the rotation to the stack
        scheduledRotations.Push(new RotationData(JAFData.rotationSmoothTime));

        // Update Player Speed
        /*if (JAFData.updateSpeedWhileInAir)
        {

            Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
            if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);


            MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod, JAFData.targetSpeed_Input, JAFData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
        }*/

        /*if (JAFData.updateSpeedWhileInAir)
        {
            Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
            if (onSlope) playerAdjustedHorizontalVelocity = playerData.slopeCheck.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);
            // If the player is not holding horizontal input, don't add additional velocity
            if (playerData.inputVector.magnitude <= 0.01f)
            {
                //_speed = 0;
            }
            else
            {
                // If the player is jumping from standstill, accelerate to the movement speed
                if (playerAdjustedHorizontalVelocity.magnitude <= JAFData.targetSpeed )
                {
                    MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod_Accelerate, JAFData.targetSpeed , JAFData.speedOffset,  playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate_Accelerate);
                    //MovementHelpers.SlowToTargetSpeed(ref _speed, JAFData.targetSpeed_Input, JAFData.speedOffset,  playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
                }
                else
                {
                    // If the player's movement speed is greater than the target speed, slow down
                    //MovementHelpers.AccelerateToTargetSpeed(ref _speed, JAFData.targetSpeed_Input, JAFData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
                    MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod_Decelerate, JAFData.targetSpeed , JAFData.speedOffset,  playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate_Decelerate);
                   
                }
            }

            //Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
           // if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

            //MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod, JAFData.targetSpeed_Input, JAFData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
        }


           
    }

    // ----------------------------------------- Slidiing State -----------------------------------------

    void HandleSlidingState()
    {
        ///Summary
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
        ///

        // Falling Transition:
        if (!grounded)
        {
            if (logStateTransitions) Debug.Log("Sliding State -> Falling");
            currentState = MovementState.Falling;
        }

        // Jumping Transition:
        else if (playerData.jump == 1 && JAFData.GetCanJump())
        {
            if (logStateTransitions) Debug.Log("Sliding State -> Jumping");
            //onSlidePS.Stop();
            playerData.UnLockInputBuffer();

            SAData.ResetInternalSlideTimer();
            SAData.SetState_IsSliding(false);
            SAData.SetState_IsInputLocked(false);

            JAFData.UseJumpCooldown(ref _verticalVelocity);
            currentState = MovementState.Jumping;
        } 

        // SlidingOnSlope Transition:
        else if (onSlope && SAData.state_movingDownSlope)
        {
            if (logStateTransitions) Debug.Log("Sliding State -> SlidingOnSlope");
            currentState = MovementState.SlidingOnSlope;
        }

        // Idle Transition:
        else if (!SAData.GetState_IsSliding())
        {
            if (logStateTransitions) Debug.Log("Sliding State -> Idle");
            currentState = MovementState.Idle;
        }

        // Handle Slide Logic
        else if (SAData.GetState_IsSliding()) Slide();

        // Update animator 
        if (_hasAnimator) animator.SetBool(_animIDIsSliding, true);


    }

    void Slide()
    {
        // lock the input buffer for an initial duration of the slide
        bool shouldLockInput = SAData.GetState_IsInputLocked();
        if (!shouldLockInput) playerData.UnLockInputBuffer();


        Vector3 projected_velocity = Vector3.Project(characterController.velocity, playerRoot.transform.forward);
        float currentForwardSpeed = projected_velocity.magnitude;

        Debug.DrawRay(transform.position, projected_velocity * 5, Color.red);


        Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
        if (onSlope) playerAdjustedHorizontalVelocity = playerData.slopeCheck.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

        // Update Player Speed
        MovementHelpers.UpdateSpeed(ref _speed, SAData.speedUpdateMethod, SAData.baseSlideSpeed, SAData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, SAData.speedChangeRate);

        // Push the rotation to the stack
        scheduledRotations.Push(new RotationData(SAData.slideRotationSmoothTime, true));

        // Tick Timers
        SAData.TickAndUpdate_StateIsSliding();
        SAData.TickAndUpdate_StateIsInputLocked();
    }


    // -----------------------------------------   -----------------------------------------

    void HandleSlidingOnSlopeState()
    {
        // SlidingOnSlope State -> Falling || Jumping  || Sliding || SlidingOnSlopeGravityAccel

        // Check if player is no longer on the ground to transition to the falling state
        // Check if the player is no longer on a slope to transition back to the normal sliding state
        Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
        if (onSlope) playerAdjustedHorizontalVelocity = playerData.slopeCheck.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

        // Falling Transition:
        if (!grounded)
        {
            if (logStateTransitions) Debug.Log("SlidingOnSlope State -> Falling");
            currentState = MovementState.Falling;
        }

        // Jumping Transition:
        else if (playerData.jump == 1 && JAFData.GetCanJump())
        {
            if (logStateTransitions) Debug.Log("SlidingOnSlope State -> Jumping");
            //onSlidePS.Stop();
            playerData.UnLockInputBuffer();

            SAData.ResetInternalSlideTimer();
            SAData.SetState_IsSliding(false);
            SAData.SetState_IsInputLocked(false);

            JAFData.UseJumpCooldown(ref _verticalVelocity);
            currentState = MovementState.Jumping;
        }

        // Sliding Transition:
        else if (!onSlope || !SAData.state_movingDownSlope)
        {
            if (logStateTransitions) Debug.Log("SlidingOnSlope State -> Sliding");
            currentState = MovementState.Sliding;
        }

        else if (Mathf.Abs(playerAdjustedHorizontalVelocity.magnitude - SAData.baseSlideSpeed) <= 0.1f)
        {
            if (logStateTransitions) Debug.Log("SlidingOnSlope State -> SlidingOnSlopeGravAccel");
            currentState = MovementState.SlidingOnSlopeGravityAccel;
        }
          
        
        // Handle the player sliding on the slope
        SlideOnSlope();
    }


    void SlideOnSlope()
    {
        // lock the input buffer for an initial duration of the slide
        bool shouldLockInput = SAData.GetState_IsInputLocked();
        if (!shouldLockInput) playerData.UnLockInputBuffer();


        Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
        if (onSlope) playerAdjustedHorizontalVelocity = playerData.slopeCheck.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);


        // Update Player Speed
        MovementHelpers.UpdateSpeed(ref _speed, SAData.speedUpdateMethod, SAData.baseSlideSpeed, SAData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, SAData.speedChangeRate);

        // Push the rotation to the stack
        scheduledRotations.Push(new RotationData(SAData.slideRotationSmoothTime, true));

        // Tick Timers
        //if(!SAData.state_movingDownSlope) SAData.TickAndUpdate_StateIsSliding();
        SAData.TickAndUpdate_StateIsInputLocked();
    }

    void HandleSlidingOnSlopeGravityAccelState()
    {
        // SlidingOnSlope State -> Sliding || Falling || Jumping 

        // Falling Transition:
        if (!grounded)
        {
            SAData.ResetMaxSlideSpeed();
            if (logStateTransitions) Debug.Log("SlidingOnSlopeGravAccel -> Falling");
            currentState = MovementState.Falling;
        }

        // Jumping Transition:
        else if (playerData.jump == 1 && JAFData.GetCanJump())
        {
            SAData.ResetMaxSlideSpeed();
            if (logStateTransitions) Debug.Log("SlidingOnSlopeGravAccel -> Jumping");
            //onSlidePS.Stop();
            playerData.UnLockInputBuffer();

            SAData.ResetInternalSlideTimer();
            SAData.SetState_IsSliding(false);
            SAData.SetState_IsInputLocked(false);

            JAFData.UseJumpCooldown(ref _verticalVelocity);
            currentState = MovementState.Jumping;
        }

        // Sliding Transition:
        else if (!onSlope || !SAData.state_movingDownSlope)
        {
            SAData.ResetMaxSlideSpeed();
            if (logStateTransitions) Debug.Log("SlidingOnSlopeGravAccel -> Sliding");
            currentState = MovementState.Sliding;
        }

        // Handle the player sliding on the slope
        SlideOnSlopeGravAccel();
    }

    void SlideOnSlopeGravAccel()
    {
        // lock the input buffer for an initial duration of the slide
        bool shouldLockInput = SAData.GetState_IsInputLocked();
        if (!shouldLockInput) playerData.UnLockInputBuffer();


        Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
        if (onSlope) playerAdjustedHorizontalVelocity = playerData.slopeCheck.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);


        // Get Gravity affecting horizontal speed
        float gravityComponent = SAData.useCustomGravity == true ? Mathf.Cos(Mathf.Deg2Rad * playerData.slopeCheck.slopeAngle) * -SAData.gravity * Time.deltaTime : Mathf.Cos(Mathf.Deg2Rad * playerData.slopeCheck.slopeAngle) * -JAFData.gravity * Time.deltaTime;
        SAData.TryUpdataMaxSlideSpeed(gravityComponent);

        // Update Player Speed
        MovementHelpers.UpdateSpeed(ref _speed, SAData.speedUpdateMethod, SAData.currentMaxslideSpeed, SAData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, SAData.gravityAccelerateSpeedChangeRate);

        // Push the rotation to the stack
        scheduledRotations.Push(new RotationData(SAData.slideRotationSmoothTime, true));

        // Tick Timers
        //if(!SAData.state_movingDownSlope) SAData.TickAndUpdate_StateIsSliding();
        SAData.TickAndUpdate_StateIsInputLocked();
    }

   void HandleEdgeHold()
   {
        //AlignPlayerToEdge();
        /*
          State Transitions: EdgeHold -> Jumping 
          State Transition Priority: Falling > Jumping 


          Transition Conditions:
        */

    
        // update animator if using character
 /*       if (_hasAnimator)
        {
            animator.SetFloat(_animIDSpeed, 0);
            //animator.SetFloat(_animIDMotionSpeed, playerData.currentHorizontalSpeed_Projected);
            animator.SetFloat(_animIDMotionSpeed, 1);
            animator.SetBool(_animIDEdgeHold, true);
        }

        _speed = 0;
        _verticalVelocity = 0.0f;

        // Jumping Transition:
        if (playerData.jump == 1 && JAFData.GetCanJump())
        {
            playerData.edgeHoldCheck.edgeHoldState = false;
            playerData.edgeHoldCheck.shouldCheckEdgeHold = true;
            if(logStateTransitions) Debug.Log("EdgeHold -> Jumping");
            animator.SetBool(_animIDEdgeHold, false);

            OnPlayerEdgeHoldJump?.Invoke();


            // Use the Jump Cooldown
            JAFData.UseJumpCooldown(ref _verticalVelocity);
            currentState = MovementState.Jumping;
        }

        
        //playerData.AlignPlayerToEdge();
   }

   void HandlePlayerEdgeHoldAlignment(bool alignToEdge)
   {
        Debug.LogError("Aligning to Edge event invoked recieved");
        JAFData.SetCanJump(false);
        JAFData.SetJumpCooldownTimer(edgeHoldJumpDelay);
   }


   void HandleGroundSlam()
   {
        // GroundSlam -> Idle
        animator.SetBool(_animIDGroundSlam, false);
        // Idle Transition:
        if (grounded)
        {
            if (logStateTransitions) Debug.Log("GroundSlam State -> Idle");
            currentState = MovementState.Idle;
        }

        // Implement GroundSlam Logic
        //_verticalVelocity = JAFData.HandleGroundSlamLogic(_verticalVelocity);

        _verticalVelocity = JAFData.HandleFallingLogic(_verticalVelocity);


        // Push the rotation to the stack
        scheduledRotations.Push(new RotationData(JAFData.rotationSmoothTime));

        // Update Player Speed
        /*if (JAFData.updateSpeedWhileInAir)
        {

            Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
            if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);


            MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod, JAFData.targetSpeed_Input, JAFData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
        }*/

    /*    if (JAFData.updateSpeedWhileInAir)
        {
            Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
            if (onSlope) playerAdjustedHorizontalVelocity = playerData.slopeCheck.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);
            // If the player is not holding horizontal input, don't add additional velocity
            if (playerData.inputVector.magnitude <= 0.01f)
            {
                //_speed = 0;
            }
            else
            {
                // If the player is jumping from standstill, accelerate to the movement speed
                if (playerAdjustedHorizontalVelocity.magnitude <= JAFData.targetSpeed )
                {
                    MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod_Accelerate, JAFData.targetSpeed , JAFData.speedOffset,  playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate_Accelerate);
                    //MovementHelpers.SlowToTargetSpeed(ref _speed, JAFData.targetSpeed_Input, JAFData.speedOffset,  playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
                }
                else
                {
                    // If the player's movement speed is greater than the target speed, slow down
                    //MovementHelpers.AccelerateToTargetSpeed(ref _speed, JAFData.targetSpeed_Input, JAFData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
                    MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod_Decelerate, JAFData.targetSpeed , JAFData.speedOffset,  playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate_Decelerate);
                   
                }
            }

            //Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
           // if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

            //MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod, JAFData.targetSpeed_Input, JAFData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
        }
   }




    //-----------------------------------------------------------------------------------------


    private void HandleCharacterRotation(RotationData rotationData)
    {
        if (playerData.inputVector != Vector2.zero)
        {
            Quaternion targetRotation;

            if (rotationData.lockInputDirZ)
            {
                float rawRotation = Mathf.Atan2(playerData.inputDirection.x, playerData.inputDirection.z) * Mathf.Rad2Deg + main_camera.transform.eulerAngles.y;
                Quaternion rawQuaternion = Quaternion.Euler(0.0f, rawRotation, 0.0f);

                targetRotation = ClampRotation(rawQuaternion, SAData.GetStartRotation(), -SAData.maxAngle, SAData.maxAngle);
                
                //Debug.Log("ID.x" + playerData.inputDirection.x + " ID.z" + playerData.inputDirection.z + " RawRotation" + rawRotation);              
                //Debug.Log("MinAngle: " + minAngle + " MaxAngle: " + maxAngle);         
                Debug.DrawRay(transform.position, Quaternion.Euler(0.0f, SAData.GetStartRotation(), 0.0f) * Vector3.forward * 20, Color.cyan);
                Debug.DrawRay(transform.position, Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward * 30, Color.white);
                Debug.DrawRay(transform.position, Quaternion.Euler(0.0f, SAData.GetStartRotation() - SAData.maxAngle, 0.0f) * Vector3.forward * 20, Color.green);
                Debug.DrawRay(transform.position, Quaternion.Euler(0.0f, SAData.GetStartRotation() + SAData.maxAngle, 0.0f) * Vector3.forward * 20, Color.blue);

            }
            else
            {
               float rawRotation = Mathf.Atan2(playerData.inputDirection.x, playerData.inputDirection.z) * Mathf.Rad2Deg + main_camera.transform.eulerAngles.y;
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

    /*        float rotation = Mathf.SmoothDampAngle(playerRoot.transform.eulerAngles.y, targetRotation.eulerAngles.y, ref _rotationVelocity, rotationData.rotationSmoothTime * baseRotationScalar);
            playerRoot.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            

            target_direction = Quaternion.Euler(0.0f, targetRotation.eulerAngles.y, 0.0f) * Vector3.forward;
        }
    }


    

    public void MoveForward(float distance)
    {
        // Calculate the forward movement vector
        Vector3 moveVector = transform.forward * distance;

        // Move the player
        //characterController.Move(moveVector);
    }


    private static float NormalizeAngle(float angle)
    {
        if (angle > 180)
        {
            return angle - 360;
        }
        return angle;
    }



}*/
