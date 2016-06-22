using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using Invector;
using Invector.CharacterController;
using UnityEngine.EventSystems;

public class vCreateMeleeWeaponEditor : EditorWindow
{
    GUISkin skin;
    GameObject equipModel;    
    Vector2 rect = new Vector2(500, 590);
    Vector2 scrool;
    Editor fbxPreview;
    Rect buttomRect = new Rect();

    public string equipmentName;

    public vMeleeWeapon.MeleeType equipmentType = vMeleeWeapon.MeleeType.All;

    /// <summary>
	/// 3rdPersonController Menu 
    /// </summary>    
    [MenuItem("3rd Person Controller/Create New Melee Weapon", false, 1)]
    public static void CreateNewMeleeWeapon()
    {
        GetWindow<vCreateMeleeWeaponEditor>();
    }    
        
	void OnGUI()
	{		
		if (!skin) skin = Resources.Load("skin") as GUISkin;
		GUI.skin = skin;
		this.minSize = rect;  
		this.titleContent = new GUIContent("Melee Weapon", null, "Equipment Creator");

        GUILayout.BeginVertical("Melee Creator Window", "window");
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.BeginVertical("box");
        equipmentType = (vMeleeWeapon.MeleeType)EditorGUILayout.EnumPopup("Melee Type", equipmentType);
		
		buttomRect = GUILayoutUtility.GetLastRect();
		buttomRect.position = new Vector2(0, buttomRect.position.y);
		buttomRect.width = this.maxSize.x;    						
		
		equipModel = EditorGUILayout.ObjectField("FBX Model", equipModel, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;        
		
		if (GUI.changed && equipModel !=null)
			fbxPreview = Editor.CreateEditor(equipModel);        
                
		EditorGUILayout.Space();
        GUILayout.EndVertical();
        if (equipModel != null)
        {            
            DrawHumanoidPreview();
            GUILayout.BeginHorizontal("box");
            equipmentName = EditorGUILayout.TextField("Melee Weapon Name", equipModel.gameObject.name);            
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create"))
                CreateMeleeWeapon();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }				
		
        GUILayout.EndVertical();
    }

    /// <summary>
    /// Draw the Preview window
    /// </summary>
    void DrawHumanoidPreview()
    {        
        GUILayout.FlexibleSpace();
        
        if (fbxPreview != null)        
            fbxPreview.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(100, 400), "window");          
    }

    /// <summary>
    /// Created the Third Person Controller
    /// </summary>
    void CreateMeleeWeapon()
    {
		// base for the character
		var equipment = GameObject.Instantiate(equipModel, Vector3.zero, Quaternion.identity) as GameObject;
        if (!equipment)
            return;
        SceneView view = SceneView.lastActiveSceneView;
        if (SceneView.lastActiveSceneView == null)
            throw new UnityException("The Scene View can't be access");

        Vector3 spawnPos = view.camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
        equipment.transform.position = spawnPos;
        equipment.name = equipmentName;

        // add collectable components
        var rigidbody = equipment.AddComponent<Rigidbody>();
        equipment.AddComponent<BoxCollider>();
        var sphereCollider = equipment.AddComponent<SphereCollider>();
        equipment.AddComponent<vCollectableMelee>();

        // add hitbox components
        var meleeScript = new GameObject("hitBox");
        meleeScript.transform.SetParent(equipment.transform);
        meleeScript.transform.localPosition = Vector3.zero;
        var meleeWeapon = meleeScript.AddComponent<vMeleeWeapon>();
        meleeWeapon.meleeType = equipmentType;

        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        sphereCollider.isTrigger = true;

	    equipment.tag = "Weapon";		
	    equipment.layer = LayerMask.NameToLayer("Ignore Raycast");
        foreach (Transform t in equipment.transform.GetComponentsInChildren<Transform>())
        {
            t.gameObject.layer = equipment.layer;
            t.gameObject.tag = equipment.tag;
        }            

        this.Close();
    }   
}
