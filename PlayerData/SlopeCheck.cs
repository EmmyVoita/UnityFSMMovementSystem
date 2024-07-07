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

public class SlopeCheck : MonoBehaviour
{

    // Define a delegate type for the event
    public delegate void SlopeCheckChangedHandler(bool playerOnSlope, float slopeAngle, float max_SlopeAngle);

    // Define the event using the delegate type
    public event SlopeCheckChangedHandler OnSlopeCheckChanged;

    [TabGroup("tab1","General")] public SlideActionData SAData;
    // Slope Check
    [TabGroup("tab1","Inscribed", TextColor = "green")] public float slopeRayVecrticalOffset = 1.0f;
    [TabGroup("tab1","Inscribed")] public float slope2_verticalOffset = 1.0f;
    [TabGroup("tab1","Inscribed")] public float slopeRay_2_MaxDistance = 1.5f;
    [TabGroup("tab1","Inscribed")] public float slopeRayMaxDistance = 10.0f;

    [TabGroup("tab1","Inscribed")] public float min_SlopeAngle = 15.0f;
    [TabGroup("tab1","Inscribed")] public float max_SlopeAngle = 75.0f;

    



    [TabGroup("tab1","Dynamic", TextColor = "blue")]  [SerializeField] [ReadOnly]  public Quaternion slopeRotation;
    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly] public Quaternion slopeRotation_Vert;
    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly] public float slopeAngle;
    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly]  public bool playerOnSlope = false;
    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly] PlayerDataManagement playerDataManagement;
    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly] GameObject playerRoot;
    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly] LayerMask groundLayers;

    private void Awake()
    {
        playerDataManagement = this.gameObject.GetComponent<PlayerDataManagement>();
        playerDataManagement.OnPassPlayerData += HandlePlayerData;
    }


    private void HandlePlayerData(GameObject playerRoot, LayerMask groundLayers)
    {
        this.playerRoot = playerRoot;
        this.groundLayers = groundLayers;   
    }

    void FixedUpdate()
    {
        CheckState();
    }

    private void CheckState()
    {
        var ray = new Ray(playerRoot.transform.position + Vector3.up * slopeRayVecrticalOffset, Vector3.down);
        Debug.DrawRay(playerRoot.transform.position+ Vector3.up * slopeRayVecrticalOffset, Vector3.down * slopeRayMaxDistance, Color.white);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, slopeRayMaxDistance, groundLayers))
        {
            Vector3 negativeNormal = -(slopeRotation * Vector3.up);

            var ray_slope_2 = new Ray(playerRoot.transform.position + Vector3.up * slope2_verticalOffset, negativeNormal);
            Debug.DrawRay(playerRoot.transform.position + Vector3.up * slope2_verticalOffset, negativeNormal, Color.blue);

            if(slopeAngle != 0 && Physics.Raycast(ray_slope_2, out RaycastHit hitInfo_2, slopeRay_2_MaxDistance, groundLayers))
            {
                //playerOnSlope = false;
                //return;
            }
            else
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
                    return;
                }
                else
                {
                    //slopeRotation = 0;
                    slopeAngle = 0;
                    playerOnSlope = false;
                    return;
                }
            }
        }
        else
        {
            // Creates a rotation which rotates from from Vector3.up to the slope normal.
            //slopeRotation = 0;
            slopeAngle = 0;

            playerOnSlope = false;
            return;
        }

        slopeAngle = 0;

    }

    
    public Vector3 AdjustVelocityToSlopeVertical(Vector3 velocity)
    {
        // Calculate the slope rotation based on the slope normal
        Quaternion slopeRotation_V = Quaternion.FromToRotation(Vector3.up, slopeRotation * Vector3.up);

        Vector3 normal = Vector3.Normalize(slopeRotation * Vector3.up);
        Vector3 slopeDirection_ = Vector3.Cross(normal, Vector3.up).normalized;
        Vector3 slopeDirectionUp_ = -Vector3.Cross(normal, slopeDirection_).normalized;

     
        Quaternion slopeRotation_parallel = Quaternion.FromToRotation(Vector3.up, slopeDirectionUp_);
        // Calculate the slope rotation based on the slope normal
        //Quaternion slopeRotation_U = Quaternion.FromToRotation(Vector3.up, slopeDirectionUp_);

        Vector3 adjustedVelocity = slopeRotation_parallel * velocity;
        // Adjust the velocity to be parallel to the slope
        //Vector3 adjustedVelocity = slopeRotation_U * velocity;

        Debug.DrawRay(playerRoot.transform.position, adjustedVelocity, Color.yellow);
        return adjustedVelocity;

        /*if (slopeRotation.y < 0)
        {
            // If the slope is facing downward, set the state
            if (playerOnSlope) SAData.state_movingDownSlope = true;

            return adjustedVelocity;
        }

        // If the slope is facing upward or is flat, reset the state
        SAData.state_movingDownSlope = false;

        return velocity;*/
    }

    
    public Vector3 AdjustVelocityToSlope(Vector3 velocity)
    {
        Vector3 adjustedVelocity = slopeRotation * velocity;

        if(adjustedVelocity.y < 0)
        {
            if (playerOnSlope) playerDataManagement.playerRuntimeData.slideRuntimeData.state_movingDownSlope = true;
            return adjustedVelocity;
        }

        playerDataManagement.playerRuntimeData.slideRuntimeData.state_movingDownSlope = false;
        return velocity;
    }

    

    /*
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
    }*/
    



}
