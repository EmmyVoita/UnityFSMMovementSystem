using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/FSM/DashAction")]
public class DashActionData : ScriptableObject
{
    [Header("Inscribed")]
    public float dashDuration = 0.3f;
    public float dashCooldown = 1.0f;
    public float dashSpeed = 10.0f;

    [Header(" - SpeedChangeRateCurve")]
    public float speedOffset = 0.1f;
    public float speedChangeRate = 0.1f;

    [Header("Dynamic")]
    [SerializeField]
    private float internalDashTimer = 0;
    [SerializeField]
    private float internalDashCooldownTimer = 0;
    [SerializeField]
    private bool canDash = true;

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
        if(internalDashCooldownTimer <= 0.0f)
        {
            SetCanDash(true);
            internalDashCooldownTimer = 0;
        }
    }

    public void ResetInternalDashTimer()
    {
        internalDashTimer = 0;
    }

}
