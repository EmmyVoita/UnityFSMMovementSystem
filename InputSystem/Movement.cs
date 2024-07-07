using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    // Start is called before the first frame update
     [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDIsSliding;

        public float GroundedCheckDistance = 1.0f;
        public float SlopeThresholdMin = 5.0f;
        public float SlopeThresholdMax = 80.0f;
        public bool IsOnSlope = false;
        //
        // dash
        //------------------------------
        public float DashDuration = 0.5f;
        public float DashSpeed = 10.0f;
        public float DashCooldown = 2.0f;  
        
        // ----------------------------------- slide
        [Header("SlideAction")]
        public AudioClip SlidingAudioClip;
        [Range(0, 1)] public float SlidingAudioVolume = 0.5f;
        public float SlideDuration = 0.5f;
        public float SlideSpeed = 10.0f;
        public float SlideCooldown = 2.0f;  
        public float SlideRotationSmoothTime = 0.4f;  
        public float slide_custom_speed_change_rate = 0.5f;

       
        //public float SlideRotation = 0.4f;  

        // ------------------------------------ slide jump

        public float SlideJumpDuration = 0.5f;
        public float SlideJumpSpeed = 10.0f;
        public float SlideJumpCooldown = 2.0f;  
      

        MovementAction slideAction;
        MovementAction dashAction;
        MovementAction slideJumpAction;

        //-----------------------------------
        public GameObject CharacterModel;

        private Animator _animator;
        private CharacterController _controller;
        private GameObject _mainCamera;

       

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private PlayerInput playerInput;




    private PlayerInputActions playerInputActions;
    Vector3 extra_gravity = Vector3.zero;
    private void Awake()
    {

        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }


        playerInput = GetComponent<PlayerInput>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        slideAction = new MovementAction(SlideDuration,SlideSpeed, SlideCooldown);
        dashAction = new MovementAction(DashDuration,DashSpeed,DashCooldown);
        slideJumpAction = new MovementAction(SlideJumpDuration,SlideJumpSpeed,SlideJumpCooldown);
        //playerInputActions.Player.Jump.performed += Jump;
    }
    void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    // Update is called once per frame
    private void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);

        JumpAndGravity();
        GroundedCheck();
        //Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        Move();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDIsSliding = Animator.StringToHash("IsSliding");
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);



       

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private Vector3 AdjustSlopeVelocity(Vector3 input_velocity)
    {
        var ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, GroundedCheckDistance, GroundLayers))
        {

            // Creates a rotation which rotates from from Vector3.up to the slope normal.
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            float slopeAngle = Quaternion.Angle(Quaternion.identity, slopeRotation);

            // If the slope angle is within some threshold orientate the player velocity to be parallel to the slope
            if (slopeAngle > SlopeThresholdMin && slopeAngle < SlopeThresholdMax)
            {
                IsOnSlope = true;

                // Calculate the slope direction in world space
                Vector3 slopeDirection = slopeRotation * Vector3.forward;

                // Since we are using the player controller we need to accumulate gravity in a variable outside of the function.
                // Apply additional gravity along the slope direction
                Vector3 extraGravity = -slopeDirection * Gravity * Mathf.Sin(Mathf.Deg2Rad * slopeAngle) * 0.01f;
                extra_gravity += extraGravity * Time.deltaTime;

                Vector3 adjusted_input_velocity = slopeRotation * input_velocity;
                // Apply the extra gravity to the player's velocity
                Debug.Log("Extra Gravity: " + extra_gravity  + " Time :" + Time.deltaTime);
                adjusted_input_velocity += extra_gravity;


                Debug.DrawRay(transform.position, extraGravity, Color.cyan);
                Debug.DrawRay(transform.position, Vector3.down * GroundedCheckDistance, Color.red);
                Debug.DrawRay(transform.position, slopeRotation * Vector3.up, Color.blue);
                Debug.DrawRay(transform.position, slopeRotation * Vector3.forward, Color.yellow);
                Debug.Log("Character is on a slope! : " + slopeAngle);
                return adjusted_input_velocity;
            }
            
        }
        IsOnSlope = false;
        return input_velocity;
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (playerInputActions.Player.Look.ReadValue<Vector2>().sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += playerInputActions.Player.Look.ReadValue<Vector2>().x * deltaTimeMultiplier;
            _cinemachineTargetPitch += playerInputActions.Player.Look.ReadValue<Vector2>().y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

 

    private void Move()
    {   
        
        // move the player
        
        if(!CheckSlide())
        {   
            UseBaseMovement();
            
        }
    }

    private void UseBaseMovement()
    {
        Vector2 inputVector = new Vector2(0,0);
        inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

    // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = playerInputActions.Player.Sprint.ReadValue<float>() == 1 ? SprintSpeed : MoveSpeed;

        if(playerInputActions.Player.Sprint.ReadValue<float>() == 1)
            targetSpeed = SprintSpeed;
        else if(playerInputActions.Player.Slide.ReadValue<float>() == 1)
        {   
            targetSpeed = MoveSpeed;
            //SpeedChangeRate = SlideSpeed;
        }
        else
            targetSpeed = MoveSpeed;

        /*if (crouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }*/
        //_controller.attachedRigidBody.AddForce(Vector3.down * Time.deltaTime * 3000);
        // If sliding down a ramp, add force down so player stays grounded and also builds speed
        /*if ()
        {
        // _controller.attachedRigidBody.AddForce(Vector3.down * Time.deltaTime * 3000)
            //rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }*/

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (inputVector == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        //float inputMagnitude = playerInputActions.AnalogMovement ? playerInputActions.Move.ReadValue<Vector2>().magnitude : 1f;
        float inputMagnitude = inputVector.magnitude;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        Vector3 inputDirection = new Vector3(inputVector.x, 0.0f, inputVector.y).normalized;

    

        if (inputVector != Vector2.zero)
        {   
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        


        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (playerInputActions.Player.Jump.ReadValue<float>() == 1  && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                float jump_height = JumpHeight;

                _verticalVelocity = Mathf.Sqrt(jump_height * -2f * Gravity);

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, true);
                }
            }
          
            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }

            // if we are not grounded, do not jump
            //playerInputActions.Player.Look.SetValue<bool>() = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
        //_verticalVelocity += Gravity * Time.deltaTime;
    }

    
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }


    Vector3 GetTargetDirection(float custom_rotation_smooth_time)
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        Vector3 inputDirection = new Vector3(inputVector.x, 0.0f, inputVector.y).normalized;

         //_rotationVelocity = 1.0f;
         //RotationSmoothTime = 0.1f;

        if (inputVector != Vector2.zero)
        {   
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;

            // SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed = Mathf.Infinity, float deltaTime = Time.deltaTime);
            //
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, custom_rotation_smooth_time);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        return targetDirection;
    }

    Vector3 GetTargetDirection()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        Vector3 inputDirection = new Vector3(inputVector.x, 0.0f, inputVector.y).normalized;

         //_rotationVelocity = 1.0f;
         //RotationSmoothTime = 0.1f;

        if (inputVector != Vector2.zero)
        {   
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;

            // SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed = Mathf.Infinity, float deltaTime = Time.deltaTime);
            //
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        return targetDirection;
    }

    void UpdateCurrentSpeed(float target_speed, float custom_speed_change_rate)
    {
        // Read the input vector from the player input actions
        Vector2 input_vector = playerInputActions.Player.Move.ReadValue<Vector2>();

        // Calculate the magnitude (length) of the input vector
        float input_magnitude = input_vector.magnitude;

        // Define a small offset for speed comparison
        float speed_offset = 0.1f;

        // a reference to the players current horizontal velocity
        float current_horizontal_speed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        // accelerate or decelerate to target speed
        if (current_horizontal_speed < target_speed - speed_offset || current_horizontal_speed > target_speed + speed_offset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(current_horizontal_speed, target_speed * input_magnitude, Time.deltaTime * custom_speed_change_rate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {   
            _speed = target_speed;
        }
    }

    private bool CheckSlide()
    {
        // Check if the player inputs the slide button
        if (playerInputActions.Player.Slide.ReadValue<float>() == 1 && !slideAction.IsUsingAction && slideAction.CanPerformAction)
        {
            //extra_gravity = Vector3.zero;
            slideAction.UseCooldown();
            AudioSource.PlayClipAtPoint(SlidingAudioClip, transform.TransformPoint(_controller.center), SlidingAudioVolume);
        }
            

        // Manage the slide cooldown time.
        slideAction.ManageCooldownTimer();

        // Manage the animator
        _animator.SetBool(_animIDIsSliding, slideAction.IsUsingAction);

        // If the player is sliding, then perform the following action
        if (slideAction.IsUsingAction)
        {   
            // determine the slide velocity
            Vector3 slideVelocity = transform.forward * slideAction.ActionSpeed;

            
            float targetSpeed = 0.0f;
            //float custom_speed_change_rate = 0.1f;

            UpdateCurrentSpeed(targetSpeed, slide_custom_speed_change_rate);
            Vector3 targetDirection = GetTargetDirection(SlideRotationSmoothTime);
            
            if(!IsOnSlope || _controller.velocity.magnitude == 0)
            {
                //Debug.Log(slideAction._ActionTimer);
                slideAction.HandleActionTime(true);
            }
            /*
            else
            {
                //_verticalVelocity
                if (_verticalVelocity < _terminalVelocity)
                {
                    _verticalVelocity += Gravity * Time.deltaTime;
                }
            }*/
            
            //Vector3 slope_slide_velocity;

            //if (IsOnSlope)
                //slideVelocity = AdjustSlopeVelocity(slideVelocity * Time.deltaTime);

            //need to adjust the character velocity to be parallel to the angle of the slope to prevent bouncing.


            if (slideAction._ActionTimer >= 0.7 * slideAction.ActionDuration)
            {
                _controller.Move(AdjustSlopeVelocity(slideVelocity * Time.deltaTime) + new Vector3(0,extra_gravity.x,0));
            }
            else
            {
                //_controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
                _controller.Move(AdjustSlopeVelocity(targetDirection.normalized * (_speed * Time.deltaTime)) + new Vector3(0, extra_gravity.x, 0)); //+ new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }

            //slideAction.HandleActionTime(false);

            //Debug.Log(AdjustSlopeVelocity(slideVelocity * Time.deltaTime));
            /*if (_verticalVelocity > 0)
            {
                Debug.Log("Positive Vertical Velocity : " + _verticalVelocity);
            }*/

            
            

            return true;
        }

        //slideAction.HandleGracePeriod();
        //CharacterModel.transform.y = 0;
        return false;
    }


}












    /*
    private void Dash()
    {
        // Dash input
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        Vector3 inputDirection = new Vector3(inputVector.x, 0.0f, inputVector.y).normalized;

        if (playerInputActions.Player.Dash.ReadValue<float>() == 1 && !dashAction.IsUsingAction && dashAction.CanPerformAction)
        {
            dashAction.UseCooldown();
        }

        dashAction.ManageCooldownTimer();

        // Check if dashing
        if (dashAction.IsUsingAction)
        {

            Vector3 dashVelocity;

            if(playerInputActions.Player.Jump.ReadValue<float>() == 1)
            {
                dashVelocity = transform.up * dashAction.ActionSpeed;
            }
            else if(inputDirection.magnitude != 0)
            {
                dashVelocity = inputDirection *  dashAction.ActionSpeed;
            }
            else
            {
                dashVelocity = _mainCamera.transform.forward *  dashAction.ActionSpeed;
            }

            _controller.Move(dashVelocity * Time.deltaTime);

            dashAction.HandleActionTime(false);
        }
    }*/

/*
private bool CheckSlideJump()
{
    //Debug.Log("Chedking slide jump");
    //if(slideAction.IsUsingAction && playerInputActions.Player.Jump.ReadValue<float>() == 1 && !slideJumpAction.IsUsingAction && slideJumpAction.CanPerformAction)
    if(slideAction.IsUsingAction == true && playerInputActions.Player.Jump.ReadValue<float>() == 1 && !slideJumpAction.IsUsingAction && slideJumpAction.CanPerformAction)
    //if(slideAction.IsUsingAction == true || slideAction.IsInGracePeriod == true && playerInputActions.Player.Jump.ReadValue<float>() == 1 && !slideJumpAction.IsUsingAction && slideJumpAction.CanPerformAction)
    {
        slideJumpAction.UseCooldown();
        slideAction.CancelAction();
        Debug.Log("Player used slide jump");
    }

    slideJumpAction.ManageCooldownTimer();

    if(slideJumpAction.IsUsingAction)
    {
        Debug.Log("SLide jump");
        Vector3 slideJumpVelocity = transform.forward * slideJumpAction.ActionSpeed;

        //_verticalVelocity = Mathf.Sqrt(JumpHeight * 1.2f * -2f * Gravity);

        float targetSpeed = 0.0f;

        GeCurrentSpeed(targetSpeed, 0.1f);
        Vector3 targetDirection = GetTargetDirection();

        //_controller.Move(slideJumpVelocity + new Vector3(0.0f, _verticalVelocity, 0.0f) *  Time.deltaTime);


        if(slideJumpAction._ActionTimer >= 0.7 * slideJumpAction.ActionDuration)
        {
            _controller.Move(slideJumpVelocity *  Time.deltaTime + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }
        else
        {   
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }


        slideJumpAction.HandleActionTime(false);
        return true;
    }

    return false;
}*/

