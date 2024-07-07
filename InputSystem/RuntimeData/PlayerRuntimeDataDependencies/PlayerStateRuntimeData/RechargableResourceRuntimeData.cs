using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlayerData
{
    [System.Serializable]
    public class RechargableResourceRuntimeData
    {
        RechargableResourceData RRData;
        [SerializeField] private float currentResource = 0.0f;
        [SerializeField] private float internalRechargeDelayTimer = 0.0f;

        public RechargableResourceRuntimeData(RechargableResourceData rechargableResourceData)
        {
            this.RRData = rechargableResourceData;
            currentResource = rechargableResourceData.maximumResource;
        }
        
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
            
            if(internalRechargeDelayTimer >= RRData.resourceRechargeDelay)
            {
                currentResource += Time.deltaTime * RRData.resourceRechargeRate;
                currentResource = Mathf.Clamp(currentResource, 0.0f, RRData.maximumResource);

            }
            else
            {
                internalRechargeDelayTimer += Time.deltaTime;
            }
        }
    }
}
