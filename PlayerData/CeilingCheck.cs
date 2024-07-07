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


public class CeilingCheck : MonoBehaviour
{
    






    [TabGroup("tab1","Debug")] [SerializeField] bool DebugDrawCeilingCheck = false;
    [TabGroup("tab1","Inscribed", TextColor = "green")] public float ceiling_SphereIntersection_Radius = 0.3f;
    [TabGroup("tab1","Inscribed")] public float ceiling_SphereIntersection_YOffset = 0.3f;
    [TabGroup("tab1","Dynamic", TextColor = "blue")]  [SerializeField] [ReadOnly]  public bool playerState_HitCeiling;


    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly]  private PlayerDataManagement playerDataManagement;
    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly]  private GameObject playerRoot;
    [TabGroup("tab1","Dynamic")]  [SerializeField] [ReadOnly]  private LayerMask groundLayers;

    private void Awake()
    {
        playerDataManagement = this.gameObject.GetComponent<PlayerDataManagement>();
        playerDataManagement.OnPassPlayerData += HandlePlayerData;
        playerState_HitCeiling = false;
    }

    private void HandlePlayerData(GameObject playerRoot, LayerMask groundLayers)
    {
        this.playerRoot = playerRoot;
        this.groundLayers = groundLayers;   
    }


    void FixedUpdate()
    {
        playerState_HitCeiling = CheckState();
    }
  
    private bool CheckState()
    {
        // set sphere position, with offset
        Vector3 sphere_position = new Vector3(playerRoot.transform.position.x, playerRoot.transform.position.y + ceiling_SphereIntersection_YOffset, playerRoot.transform.position.z);
        bool hitCeiling = Physics.CheckSphere(sphere_position, ceiling_SphereIntersection_Radius, groundLayers, QueryTriggerInteraction.Ignore);
 
        return hitCeiling;
    }

    
    void OnDrawGizmosSelected()
    {

        if(DebugDrawCeilingCheck)
        {
            // Draw a yellow sphere at the transform's position
            // set sphere position, with offset
            Vector3 sphere_position = playerRoot.transform.position + playerRoot.transform.up * ceiling_SphereIntersection_YOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(sphere_position, ceiling_SphereIntersection_Radius);
        }
    }

}
