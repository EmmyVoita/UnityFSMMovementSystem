using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;

namespace PlayerData
{
    [System.Serializable]
    public class SimpleCooldownTimer
    {
        public SimpleCooldownTimer(float coolDownAmount)
        {
            this.coolDownAmount = coolDownAmount;
        }

        public float coolDownAmount;
        [SerializeField] private float m_coolDownCompleteTime;
        [SerializeField] public bool CoolDownComplete => Time.time > m_coolDownCompleteTime;
        public void StartCoolDown()
        {
            m_coolDownCompleteTime = Time.time + coolDownAmount;
        }

        public void ResetCooldown()
        {
            m_coolDownCompleteTime = 0;
        }

        public void SetCooldown(float value)
        {
            m_coolDownCompleteTime = Time.time + value;
        }

        public bool TryUseCooldown()
        {
            if(!CoolDownComplete) return false;
            else
            {
                StartCoolDown();
                return true;
            }
        }
    }
}

