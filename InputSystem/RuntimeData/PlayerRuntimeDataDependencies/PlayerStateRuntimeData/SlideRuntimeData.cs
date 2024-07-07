using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;

namespace PlayerData
{
    [System.Serializable]
    public class SlideRuntimeData 
    {
        
        SlideActionData SAData;

        public float currentMaxslideSpeed = 10.0f;
        
        [SerializeField] [ReadOnly] private float timer_IsSliding = 0;
        [SerializeField] [ReadOnly] private float timer_IsInputLocked = 0;
        [SerializeField] [ReadOnly] private float timer_CooldownSlide = 0;

        [SerializeField] [ReadOnly] private bool canLockInput;
        [SerializeField] [ReadOnly] private bool canSlide = true;

        [SerializeField] [ReadOnly] private float startRotation;

    
        [SerializeField] [ReadOnly] public  bool state_movingDownSlope;
        [SerializeField] [ReadOnly] private bool state_IsInputLocked;


        [SerializeField] [ReadOnly] public SimpleCooldownTimer slideCooldownTimer;       
        [SerializeField] [ReadOnly] public SimpleTimer slideTimer;
        [SerializeField] [ReadOnly] public SimpleTimer inputLockTimer;

        public SlideRuntimeData(SlideActionData slideActionData)
        {
            this.SAData = slideActionData;
            this.timer_IsSliding = 0;
            this.timer_IsInputLocked = 0;
            this.timer_CooldownSlide = 0;
            this.slideCooldownTimer = new SimpleCooldownTimer(SAData.slideCooldown);
            this.slideTimer = new SimpleTimer();
            this.inputLockTimer = new SimpleTimer();
        }

        public void UseSlideCooldown()
        {
            Debug.LogError("Try Use Slide Cooldown" + slideCooldownTimer.CoolDownComplete);
            if(slideCooldownTimer.TryUseCooldown())
            {
                SetCanLockInput(true);
                slideTimer.SetFlag(true, SAData.slideDuration);
                inputLockTimer.SetFlag(true, SAData.inputLockTime);
            }
        }

        // Input Lockout
        // -----------------------------------------------------------------------------------------------
        public void SetCanLockInput(bool value) { canLockInput = value; }
        public bool GetCanLockInput() { return canLockInput; }

        // Slide Cooldown
        // -----------------------------------------------------------------------------------------------
        public bool GetCanSlide() { return slideCooldownTimer.CoolDownComplete; }
        public void SetCanSlide(bool value) { canSlide = value; }
        public void ResetInternalSlideTimer() { CooldownManagement.ResetCooldownTimer(ref timer_IsSliding); }

        // Is Sliding State Check
        // -----------------------------------------------------------------------------------------------

        public bool GetState_IsSliding() { return slideTimer.flag; }
        public void SetState_IsSliding(bool value) { slideTimer.flag = value; } 
        public void ResetTimer_StateIsSliding() { slideTimer.flag = false; slideTimer.StopTimer(); }

        // Is InputLocked State Check
        // -----------------------------------------------------------------------------------------------

        public void SetState_IsInputLocked(bool value) { inputLockTimer.flag = value; }
        public bool GetState_IsInputLocked() { return inputLockTimer.flag; }
        public void ResetTimer_StateIsInputLocked() { inputLockTimer.flag = false; inputLockTimer.StopTimer(); }


        // Movement Lockout
        // ------------------------------------------------------------------------------------- ----------

        public void SetStartRotation(float rotation) { startRotation = rotation; }
        public float GetStartRotation() { return startRotation; }


        // Speed Update 
        // -----------------------------------------------------------------------------------------------
        public void ResetMaxSlideSpeed()
        {
            currentMaxslideSpeed = SAData.baseSlideSpeed;
        }
        public void TryUpdataMaxSlideSpeed(float gravityComponent)
        {
            currentMaxslideSpeed = Mathf.Min(SAData.maxGravityAdjustedSlideSpeed, currentMaxslideSpeed + gravityComponent);
        }
    }

}

