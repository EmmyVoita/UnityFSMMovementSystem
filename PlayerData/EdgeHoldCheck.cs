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
using PlayerData;
using PlayerStates;

public class EdgeHoldCheck : MonoBehaviour
{
    // Define a delegate type for the event
    public delegate void PlayerEdgeHoldHandler(bool holdEdge);

    // Define the event using the delegate type
    public event PlayerEdgeHoldHandler OnPlayerEdgeHoldChanged;
  


    [TabGroup("tab1","Debug")] [SerializeField] bool DebugDrawEdge = false;
    [TabGroup("tab1","Debug")] [SerializeField] bool DebugDrawSphereCheck = false;
    [TabGroup("tab1","Debug")] [SerializeField] bool DebugDrawSphereNormalCheck = false;

    [Title("Double Sphere Check Settings")]
    [TabGroup("tab1","Inscribed")] public float EdgeTopSphere_Radius = 0.3f;
    [TabGroup("tab1","Inscribed")] public float EdgeBottomSphere_Radius = 0.3f;

    [TabGroup("tab1","Inscribed")] public Vector3 EdgeBottomSphereCheck_Offset;
    [TabGroup("tab1","Inscribed")] public Vector3 EdgeTopSphereCast_Offset;
    [TabGroup("tab1","Inscribed")] public float EdgeTopSphereCast_MaxDistance = 1f;


    
    [Title("Normal Sphere Cast Settings")]
    [TabGroup("tab1","Inscribed")] public float  EdgeNormalSphere_Radius = 0.3f;
    [TabGroup("tab1","Inscribed")] public Vector3 EdgeNormalSphereCast_Offset;
    [TabGroup("tab1","Inscribed")] public float EdgeNormalSphereCast_MaxDistance = 1f;
    // Define the number of raycasts and the angle between them
    [TabGroup("tab1","Inscribed")] [SerializeField] int numRaysHorizontal = 2;
    [TabGroup("tab1","Inscribed")] [SerializeField] int numRaysVertical = 3;
    [TabGroup("tab1","Inscribed")] [SerializeField] float angleBetweenRays = 10f;
    [TabGroup("tab1","Inscribed")] [SerializeField] float verticalOffset = 0.5f;
    [TabGroup("tab1","Inscribed")] [SerializeField] float samplingPositionOffset = 1.0f;
    [TabGroup("tab1","Inscribed")] [SerializeField] Vector3 TopRayCastPos;


    [TabGroup("tab1","Inscribed")] [SerializeField] MovementState movementState;
    [TabGroup("tab1","Inscribed")] [SerializeField] private Vector3 edgeHoldPositionOffset;


