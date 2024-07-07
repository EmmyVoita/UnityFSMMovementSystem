using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;


public class PlayerCameraManagement : MonoBehaviour
{
    private const float _threshold = 0.01f;



    [TabGroup("tab1","General", TextColor = "green")] public GameObject cinemachine_camera_target;
    [TabGroup("tab1/General/SubTabGroup","DynamicZoom", TextColor = "green")] [ReadOnly] public float zoomFactor;
    [TabGroup("tab1/General/SubTabGroup","DynamicZoom")] public float zoom_maxSpeed = 20f;
    [TabGroup("tab1/General/SubTabGroup","DynamicZoom")] public float minZoom = 3.0f;
    [TabGroup("tab1/General/SubTabGroup","DynamicZoom")] public float maxZoom = 10.0f;
    [TabGroup("tab1/General/SubTabGroup","DynamicZoom")] [SerializeField] float speedThreshold = 5f;
    [TabGroup("tab1/General/SubTabGroup","DynamicZoom")] [SerializeField] float zoomSpeed = 0.1f; // Adjust this value to control the speed of the zoom
    [TabGroup("tab1/General/SubTabGroup","DynamicZoom")] [SerializeField] float verticalSpeedZoomFactor = 0.2f;
    [TabGroup("tab1/General/SubTabGroup","DynamicZoom")] [SerializeField] CinemachineVirtualCamera virtualCam;


    [TabGroup("tab1/General/SubTabGroup","CameraClamp", TextColor = "blue")] [SerializeField] float top_clamp = 70.0f;
    [TabGroup("tab1/General/SubTabGroup","CameraClamp")] [SerializeField] float bottom_clamp = -30.0f;
    [TabGroup("tab1/General/SubTabGroup","CameraClamp")] [SerializeField] public float CameraAngleOverride = 0.0f;

    [TabGroup("tab1/General/SubTabGroup","CameraState", TextColor = "purple")] [SerializeField] bool LockCameraPosition = false;

    
    [TabGroup("tab1/General/SubTabGroup","CameraDamping")] [SerializeField] private float defaultM_YDamping = 0.3f;
    [TabGroup("tab1/General/SubTabGroup","CameraDamping")] [SerializeField] private float freeFallM_YDamping = 0.1f;
    [TabGroup("tab1/General/SubTabGroup","CameraDamping")] [SerializeField] private float dampingChangeSpeed = 1.0f;


    [TabGroup("tab1","Dynamic", TextColor = "blue")] [ReadOnly] public Vector3 prevCamInputDir;
    [TabGroup("tab1","Dynamic")] [ReadOnly] public Vector3 camInputDir;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] PlayerDataManagement playerData;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] GameObject main_camera;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] float _cinemachineTargetYaw;
    [TabGroup("tab1","Dynamic")] [SerializeField] [ReadOnly] float _cinemachineTargetPitch;
    private Coroutine modifyYDampingCoroutine;




    


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


       
            
        
        //I am using virtual camera but it should work with regular one as well.
        //virtualCam.m_Lens.OrthographicSize = Mathf.Lerp(minZoom, maxZoom, zoomFactor);
    }

    void FixedUpdate()
    {
        
        //zoomFactor = playerData.characterController.velocity.magnitude / zoom_maxSpeed;
        zoomFactor = (playerData.currentHorizontalSpeed / zoom_maxSpeed) + (playerData.characterController.velocity.y /zoomSpeed) * verticalSpeedZoomFactor;
        

        var componentBase = virtualCam.GetCinemachineComponent(CinemachineCore.Stage.Body);
        if (componentBase is Cinemachine3rdPersonFollow)
        {
            var thirdPersonFollow = componentBase as Cinemachine3rdPersonFollow;
            if (playerData.characterController.velocity.magnitude > speedThreshold)
            {
                float targetZoom = Mathf.Lerp(minZoom, maxZoom, zoomFactor);
                thirdPersonFollow.CameraDistance = Mathf.Lerp(thirdPersonFollow.CameraDistance, targetZoom, zoomSpeed * Time.fixedDeltaTime);
            }
            else
            {
                thirdPersonFollow.CameraDistance = Mathf.Lerp(thirdPersonFollow.CameraDistance, minZoom, zoomSpeed * Time.fixedDeltaTime);
            }
        }
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

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            //Debug.Log("Application is focussed");
        }
        else
        {
            //Debug.Log("Application lost focus");
        }
    }

    public void OnPlayerEnterKillBox()
    {
        if (modifyYDampingCoroutine != null)
        {
            StopCoroutine(modifyYDampingCoroutine);
        }
        StartCoroutine(HandleOnEnterKillBox());
    }

    private IEnumerator HandleOnEnterKillBox()
    {
        modifyYDampingCoroutine = StartCoroutine(ModifyYDamping(freeFallM_YDamping));
        yield return new WaitForSeconds(1.0f);
        LockCameraPosition = true;
    }

    public void OnPlayerTeleportFromKillBox()
    {
        if (modifyYDampingCoroutine != null)
        {
            StopCoroutine(modifyYDampingCoroutine);
        }
        modifyYDampingCoroutine = StartCoroutine(ModifyYDamping(defaultM_YDamping));
        LockCameraPosition = false;
    }


    private IEnumerator ModifyYDamping(float targetValue)
    {
        // Get the CinemachineTransposer component
        Debug.Log("virtualCam: " + virtualCam);
        Cinemachine3rdPersonFollow thirdPersonFollow = virtualCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        Debug.Log("thirdPersonFollow: " + thirdPersonFollow);

        if (thirdPersonFollow != null)
        {
            // While the current damping value is not equal to the target value
            while (!Mathf.Approximately(thirdPersonFollow.Damping.y, targetValue))
            {
                // Lerp the current damping value towards the target value
                //transposer.m_YDamping = Mathf.Lerp(transposer.m_YDamping, targetValue, dampingChangeSpeed * Time.deltaTime);
                thirdPersonFollow.Damping.y = Mathf.Lerp(thirdPersonFollow.Damping.y, targetValue, dampingChangeSpeed * Time.deltaTime);

                // Yield control to the next frame
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("CinemachineTransposer component not found on virtual camera");
        }
    }

}
