using UnityEngine;
using UnityEditor;
using System.Collections;
using Invector;

[CanEditMultipleObjects]
[CustomEditor(typeof(vMeleeManager))]
public class vMeleeManagerEditor : Editor
{
    GUISkin skin;
    GameObject handler;
    Animator animator;
    vMeleeManager meleeEquip;
    SerializedObject _meleeObject;
    Transform rightHand, leftHand, rightArm, leftArm, leftLeg, leftFoot, rightLeg, rightFoot;

    [MenuItem("3rd Person Controller/Component/Melee Equip Manager")]
    static void MenuComponent()
    {
        Selection.activeGameObject.AddComponent<vMeleeManager>();
    }

    void OnSceneGUI()
    {
        if (Selection.activeGameObject != null && PrefabUtility.GetPrefabType(Selection.activeGameObject) == PrefabType.Prefab || !Selection.activeGameObject.activeSelf)
            return;
        meleeEquip = (vMeleeManager)target;

        var coll = meleeEquip.gameObject.GetComponent<Collider>();
        if (coll != null && meleeEquip != null && meleeEquip.hitProperties != null && meleeEquip.hitProperties.useRecoil && meleeEquip.hitProperties.drawRecoilGizmos)
        {
            Handles.DrawWireDisc(coll.bounds.center, Vector3.up, 0.5f);
            Handles.color = new Color(1, 0, 0, 0.2f);
            Handles.DrawSolidArc((Vector3)coll.bounds.center, Vector3.up, meleeEquip.transform.forward, (float)meleeEquip.hitProperties.recoilRange, 0.5f);
            Handles.DrawSolidArc((Vector3)coll.bounds.center, Vector3.up, meleeEquip.transform.forward, (float)-meleeEquip.hitProperties.recoilRange, 0.5f);
        }
    }

