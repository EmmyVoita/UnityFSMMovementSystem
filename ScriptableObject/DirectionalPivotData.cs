using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/FSM/DirectionalPivotAction")]
public class DirectionalPivotData : ScriptableObject
{
    [Header("Directional Pivot Inscribed")]
    public SpeedUpdateMethod speedUpdateMethod;
    public float playerSpeed = 0.1f;
    public float speedChangeRate = 10.0f;
    public float speedOffset = 0.1f;
    public float pivotThreshold = -0.5f;

    [Header(" - Lockout Settings")]
   
    public float maxLockoutTime = 0.4f;
    public float minLockoutTime = 0.1f;
    public float speedThreshold = 0.1f;

    [Header(" - Rotation Management")]
    public float rotationSmoothTime = 0.1f;
     public float rotationLockTime = 0.4f;
    [SerializeField]
    private float timer_IsRotationLocked = 0;
    [SerializeField]
    private bool state_IsRotationLocked;




    public float speedFactor = 0.1f;

    [Header("Dynamic")]
    [SerializeField]
    private float lockoutTime = 0.0f;
     [SerializeField]
    private float internalLockoutTimer = 0.0f;
    [SerializeField]
    private Vector3 initalDirection = Vector3.zero;

    public void SetInitialDirection(Vector3 direction)
    {
        initalDirection = direction;
    }

    public Vector3 GetInitalDirection()
    {
        return initalDirection;
    }

    public bool IsDirectionalPivotCheck()
    {
        return internalLockoutTimer >= lockoutTime ? false : true;
    }

    public void SetLockoutTime(float playerSpeed)
    {
        lockoutTime = Mathf.Lerp(minLockoutTime, maxLockoutTime, playerSpeed);
    }

    public void IncrementInternalLockoutTimer()
    {
        internalLockoutTimer += Time.deltaTime;
    }

    public void ResetInternalLockoutTimer()
    {
        internalLockoutTimer = 0.0f;
    }



    public void SetState_IsRotationLocked(bool value) { state_IsRotationLocked = value; }
    public bool GetState_IsRotationLocked() { return state_IsRotationLocked; }
    public void TickAndUpdate_StateIsRotationLocked() { CooldownManagement.TickBasicTimer(ref timer_IsRotationLocked, rotationLockTime, ref state_IsRotationLocked, true); }
    public void ResetTimer_StateIsRotationLockedg() { CooldownManagement.ResetBasicTimer(ref timer_IsRotationLocked, ref state_IsRotationLocked); }

}

