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

public class PivotCheck : MonoBehaviour
{
    
    [TabGroup("tab1","Inscribed", TextColor = "green")]
    [TabGroup("tab1","Inscribed")] public float pivotThreshold = -0.5f;
    [TabGroup("tab1","Dynamic")] public bool isPivoting = false;
    [TabGroup("tab1","Dynamic")] [SerializeField] public bool canPivot = false;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] PlayerDataManagement playerDataManagement;
    CircularBuffer<Vector2> inputBuffer;


    void Awake()
    {
        playerDataManagement = this.gameObject.GetComponent<PlayerDataManagement>();
        playerDataManagement.OnReferencesSet += HandleReferenceSetEvent;
        

        //playerDataManagement.OnPassPlayerData += HandlePlayerData;
        
    }

    private void HandleReferenceSetEvent()
    {
        //playerDataManagement.OnPassPlayerData += HandlePlayerData;
        inputBuffer = playerDataManagement.inputBuffer;
    }

    private void HandlePlayerData()
    {

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
   
   

 
}
