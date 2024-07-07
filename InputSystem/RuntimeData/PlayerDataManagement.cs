using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;
using UnityEngine.UIElements;


using PlayerData;


public class PlayerDataManagement : MonoBehaviour
{
    [TabGroup("tab1","StateData")] public JumpAndFallingData JAFData; 
    [TabGroup("tab1","StateData")] public SlideActionData SAData; 
    [TabGroup("tab1","StateData")] public WalkActionData WAData;
    [TabGroup("tab1","StateData")] public DirectionalPivotData DPData;
    [TabGroup("tab1","StateData")] public RechargableResourceData SARRData;
    [TabGroup("tab1","StateData")] public IdleActionData IAData;
    [TabGroup("tab1","StateData")] public GroundSlamDecisionData GSDData;
    [TabGroup("tab1","StateData")] public RechargableResourceData GSRRData;


    [TabGroup("tab1","OtherData")] public LastSafePositionData lastSafePositionData;

    [TabGroup("tab1","StateData")] public PlayerRuntimeData playerRuntimeData;


    public string rotationTransformName = "PlayerAimHandler";

    [TabGroup("tab1","VFXData")] public List<VisualEffectStruct> onWalkVFX = new List<VisualEffectStruct>();
    [TabGroup("tab1","VFXData")] public List<VisualEffectStruct> onLandVFX = new List<VisualEffectStruct>();
    [TabGroup("tab1","VFXData")] public List<VisualEffectStruct> onSlideVFX = new List<VisualEffectStruct>();
    [TabGroup("tab1","VFXData")] public List<VisualEffectStruct> onGroundSlamVFX = new List<VisualEffectStruct>();
    [TabGroup("tab1","VFXData")] public List<VisualEffectStruct> onGroundSlamToSlideVFX = new List<VisualEffectStruct>();
    [TabGroup("tab1","VFXData")] public List<VisualEffectStruct> onGroundSlamToJumpVFX = new List<VisualEffectStruct>();

    public float playerDirectionalLeanInfluence = 0.5f;
    public float blendSpeed = 5f; // Adjust this value to control the speed of the blend

    public GameObject playerTargetRoot;









    // Define a delegate type for the event
    public delegate void PassPlayerDataHandler(GameObject playerRoot, LayerMask groundLayers);

    // Define the event using the delegate type
    public event PassPlayerDataHandler OnPassPlayerData;



    // An event that will be invoked when all references are set:
    public delegate void ReferencesSetHandler();
    public event ReferencesSetHandler OnReferencesSet;

    
    
    [TabGroup("tab1","Inscribed", TextColor = "green")]
    
    [TabGroup("tab1","Inscribed")] public GameObject playerRoot;
    [TabGroup("tab1","Inscribed")] public GameObject animatorRoot;
    [TabGroup("tab1","Inscribed")] public LayerMask groundLayers;
    [TabGroup("tab1","Inscribed")] public CelShadePlayerController celShadePlayerController;



    [TabGroup("tab1","Dynamic", TextColor = "blue")]
    [TabGroup("tab1","Dynamic")] [ReadOnly] public EdgeHoldCheck edgeHoldCheck;
    [TabGroup("tab1","Dynamic")] [ReadOnly] public GroundCheck groundCheck;
    [TabGroup("tab1","Dynamic")] [ReadOnly] public SlopeCheck slopeCheck;
    [TabGroup("tab1","Dynamic")] [ReadOnly] public CeilingCheck ceilingCheck;
    [TabGroup("tab1","Dynamic")] [ReadOnly] public PivotCheck pivotCheck;




