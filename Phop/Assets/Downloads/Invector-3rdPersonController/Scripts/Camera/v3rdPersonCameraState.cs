using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class v3rdPersonCameraState
{
	public string Name;
	public float forward;
	public float right;
    public float defaultDistance;
	public float maxDistance;
	public float minDistance;
	public float height;
    public float smoothFollow;
    public float yMinLimit;
    public float yMaxLimit;
    public float xMinLimit;
    public float xMaxLimit;
    public Vector3 rotationOffSet;
    public float cullingHeight;
    public float cullingMinDist;
    public bool useZoom;   
    public Vector2 fixedAngle;
    public List<LookPoint> lookPoints;
    public TPCameraMode cameraMode;
	public v3rdPersonCameraState(string name)
	{
		this.Name = name;
		this.forward = -1f;
		this.right = 0.35f;
        this.defaultDistance = 1.5f;
		this.maxDistance = 3f;
		this.minDistance = 0.5f;
		this.height = 1.5f;
        this.smoothFollow = 10f;
        this.yMinLimit = -40f;
        this.yMaxLimit = 80f;
        this.xMinLimit = -360f;
        this.xMaxLimit = 360f;
        this.cullingHeight = 1f;
        this.cullingMinDist = 0.1f;
        this.useZoom = false;       
        this.fixedAngle = Vector2.zero;       
        this.cameraMode = TPCameraMode.FreeDirectional;
	}
}
[System.Serializable]
public class LookPoint
{
    public string pointName;
    public Vector3 positionPoint;
    public Vector3 eulerAngle;
    public bool freeRotation;
}
public enum TPCameraMode
{
    FreeDirectional,
    FixedAngle,
    FixedPoint
}