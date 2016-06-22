using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;
using Invector;

public class v3rdPersonCamera : MonoBehaviour
{
    private static v3rdPersonCamera _instance;
    public static v3rdPersonCamera instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<v3rdPersonCamera>();

                //Tell unity not to destroy this object when loading a new scene!
                //DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    #region inspector properties    
    public Transform target;
    public float xMouseSensitivity = 3f;
    public float yMouseSensitivity = 3f;
    [Tooltip("Lerp speed between Camera States")]
    public float smoothBetweenState = 0.05f;
    public float smoothCameraRotation = 12f;
    public float scrollSpeed = 10f;

    [Tooltip("What layer will be culled")]
    public LayerMask cullingLayer = 1 << 0;
    [Tooltip("Change this value If the camera pass through the wall")]
    public float clipPlaneMargin;
    public bool showGizmos;
    [Tooltip("Debug purposes, lock the camera behind the character for better align the states")]
    public bool lockCamera;
    #endregion

    #region hide properties    
    [HideInInspector]
    public int indexList, indexLookPoint;
    [HideInInspector]
    public float offSetPlayerPivot;
    [HideInInspector]
    public string currentStateName;
    [HideInInspector]
    public Transform currentTarget;
    [HideInInspector]
    public v3rdPersonCameraState currentState;
    [HideInInspector]
    public v3rdPersonCameraListData CameraStateList;
    [HideInInspector]
    public Transform lockTarget;
    private v3rdPersonCameraState lerpState;
    private Transform targetLookAt;
    private Vector3 currentTargetPos;
    private Vector3 lookPoint;
    private Vector3 cPos;
    private Vector3 oldTargetPos;
    private Camera _camera;
    private float distance = 5f;
    private float mouseY = 0f;
    private float mouseX = 0f;
    private float targetHeight;
    private float currentZoom;
    private float desiredDistance;
    private float oldDistance;
    private bool useSmooth;

    #endregion

    void Start()
    {
        Init();
    }

    public void Init()
    {
        //Cursor.visible = false;
        if (target == null)
            return;

        _camera = GetComponent<Camera>();
        currentTarget = target;
        currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);
        targetLookAt = new GameObject("targetLookAt").transform;
        targetLookAt.position = currentTarget.position;
        targetLookAt.hideFlags = HideFlags.HideInHierarchy;
        targetLookAt.rotation = currentTarget.rotation;
        // initialize the first camera state
        mouseY = currentTarget.eulerAngles.x;
        mouseX = currentTarget.eulerAngles.y;

