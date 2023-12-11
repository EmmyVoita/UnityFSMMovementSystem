using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraManagement : MonoBehaviour
{
    private PlayerDataManagement playerData;

    [Header("Cinemachine/Camera Settings")]
    public GameObject cinemachine_camera_target;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    public float top_clamp = 70.0f;
    public float bottom_clamp = -30.0f;
    private const float _threshold = 0.01f;
    public bool LockCameraPosition = false;
    public float CameraAngleOverride = 0.0f;



    public Vector3 camInputDir;
    public Vector3 prevCamInputDir;



    private GameObject main_camera;


    private void Awake()
    {
        playerData = GetComponent<PlayerDataManagement>();
        // Get the main camera
        if (main_camera == null)
            main_camera = GameObject.FindGameObjectWithTag("MainCamera");
    }
   
    void Start()
    {
        _cinemachineTargetYaw = cinemachine_camera_target.transform.rotation.eulerAngles.y;
        prevCamInputDir = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        // Get the camera's current rotation around the y-axis
        float cameraRotation = main_camera.transform.eulerAngles.y;
        // Convert the rotation to radians for Mathf.Sin and Mathf.Cos functions
        float cameraRotationRad = -cameraRotation * Mathf.Deg2Rad;
        // Calculate the combined direction vector
        camInputDir = new Vector3(playerData.inputDirection.x * Mathf.Cos(cameraRotationRad) - playerData.inputDirection.z * Mathf.Sin(cameraRotationRad), 0.0f, playerData.inputDirection.x * Mathf.Sin(cameraRotationRad) + playerData.inputDirection.z * Mathf.Cos(cameraRotationRad));  
    }

    private void LateUpdate()
    {
        CameraRotation();
        prevCamInputDir = camInputDir;
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (playerData.lookVector.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = playerData.GetIsCurrentDeviceMouse() ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += playerData.lookVector.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += playerData.lookVector.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottom_clamp, top_clamp);

        // Cinemachine will follow this target
        cinemachine_camera_target.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

}
