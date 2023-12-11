using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;
using UnityEngine.UIElements;

public class PlayerDataManagement : MonoBehaviour
{

    public SlideActionData SAData;
    public JumpAndFallingData JAFData;   
    // Slope Check
    [Header("Slope Check")]
    public float slopeRayMaxDistance = 10.0f;
    public Quaternion slopeRotation;
    public Quaternion slopeRotation_Vert;
    public float slopeAngle;
    public float min_SlopeAngle = 15.0f;
    public float max_SlopeAngle = 75.0f;
    [SerializeField]
    private bool playerOnSlope = false;

    // Grounded Check
    [Header("Grounded Check")]
    public float grounded_radius = 0.28f;
    public float grounded_offset = -0.14f;
    public LayerMask ground_layers;
    [SerializeField]
    private bool playerGrounded = false;

    [Header("Pivot")]
    public float pivotThreshold = -0.5f;
    public bool isPivoting = false;
    private bool canPivot = false;

    // Player Input Actions
    private PlayerInput playerInput;
    private PlayerInputActions playerInputActions;
    
    private PlayerCameraManagement playerCameraData;
    CircularBuffer<Vector2> inputBuffer = new CircularBuffer<Vector2>(4);
    [SerializeField]
    private bool bufferLock = false;

    [Header("Player")]
    public GameObject playerRoot;
    private CharacterController characterController;

    // Player input actions
    public float jump = 0;
    public float slide = 0;
    public float dash = 0;
    public float primary = 0;
    public float currentHorizontalSpeed_Projected;
    public float currentHorizontalSpeed;
    public Vector3 characterVelocityNormalized;
    public Vector2 inputVector;
    public Vector2 previousInputVector;
    public Vector3 inputDirection;
    public Vector3 localHorizontalInputDirection;
    public Vector3 previousInputDirection;
    public Vector2 lookVector;
    public float pV_ID_DotProduct;

    private bool IsCurrentDeviceMouse
    {
        get
        {
        #if ENABLE_INPUT_SYSTEM
            Debug.Log("Input device is mouse");
            return playerInput.currentControlScheme == "KeyboardMouse";
        #else
            Debug.Log("Input device not mouse");
		    return false;
        #endif
        }
    }

    public bool GetPlayerOnSlope()
    {
        return playerOnSlope;
    }
    public bool GetPlayerGrounded()
    {
        return playerGrounded;
    }
    public bool GetIsCurrentDeviceMouse()
    {
        return IsCurrentDeviceMouse;
    }   
    public void LockInputBuffer()
    {
        bufferLock = true;
    }
    public void UnLockInputBuffer()
    {
        bufferLock = false;
    }

    private void Awake()
    {
        playerCameraData = GetComponent<PlayerCameraManagement>();
        characterController = playerRoot.GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        inputBuffer.FillValue(4, Vector2.zero);
        JAFData.state_CanStartCoyoteTime = true;
    }

    // Update is called once per frame
    void Update()
    {
        GroundCheck();
        SlopeCheck();

        lookVector = playerInputActions.Player.Look.ReadValue<Vector2>();

        if (!bufferLock)
        {
            // Read values from the player input actions
            jump = playerInputActions.Player.Jump.ReadValue<float>();
            slide = playerInputActions.Player.Slide.ReadValue<float>();
            dash = playerInputActions.Player.Dash.ReadValue<float>();
            primary = playerInputActions.Player.Primary.ReadValue<float>();

            inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
            inputDirection = new Vector3(inputVector.x, 0.0f, inputVector.y).normalized;
            // Project inputDirection onto the forward plane (plane defined by transform.forward)
            //localHorizontalInputDirection = Vector3.ProjectOnPlane(playerRoot.transform.TransformDirection(inputDirection), playerRoot.transform.forward);
            localHorizontalInputDirection = Vector3.ProjectOnPlane(inputDirection, playerRoot.transform.forward);
            Debug.DrawRay(playerRoot.transform.position, localHorizontalInputDirection * 10, Color.magenta);

       

            // Current Horizontal Speed with respect to the slope rotation
            //Vector3 projectPlayerVelocity = Vector3.ProjectOnPlane(characterController.velocity, (slopeRotation * playerRoot.transform.up).normalized);
        

            /// summary:
            /// currentHorizontalSpeed_Projected is the magnitude of the horizontal component of the player's 
            /// velocity after it's been projected onto the slope.
            
            Vector3 projectPlayerVelocity = Vector3.ProjectOnPlane(characterController.velocity, slopeRotation * playerRoot.transform.up);
            Debug.DrawRay(playerRoot.transform.position, slopeRotation * playerRoot.transform.up, Color.green);
            currentHorizontalSpeed_Projected = projectPlayerVelocity.magnitude;

            // Assuming gravity is pointing downwards (negative y direction)
            //float verticalComponent = Vector3.Dot(characterController.velocity, Vector3.down);

            // Calculate the effective speed along the slope (including vertical component)
            //currentSlopeSpeed = Mathf.Sqrt(currentHorizontalSpeed_Projected * currentHorizontalSpeed_Projected + verticalComponent * verticalComponent);
            

            currentHorizontalSpeed = (new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z)).magnitude;

            //currentHorizontalSpeed = (new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z)).magnitude;
            characterVelocityNormalized = characterController.velocity.normalized;
        }
           

