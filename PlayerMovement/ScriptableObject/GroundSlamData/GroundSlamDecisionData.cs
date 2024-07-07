using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObject/FSM/GroundSlamDecisionData")]
public class GroundSlamDecisionData : ScriptableObject
{
    [Header("Inscribed")]
    public SpeedUpdateMethod speedUpdateMethod;
    public float speedChangeRate = 3.0f;
    public float targetSpeed = 0.0f;
    public float exitSpeedBaseMultiplier = 2.5f;
    public float speedOffset = 0.1f;
    public float stateTransitionIdleThresholdSpeed = 0.1f;
    public float HorizontalContribution = 1.0f;
    public float VerticalContribution = 1.0f;
    public float verticalWeight = 0.1f;
    public float horizontalWeight = 0.1f;
    public float passiveChargeRate = 0.1f;
    public float groundSlamResourceCost = 0.3f;

}
