using UnityEngine;
using UnityEditor;
using System.Collections;
using Invector;

[CustomEditor(typeof(vLockOnTargetControl),true)]
public class vLockOnTargetControlEditor : Editor
{
    GUISkin skin;

    public override void OnInspectorGUI()
    {
        if (!skin) skin = Resources.Load("skin") as GUISkin;
        GUI.skin = skin;

        vLockOnTargetControl lockon = (vLockOnTargetControl)target;

        GUILayout.BeginVertical("Lock-on by Invector", "window");

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (lockon.layerOfObstacles == 0)
        {
            EditorGUILayout.HelpBox("Please assign the Layer of Obstacles to 'Default' ", MessageType.Warning);
        }

        EditorGUILayout.BeginVertical();       
        base.OnInspectorGUI();        
        GUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }
}