    // Player Input Actions


 



    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] private PlayerInput playerInput;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] private PlayerInputActions playerInputActions;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] private PlayerCameraManagement playerCameraData;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public CharacterController characterController;
    



    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public float jump = 0;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public float slide = 0;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public float groundSlam = 0;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public float dash = 0;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public float primary = 0;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public float currentHorizontalSpeed_Projected;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public float currentHorizontalSpeed;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public Vector3 characterVelocityNormalized;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public Vector2 inputVector;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public Vector2 previousInputVector;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public Vector3 inputDirection;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public Vector3 localHorizontalInputDirection;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public Vector3 previousInputDirection;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public Vector2 lookVector;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public float pV_ID_DotProduct;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public Vector3 playerForwardVector;
    
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] private bool bufferLock = false;
    public CircularBuffer<Vector2> inputBuffer = new CircularBuffer<Vector2>(4);


    

    private bool IsCurrentDeviceMouse
    {
        get
        {
        #if ENABLE_INPUT_SYSTEM
            //Debug.Log("Input device is mouse");
            return playerInput.currentControlScheme == "KeyboardMouse";
        #else
            //Debug.Log("Input device not mouse");
            return false;
        #endif
        }
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
        //jumpAndFallingRuntimeData = new JumpAndFallingRuntimeData(JAFData);
        //slideRuntimeData = new SlideRuntimeData(JAFData);
        //walkRuntimeData = new WalkRuntimeData(JAFData);
        playerRuntimeData = new PlayerRuntimeData(
            JAFData, 
            SAData, 
            WAData, 
            IAData, 
            SARRData,
            GSDData, 
            GSRRData, 
            lastSafePositionData,
            this, 
            playerRoot, 
            this.gameObject, 
            playerTargetRoot,
            rotationTransformName
            );



        playerCameraData = GetComponent<PlayerCameraManagement>();
        characterController = playerRoot.GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        inputBuffer.FillValue(4, Vector2.zero);
        playerRuntimeData.jumpAndFallingRuntimeData.state_CanStartCoyoteTime = true;



        edgeHoldCheck = GetComponent<EdgeHoldCheck>();
        groundCheck = GetComponent<GroundCheck>();
        slopeCheck = GetComponent<SlopeCheck>();
        ceilingCheck = GetComponent<CeilingCheck>();
        pivotCheck = GetComponent<PivotCheck>();

        
    }

    private void Start()
    {
        OnReferencesSet?.Invoke();
        OnPassPlayerData?.Invoke(playerRoot, groundLayers);
    }

    // Update is called once per frame
    void Update()
    {

        lookVector = playerInputActions.Player.Look.ReadValue<Vector2>();

        if (!bufferLock)
        {
            // Read values from the player input actions
            jump = playerInputActions.Player.Jump.ReadValue<float>();
            slide = playerInputActions.Player.Slide.ReadValue<float>();
            groundSlam = playerInputActions.Player.GroundSlam.ReadValue<float>();
            dash = playerInputActions.Player.Dash.ReadValue<float>();
            primary = playerInputActions.Player.Primary.ReadValue<float>();

            inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
            inputDirection = new Vector3(inputVector.x, 0.0f, inputVector.y).normalized;
            // Project inputDirection onto the forward plane (plane defined by transform.forward)
            //localHorizontalInputDirection = Vector3.ProjectOnPlane(playerRoot.transform.TransformDirection(inputDirection), playerRoot.transform.forward);
            //localHorizontalInputDirection = Vector3.ProjectOnPlane(inputDirection, playerRoot.transform.forward);
            // Debug.DrawRay(playerRoot.transform.position, localHorizontalInputDirection * 10, Color.magenta);

    

            // Current Horizontal Speed with respect to the slope rotation
            //Vector3 projectPlayerVelocity = Vector3.ProjectOnPlane(characterController.velocity, (slopeRotation * playerRoot.transform.up).normalized);


            /// summary:
            /// currentHorizontalSpeed_Projected is the magnitude of the horizontal component of the player's 
            /// velocity after it's been projected onto the slope.
            
        
        }
        playerForwardVector = playerRoot.transform.forward;
        Vector3 projectPlayerVelocity = Vector3.ProjectOnPlane(characterController.velocity, slopeCheck.slopeRotation * playerRoot.transform.up);
        Debug.DrawRay(playerRoot.transform.position, slopeCheck.slopeRotation * playerRoot.transform.up, Color.green);
        currentHorizontalSpeed_Projected = projectPlayerVelocity.magnitude;

        //Assuming gravity is pointing downwards (negative y direction)
        float verticalComponent = Vector3.Dot(characterController.velocity, Vector3.down);
                
        // Calculate the effective speed along the slope (including vertical component)
        //slopeCheck.currentSlopeSpeed = Mathf.Sqrt(currentHorizontalSpeed_Projected * currentHorizontalSpeed_Projected + verticalComponent * verticalComponent);
        

        currentHorizontalSpeed = (new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z)).magnitude;
        characterVelocityNormalized = characterController.velocity.normalized;
        

        if (inputVector != previousInputVector && !bufferLock)
        {
            inputBuffer.Add(inputVector);
            pivotCheck.canPivot = true;
        }

        //inputVector = inputHandler.GetLastInputDirection();

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
}
