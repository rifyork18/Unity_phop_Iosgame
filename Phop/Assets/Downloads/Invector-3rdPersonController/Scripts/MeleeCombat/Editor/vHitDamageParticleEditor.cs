using UnityEngine;
using System.Collections;
using UnityEditor;
using Invector;

[CanEditMultipleObjects]
[CustomEditor(typeof(vHitDamageParticle), true)]
public class vHitDamageParticleEditor : Editor
{
    GUISkin skin;

    public override void OnInspectorGUI()
    {
        if (!skin) skin = Resources.Load("skin") as GUISkin;
        GUI.skin = skin;        
        GUILayout.BeginVertical("HitDamage Particle by Invector", "window");

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("Default hit Particle to instantiate every time you receive damage", MessageType.Info);

        base.OnInspectorGUI();

        EditorGUILayout.HelpBox("Custom hit Particle to instantiate based on a custom AttackName from a Attack Animation State", MessageType.Info);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
    }
}
