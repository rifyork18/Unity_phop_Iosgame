using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class v3rdPersonCameraListData : ScriptableObject 
{
	[SerializeField] public string Name;
	[SerializeField] public List<v3rdPersonCameraState> tpCameraStates;

	public v3rdPersonCameraListData()
	{
		tpCameraStates = new List<v3rdPersonCameraState> ();
		tpCameraStates.Add (new v3rdPersonCameraState ("Default"));
	}
}
