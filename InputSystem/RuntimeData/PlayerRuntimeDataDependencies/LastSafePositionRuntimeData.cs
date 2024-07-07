using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PlayerData
{
        
    [System.Serializable]
    public class LastSafePositionRuntimeData 
    {
        LastSafePositionData lastSafePositionData;
        PlayerDataManagement playerDataManagement;

        public Vector3 safePosition;
        //private CharacterController characterController;

        // Time interval for updating the safe position (in seconds)
       
        private float timeSinceLastUpdate = 0.0f;

        public LastSafePositionRuntimeData(LastSafePositionData lastSafePositionData, PlayerDataManagement playerDataManagement)
        {
            safePosition = Vector3.zero;
            this.lastSafePositionData = lastSafePositionData;
            this.playerDataManagement = playerDataManagement;
        }


        public void Ontart()
        {
            //characterController = GetComponent<CharacterController>();
            safePosition = playerDataManagement.playerRuntimeData.generalData.playerRoot.transform.position; 
        }

        public void OnUpdate()
        {
            timeSinceLastUpdate += Time.deltaTime;

            // Check if it's time to update the safe position
            if (timeSinceLastUpdate >= lastSafePositionData.updateInterval)
            {
                // Check if the player is on the ground
                if (playerDataManagement.groundCheck.playerGrounded)
                {
                    // If the player is on the ground, update the safe position
                    safePosition = playerDataManagement.playerRuntimeData.generalData.playerRoot.transform.position; 
                }

                // Reset the timer
                timeSinceLastUpdate = 0.0f;
            }
        }
    }
}