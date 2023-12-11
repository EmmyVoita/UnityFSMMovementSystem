using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/FSM/PhaseSync")]
public class PhaseSyncData : ScriptableObject
{
    [Header("Inscribed")]   
    public float resourceDepletionRate = 0.1f;

    [Header("Dynamic")]
    [SerializeField]
    private bool isUsingAction = false;

    public bool GetIsUsingAction() { return isUsingAction; }
    public void SetIsUsingAction(bool value) { isUsingAction = value; }


}
