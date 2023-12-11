using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/FSM/RechargableResource")]
public class RechargableResourceData : ScriptableObject
{
    [Header("Inscribed")]

    public float resourceRechargeRate = 0.1f;
    public float resourceRechargeDelay = 0.1f;
    public float resourceDepletionRate = 0.1f;
    public float maximumResource = 1.0f;


    [Header("Dynamic")]

    [SerializeField]
    private float currentResource = 0.0f;
    [SerializeField]
    private float internalRechargeDelayTimer = 0.0f;

    public float GetCurrentResource() { return currentResource; }
    
    public bool TryUseResource(float value)
    {
        if (currentResource - value >= 0.0)
        {
            currentResource -= value;
            internalRechargeDelayTimer = 0.0f;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RechargeResource()
    {
        
        if(internalRechargeDelayTimer >= resourceRechargeDelay)
        {
            currentResource += Time.deltaTime * resourceRechargeRate;
            currentResource = Mathf.Clamp(currentResource, 0.0f, maximumResource);

        }
        else
        {
            internalRechargeDelayTimer += Time.deltaTime;
        }
    }


}
