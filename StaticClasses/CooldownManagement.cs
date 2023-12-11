using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class CooldownManagement
{

    public static void UseCooldown(ref float timer, ref bool canUseCooldown, float duration)
    {
        timer = duration;
        canUseCooldown = false;
    }

    public static void TickCooldownTimer(ref float timer, ref bool canUseCooldown)
    {
        timer = Mathf.Max(timer - Time.deltaTime, 0);
        if(timer == 0) 
        {
            canUseCooldown = true;
        }
    }

    public static void ResetCooldownTimer(ref float timer)
    {
        timer = 0.0f;
    }

    public static void TickBasicTimer(ref float timer, float maxValue, ref bool isTimerComplete)
    {
        if(isTimerComplete == false)
        {
            timer = Mathf.Min(timer + Time.deltaTime, maxValue);
            if (timer == maxValue)
            {
                timer = 0.0f;
                isTimerComplete = true;
            }
        }
    }

  
    public static void TickBasicTimer(ref float timer, float maxValue, ref bool isTimerComplete, bool flipBool)
    {
        if(isTimerComplete == false && !flipBool || isTimerComplete == true && flipBool)
        {
            timer = Mathf.Min(timer + Time.deltaTime, maxValue);
            if (timer == maxValue)
            {
                timer = 0.0f;
                if (flipBool) isTimerComplete = false;
                else isTimerComplete = true;
            }
        }
    }

    public static void ResetBasicTimer(ref float timer, ref bool isTimerComplete)
    {
        timer = 0.0f;
        isTimerComplete = false;
    }

    public static void ResetBasicTimer(ref float timer, ref bool isTimerComplete, bool flipBool)
    {
        timer = 0.0f;
        if (flipBool) isTimerComplete = false;
        else isTimerComplete = true;
    }





}
