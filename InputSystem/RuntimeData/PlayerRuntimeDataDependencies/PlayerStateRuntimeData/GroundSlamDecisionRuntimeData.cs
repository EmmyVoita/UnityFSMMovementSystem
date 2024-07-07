using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using System.Linq;

namespace PlayerData
{
    [System.Serializable]
    public class GroundSlamDecisionRuntimeData
    { 
        [SerializeField] [ReadOnly] private GroundSlamDecisionData GSDData;
        [SerializeField] [ReadOnly] public float chargeAmount;
        
        [SerializeField] private List<float> velocitySamplesHorizontal;
        [SerializeField] private List<float> velocitySamplesVertical;
        private Coroutine velocitySamplingCoroutine;
        private Coroutine chargeCoroutine;


        public GroundSlamDecisionRuntimeData(GroundSlamDecisionData groundSlamDecisionData)
        {
            this.GSDData = groundSlamDecisionData;
            this.chargeAmount = 0.0f;
        }

        public void ResetChargeAmount()
        {
            chargeAmount = 0.0f;
        }

        public void StartChargeGeneration(PlayerRuntimeData data)
        {
            // If a charge coroutine is already running, stop it
            if (chargeCoroutine != null)
            {
                CoroutineStarter.Instance.StopCoroutine(chargeCoroutine);
            }

            // Start a new charge coroutine
            chargeCoroutine = CoroutineStarter.Instance.StartCoroutine(ChargeGenerationCoroutine(data, GSDData.passiveChargeRate));
        }

        public void StopChargeGeneration()
        {
            // If a charge coroutine is running, stop it
            if (chargeCoroutine != null)
            {
                CoroutineStarter.Instance.StopCoroutine(chargeCoroutine);
                chargeCoroutine = null;
            }
        }

        private IEnumerator ChargeGenerationCoroutine(PlayerRuntimeData data, float chargeRate)
        {
            while (true)
            {
                // Increase the charge amount
                chargeAmount += chargeRate * Time.deltaTime;

                // Clamp the charge amount between 0 and 100
                chargeAmount = Mathf.Clamp(chargeAmount, 0, 100);

                yield return null;
            }
        }


        public void StartVelocitySampling(PlayerRuntimeData data)
        {
            // If a velocity sampling coroutine is already running, stop it
            if (velocitySamplingCoroutine != null)
            {
                CoroutineStarter.Instance.StopCoroutine(velocitySamplingCoroutine);
                velocitySamplesHorizontal = null;
                velocitySamplesVertical = null;
            }

            // Initialize the velocity samples list
            velocitySamplesHorizontal = new List<float>();
            velocitySamplesVertical = new List<float>();

            // Start a new velocity sampling coroutine
            velocitySamplingCoroutine = CoroutineStarter.Instance.StartCoroutine(VelocitySamplingCoroutine(data));
        }

        public void StopVelocitySampling()
        {
            // If a velocity sampling coroutine is running, stop it
            if (velocitySamplingCoroutine != null)
            {
                CoroutineStarter.Instance.StopCoroutine(velocitySamplingCoroutine);
                velocitySamplingCoroutine = null;
            }

            // Calculate the average velocity
            //float averageVelocity = velocitySamples.Average();

            // Reset the velocity samples list
           

            // Use the average velocity to calculate the charge amount
            //chargeAmount += averageVelocity;
        }

        private IEnumerator VelocitySamplingCoroutine(PlayerRuntimeData data)
        {
            while (true)
            {
                velocitySamplesHorizontal.Add(Mathf.Abs(data.playerData.currentHorizontalSpeed_Projected));
                velocitySamplesVertical.Add(Mathf.Abs(data.verticalVelocity));
                yield return new WaitForSeconds(0.1f);
            }
        }

        public void CalculateChargeAmount(PlayerRuntimeData data)
        {
            // Stop velocity sampling and calculate the average velocity
            StopVelocitySampling();

            // If there are no velocity samples, return
            if (velocitySamplesHorizontal == null || velocitySamplesHorizontal.Count == 0)
            {
                return;
            }

            // Calculate the average velocity
            float averageVelocityHorizontal = velocitySamplesHorizontal.Average();
            float averageVelocityVertical = velocitySamplesVertical.Average();

            // Calculate the weighted velocity
            Vector2 velocity = new Vector2(averageVelocityHorizontal * GSDData.horizontalWeight, averageVelocityVertical * GSDData.verticalWeight);
            float velocityMagnitude = velocity.magnitude;

            // Normalize the magnitude to be between 0 and 1
            float maxPossibleSpeed = Mathf.Sqrt((GSDData.HorizontalContribution * GSDData.horizontalWeight) * (GSDData.HorizontalContribution * GSDData.horizontalWeight) + (GSDData.VerticalContribution * GSDData.verticalWeight) * (GSDData.VerticalContribution * GSDData.verticalWeight));
            chargeAmount += Mathf.Clamp(velocityMagnitude / maxPossibleSpeed, 0, 1);
        }
    }
}
