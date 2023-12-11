using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

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
    [Header("Debug Log")]
    public bool logStateTransitions = false;
    public bool logJumpSpeedChange = false;

    [Header("Player Inscribed")]
    public GameObject playerRoot;
    private CharacterController characterController;

    private PlayerDataManagement playerData;
    private PlayerCameraManagement playerCameraData;
    private GameObject main_camera;
    
    private bool grounded;
    private bool onSlope;


    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float rotation_smooth_time = 0.12f;

     
    // Character controller
    private Animator animator;


    // player
    
    private Stack<RotationData> scheduledRotations = new Stack<RotationData>();
    public bool rotationLock;
    public float  baseRotationScalar = 1.0f;

    public GameObject lineRenderer;
    private Vector3 target_direction;
    private Vector3 pivot_vector;


    private float _speed;
    private float _animationBlend;
    [SerializeField]
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    [SerializeField]
    private float _verticalVelocity;


   
    // animation IDs
    private bool _hasAnimator;
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    private int _animIDIsSliding;
    private int _animIDDirectionalPivot;

    //FSM state
    [Header("FSM state")]
    public MovementState currentState;

    [Header("Sliding Action")]
    public SlideActionData SAData;

    [Header("Slide [RechargableResource]")]
    public RechargableResourceData SARRData;

    [Header("Idle Action")]
    public IdleActionData IAData;

    [Header("Walk Action")]
    public WalkActionData WAData;

    [Header("Jump/Fall Action")]
    public JumpAndFallingData JAFData;

    [Header("Directional Pivot Action")]
    public DirectionalPivotData DPData;

    [Header("Cling to Wall Action")]
    //public ClingToWallData CTWData;


    [Header("Animation VFX")]
    public ParticleSystem onLandPS;
    public ParticleSystem onSlidePS;


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
        // Set the starting state
        currentState = MovementState.Idle;
        _hasAnimator = playerRoot.TryGetComponent(out animator);
        AssignAnimationIDs();
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
        if(_hasAnimator) animator.SetBool(_animIDGrounded, grounded);
        if(_hasAnimator) animator.SetFloat(_animIDMotionSpeed, playerData.currentHorizontalSpeed_Projected);

        // Fetch Frequently Used Player States:
        grounded = playerData.GetPlayerGrounded();
        onSlope = playerData.GetPlayerOnSlope();
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
            playerVelocity = playerData.AdjustVelocityToSlope(playerVelocity);
        }

        if(playerData.slopeAngle > playerData.max_SlopeAngle)
        {
            playerVelocity += playerData.AdjustVelocityToSlopeVertical(verticalVelocity);
        }
        else
        {
            playerVelocity += verticalVelocity;
        }
        

        Debug.DrawRay(playerRoot.transform.position, playerVelocity, Color.cyan);

        characterController.Move(playerVelocity * Time.deltaTime);
  
    }


    void ApplyRotations()
    {
        //Debug.Log("NumScheduledRotations: " + scheduledRotations.Count);
        while (scheduledRotations.Count > 0)
        {
            HandleCharacterRotation(scheduledRotations.Pop());
        }
    }
  

    // ----------------------------------------- Idle State -----------------------------------------

    void HandleIdleState()
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

        
        SAData.ResetInternalSlideTimer();
        JAFData.SetExtendJump(false);
        JAFData.SetForceJump(false);
        //JAFData.ResetExtendJumpTimer();
        //JAFData.ResetForceJumpTimer();

        // reset the fall timeout timer

        onSlidePS.Stop();
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
        if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);



        MovementHelpers.UpdateSpeed(ref _speed, IAData.speedUpdateMethod, IAData.targetSpeed, IAData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, IAData.speedChangeRate);
    }

    // ----------------------------------------- Walking State -----------------------------------------

    void HandleWalkingState()
    {
        /*
            State Transitions: Walking State -> Idle || Jumping || Sliding || Falling || DirectionalPivot
            State Transition Priority:  Falling > Sliding > Jumping > DirectionalPivot > Idle

            Transition Conditions:

                1. If the player's speed is below the defined threshold for transitioning to idle, change the state to Idle.
                2. If the player presses the jump key and they are eligible to jump, change the state to Jumping.
                3. If the player presses the slide key and they are eligible to slide, change the state to Sliding.
                4. If the player is no longer on the ground, switch to the Falling state.
                5. Parse the input buffer to check if the player is attempting a directional pivot. If so, transition to the Directional Pivot state.
        */

        // Falling Transition:
        if (!grounded)
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
                onSlidePS.Play();
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
        else if (playerData.ParseInputBufferForPivot() && playerData.currentHorizontalSpeed_Projected >= DPData.speedThreshold)
        {
            if (logStateTransitions) Debug.Log("Walking State -> DirectionalPivot");
            rotationLock = true;

            DPData.SetInitialDirection(playerData.characterVelocityNormalized);
            DPData.SetState_IsRotationLocked(true);
            
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
        Walk();
    }


    void Walk()
    {

        scheduledRotations.Push(new RotationData(WAData.rotationSmoothTime));


        //Debug.Log("Current Horizontal Speed: " + playerData.currentHorizontalSpeed_Projected);

        Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
        if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

       

        // Update Player Speed
        MovementHelpers.UpdateSpeed(ref _speed, WAData.speedUpdateMethod, WAData.playerSpeed, WAData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, WAData.speedChangeRate);
       
        _animationBlend = Mathf.Lerp(_animationBlend, WAData.playerSpeed, Time.deltaTime * WAData.speedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // update animator if using character
        if (_hasAnimator)
        {
            animator.SetFloat(_animIDSpeed, _animationBlend);
            animator.SetFloat(_animIDMotionSpeed, playerData.currentHorizontalSpeed_Projected);
        }
    }
    

    // ----------------------------------------- Directional Pivot State -----------------------------------------

    void HandleDirectionalPivot()
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
        if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

    


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


        playerData.UnLockInputBuffer();

        // Falling Transition:
        if (!JAFData.GetForceJump() && !JAFData.GetExtendJump())
        {
            JAFData.state_CanStartCoyoteTime = false;
            if (logStateTransitions) Debug.Log("Jumping State -> Falling");
            JAFData.ResetExtendJumpTimer();
            JAFData.ResetForceJumpTimer();
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
        if (JAFData.updateSpeedWhileInAir)
        {
            Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
            if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

            MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod, JAFData.targetSpeed_Input, JAFData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
        }

        // Tick Timers
        JAFData.UpdateForceJumpTimer();
        JAFData.UpdateExtendJumpTimer();
        if(playerData.jump == 0) JAFData.SetExtendJump(false);
    }

    void HandleFallingState()
    {
        /*
            State Transitions: Falling State -> Idle

            Transition Conditions:

            1.   if player is on the ground to transition back to OnGround state
        */

        onSlidePS.Stop();
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

        // Idle Transition:
        if (grounded)
        {
            JAFData.state_CanStartCoyoteTime = true;
            if (logStateTransitions) Debug.Log("Falling State -> Idle");
            Debug.Log("On Land Event");
            if(onLandPS == null) Debug.Log("On Land PS is null");
            else JAFData.HandleOnLandEvents(ref onLandPS);
           _verticalVelocity = 0.0f;
           currentState = MovementState.Idle;
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
        if (JAFData.updateSpeedWhileInAir)
        {

            Vector3 playerAdjustedHorizontalVelocity = playerRoot.transform.forward * _speed;
            if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);


            MovementHelpers.UpdateSpeed(ref _speed, JAFData.speedUpdateMethod, JAFData.targetSpeed_Input, JAFData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, JAFData.speedChangeRate);
        }

           
    }

    // ----------------------------------------- Slidiing State -----------------------------------------

    void HandleSlidingState()
    {
        /*
          State Transitions: Sliding State -> Idle || Jumping || Falling || SlidingOnSlope
          State Transition Priority: Falling > Jumping > SlidingOnSlope > Idle


          Transition Conditions:

          Idle:
            *   if the slide ends naturally (i.e. the slide duration ends), then transition back to the idle state. 
          Jumping:
            *   if the player presses the jump key, then transition to the jumping state.
          Falling:
            *   if the player is no longer on the ground, then transition to the falling state.   
          SlidingOnSlope:
            *   if the player is on a slope, then transition to the sliding on slope state where they will continue to slide down the slope.

        */

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
            onSlidePS.Stop();
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
        if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

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
        if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);

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
            onSlidePS.Stop();
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
        if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);


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
            onSlidePS.Stop();
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
        if (onSlope) playerAdjustedHorizontalVelocity = playerData.AdjustVelocityToSlope(playerAdjustedHorizontalVelocity);


        // Get Gravity affecting horizontal speed
        float gravityComponent = SAData.useCustomGravity == true ? Mathf.Cos(Mathf.Deg2Rad * playerData.slopeAngle) * -SAData.gravity * Time.deltaTime : Mathf.Cos(Mathf.Deg2Rad * playerData.slopeAngle) * -JAFData.gravity * Time.deltaTime;
        SAData.TryUpdataMaxSlideSpeed(gravityComponent);

        // Update Player Speed
        MovementHelpers.UpdateSpeed(ref _speed, SAData.speedUpdateMethod, SAData.currentMaxslideSpeed, SAData.speedOffset, playerAdjustedHorizontalVelocity.magnitude, SAData.gravityAccelerateSpeedChangeRate);

        // Push the rotation to the stack
        scheduledRotations.Push(new RotationData(SAData.slideRotationSmoothTime, true));

        // Tick Timers
        //if(!SAData.state_movingDownSlope) SAData.TickAndUpdate_StateIsSliding();
        SAData.TickAndUpdate_StateIsInputLocked();
    }

    void HandleClingToWall()
    {

    }



    // ----------------------------------------- Animation Events -----------------------------------------

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDIsSliding = Animator.StringToHash("IsSliding");
        _animIDDirectionalPivot = Animator.StringToHash("DirectionalPivot");
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

            float rotation = Mathf.SmoothDampAngle(playerRoot.transform.eulerAngles.y, targetRotation.eulerAngles.y, ref _rotationVelocity, rotationData.rotationSmoothTime * baseRotationScalar);
            playerRoot.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

            target_direction = Quaternion.Euler(0.0f, targetRotation.eulerAngles.y, 0.0f) * Vector3.forward;
        }
    }


    private static Quaternion ClampRotation(Quaternion rotation, float startAngle, float minAngle, float maxAngle)
    {
        /*
           Quaternion operations in most programming languages, including Unity's C#, typically 
           expect angles to be in radians rather than degrees. Converting angles to radians 
           ensures that the calculations involving quaternions are performed correctly.
       */

        float startAngleRad = startAngle * Mathf.Deg2Rad;
        float minAngleRad = minAngle * Mathf.Deg2Rad;
        float maxAngleRad = maxAngle * Mathf.Deg2Rad;

 

        //Debug.Log("Converting angles to radians. StartAngle: " + startAngleRad + " MinAngle: " + minAngleRad + " MaxAngle: " + maxAngleRad);

        /*
            This line creates a quaternion (startQuaternion) that represents a rotation 
            around the Vector3.up axis. Quaternion.AngleAxis(angle, axis) is a Unity 
            function that generates a quaternion for a specified angle around a given axis.
        */

        // Convert the start angle to a quaternion
        Quaternion startQuaternion = Quaternion.AngleAxis(startAngle, Vector3.up);

        /*
            The relativeRotation represents the rotation relative to a specified start angle. 
            In other words, it gives you the rotation in relation to the initial orientation 
            defined by the startAngle.This is useful in scenarios where you want to perform 
            rotations relative to a specific reference direction or angle. It allows you to 
            keep track of how an object has been rotated with respect to its initial state, 
            rather than in absolute terms.
        */

        // Convert the rotation to be relative to the start angle
        Quaternion relativeRotation = Quaternion.Inverse(startQuaternion) * rotation;

        // Convert the relative rotation to euler angles for easier manipulation
        Vector3 relativeEulerAngles = relativeRotation.eulerAngles;


        // Convert relativeEulerAngles.y to radians before clamping
        float relativeAngleRad = relativeRotation.eulerAngles.y * Mathf.Deg2Rad;

        //Debug.Log("relativeEulerAngles.y (rad) '" + relativeAngleRad + "' clamped to '" + Mathf.Clamp(relativeEulerAngles.y, minAngleRad, maxAngleRad) + "'. \n Using min: '" + minAngleRad + "' and max: '" + maxAngleRad + "'.");


        // Clamp the relative rotation's y (vertical) angle
        relativeAngleRad = Mathf.Clamp(relativeAngleRad, minAngleRad, maxAngleRad);

        // Convert back to degrees
        relativeRotation.eulerAngles = new Vector3(0, relativeAngleRad * Mathf.Rad2Deg, 0);

        // Convert the clamped relative rotation back to a quaternion
        Quaternion clampedRelativeRotation = Quaternion.Euler(relativeEulerAngles);

        // Combine the clamped relative rotation with the start angle to get the final rotation
        Quaternion finalRotation = startQuaternion * clampedRelativeRotation;

        /*Debug.Log("StartQuaternion.eulerAngles.y '" + startQuaternion.eulerAngles.y +
                    "' \n clampedRelativeRotation.eulerAngles.y '" + clampedRelativeRotation.eulerAngles.y + 
                    "' \n finalRotation.eulerAngles.y '" + finalRotation.eulerAngles.y + "' .");*/

        return finalRotation;
    }


    private static float NormalizeAngle(float angle)
    {
        if (angle > 180)
        {
            return angle - 360;
        }
        return angle;
    }



}
