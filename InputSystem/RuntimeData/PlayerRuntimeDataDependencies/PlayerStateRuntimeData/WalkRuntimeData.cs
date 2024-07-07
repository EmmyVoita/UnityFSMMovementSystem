using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlayerData
{
    public class WalkRuntimeData 
    {
        WalkActionData walkActionData;
        public WalkRuntimeData(WalkActionData walkActionData)
        {
            this.walkActionData = walkActionData;
        }

        public float playerSpeed = 5.0f;
        public float getPlayerSpeed() { return playerSpeed; }
        public void setPlayerSpeed(float speed) { playerSpeed = speed; }
    }
}