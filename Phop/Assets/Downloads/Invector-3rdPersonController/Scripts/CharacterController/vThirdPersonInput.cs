using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Invector;

public class vThirdPersonInput : vThirdPersonMotor
{
	/// <summary>
    /// INPUT TYPE - check in real time if you are using the controller ou mouse/keyboard
    /// </summary>
	[HideInInspector]
	public enum InputType
	{
		MouseKeyboard,
		Controler,
		Mobile
	};
	
	[HideInInspector]
	public InputType inputType = InputType.MouseKeyboard;

    /// <summary>
    /// GAMEPAD VIBRATION - call this method to use vibration on the gamepad
    /// </summary>
    /// <param name="vibTime">duration of the vibration</param>
    /// <returns></returns>
    #if UNITY_STANDALONE_WIN || UNITY_EDITOR
	public IEnumerator GamepadVibration(float vibTime)
	{
		if (inputType == InputType.Controler)
		{
			XInputDotNetPure.GamePad.SetVibration(0, 1, 1);
			yield return new WaitForSeconds(vibTime);
			XInputDotNetPure.GamePad.SetVibration(0, 0, 0);
		}
	}
    #endif

    void OnGUI()
	{
		switch (inputType)
		{
		case InputType.MouseKeyboard:
			if (isControlerInput())
			{
				inputType = InputType.Controler;
				hud.controllerInput = true;
				if (hud != null)
					hud.FadeText("Control scheme changed to Controller", 2f, 0.5f);
			}
			else if (isMobileInput())
			{
				inputType = InputType.Mobile;
				hud.controllerInput = true;
				if (hud != null)
					hud.FadeText("Control scheme changed to Mobile", 2f, 0.5f);
			}
			break;
		case InputType.Controler:
			if (isMouseKeyboard())
			{
				inputType = InputType.MouseKeyboard;
				hud.controllerInput = false;
				if (hud != null)
					hud.FadeText("Control scheme changed to Keyboard/Mouse", 2f, 0.5f);
			}
			else if (isMobileInput())
			{
				inputType = InputType.Mobile;
				hud.controllerInput = true;
				if (hud != null)
					hud.FadeText("Control scheme changed to Mobile", 2f, 0.5f);
			}
			break;
		case InputType.Mobile:
			if (isMouseKeyboard())
			{
				inputType = InputType.MouseKeyboard;
				hud.controllerInput = false;
				if (hud != null)
					hud.FadeText("Control scheme changed to Keyboard/Mouse", 2f, 0.5f);
			}
			else if (isControlerInput())
			{
				inputType = InputType.Controler;
				hud.controllerInput = true;
				if (hud != null)
					hud.FadeText("Control scheme changed to Controller", 2f, 0.5f);
			}
			break;
		}
	}
	
	public InputType GetInputState() { return inputType; }
	
	private bool isMobileInput()
	{
            #if UNITY_EDITOR && UNITY_MOBILE
            if (EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
            {
                return true;
            }
		
            #elif MOBILE_INPUT
            if (EventSystem.current.IsPointerOverGameObject() || (Input.touches.Length > 0))
                return true;
            #endif
		return false;
	}
	
	private bool isMouseKeyboard()
	{
#if MOBILE_INPUT
                return false;
#else
            // mouse & keyboard buttons
		if (Event.current.isKey || Event.current.isMouse)
			return true;
            // mouse movement
		if (Input.GetAxis("Mouse X") != 0.0f || Input.GetAxis("Mouse Y") != 0.0f)
			return true;
		
		return false;
#endif
	}
	
	private bool isControlerInput()
	{
            // joystick buttons
		if (Input.GetKey(KeyCode.Joystick1Button0) ||
			Input.GetKey(KeyCode.Joystick1Button1) ||
			Input.GetKey(KeyCode.Joystick1Button2) ||
			Input.GetKey(KeyCode.Joystick1Button3) ||
			Input.GetKey(KeyCode.Joystick1Button4) ||
			Input.GetKey(KeyCode.Joystick1Button5) ||
			Input.GetKey(KeyCode.Joystick1Button6) ||
			Input.GetKey(KeyCode.Joystick1Button7) ||
			Input.GetKey(KeyCode.Joystick1Button8) ||
			Input.GetKey(KeyCode.Joystick1Button9) ||
			Input.GetKey(KeyCode.Joystick1Button10) ||
			Input.GetKey(KeyCode.Joystick1Button11) ||
			Input.GetKey(KeyCode.Joystick1Button12) ||
			Input.GetKey(KeyCode.Joystick1Button13) ||
			Input.GetKey(KeyCode.Joystick1Button14) ||
			Input.GetKey(KeyCode.Joystick1Button15) ||
			Input.GetKey(KeyCode.Joystick1Button16) ||
			Input.GetKey(KeyCode.Joystick1Button17) ||
			Input.GetKey(KeyCode.Joystick1Button18) ||
			Input.GetKey(KeyCode.Joystick1Button19))
		{
			return true;
		}
		
            // joystick axis
		if (Input.GetAxis("LeftAnalogHorizontal") != 0.0f ||
			Input.GetAxis("LeftAnalogVertical") != 0.0f ||
			Input.GetAxis("Triggers") != 0.0f ||
			Input.GetAxis("RightAnalogHorizontal") != 0.0f ||
			Input.GetAxis("RightAnalogVertical") != 0.0f)
		{
			return true;
		}
		return false;
	}
}
