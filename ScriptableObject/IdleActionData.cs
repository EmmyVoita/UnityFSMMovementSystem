using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/FSM/IdleAction")]
public class IdleActionData : ScriptableObject
{
    [Header("Inscribed")]
    public SpeedUpdateMethod speedUpdateMethod;
    public float speedChangeRate = 3.0f;
    public float targetSpeed = 0.0f;
    public float speedOffset = 0.1f;
    public float stateTransitionIdleThresholdSpeed = 0.1f;
}
