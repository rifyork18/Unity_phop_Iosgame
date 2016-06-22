using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;

[CanEditMultipleObjects]
[CustomEditor(typeof(vDynamicTriggerAction), true)]
public class vDynamicTriggerActionEditor : Editor
{
    GUISkin skin;
    [DrawGizmo(GizmoType.Selected)]

    private static void DrawGizmos(Transform transform, GizmoType aGizmoType)
    {
        var dinamicTrigger = transform.GetComponent<vDynamicTriggerAction>();
        Color red = new Color(1, 0, 0, 0.5f);
        Color green = new Color(0, 1, 0, 0.5f);
        if (dinamicTrigger)
        {
            for (int i = 0; i < dinamicTrigger.boxTriggers.Length; i++)
            {
                if (dinamicTrigger.boxTriggers[i] != null)
                {
                    var triggerBox = dinamicTrigger.boxTriggers[i].transform;
                    if (dinamicTrigger.boxTriggers[i].inCollision)
                        Gizmos.color = red;
                    else
                        Gizmos.color = green;
                    Gizmos.matrix = Matrix4x4.TRS(triggerBox.position, triggerBox.rotation, triggerBox.lossyScale);
                    Gizmos.DrawCube(Vector3.zero, Vector3.one);
                }
            }
        }
        else
        {
            dinamicTrigger = transform.GetComponentInParent<vDynamicTriggerAction>();
            if (dinamicTrigger)
            {
                for (int i = 0; i < dinamicTrigger.boxTriggers.Length; i++)
                {
                    if (dinamicTrigger.boxTriggers[i] != null)
                    {                       
                        if (dinamicTrigger.boxTriggers[i].inCollision)
                            Gizmos.color = red;
                        else
                            Gizmos.color = green;
                        Gizmos.matrix = Matrix4x4.TRS(dinamicTrigger.boxTriggers[i].transform.position, dinamicTrigger.boxTriggers[i].transform.rotation, dinamicTrigger.boxTriggers[i].transform.lossyScale);
                        Gizmos.DrawCube(Vector3.zero, Vector3.one);
                    }
                }
            }
        }
    }

    static bool BoxCast(vBoxTrigger boxCast)
    {

        return boxCast.inCollision;
    }

    void OnEnable()
    {
        skin = Resources.Load("skin") as GUISkin;
    }

    public override void OnInspectorGUI()
    {
        if (skin != null)
            GUI.skin = skin;

        base.OnInspectorGUI();
        DrawDinamicTrigger();
    }

    void DrawDinamicTrigger()
    {
        var transform = (serializedObject.targetObject as MonoBehaviour).transform;        

        var boxTriggers = transform.GetComponentsInChildren<vBoxTrigger>();
        var mono = (vDynamicTriggerAction)target;
        if(mono.boxTriggers != boxTriggers)
        {
            mono.boxTriggers = boxTriggers;
            EditorUtility.SetDirty(mono);
        }
           
        serializedObject.Update();
        var triggerBoxes = serializedObject.FindProperty("boxTriggers");
        GUILayout.BeginVertical("window");
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Triggers");


        if (GUILayout.Button("Add Trigger", EditorStyles.miniButton))
        {
            var trigger = new GameObject("trigger" + triggerBoxes.arraySize.ToString("00"), typeof(vBoxTrigger));

            trigger.transform.position = transform.position;
            trigger.transform.rotation = transform.rotation;
            trigger.transform.parent = transform;
            trigger.transform.localScale = Vector3.one * 0.5f;
            trigger.GetComponent<BoxCollider>().isTrigger = true;
        }
        GUILayout.EndHorizontal();
        triggerBoxes.isExpanded = true;

        for (int i = 0; i < triggerBoxes.arraySize; i++)
        {
            GUILayout.BeginVertical("box");           
            SerializedObject obj = new SerializedObject(triggerBoxes.GetArrayElementAtIndex(i).objectReferenceValue);
            if (obj != null)
            {
                obj.Update();
                EditorGUILayout.ObjectField(obj.targetObject, typeof(vBoxTrigger), true);
                ListIterator(obj.FindProperty("tagsToIgnore"));
                obj.ApplyModifiedProperties();
            }
            GUILayout.EndVertical();
        }


        GUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
    }

    public void ListIterator(SerializedProperty listProperty)
    {
        listProperty.isExpanded = EditorGUILayout.Foldout(listProperty.isExpanded, listProperty.name);
        if (listProperty.isExpanded)
        {
            listProperty.arraySize = EditorGUILayout.IntField("Tags", listProperty.arraySize, GUILayout.ExpandWidth(true));
            EditorGUI.indentLevel++;
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(elementProperty);
            }
            EditorGUI.indentLevel--;
        }
    }
}
