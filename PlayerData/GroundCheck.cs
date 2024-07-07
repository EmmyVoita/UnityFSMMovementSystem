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


public class GroundCheck : MonoBehaviour
{
    // Define a delegate type for the event
    public delegate void PlayerGroundedChangedHandler(bool isGrounded);

    // Define the event using the delegate type
    public event PlayerGroundedChangedHandler OnPlayerGroundedChanged;



    
    [TabGroup("tab1","Inscribed", TextColor = "green")]
    [TabGroup("tab1","Inscribed")] public float grounded_radius = 0.28f;
    [TabGroup("tab1","Inscribed")] public float grounded_offset = -0.14f;
   



    [TabGroup("tab1","Inscribed")] public JumpAndFallingData JAFData;   


    [TabGroup("tab1","Dynamic", TextColor = "blue")]  
    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly]   public bool playerGrounded = false;
    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly]  PlayerDataManagement playerDataManagement;   
    


    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly] private float slopeAngle;
    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly] private float max_SlopeAngle;
    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly] private bool playerOnSlope;
    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly] private GameObject playerRoot;
    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly] private LayerMask groundLayers;
    
    
    
    private void Awake()
    {
        playerDataManagement = this.gameObject.GetComponent<PlayerDataManagement>();
        playerDataManagement.OnReferencesSet += HandleReferenceSetEvent;
    }

    private void HandleReferenceSetEvent()
    {
        // Subscribe to events
        playerDataManagement.slopeCheck.OnSlopeCheckChanged += HandleSlopeCheckChanged;
        playerDataManagement.OnPassPlayerData += HandlePlayerData;
    }
    
    // Function Call when SlopeCheckChanged event is invoked (need the slope data in CheckState())
    private void HandleSlopeCheckChanged(bool playerOnSlope, float slopeAngle, float max_SlopeAngle)
    {
        this.slopeAngle = slopeAngle;
        this.max_SlopeAngle = max_SlopeAngle;
        this.playerOnSlope = playerOnSlope;
    }
           

    // HandlePlayerData method to get and set necessary references
    private void HandlePlayerData(GameObject playerRoot, LayerMask groundLayers)
    {
        this.playerRoot = playerRoot;
        this.groundLayers = groundLayers;   
    }

    void FixedUpdate()
    {
        CheckState();
    }


    // CheckState method to check the state of the player (Are they grounded?)
    private void CheckState()
    {
        // Store previous grounded state
        bool previousValue = playerGrounded;

        // Set sphere position for ground check
        Vector3 sphere_position = new Vector3(playerRoot.transform.position.x, playerRoot.transform.position.y - grounded_offset, playerRoot.transform.position.z);
        // Perform ground check
        bool isGrounded = Physics.CheckSphere(sphere_position, grounded_radius, groundLayers, QueryTriggerInteraction.Ignore);
       
        // Update grounded state based on slope and ground check
        playerGrounded = slopeAngle > max_SlopeAngle ? false : isGrounded;

        // If the grounded state changes, invoke event
        if(previousValue != playerGrounded)
        {
            OnPlayerGroundedChanged?.Invoke(playerGrounded);
        }
    }
}
