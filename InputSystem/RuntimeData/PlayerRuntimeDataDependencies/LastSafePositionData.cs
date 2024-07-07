using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/PlayerRuntimeData/LastSafePositionData")]
public class LastSafePositionData : ScriptableObject
{
    public float updateInterval = 1.0f;
}
