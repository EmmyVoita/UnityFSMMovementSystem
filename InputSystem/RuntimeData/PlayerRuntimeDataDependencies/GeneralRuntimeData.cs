using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;


namespace PlayerData
{
    [System.Serializable]
    public class GeneralRuntimeData
    {
        [ReadOnly] public GameObject movementManager;
        [ReadOnly] public GameObject playerRoot;
        [ReadOnly] public GameObject playerTargetRoot;
        [ReadOnly] public CharacterController characterController;
        [ReadOnly] public PlayerCameraManagement playerCameraData;
        [ReadOnly] public GameObject mainCamera;
        [ReadOnly] public GameObject rotationTransform;


        public  GeneralRuntimeData(GameObject movementManager, GameObject playerRoot, GameObject playerTargetRoot, string rotationTransformName)
        {
            this.movementManager = movementManager;
            this.playerRoot = playerRoot;
            this.playerTargetRoot = playerTargetRoot;
            this.characterController = playerRoot.GetComponent<CharacterController>();
            this.playerCameraData = movementManager.GetComponent<PlayerCameraManagement>();
            this.mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            this.rotationTransform = FindTransformInHierarchy(playerRoot.transform, rotationTransformName).gameObject;
        }

        public static Transform FindTransformInHierarchy(Transform parent, string name)
        {
            // Check if the current transform's name matches the target name
            if (parent.name == name)
            {
                return parent;
            }

            // Check all children
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform result = FindTransformInHierarchy(parent.GetChild(i), name);

                if (result != null)
                {
                    return result;
                }
            }

            // If the transform was not found in this hierarchy, return null
            return null;
        }
    }
}