    void OnEnable()
    {
        meleeEquip = (vMeleeManager)target;
        animator = meleeEquip.gameObject.GetComponent<Animator>();
        if (animator)
        {
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            rightArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            leftArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);

            leftLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            rightLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }
        CheckDefaultHitBox();
    }

    public override void OnInspectorGUI()
    {
        if (!skin) skin = Resources.Load("skin") as GUISkin;
        GUI.skin = skin;

        vMeleeManager meleeEquip = (vMeleeManager)target;
        _meleeObject = new SerializedObject(target);
		serializedObject.Update ();
        if (!meleeEquip || _meleeObject == null)
            return;

        GUILayout.BeginVertical("Melee Manager by Invector", "window");
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical();
        if (!AnimatorCheck()) return;

        DrawHitPropertiesInfo();
        DrawDefaultHitboxInfo();
        DrawMeleeWeaponInfo();

        GUILayout.EndVertical();
        EditorGUILayout.EndVertical();
        if (GUI.changed)
        {
            EditorUtility.SetDirty(meleeEquip);
			serializedObject.ApplyModifiedProperties();
        }
    }

    bool AnimatorCheck()
    {
        if (animator == null)
        {
            EditorGUILayout.HelpBox("Missing Animator Component", MessageType.Info);
            GUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            return false;
        }
        return true;
    }

    void DrawHitPropertiesInfo()
    {
		var hitProperties = serializedObject.FindProperty("hitProperties");
        EditorGUILayout.PropertyField(hitProperties);

        if (meleeEquip.hitProperties != null && meleeEquip.hitProperties.useRecoil && meleeEquip.hitProperties.hitRecoilLayer == 0)
            EditorGUILayout.HelpBox("Please assign the HitRecoilLayer to Default", MessageType.Warning);

        if (meleeEquip.hitProperties != null && meleeEquip.hitProperties.hitDamageTags != null)
        {
            if (meleeEquip.hitProperties.hitDamageTags.Contains(meleeEquip.gameObject.tag))
                EditorGUILayout.HelpBox("Please change your HitDamageTags inside the HitProperties, they cannot have the same tag as this gameObject.", MessageType.Error);
        }
    }

    void DrawDefaultHitboxInfo()
    {
        EditorGUILayout.BeginVertical("window");
        GUILayout.Box("Default Hitbox", GUILayout.ExpandWidth(true));
        EditorGUILayout.HelpBox("Use for hand to hand combat", MessageType.Info);
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("useDefaultHitbox"), new GUIContent ("Use Default Hitbox"));

		if (serializedObject.FindProperty("useDefaultHitbox").boolValue)
        {
			var leftArmHitbox = serializedObject.FindProperty("leftArmHitbox");
			var rightArmHitbox = serializedObject.FindProperty("rightArmHitbox");
			var leftLegHitbox = serializedObject.FindProperty("leftLegHitbox");
			var rightLegHitbox = serializedObject.FindProperty("rightLegHitbox");

            EditorGUILayout.BeginHorizontal();
            GUI.color = leftArmHitbox.objectReferenceValue == null ? Color.black : Color.white;
            if (GUILayout.Button(leftArmHitbox.objectReferenceValue == null ? leftArmHitbox.name + ": Null" : leftArmHitbox.name, EditorStyles.miniButtonMid) && leftArmHitbox.objectReferenceValue != null)
                Selection.activeObject = leftArmHitbox.objectReferenceValue;
            GUI.color = rightArmHitbox.objectReferenceValue == null ? Color.black : Color.white;
            if (GUILayout.Button(rightArmHitbox.objectReferenceValue == null ? rightArmHitbox.name + ": Null" : rightArmHitbox.name, EditorStyles.miniButtonMid) && rightArmHitbox.objectReferenceValue != null)
                Selection.activeObject = rightArmHitbox.objectReferenceValue;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(leftLegHitbox.objectReferenceValue == null ? leftLegHitbox.name + ": Null" : leftLegHitbox.name, EditorStyles.miniButtonMid) && leftLegHitbox.objectReferenceValue != null)
                Selection.activeObject = leftLegHitbox.objectReferenceValue;
            if (GUILayout.Button(rightLegHitbox.objectReferenceValue == null ? rightLegHitbox.name + ": Null" : rightLegHitbox.name, EditorStyles.miniButtonMid) && rightLegHitbox.objectReferenceValue != null)
                Selection.activeObject = rightLegHitbox.objectReferenceValue;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    void DrawMeleeWeaponInfo()
    {
        if (animator != null)
        {
            EditorGUILayout.BeginVertical("window");
            GUILayout.Box("Melee Weapon Handlers", GUILayout.ExpandWidth(true));
            EditorGUILayout.HelpBox("Create a new handler by clicking on the '+' button next to the bone you want, them assign the handler to the corresponding weapon.", MessageType.Info);

            EditorGUILayout.BeginVertical();

			var currentMeleeWeaponLA = serializedObject.FindProperty("currentMeleeWeaponLA");
			var currentMeleeWeaponRA = serializedObject.FindProperty("currentMeleeWeaponRA");
            GUI.enabled = currentMeleeWeaponLA.objectReferenceValue != null;
            if (GUILayout.Button(currentMeleeWeaponLA.objectReferenceValue == null ? currentMeleeWeaponLA.name + ": Null" : currentMeleeWeaponLA.name, EditorStyles.miniButtonMid) && currentMeleeWeaponLA.objectReferenceValue != null)
                Selection.activeObject = currentMeleeWeaponLA.objectReferenceValue;
            GUI.enabled = currentMeleeWeaponRA.objectReferenceValue != null;
            if (GUILayout.Button(currentMeleeWeaponRA.objectReferenceValue == null ? currentMeleeWeaponRA.name + ": Null" : currentMeleeWeaponRA.name, EditorStyles.miniButtonMid) && currentMeleeWeaponRA.objectReferenceValue != null)
                Selection.activeObject = currentMeleeWeaponRA.objectReferenceValue;

            GUI.enabled = true;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+"))
                AddHandler(rightArm);

            if (GUILayout.Button("Right Arm", GUILayout.MinWidth(100)))
                SelectHandler(rightArm);
            if (GUILayout.Button("Left Arm", GUILayout.MinWidth(100)))
                SelectHandler(leftArm);

            if (GUILayout.Button("+"))
                AddHandler(leftArm);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+"))
                AddHandler(rightHand);

            if (GUILayout.Button("Right Hand", GUILayout.MinWidth(100)))
                SelectHandler(rightHand);
            if (GUILayout.Button("Left Hand", GUILayout.MinWidth(100)))
                SelectHandler(leftHand);

            if (GUILayout.Button("+"))
                AddHandler(leftHand);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }

    void CheckDefaultHitBox()
    {
        if (meleeEquip.leftArmHitbox == null)
            CreateDefaultHitbox(ref meleeEquip.leftArmHitbox, leftHand, leftArm, 0.2f, false);
        if (meleeEquip.rightArmHitbox == null)
            CreateDefaultHitbox(ref meleeEquip.rightArmHitbox, rightHand, rightArm, 0.2f, false);
        if (meleeEquip.leftLegHitbox == null)
            CreateDefaultHitbox(ref meleeEquip.leftLegHitbox, leftLeg, leftFoot, 0f, true);
        if (meleeEquip.rightLegHitbox == null)
            CreateDefaultHitbox(ref meleeEquip.rightLegHitbox, rightLeg, rightFoot, 0f, true);
    }

    void CreateDefaultHitbox(ref vMeleeWeapon meleeWeapon, Transform hand, Transform arm, float height, bool invertRotation)
    {
        if (Selection.activeGameObject != null && PrefabUtility.GetPrefabType(Selection.activeGameObject) == PrefabType.Prefab || !Selection.activeGameObject.activeSelf)
            return;
        GameObject defaultHitbox = Resources.Load("defaultHitBox") as GameObject;
        if (defaultHitbox == null) return;

        GameObject def = Instantiate(defaultHitbox);
        meleeWeapon = def.GetComponent<vMeleeWeapon>();
        def.transform.position = hand.position;

        var scale = Vector3.Distance(hand.position, arm.position) * 0.5f;
        def.transform.localScale = new Vector3(scale, scale, scale);

        SetupTransform(def.transform, hand, arm, height, invertRotation);
        def.transform.parent = hand;
        def.tag = "Weapon";
        def.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    void SetupTransform(Transform target, Transform A, Transform B, float height, bool invertRotation)
    {
        var direction = A.position - B.position;
        target.eulerAngles = Vector3.zero;
        target.rotation = ((Quaternion.FromToRotation(target.up, direction) * Quaternion.LookRotation(meleeEquip.transform.forward)));

        if (invertRotation)
        {
            Vector3 up = target.InverseTransformDirection(-target.up);
            target.up = up;
        }

        var position = (A.position + B.position) * 0.5f + (target.up * height);
        target.position = position;
    }

    void AddHandler(Transform bone)
    {
        var customHandlers = bone.FindChild(bone.name + " Handlers");
        if (customHandlers == null)
        {
            var go = new GameObject(bone.name + " Handlers");
            go.tag = "Weapon";
            customHandlers = go.transform;
            customHandlers.transform.parent = bone;
            customHandlers.transform.localPosition = Vector3.zero;
            customHandlers.transform.localEulerAngles = Vector3.zero;
        }

        handler = new GameObject("handler@weaponName");
        handler.tag = "Weapon";
        handler.layer = LayerMask.NameToLayer("Ignore Raycast");
        handler.transform.parent = customHandlers;
        handler.transform.localPosition = Vector3.zero;
        Selection.activeTransform = handler.transform;
    }

    void SelectHandler(Transform bone)
    {
        var customHandlers = bone.FindChild(bone.name + " Handlers");
        if (customHandlers == null)
        {
            var go = new GameObject(bone.name + " Handlers");
            go.tag = "Weapon";
            go.layer = LayerMask.NameToLayer("Ignore Raycast");
            customHandlers = go.transform;
            customHandlers.transform.parent = bone;
            customHandlers.transform.localPosition = Vector3.zero;
            customHandlers.transform.localEulerAngles = Vector3.zero;
        }

        Selection.activeTransform = customHandlers;
    }
}