        if (inputVector != previousInputVector && !bufferLock)
        {
            inputBuffer.Add(inputVector);
            canPivot = true;
        }
            
        //inputVector = inputHandler.GetLastInputDirection();
       
        
        

    }

    public float CalculateFinalSpeed(Vector3 horizontalVelocity, float verticalVelocity)
    {
       

        float horizontalSpeed = horizontalVelocity.magnitude;
        float finalSpeed = Mathf.Sqrt(horizontalSpeed * horizontalSpeed + verticalVelocity * verticalVelocity);

        return finalSpeed;
    }


    public void LogCurrentValues()
    {
        Debug.Log("PlayerRotation: " + playerRoot.transform.rotation.eulerAngles + "\n"
      + "bufferLock: " + bufferLock + "\n"
      + "Slide: " + slide + "\n"
      + "InputVector: " + inputVector + "\n"
      + "InputDirection: " + inputDirection + "\n"
      + "LocalHorizontalInputDirection: " + localHorizontalInputDirection + "\n"
      + "CurrentHorizontalSpeed: " + currentHorizontalSpeed_Projected + "\n"
      + "CharacterVelocityNormalized: " + characterVelocityNormalized);


    }

    private void LateUpdate()
    {
        if (!bufferLock) previousInputVector = inputVector;

    }

    public bool ParseInputBufferForPivot()
    {
        if(!canPivot)
            return false;

        bool isPivoting = false;    

        Vector2 past_input_1 = inputBuffer.GetPastElementFromIndex(0);
        Vector2 past_input_2 = inputBuffer.GetPastElementFromIndex(1);
        Vector2 past_input_3 = inputBuffer.GetPastElementFromIndex(2);
        
        // check dot product of last 2 inputs
        if(Vector2.Dot(past_input_1, past_input_2) < pivotThreshold)
        {
            isPivoting = true;
        }
        // check dot product of 1 and 3 input since inputs can cancel each other out
        else if (past_input_2 == Vector2.zero && Vector2.Dot(past_input_1, past_input_3) < pivotThreshold)
        {
            isPivoting = true;
        }
        else
        {
            isPivoting = false;
        }

        canPivot = false;
        return isPivoting;
    }
   

    private void SlopeCheck()
    {
        var ray = new Ray(playerRoot.transform.position, Vector3.down);
        Debug.DrawRay(playerRoot.transform.position, Vector3.down * slopeRayMaxDistance, Color.yellow);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, slopeRayMaxDistance, ground_layers))
        {
            // Creates a rotation which rotates from from Vector3.up to the slope normal.
            slopeRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

          
            slopeAngle = Quaternion.Angle(Quaternion.identity, slopeRotation);

            // Convert the slope angle to radians
            float slopeRadians = Mathf.Deg2Rad * slopeAngle;

            // Calculate the direction vector parallel to the slope
            Vector3 slopeDirection = new Vector3(Mathf.Cos(slopeRadians), 0f, Mathf.Sin(slopeRadians));


            slopeRotation_Vert = Quaternion.FromToRotation(Vector3.up, slopeDirection);

            // If the slope angle is within some threshold orientate the player velocity to be parallel to the slope
            if (slopeAngle > min_SlopeAngle && slopeAngle < max_SlopeAngle)
            {
                playerOnSlope = true;
            }
            else
            {
                playerOnSlope = false;
            }
               
        }
        else
        {
            playerOnSlope = false;
        }
            
    }



    private void GroundCheck()
    {
        // set sphere position, with offset
        Vector3 sphere_position = new Vector3(playerRoot.transform.position.x, playerRoot.transform.position.y - grounded_offset, playerRoot.transform.position.z);
        bool isGrounded = Physics.CheckSphere(sphere_position, grounded_radius, ground_layers, QueryTriggerInteraction.Ignore);
       


        
        if (playerGrounded == false && isGrounded == true)
        {
            // Player is grounded, reset Coyote Time
            //playerGrounded = true;
            JAFData.ResetTimer_CoyoteTime();
            JAFData.SetState_CoyoteTimeActive(false);
        }

        playerGrounded = slopeAngle > max_SlopeAngle ? false : isGrounded;

    }


    /*public Vector3 AdjustVelocityToSlopeVertical(Vector3 velocity)
    {
        Vector3 adjustedVelocity = slopeRotation_Vert * velocity;

        if (adjustedVelocity.y < 0)
        {
            //if (playerOnSlope) SAData.state_movingDownSlope = true;
            return adjustedVelocity;
        }

        SAData.state_movingDownSlope = false;
        return velocity;
    }*/

    public Vector3 AdjustVelocityToSlopeVertical(Vector3 velocity)
    {
        // Calculate the slope rotation based on the slope normal
        Quaternion slopeRotation_V = Quaternion.FromToRotation(Vector3.up, slopeRotation * Vector3.up);

        // Adjust the velocity to be parallel to the slope
        Vector3 adjustedVelocity = slopeRotation_V * velocity;

        if (slopeRotation.y < 0)
        {
            // If the slope is facing downward, set the state
            if (playerOnSlope) SAData.state_movingDownSlope = true;

            return adjustedVelocity;
        }

        // If the slope is facing upward or is flat, reset the state
        SAData.state_movingDownSlope = false;

        return velocity;
    }


     public Vector3 AdjustVelocityToSlope(Vector3 velocity)
    {
        Vector3 adjustedVelocity = slopeRotation * velocity;

        if(adjustedVelocity.y < 0)
        {
            if (playerOnSlope) SAData.state_movingDownSlope = true;
            return adjustedVelocity;
        }

        SAData.state_movingDownSlope = false;
        return velocity;
    }

    public Vector3 GetAdjustedVelocity(Vector3 verticalVelocity, Vector3 horizontalVelocity)
    {
        Vector3 adjustedVelocity;

        // Calculate the effective speed along the slope
        float horizontalSpeedSqr = horizontalVelocity.sqrMagnitude;
        float verticalSpeedSqr = verticalVelocity.sqrMagnitude;
        float finalSpeed = Mathf.Sqrt(horizontalSpeedSqr + verticalSpeedSqr);

        // If player is on a slope, adjust velocity
        if (slopeRotation != Quaternion.identity)
        {
            Vector3 slopeNormal = slopeRotation * Vector3.up;

            // Project the velocity onto the slope's plane
            Vector3 horizontalVelocityOnSlope = Vector3.ProjectOnPlane(horizontalVelocity, slopeNormal);
            float horizontalSpeedOnSlope = horizontalVelocityOnSlope.magnitude;

            // Calculate the adjusted velocity vector
            adjustedVelocity = slopeRotation * new Vector3(horizontalSpeedOnSlope, verticalVelocity.y, 0f);

            // Normalize the adjusted velocity to the final speed
            adjustedVelocity = adjustedVelocity.normalized * finalSpeed;
        }
        else
        {
            // If not on a slope, use the original horizontal velocity
            adjustedVelocity = new Vector3(horizontalVelocity.x, verticalVelocity.y, horizontalVelocity.z);
        }

        return adjustedVelocity;
    }
    

}
