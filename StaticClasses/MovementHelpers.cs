using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public enum SpeedUpdateMethod
{
    Accelerate,
    Slow,
    Adjust,
}

public static class MovementHelpers
{
    public static void UpdateSpeed(ref float _speed, SpeedUpdateMethod updateMethod, float targetSpeed, float speedOffset, float currentForwardSpeed, float speedChangeRate)
    {
        switch (updateMethod)
        {
            case SpeedUpdateMethod.Accelerate:
                AccelerateToTargetSpeed(ref _speed, targetSpeed, speedOffset, currentForwardSpeed, speedChangeRate);
                break;

            case SpeedUpdateMethod.Slow:
                SlowToTargetSpeed(ref _speed, targetSpeed, speedOffset, currentForwardSpeed, speedChangeRate);
                break;

            case SpeedUpdateMethod.Adjust:
                AdjustToTargetSpeed(ref _speed, targetSpeed, speedOffset, currentForwardSpeed, speedChangeRate);
                break;

            default:
                Debug.LogWarning("Unhandled SpeedUpdateMethod: " + updateMethod);
                break;
        }
    }

    public static void AccelerateToTargetSpeed(ref float _speed, float targetSpeed, float speedOffset, float currentForwardSpeed, float speedChangeRate)
    {
        // accelerate to target speed
        if (currentForwardSpeed < targetSpeed - speedOffset)
        {
            //accleration over time function
            _speed = Mathf.Lerp(currentForwardSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }
    }

    public static void SlowToTargetSpeed(ref float _speed, float targetSpeed, float speedOffset, float currentForwardSpeed, float speedChangeRate)
    {
        // accelerate to target speed
        if (currentForwardSpeed > targetSpeed - speedOffset)
        {
            //accleration over time function
            _speed = Mathf.Lerp(currentForwardSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }
    }

    public static void AdjustToTargetSpeed(ref float _speed, float targetSpeed, float speedOffset, float currentForwardSpeed, float speedChangeRate)
    {
        // accelerate to target speed
        if (currentForwardSpeed > targetSpeed - speedOffset || targetSpeed < targetSpeed - speedOffset)
        {
            //accleration over time function
            _speed = Mathf.Lerp(currentForwardSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }
    }

}
