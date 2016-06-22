using UnityEngine;
using System.Collections;

public class vDraggableBox : MonoBehaviour
{    
	public float cameraSmoothRotation = 1f;
	public Rigidbody _rigidbody;
	public Transform LeftHandPoint;
	public Transform RightHandPoint;
	
	public vBoxTrigger dragBoxTrigger;//dragBoxTriggerNextPosition;
	[Tooltip("Max Distance of DragBoxTrigger  movement ")]
	public float dragTriggerMaxDist = 0.1f;
	[HideInInspector]
	public bool inDrag;
	
	void Start()
	{
		var colliders = GetComponentsInChildren<Collider>();  
		if(dragBoxTrigger == null )
		{
			Debug.Log("Missing DragBoxTrigger of " + gameObject.name);
			#if UNITY_EDITOR
			UnityEditor.Selection.activeObject = this.gameObject;
			#endif
			this.gameObject.SetActive(false);
			return;
		}     
		var triggerCollider = dragBoxTrigger.GetComponent<Collider>();
		
		foreach(Collider coll  in colliders)
		{
			Physics.IgnoreCollision(triggerCollider, coll);
		}  
		_rigidbody.isKinematic = true;
	}
	
	
	
	void Update()
	{
		if (!inDrag && _rigidbody.IsSleeping()&& !_rigidbody.isKinematic)
		{
			_rigidbody.isKinematic = true;
		}
	}
	
	public void SetInDrag(bool value)
	{
		inDrag = value;
		if (value) _rigidbody.isKinematic = false;
	}
	
	public bool CanMoveToDirection(Transform _transform,Vector3 direction)
	{
		// var direction = (Quaternion.Euler(_transform.eulerAngles) * new Vector3(input.x, 0, input.y));      
		var ray = new Ray(transform.position, direction);
		Debug.DrawRay(ray.origin, ray.direction, Color.red);
		var point = ray.GetPoint(dragTriggerMaxDist);
		if (direction.magnitude > 0)
			dragBoxTrigger.transform.position = point;
		else
			dragBoxTrigger.transform.localPosition = Vector3.zero;
		return !dragBoxTrigger.inCollision;
	}

}
