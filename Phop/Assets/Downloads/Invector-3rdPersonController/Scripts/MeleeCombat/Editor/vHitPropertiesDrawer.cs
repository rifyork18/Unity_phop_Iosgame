using UnityEngine;
using System.Collections;
using UnityEditor;
using Invector;

[CustomPropertyDrawer(typeof(HitProperties))]
[CanEditMultipleObjects]
public class vHitPropertiesDrawer : PropertyDrawer
{
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		var useRecoil = property.FindPropertyRelative ("useRecoil");
        var drawRecoilGizmos = property.FindPropertyRelative("drawRecoilGizmos");
        var recoilRange =  property.FindPropertyRelative ("recoilRange");		
		var hitRecoilLayer =  property.FindPropertyRelative ("hitRecoilLayer");
		var hitDamageTags = property.FindPropertyRelative ("hitDamageTags");	

		EditorGUILayout.BeginVertical ("window");
        GUILayout.Box ("Hit Properties",GUILayout.ExpandWidth(true));
        EditorGUILayout.HelpBox("Who you can hit and inflict damage", MessageType.Info);

        EditorGUILayout.PropertyField(hitDamageTags, true);
        EditorGUILayout.HelpBox("Trigger Recoil animation if you hit a wall", MessageType.Info);
        EditorGUILayout.PropertyField (useRecoil);
		if (useRecoil.boolValue)
        {
            EditorGUILayout.PropertyField(drawRecoilGizmos);
            EditorGUILayout.PropertyField (recoilRange);
            EditorGUILayout.PropertyField(hitRecoilLayer);
        }		
		
		EditorGUILayout.EndVertical ();
		//base.OnGUI (position, property, label);
	}
}

[CustomPropertyDrawer(typeof(v_AISphereSensor))]
public class SensorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUILayout.LabelField("--- Sensors ---", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(property);
        if(property.objectReferenceValue != null)
        {
            var _object = new SerializedObject(property.objectReferenceValue);
            var passiveToDamage = _object.FindProperty("passiveToDamage");
            var obstacleLayer = _object.FindProperty("obstacleLayer");
            var tagsToDetect = _object.FindProperty("tagsToDetect");
            var getFromDistance = _object.FindProperty("getFromDistance");

            EditorGUILayout.PropertyField(passiveToDamage);
            EditorGUILayout.PropertyField(getFromDistance);
            EditorGUILayout.PropertyField(obstacleLayer);
            EditorGUILayout.PropertyField(tagsToDetect, true);
            
            if(GUI.changed)
            {
                _object.ApplyModifiedProperties();
            }            
        }
    }
}
//[CustomPropertyDrawer(typeof(HitEffect))]
//public class HitEffectDrawer : PropertyDrawer {
//
//	// Use this for initialization
//	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
//	{
//		try{
//			GUILayout.Box("TESTE");
//		}catch{
//		}
//
//
////		var Name = property.FindPropertyRelative ("Name");
////		var hitPrefab =property.FindPropertyRelative("hitPrefab");
////		EditorGUILayout.BeginVertical ();
////
////
////		//EditorGUILayout.PropertyField (hitPrefab);
////		EditorGUILayout.EndVertical ();
//
//	}
//}