    [TabGroup("tab1","Dynamic", TextColor = "blue")] [SerializeField] [ReadOnly] Vector3 edgeIntersectNormal;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] Vector3 edgeIntersectPosition;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public bool edgeHoldState = false;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] public bool shouldCheckEdgeHold  = false;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] private PlayerDataManagement playerDataManagement;


    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] bool playerIsGrounded;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] GameObject playerRoot;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] private LayerMask groundLayers;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] private FSMMovement fsmMovement;



    void Awake()
    {
        edgeHoldState = false;
        shouldCheckEdgeHold = false;

        playerDataManagement = this.gameObject.GetComponent<PlayerDataManagement>();
        fsmMovement = this.gameObject.GetComponent<FSMMovement>();
        playerDataManagement.OnReferencesSet += HandleReferenceSetEvent;
    
    }

    private void HandleReferenceSetEvent()
    {
        playerDataManagement.groundCheck.OnPlayerGroundedChanged += HandlePlayerGroundedChanged;
        playerDataManagement.OnPassPlayerData += HandlePlayerData;
        //fsmMovement.OnPlayerEdgeHoldJump += HandleEdgeHoldJump;
    }

    private void HandlePlayerData(GameObject playerRoot, LayerMask groundLayers)
    {
        this.playerRoot = playerRoot;
        this.groundLayers = groundLayers;   
    }
   
    private void HandlePlayerGroundedChanged(bool isGrounded)
    {
        playerIsGrounded = isGrounded;
        ShouldCheckState();
    }

    private void HandleEdgeHoldJump()
    {
        shouldCheckEdgeHold = false;
        edgeHoldState = false;
    }

    
    void FixedUpdate()
    {
        DrawRays();
        if (shouldCheckEdgeHold)
        {
            edgeHoldState = CheckState();

            if (edgeHoldState)
            {
                shouldCheckEdgeHold = false;
            }
        }
   }

    public void ShouldCheckState()
    {
        if (playerIsGrounded == false)
        {
            shouldCheckEdgeHold = true;
        }
    }

    
    private bool CheckState()
    {
        if(playerRoot == null)
        {
            return false;
        }

        RaycastHit TopSphereHit;
        RaycastHit NormalSphereHit;

        // Calculate Sphere Postions
        Vector3 BottomSpherePos = playerRoot.transform.TransformPoint(EdgeBottomSphereCheck_Offset);
        Vector3 TopSpherePos = playerRoot.transform.TransformPoint(EdgeTopSphereCast_Offset);
        Vector3 NormalSpherePos = playerRoot.transform.TransformPoint(EdgeNormalSphereCast_Offset);

        // Bottom Sphere Check
        bool BottomSphereCollision = Physics.CheckSphere(BottomSpherePos, EdgeBottomSphere_Radius, groundLayers, QueryTriggerInteraction.Ignore);


        if(BottomSphereCollision)
        { 
            // Top Sphere Cast
            bool TopSphereCastCollision = Physics.SphereCast(TopSpherePos, EdgeTopSphere_Radius, playerRoot.transform.forward,  out TopSphereHit, EdgeTopSphereCast_MaxDistance, groundLayers);

            if(!TopSphereCastCollision)
            {
                // Normal Sphere Cast
                //bool NormalSphereCastCollision = Physics.SphereCast(NormalSpherePos, EdgeNormalSphere_Radius, playerRoot.transform.forward, out NormalSphereHit, EdgeNormalSphereCast_MaxDistance, groundLayers);
                

                bool multiHit = GetMultiRayCastNormal();





                /*if(NormalSphereCastCollision)
                {
                    //Debug.LogError($"NormalSphereHit.point: {NormalSphereHit.point}, NormalSphereHit.normal: {NormalSphereHit.normal}");
                    edgeIntersectPosition = NormalSphereHit.point;
                    edgeIntersectNormal = NormalSphereHit.normal;

                    //shouldCheckEdgeHold = false;
                }
                else
                {
                    Debug.LogError("EdgeHoldCheck: NormalSphere did not intersect");
                    edgeIntersectPosition = TopSphereHit.point;
                    edgeIntersectNormal = TopSphereHit.normal;
                }*/

                if(!multiHit)
                {
                    return false;
                }

                if(edgeHoldState == false && multiHit)
                {
                    OnPlayerEdgeHoldChanged?.Invoke(true);
                    AlignPlayerToEdge();
                }   
                //OnPlayerEdgeHoldChanged?.Invoke(true);
                return true;
            }
        }

        return false;
    }

   public bool GetMultiRayCastNormal()
    {
        Vector3 sumNormals = Vector3.zero;
        Vector3 sumPositions = Vector3.zero;
        int hitCount = 0;

        for (int j = 0; j < numRaysVertical; j++)
        {
            // Calculate the vertical offset for this row of raycasts
            float yOffset = (j - numRaysVertical / 2) * verticalOffset;

            // Calculate the starting direction
            Vector3 startDirection = Quaternion.Euler(0, -numRaysHorizontal / 2 * angleBetweenRays, 0) * playerRoot.transform.forward;

            for (int i = 0; i < numRaysHorizontal; i++)
            {
                // Calculate the direction of the ray
                Vector3 rayDirection = Quaternion.Euler(0, i * angleBetweenRays, 0) * startDirection;

                // Perform the raycast
                RaycastHit hit;
                Vector3 rayOrigin = playerRoot.transform.TransformPoint(TopRayCastPos) + new Vector3(0, yOffset + samplingPositionOffset, 0);
                if (Physics.Raycast(rayOrigin, rayDirection, out hit, EdgeNormalSphereCast_MaxDistance, groundLayers))
                {
                    // Add the hit normal and position to the sums
                    sumNormals += hit.normal;
                    sumPositions += hit.point;
                    hitCount++;

                    // Draw the ray for debugging
                    Debug.DrawRay(rayOrigin, rayDirection * EdgeNormalSphereCast_MaxDistance, Color.red);
                }
            }
        }

        if (hitCount > 0)
        {
            // Calculate the average normal and position
            Vector3 averageNormal = sumNormals / hitCount;
            Vector3 averagePosition = sumPositions / hitCount;

            // Use the average normal and position for the edge intersect normal and position
            edgeIntersectNormal = averageNormal;
            edgeIntersectPosition = averagePosition;

            return true;
        }
        else
        {
            Debug.LogError("EdgeHoldCheck: No hits ");
            return false;
        }
    }

   public void DrawRays()
    {
        for (int j = 0; j < numRaysVertical; j++)
        {
            // Calculate the vertical offset for this row of raycasts
            float yOffset = (j - numRaysVertical / 2) * verticalOffset;

            // Calculate the starting direction
            Vector3 startDirection = Quaternion.Euler(0, -numRaysHorizontal / 2 * angleBetweenRays, 0) * playerRoot.transform.forward;

            for (int i = 0; i < numRaysHorizontal; i++)
            {
                // Calculate the direction of the ray
                Vector3 rayDirection = Quaternion.Euler(0, i * angleBetweenRays, 0) * startDirection;

                // Draw the ray for debugging
                Vector3 rayOrigin = playerRoot.transform.TransformPoint(TopRayCastPos) + new Vector3(0, yOffset + samplingPositionOffset, 0);
                Debug.DrawRay(rayOrigin, rayDirection * EdgeNormalSphereCast_MaxDistance, Color.green);
            }
        }
    }

    public void AlignPlayerToEdge()
    {
        if(playerRoot == null)
        {
            return;
        }

        if(edgeIntersectNormal == null || edgeIntersectPosition == null)
        {
            Debug.LogError("EdgeHoldCheck: Edge Normal or Position is null");
            return;
        }

        // Align the player with the surface
        playerRoot.transform.forward = -edgeIntersectNormal;

        Vector3 worldOffset = playerRoot.transform.TransformDirection(edgeHoldPositionOffset);
        playerRoot.transform.position = edgeIntersectPosition + worldOffset;
    }


    
    void OnDrawGizmosSelected()
    {
        if(playerRoot == null)
        {
            return;
        }
        
        if(DebugDrawSphereCheck)
        {
             // Assuming edgeHold_SphereIntersection_Offset is a local offset
            Vector3 sphere1_position_EdgeHold = playerRoot.transform.TransformPoint(EdgeBottomSphereCheck_Offset);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(sphere1_position_EdgeHold, EdgeTopSphere_Radius);

            // Assuming edgeHold_SphereIntersection_Offset is a local offset
            Vector3 sphere2_position_EdgeHold = playerRoot.transform.TransformPoint(EdgeTopSphereCast_Offset);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(sphere2_position_EdgeHold, EdgeTopSphere_Radius);
        }

        if(DebugDrawSphereNormalCheck)
        {
            // Assuming edgeHold_SphereIntersection_Offset is a local offset
            Vector3 sphere3_position_EdgeHold = playerRoot.transform.TransformPoint(EdgeNormalSphereCast_Offset);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(sphere3_position_EdgeHold, EdgeNormalSphere_Radius);
        }
       

        if(DebugDrawEdge)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(edgeIntersectPosition, -Vector3.Normalize(edgeIntersectNormal) * 10);
            //Gizmos.DrawRay(edgeIntersectPosition, transform.forward * 5);
        }
        
    }
}
