using UnityEngine;
using UnityEditor;
using System.Collections;
using Invector;

[CanEditMultipleObjects]
[CustomEditor(typeof(Invector.CharacterController.vRagdoll),true)]
public class vRagdollEditor : Editor
{
    GUISkin skin;

    public override void OnInspectorGUI()
    {
        if (!skin) skin = Resources.Load("skin") as GUISkin;
        GUI.skin = skin;

        GUILayout.BeginVertical("Ragdoll System by Invector", "window");

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical();       
        base.OnInspectorGUI();

        EditorGUILayout.HelpBox("Every gameobject children of the character must have their tag added in the IgnoreTag List.", MessageType.Info);
        GUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }
}