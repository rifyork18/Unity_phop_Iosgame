using UnityEngine;
using UnityEditor;
using System.Collections;
using Invector;

[CanEditMultipleObjects]
[CustomEditor(typeof(vCollectableMelee), true)]
public class vCollectableMeleeEditor : Editor
{
    GUISkin skin;

    public override void OnInspectorGUI()
    {
        if (!skin) skin = Resources.Load("skin") as GUISkin;
        GUI.skin = skin;

        vCollectableMelee collectableWeapon = (vCollectableMelee)target;       

        GUILayout.BeginVertical("Collectable Weapon by Invector", "window");

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (collectableWeapon.gameObject.layer == 0)
        {
            EditorGUILayout.HelpBox("Please assign the Layer to Ignore Raycast", MessageType.Warning);
        }

        EditorGUILayout.BeginVertical();

        base.OnInspectorGUI();

        //EditorGUILayout.HelpBox("You can create weapon handles (empty gameobject) for each weapon inside the RightHand or LeftForeArm bones.", MessageType.Info);
        GUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }
}