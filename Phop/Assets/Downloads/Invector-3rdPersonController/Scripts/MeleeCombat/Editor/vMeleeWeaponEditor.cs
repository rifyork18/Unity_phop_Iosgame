using UnityEngine;
using System.Collections;
using UnityEditor;
using Invector;

[CanEditMultipleObjects]
[CustomEditor(typeof(vMeleeWeapon), true)]
public class vMeleeWeaponEditor : Editor
{
    GUISkin skin;

    void OnSceneGUI()
    {
        vMeleeWeapon weapon = (vMeleeWeapon)target;
        DrawShieldHandle(weapon);
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    static void DrawGizmos(Transform aTarget, GizmoType aGizmoType)
    {
        var weapon = aTarget.GetComponent<vMeleeWeapon>();
        if (weapon != null && (weapon.meleeType == vMeleeWeapon.MeleeType.Attack || weapon.meleeType == vMeleeWeapon.MeleeType.All))
        {
            var meleeManager = aTarget.GetComponentInParent<vMeleeManager>();
            if (meleeManager != null && meleeManager.gameObject == Selection.activeGameObject || weapon.gameObject == Selection.activeGameObject)
                DrawWeaponHandler(weapon);
        }
        else if (weapon != null && weapon.meleeType == vMeleeWeapon.MeleeType.Defense && weapon.showHitboxes)
        {
            weapon.showHitboxes = false;
            weapon.top.gameObject.hideFlags = HideFlags.HideInHierarchy;
            weapon.center.gameObject.hideFlags = HideFlags.HideInHierarchy;
            weapon.bottom.gameObject.hideFlags = HideFlags.HideInHierarchy;
        }
    }

    void DrawShieldHandle(vMeleeWeapon weapon)
    {
        var meleeManager = weapon.gameObject.GetComponentInParent<vMeleeManager>();
        if (weapon.top != null)
        {
            if (weapon.top.gameObject.activeSelf && weapon.meleeType == vMeleeWeapon.MeleeType.Defense)
                weapon.top.gameObject.SetActive(false);
        }
        if (weapon.center != null)
        {
            if (weapon.center.gameObject.activeSelf && weapon.meleeType == vMeleeWeapon.MeleeType.Defense)
                weapon.center.gameObject.SetActive(false);
        }
        if (weapon.bottom != null)
        {
            if (weapon.bottom.gameObject.activeSelf && weapon.meleeType == vMeleeWeapon.MeleeType.Defense)
                weapon.bottom.gameObject.SetActive(false);
        }
        if (weapon != null && meleeManager != null && (weapon.meleeType == vMeleeWeapon.MeleeType.Defense || weapon.meleeType == vMeleeWeapon.MeleeType.All))
        {
            var coll = meleeManager.gameObject.GetComponent<Collider>();
            if (coll)
            {
                Handles.DrawWireDisc(coll.bounds.center, Vector3.up, .5f);
                Handles.color = new Color(1, 0, 0, 0.2f);
                Handles.DrawSolidArc((Vector3)coll.bounds.center, Vector3.up, (Vector3)meleeManager.transform.forward, (float)weapon.defenseRange, .5f);
                Handles.DrawSolidArc((Vector3)coll.bounds.center, Vector3.up, (Vector3)meleeManager.transform.forward, (float)-weapon.defenseRange, .5f);
                Handles.color = new Color(1, 1, 1, 0.5f);
                Handles.DrawSolidDisc(coll.bounds.center, Vector3.up, .3f);
            }
        }
    }

    static void DrawWeaponHandler(vMeleeWeapon weapon)
    {
        try
        {
            var parent = weapon.transform.parent;
            if (parent != null)
            {
                if (weapon.top != null)
                {
                    if (!weapon.top.gameObject.activeSelf)
                        weapon.top.gameObject.SetActive(true);
                    weapon.top.gameObject.tag = weapon.tag;
                    weapon.top.gameObject.layer = weapon.gameObject.layer;
                }

                if (weapon.center != null)
                {
                    if (!weapon.center.gameObject.activeSelf)
                        weapon.center.gameObject.SetActive(true);
                    weapon.center.gameObject.tag = weapon.tag;
                    weapon.center.gameObject.layer = weapon.gameObject.layer;
                }

                if (weapon.bottom != null)
                {
                    if (!weapon.bottom.gameObject.activeSelf)
                        weapon.bottom.gameObject.SetActive(true);
                    weapon.bottom.gameObject.tag = weapon.tag;
                    weapon.bottom.gameObject.layer = weapon.gameObject.layer;
                }
            }

            var curCenterSize = 0f;
            if ((Mathf.Abs(weapon.centerPos) + weapon.centerSize) > 2.9f)
                curCenterSize = weapon.centerSize - (Mathf.Abs(weapon.centerPos * 2f));
            else
                curCenterSize = weapon.centerSize;

            var boxSize = weapon.top.BoxSize();
            Gizmos.color = new Color(0, 1, 0, .5f);
            var resultSize = new Vector3(boxSize.x, boxSize.y, boxSize.z);
            var resultPosition = weapon.top.GetBoxPoint().center;
            var matrix = Matrix4x4.TRS(resultPosition, weapon.top.transform.rotation, resultSize);
            Gizmos.matrix = matrix;
            Gizmos.DrawCube(Vector3.zero, new Vector3(1, 1, 1));

            boxSize = weapon.center.BoxSize();
            Gizmos.color = new Color(1, 1, 0, 0.5f);
            resultSize = new Vector3(boxSize.x, boxSize.y, boxSize.z);
            resultPosition = weapon.center.GetBoxPoint().center;
            matrix = Matrix4x4.TRS(resultPosition, weapon.center.transform.rotation, resultSize);
            Gizmos.matrix = matrix;
            Gizmos.DrawCube(Vector3.zero, new Vector3(1, 1, 1));

            boxSize = weapon.bottom.BoxSize();
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            resultSize = new Vector3(boxSize.x, boxSize.y, boxSize.z);
            resultPosition = weapon.bottom.GetBoxPoint().center;
            matrix = Matrix4x4.TRS(resultPosition, weapon.bottom.transform.rotation, resultSize);
            Gizmos.matrix = matrix;
            Gizmos.DrawCube(Vector3.zero, new Vector3(1, 1, 1));

            weapon.top.gameObject.hideFlags = weapon.showHitboxes ? HideFlags.None : HideFlags.HideInHierarchy;
            weapon.center.gameObject.hideFlags = weapon.showHitboxes ? HideFlags.None : HideFlags.HideInHierarchy;
            weapon.bottom.gameObject.hideFlags = weapon.showHitboxes ? HideFlags.None : HideFlags.HideInHierarchy;

            if (weapon.lockHitBox)
            {
                weapon.top.transform.localPosition = new Vector3(0, 1.5f, 0);
                weapon.top.transform.localRotation = Quaternion.Euler(Vector3.zero);
                weapon.top.transform.localScale = new Vector3(1, ((3f * 0.5f) - (curCenterSize * 0.5f)) - weapon.centerPos, 1);
                weapon.top.size = Vector3.one;
                weapon.top.center = new Vector3(0, -0.5f, 0);

                weapon.center.transform.localPosition = new Vector3(0, weapon.centerPos, 0);
                weapon.center.transform.localRotation = Quaternion.Euler(Vector3.zero);

                weapon.center.transform.localScale = new Vector3(1, curCenterSize, 1);
                weapon.center.size = Vector3.one;
                weapon.center.center = Vector3.zero;

                weapon.bottom.transform.localPosition = new Vector3(0, -1.5f, 0);
                weapon.bottom.transform.localRotation = Quaternion.Euler(Vector3.zero);
                weapon.bottom.transform.localScale = new Vector3(1, ((3f * 0.5f) - (curCenterSize * 0.5f)) + weapon.centerPos, 1);
                weapon.bottom.size = Vector3.one;
                weapon.bottom.center = new Vector3(0, 0.5f, 0);
            }

            if (weapon.transform.childCount > 3)
            {
                for (int i = 0; i < weapon.transform.childCount; i++)
                {
                    if ((!weapon.transform.GetChild(i).Equals(weapon.top.transform)) &&
                       (!weapon.transform.GetChild(i).Equals(weapon.bottom.transform)) &&
                       (!weapon.transform.GetChild(i).Equals(weapon.center.transform)))
                    {
                        DestroyImmediate(weapon.transform.GetChild(i).gameObject);
                    }
                }
            }
        }
        catch
        {
            if (weapon.top == null || weapon.hitTop == null)
            {
                var _top = weapon.transform.FindChild("hitBox_Top");
                if (_top == null)
                {
                    _top = new GameObject("hitBox_Top").transform;
                    _top.parent = weapon.transform;
                }
                weapon.hitTop = _top.GetComponent<vHitBox>() == null ? _top.gameObject.AddComponent<vHitBox>() : _top.GetComponent<vHitBox>();
                weapon.top = _top.GetComponent<BoxCollider>();
            }
            if (weapon.center == null || weapon.hitCenter == null)
            {
                var _center = weapon.transform.FindChild("hitBox_Center");
                if (_center == null)
                {
                    _center = new GameObject("hitBox_Center").transform;
                    _center.parent = weapon.transform;
                }
                weapon.hitCenter = _center.GetComponent<vHitBox>() == null ? _center.gameObject.AddComponent<vHitBox>() : _center.GetComponent<vHitBox>();
                weapon.center = _center.GetComponent<BoxCollider>();
            }
            if (weapon.bottom == null || weapon.hitBottom == null)
            {
                var _botton = weapon.transform.FindChild("hitBox_Botton");
                if (_botton == null)
                {
                    _botton = new GameObject("hitBox_Botton").transform;
                    _botton.parent = weapon.transform;
                }
                weapon.hitBottom = _botton.GetComponent<vHitBox>() == null ? _botton.gameObject.AddComponent<vHitBox>() : _botton.GetComponent<vHitBox>();
                weapon.bottom = _botton.GetComponent<BoxCollider>();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        if (!skin) skin = Resources.Load("skin") as GUISkin;
        GUI.skin = skin;
      
		serializedObject.Update ();
        GUILayout.BeginVertical("Melee Weapon by Invector", "window");

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeType"),new GUIContent("Melee Type"));     
		switch ((vMeleeWeapon.MeleeType)System.Enum.GetValues(typeof(vMeleeWeapon.MeleeType)).GetValue(serializedObject.FindProperty("meleeType").enumValueIndex))
        {
            case vMeleeWeapon.MeleeType.Attack:
                DrawAttackPropperties();
                break;
            case vMeleeWeapon.MeleeType.Defense:
				serializedObject.FindProperty ("useTwoHand").boolValue = false;
                DrawDefensePropperties();
                break;
            case vMeleeWeapon.MeleeType.All:           
                if(!serializedObject.FindProperty("useTwoHand").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("handEquip"), new GUIContent("Hand Equip", "Set the Hand can use the Weapon"));
                else
                {
                    serializedObject.FindProperty("handEquip").enumValueIndex =(int) vMeleeWeapon.HandEquip.RightHand;                  
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("handEquip"), new GUIContent("Hand Equip", "Just Right Hand can use weapon with 'Use Two Hand' checked"));
                }

                DrawAttackPropperties();
                DrawDefensePropperties();
                break;
        }
        
        GUILayout.EndVertical();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
       
		serializedObject.ApplyModifiedProperties ();
    }

    void DrawAttackPropperties()
    {
        GUILayout.BeginVertical("Attack", "window");
        GUILayout.Space(25);
        GUILayout.BeginVertical("box");
        // Weapon Settings
        GUILayout.Box("Weapon Settings", GUILayout.ExpandWidth(true));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("useTwoHand"),new GUIContent("Use Two Hand", "Check this if your moveset or atk_id use animations with 2 hand, the character will drop the left weapon"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("ATK_ID"),new GUIContent("ATK ID"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("MoveSet_ID"),new GUIContent("MoveSet ID"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("staminaCost"),new GUIContent("StaminaCost"));  
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("a_staminaRecoveryDelay"),new GUIContent("Stamina Recovery Delay"));   
        GUILayout.Label("--- AI ONLY ---");
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("distanceToAttack"),new GUIContent("Distance to Attack", "Distance for the AI to start attacking"));
        //Damage
        GUILayout.Box("Damage", GUILayout.ExpandWidth(true));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("damage").FindPropertyRelative("value"),new GUIContent("Total Damage"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("damagePercentage").FindPropertyRelative("Top"),new GUIContent("Top Hitbox %"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("damagePercentage").FindPropertyRelative("Center"),new GUIContent("Center Hitbox %"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("damage").FindPropertyRelative("ignoreDefense"),new GUIContent("Ignore Defense", "Check this to ignore if the enemy is blocking the attack, no recoil animation will trigger"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("damage").FindPropertyRelative ("activeRagdoll"), new GUIContent ("Active Ragdoll", "Check this to activate the Enemy Ragdoll")); 
        // Hitbox Settings
        GUILayout.Box("Hitbox Settings", GUILayout.ExpandWidth(true));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("centerSize"),new GUIContent("Center Size"));  
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("centerPos"),new GUIContent("Center Position"));  
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("top"),new GUIContent("Hit Top"));  
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("center"),new GUIContent("Hit Center"));  
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("bottom"),new GUIContent("Hit Bottom"));  
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("showHitboxes"),new GUIContent("Show Hitboxes"));  
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("lockHitBox"),new GUIContent("Lock HitBox","Use this to enable free mode to edit hitboxes")); 
		// ATK Sounds
	    GUILayout.Box("Sound FX", GUILayout.ExpandWidth(true));	   
		if (serializedObject.FindProperty ("audioSource").objectReferenceValue != null) 
	    {
			var audioS = (serializedObject.FindProperty ("audioSource").objectReferenceValue as GameObject).GetComponent<AudioSource> ();
		    if (audioS == null)
			    EditorGUILayout.HelpBox ("this gameObject doesn't contains a AudioSource Component",MessageType.Error);
	    }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("audioSource"), new GUIContent("Audio Source", "Use this to instantiate a audio clip"));
        EditorGUILayout.PropertyField (serializedObject.FindProperty ("hitSounds"),true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty ("recoilSounds"), true);		
		EditorGUILayout.PropertyField(serializedObject.FindProperty ("recoilParticles"), true);
		GUILayout.EndVertical();
        GUILayout.EndVertical();
    }

	void DrawDefensePropperties()
    {
        GUILayout.BeginVertical("Defense", "window");
        GUILayout.Space(25);
        GUILayout.BeginVertical("box");
        GUILayout.Box("Defense Settings", GUILayout.ExpandWidth(true));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("DEF_ID"),new GUIContent("DEF ID"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("Recoil_ID"),new GUIContent("Recoil ID"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("defenseRate"),new GUIContent("Defense Rate"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("defenseRange"), new GUIContent("Defense Range"));
        EditorGUILayout.PropertyField (serializedObject.FindProperty ("d_staminaRecoveryDelay"),new GUIContent("Stamina Recovery Delay", "Break the attack of the opponent and trigger a recoil animation"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("breakAttack"),new GUIContent("Break Attack", "Trigger the Recoil Animation for those who attacked you, will NOT work if the weapon that attacks you is check with Ignore Defense"));
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("mirrorAnimation"),new GUIContent("Mirror Animation", "Mirror the def animation if equipped on the left hand"));
		//DEF Sounds
	    GUILayout.Box("Sound FX", GUILayout.ExpandWidth(true));	  
		if (serializedObject.FindProperty ("audioSource").objectReferenceValue != null) 
	    {
			var audioS = (serializedObject.FindProperty ("audioSource").objectReferenceValue as GameObject).GetComponent<AudioSource> ();
		    if (audioS == null)
			    EditorGUILayout.HelpBox ("this gameObject doesn't contains a AudioSource Component",MessageType.Error);
	    }
		bool enable = ((vMeleeWeapon.MeleeType)(System.Enum.GetValues (typeof(vMeleeWeapon.MeleeType)).GetValue (serializedObject.FindProperty ("meleeType").enumValueIndex))) == vMeleeWeapon.MeleeType.All;
		GUI.enabled = !enable;
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("audioSource"),new GUIContent("Audio Source","Use this to instantiate a audio clip"));
		GUI.enabled = true;
		EditorGUILayout.PropertyField(serializedObject.FindProperty("defSounds"), true);	   
        GUILayout.EndVertical();
        GUILayout.EndVertical();
    }

	Vector2 scrollList1,scrollList2;
}
