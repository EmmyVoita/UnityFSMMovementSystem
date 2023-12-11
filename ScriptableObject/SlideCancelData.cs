using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/FSM/DashCancelAction")]
public class SlideCancelData : ScriptableObject
{
    [Header("Inscribed")]

    public float waitForInputTime = 0.3f;
    

    [Header("Dynamic")]

    private float internalTimer = 0.0f;
    private bool checkForInput;

    public void IncrementInternalTimer()
    {
        internalTimer += Time.deltaTime;
        if(internalTimer >= waitForInputTime)
        {
            checkForInput = false;
        }
    }

    public void ResetInternalTimer()
    {
        internalTimer = 0.0f;
    }

    public void SetCheckForInput(bool value)
    {
        checkForInput = value;
    }

    public bool GetCheckForInput()
    {
        return checkForInput;
    }

    /*
    public void SetCanDash(bool value) { canDash = value; }
    public bool GetCanDash() { return canDash; }

    public void UseCooldown()
    {
        internalDashCooldownTimer = dashCooldown;
        SetCanDash(false);
    }

    public bool IsDashingCheck()
    {
        return internalDashTimer >= dashDuration ? false : true;
    }

    public void IncrementInternalDashTimer()
    {
        internalDashTimer += Time.deltaTime;
    }

    public void DeincrementInternalDashCooldownTimer()
    {
        internalDashCooldownTimer -= Time.deltaTime;
        if (internalDashCooldownTimer <= 0.0f)
        {
            SetCanDash(true);
            internalDashCooldownTimer = 0;
        }
    }

    public void ResetInternalDashTimer()
    {
        internalDashTimer = 0;
    }*/
}