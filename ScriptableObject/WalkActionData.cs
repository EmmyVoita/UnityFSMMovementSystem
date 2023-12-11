using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/FSM/WalkAction")]
public class WalkActionData : ScriptableObject
{
    [Header("Inscribed")]
    public SpeedUpdateMethod speedUpdateMethod;
    public float speedChangeRate = 3.0f;
    public float rotationSmoothTime = 0.1f;
    public float playerSpeed = 5.0f;
    public float speedOffset = 0.1f;

    public float getPlayerSpeed() { return playerSpeed; }

    public void setPlayerSpeed(float speed) { playerSpeed = speed; }
}
