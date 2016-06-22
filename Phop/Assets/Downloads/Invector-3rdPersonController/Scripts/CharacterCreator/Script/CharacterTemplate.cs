using UnityEngine;
using System.Collections;

public class CharacterTemplate : ScriptableObject
{
    public enum TemplateType
    {
        ThirdPerson,
        TopDown
    }
    public TemplateType templateType;
    public RuntimeAnimatorController animatorController;
    public v3rdPersonCameraListData cameraListData;
    public vHUDController hud;
}