        ChangeState("Default", false);
        currentZoom = currentState.defaultDistance;
        distance = currentState.defaultDistance;
        targetHeight = currentState.height;
        useSmooth = true;
    }

    void FixedUpdate()
    {
        if (target == null || targetLookAt == null || currentState == null || lerpState == null) return;

        switch (currentState.cameraMode)
        {
            case TPCameraMode.FreeDirectional:
                CameraMovement();
                break;
            case TPCameraMode.FixedAngle:
                CameraMovement();
                break;
            case TPCameraMode.FixedPoint:
                CameraFixed();
                break;
        }
    }

    public void SetTargetLockOn(Transform target)
    {
        //if (lockTarget != null && target == null)
        //    currentTarget.SendMessage("LostTargetLockOn", SendMessageOptions.DontRequireReceiver);
        if (target != null)
            currentTarget.SendMessage("FindTargetLockOn", target, SendMessageOptions.DontRequireReceiver);
        lockTarget = target;
    }

    public void ClearTargetLockOn()
    {
        lockTarget = null;
        currentTarget.SendMessage("LostTargetLockOn", SendMessageOptions.DontRequireReceiver);
        var lockOn = GetComponent<vLockOnTargetControl>();
        if (lockOn != null)
            lockOn.StopLockOn();
    }

    /// <summary>
    /// Set the cursorObject for TopDown only
    /// </summary>
    /// <param name="New cursorObject"></param>
    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget ? newTarget : target;
        //mouseX = transform.eulerAngles.y;
        //mouseY = transform.eulerAngles.x;
    }

    /// <summary>    
    /// Convert a point in the screen in a Ray for the world
    /// </summary>
    /// <param name="Point"></param>
    /// <returns></returns>
    public Ray ScreenPointToRay(Vector3 Point)
    {
        return this.GetComponent<Camera>().ScreenPointToRay(Point);
    }

    /// <summary>
    /// Change CameraState
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="Use smoth"></param>
    public void ChangeState(string stateName, bool hasSmooth)
    {
        if (currentState != null && currentState.Name.Equals(stateName)) return;
        // search for the camera state string name
        var state = CameraStateList.tpCameraStates.Find(delegate (v3rdPersonCameraState obj) { return obj.Name.Equals(stateName); });

        if (state != null)
        {
            currentStateName = stateName;
            currentState.cameraMode = state.cameraMode;
            lerpState = state; // set the state of transition (lerpstate) to the state finded on the list
            // in case there is no smooth, a copy will be make without the transition values
            if (currentState != null && !hasSmooth)
                currentState.CopyState(state);
        }
        else
        {
            // if the state choosed if not real, the first state will be set up as default
            if (CameraStateList.tpCameraStates.Count > 0)
            {
                state = CameraStateList.tpCameraStates[0];
                currentStateName = state.Name;
                currentState.cameraMode = state.cameraMode;
                lerpState = state;
                if (currentState != null && !hasSmooth)
                    currentState.CopyState(state);
            }
        }
        // in case a list of states does not exist, a default state will be created
        if (currentState == null)
        {
            currentState = new v3rdPersonCameraState("Null");
            currentStateName = currentState.Name;
        }

        indexList = CameraStateList.tpCameraStates.IndexOf(state);
        currentZoom = state.defaultDistance;
        currentState.fixedAngle = new Vector3(mouseX, mouseY);
        useSmooth = hasSmooth;
        indexLookPoint = 0;
    }

    /// <summary>
    /// Change State using look at point if the cameraMode is FixedPoint  
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="pointName"></param>
    /// <param name="hasSmooth"></param>
    public void ChangeState(string stateName, string pointName, bool hasSmooth)
    {
        useSmooth = hasSmooth;
        if (!currentState.Name.Equals(stateName))
        {
            // search for the camera state string name
            var state = CameraStateList.tpCameraStates.Find(delegate (v3rdPersonCameraState obj)
           {
               return obj.Name.Equals(stateName);
           });

            if (state != null)
            {
                currentStateName = stateName;
                currentState.cameraMode = state.cameraMode;
                lerpState = state; // set the state of transition (lerpstate) to the state finded on the list
                                   // in case there is no smooth, a copy will be make without the transition values
                if (currentState != null && !hasSmooth)
                    currentState.CopyState(state);
            }
            else
            {
                // if the state choosed if not real, the first state will be set up as default
                if (CameraStateList.tpCameraStates.Count > 0)
                {
                    state = CameraStateList.tpCameraStates[0];
                    currentStateName = state.Name;
                    currentState.cameraMode = state.cameraMode;
                    lerpState = state;
                    if (currentState != null && !hasSmooth)
                        currentState.CopyState(state);
                }
            }
            // in case a list of states does not exist, a default state will be created
            if (currentState == null)
            {
                currentState = new v3rdPersonCameraState("Null");
                currentStateName = currentState.Name;
            }

            indexList = CameraStateList.tpCameraStates.IndexOf(state);
            currentZoom = state.defaultDistance;
            currentState.fixedAngle = new Vector3(mouseX, mouseY);
            indexLookPoint = 0;
        }

        if (currentState.cameraMode == TPCameraMode.FixedPoint)
        {
            var point = currentState.lookPoints.Find(delegate (LookPoint obj)
           {
               return obj.pointName.Equals(pointName);
           });
            if (point != null)
            {
                indexLookPoint = currentState.lookPoints.IndexOf(point);
            }
            else
            {
                indexLookPoint = 0;
            }
        }
    }

    /// <summary>
    /// Change the lookAtPoint of current state if cameraMode is FixedPoint
    /// </summary>
    /// <param name="pointName"></param>
    public void ChangePoint(string pointName)
    {
        if (currentState == null || currentState.cameraMode != TPCameraMode.FixedPoint || currentState.lookPoints == null) return;
        var point = currentState.lookPoints.Find(delegate (LookPoint obj) { return obj.pointName.Equals(pointName); });
        if (point != null) indexLookPoint = currentState.lookPoints.IndexOf(point); else indexLookPoint = 0;
    }

    /// <summary>    
    /// Zoom baheviour 
    /// </summary>
    /// <param name="scroolValue"></param>
    /// <param name="zoomSpeed"></param>
    public void Zoom(float scroolValue)
    {
        currentZoom -= scroolValue * scrollSpeed;
    }

    /// <summary>
    /// Camera Rotation behaviour
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void RotateCamera(float x, float y)
    {
        if (currentState.cameraMode.Equals(TPCameraMode.FixedPoint)) return;
        if (!currentState.cameraMode.Equals(TPCameraMode.FixedAngle))
        {
            // Rotação livre de camera
            if (lockTarget)
            {
                CalculeLockOnPoint();
            }
            else
            {
                // free rotation 
                mouseX += x * xMouseSensitivity;
                mouseY -= y * yMouseSensitivity;
                if (!lockCamera)
                {
                    mouseY = vExtensions.ClampAngle(mouseY, currentState.yMinLimit, currentState.yMaxLimit);
                    mouseX = vExtensions.ClampAngle(mouseX, currentState.xMinLimit, currentState.xMaxLimit);
                }
                else
                {
                    mouseY = currentTarget.root.localEulerAngles.x;
                    mouseX = currentTarget.root.localEulerAngles.y;
                }
            }
        }
        else
        {
            // fixed rotation
            mouseX = currentState.fixedAngle.x;
            mouseY = currentState.fixedAngle.y;
        }
    }

    void CalculeLockOnPoint()
    {
        if (currentState.cameraMode.Equals(TPCameraMode.FixedAngle) && lockTarget) return; // check if angle of camera is fixed         
        var collider = lockTarget.GetComponent<Collider>();                 // collider to get center of bounds

        if (collider == null)
        {
            // ClearTargetLockOn();
            //lockTarget = null;
            return;
        }

        var point = collider.bounds.center;
        Vector3 relativePos = point - (cPos);                   // get position relative to transform
        Quaternion rotation = Quaternion.LookRotation(relativePos);         // convert to rotation
        //convert angle (360 to 180)
        if (rotation.eulerAngles.x < -180)
            mouseY = rotation.eulerAngles.x + 360;
        else if (rotation.eulerAngles.x > 180)
            mouseY = rotation.eulerAngles.x - 360;
        else
            mouseY = rotation.eulerAngles.x;
        mouseX = rotation.eulerAngles.y;
    }

    /// <summary>
    /// Camera behaviour
    /// </summary>    
    void CameraMovement()
    {
        if (currentTarget == null)
            return;

        if (useSmooth)
            currentState.Slerp(lerpState, smoothBetweenState);
        else
            currentState.CopyState(lerpState);

        if (currentState.useZoom)
        {
            currentZoom = Mathf.Clamp(currentZoom, currentState.minDistance, currentState.maxDistance);
            distance = useSmooth ? Mathf.Lerp(distance, currentZoom, 2f * Time.fixedDeltaTime) : currentZoom;
        }
        else
        {
            distance = useSmooth ? Mathf.Lerp(distance, currentState.defaultDistance, 2f * Time.fixedDeltaTime) : currentState.defaultDistance;
            currentZoom = distance;
        }

        desiredDistance = distance;
        var camDir = (currentState.forward * targetLookAt.forward) + (currentState.right * targetLookAt.right);
        camDir = camDir.normalized;

        var targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);
        currentTargetPos = useSmooth ? Vector3.Lerp(currentTargetPos, targetPos, lerpState.smoothFollow * Time.fixedDeltaTime) : targetPos;
        cPos = currentTargetPos + new Vector3(0, targetHeight, 0);
        oldTargetPos = targetPos + new Vector3(0, currentState.height, 0);

        RaycastHit hitInfo;
        ClipPlanePoints planePoints = _camera.NearClipPlanePoints(cPos + (camDir * (distance)), clipPlaneMargin);
        ClipPlanePoints oldPoints = _camera.NearClipPlanePoints(oldTargetPos + (camDir * oldDistance), clipPlaneMargin);
        if (CullingRayCast(cPos, planePoints, out hitInfo, distance + 0.2f, cullingLayer)) distance = desiredDistance;

        if (CullingRayCast(oldTargetPos, oldPoints, out hitInfo, oldDistance + 0.2f, cullingLayer))
        {
            var t = distance - 0.2f;
            t -= currentState.cullingMinDist;
            t /= (distance - currentState.cullingMinDist);
            targetHeight = Mathf.Lerp(currentState.cullingHeight, currentState.height, Mathf.Clamp(t, 0.0f, 1.0f));
            cPos = currentTargetPos + new Vector3(0, targetHeight, 0);
        }
        else
        {
            oldDistance = useSmooth ? Mathf.Lerp(oldDistance, distance, 2f * Time.fixedDeltaTime) : distance;
            targetHeight = useSmooth ? Mathf.Lerp(targetHeight, currentState.height, 2f * Time.fixedDeltaTime) : currentState.height;
        }

        var lookPoint = cPos;
        lookPoint += (targetLookAt.right * Vector3.Dot(camDir * (distance), targetLookAt.right));
        targetLookAt.position = cPos;
        Quaternion newRot = Quaternion.Euler(mouseY, mouseX, 0);
        targetLookAt.rotation = useSmooth ? Quaternion.Slerp(targetLookAt.rotation, newRot, smoothCameraRotation * Time.fixedDeltaTime) : newRot;
        transform.position = cPos + (camDir * (distance));
        var rotation = Quaternion.LookRotation((lookPoint) - transform.position);
        if (lockTarget)
        {
            if (!(currentState.cameraMode.Equals(TPCameraMode.FixedAngle)))
            {
                var collider = lockTarget.GetComponent<Collider>();
                if (collider != null)
                {
                    var point = collider.bounds.center - transform.position;
                    var euler = Quaternion.LookRotation(point).eulerAngles - rotation.eulerAngles;
                    lookTargetOffSet = euler;
                }
            }
        }
        else
        {
            lookTargetOffSet = Vector3.Lerp(lookTargetOffSet, Vector3.zero, 1 * Time.fixedDeltaTime);
        }
        rotation.eulerAngles += currentState.rotationOffSet + lookTargetOffSet;
        transform.rotation = rotation;
    }

    Vector3 lookTargetOffSet;
    /// <summary>
    /// Update of FixedPoint mode
    /// </summary>
    void CameraFixed()
    {
        if (useSmooth) currentState.Slerp(lerpState, smoothBetweenState);
        else currentState.CopyState(lerpState);

        var targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot + currentState.height, currentTarget.position.z);
        currentTargetPos = useSmooth ? Vector3.MoveTowards(currentTargetPos, targetPos, currentState.smoothFollow * Time.fixedDeltaTime) : targetPos;
        cPos = currentTargetPos;
        var pos = isValidFixedPoint ? currentState.lookPoints[indexLookPoint].positionPoint : transform.position;
        transform.position = useSmooth ? Vector3.Lerp(transform.position, pos, currentState.smoothFollow * Time.fixedDeltaTime) : pos;
        targetLookAt.position = cPos;
        if (isValidFixedPoint && currentState.lookPoints[indexLookPoint].freeRotation)
        {
            var rot = Quaternion.Euler(currentState.lookPoints[indexLookPoint].eulerAngle);
            transform.rotation = useSmooth ? Quaternion.Slerp(transform.rotation, rot, (currentState.smoothFollow * 0.5f) * Time.fixedDeltaTime) : rot;
        }
        else if (isValidFixedPoint)
        {
            var rot = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = useSmooth ? Quaternion.Slerp(transform.rotation, rot, (currentState.smoothFollow) * Time.fixedDeltaTime) : rot;
        }
    }

    /// <summary>
    /// Check if current state is a valid FixedPoint
    /// </summary>
    bool isValidFixedPoint
    {
        get
        {
            return (currentState.lookPoints != null && currentState.cameraMode.Equals(TPCameraMode.FixedPoint) && (indexLookPoint < currentState.lookPoints.Count || currentState.lookPoints.Count > 0));
        }
    }

    /// <summary>
    /// Custom Raycast using NearClipPlanesPoints
    /// </summary>
    /// <param name="_to"></param>
    /// <param name="from"></param>
    /// <param name="hitInfo"></param>
    /// <param name="distance"></param>
    /// <param name="cullingLayer"></param>
    /// <returns></returns>
    bool CullingRayCast(Vector3 from, ClipPlanePoints _to, out RaycastHit hitInfo, float distance, LayerMask cullingLayer)
    {
        bool value = false;
        if (showGizmos)
        {
            Debug.DrawRay(from, _to.LowerLeft - from);
            Debug.DrawLine(_to.LowerLeft, _to.LowerRight);
            Debug.DrawLine(_to.UpperLeft, _to.UpperRight);
            Debug.DrawLine(_to.UpperLeft, _to.LowerLeft);
            Debug.DrawLine(_to.UpperRight, _to.LowerRight);
            Debug.DrawRay(from, _to.LowerRight - from);
            Debug.DrawRay(from, _to.UpperLeft - from);
            Debug.DrawRay(from, _to.UpperRight - from);
        }
        if (Physics.Raycast(from, _to.LowerLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            desiredDistance = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.LowerRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (desiredDistance > hitInfo.distance) desiredDistance = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.UpperLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (desiredDistance > hitInfo.distance) desiredDistance = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.UpperRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (desiredDistance > hitInfo.distance) desiredDistance = hitInfo.distance;
        }

        return value;
    }
}
