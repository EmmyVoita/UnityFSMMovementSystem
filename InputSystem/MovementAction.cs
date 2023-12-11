using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAction
{
    
    public bool IsUsingAction { get; private set; }
    public float ActionDuration = 0.5f;
    public float ActionSpeed = 10.0f;
    public float ActionCooldown = 2.0f;  
    private float ActionCooldownTimer = 0.0f;
    public bool CanPerformAction = false;
    public float _ActionTimer = 0.0f;

    public float GracePeriodDuration = 2.0f;
    private float GracePeriodTimer = 0.0f;
    public bool IsInGracePeriod = false;

    //public float _GracePeriod
    
    public MovementAction(float ActionDuration, float ActionSpeed, float ActionCooldown)
    {
        this.ActionDuration = ActionDuration;
        this.ActionSpeed = ActionSpeed;
        this.ActionCooldown = ActionCooldown;
    }
    

    public void UseCooldown()
    {
        IsUsingAction = true;
        _ActionTimer = ActionDuration;
        ActionCooldownTimer = ActionCooldown; 
        CanPerformAction = false;
    }

    public void ManageCooldownTimer()
    {
        // Cooldown logic
        if (!CanPerformAction)
        {
            ActionCooldownTimer -= Time.deltaTime;

            if (ActionCooldownTimer <= 0.0f)
            {
                CanPerformAction = true;  // Reset flag and timer
                ActionCooldownTimer = 0.0f;
            }
        }
    }

    public void HandleActionTime(bool useGracePeriod)
    {
        // Decrement dash timer
        _ActionTimer -= Time.deltaTime;

        // Check if dash duration is over
        if ( _ActionTimer <= 0.0f)
        {
            IsUsingAction = false;
            if(useGracePeriod)
                UseGracePeriod();
            //CharacterModel.SetActive(true);
        }
    }

  
    public void HandleGracePeriod()
    {
        if (IsInGracePeriod)
        {
            GracePeriodTimer -= Time.deltaTime;

            if (GracePeriodTimer <= 0.0f)
            {
                IsInGracePeriod = false;
                GracePeriodTimer = 0.0f;
            }
        }
    }

    public void UseGracePeriod()
    {
        IsInGracePeriod = true;
        GracePeriodTimer = GracePeriodDuration;
    }

    public void CancelAction()
    {
        IsUsingAction = false;
        _ActionTimer = 0.0f; // Reset the action timer
    }
}
