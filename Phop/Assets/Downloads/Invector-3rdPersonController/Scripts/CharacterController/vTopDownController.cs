using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

namespace Invector.CharacterController
{
    public class vTopDownController : vThirdPersonAnimator
    {
        private static vTopDownController _instance;
        public static vTopDownController instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<vTopDownController>();
                    //Tell unity not to destroy this object when loading a new scene
                    //DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }

        private bool isAxisInUse;

        #region TopDown Variables

        private RaycastHit hit;

        public enum GameplayInputStyle
        {
            ClickAndMove,
            DirectionalInput
        }

        [Header("--- Topdown Input Type ---")]
        public GameplayInputStyle gameplayInputStyle = GameplayInputStyle.DirectionalInput;
        [HideInInspector]
        public TopDownCursor topDownCursor;
        
        #endregion

        void Awake()
        {
            StartCoroutine("UpdateRaycast");	// limit raycasts calls for better performance
        }

        void Start()
        {
            Init();						// setup the basic information, created on Character.cs	
        }

        void FixedUpdate()
        {
            UpdateMotor();                  // call ThirdPersonMotor methods
            UpdateAnimator();               // update animations on the Animator and their methods
            UpdateHUD();                    // update HUD elements like health bar, texts, etc
            ControlCameraState();			// change CameraStates
        }

        void LateUpdate()
        {
            InputHandle();                  // handle input from controller, keyboard&mouse or touch            
            DebugMode();					// display information about the character on PlayMode
        }

        void InputHandle()
        {
            CloseApp();
            CameraInput();
            if (!lockPlayer && !ragdolled)
            {
                // we have mapped the 360 controller as our Default gamepad, 
                // you can change the keyboard inputs by changing the Alternative Button on the InputManager.
                // check the Action Input painel to change the input name on the Character Inspector
                ControllerInput();
                InteractInput();
                JumpInput();
                RollInput();
                CrouchInput();
                AttackInput();
                DefenseInput();
                SprintInput();
                StrafeInput();
                LockOnInput();
                DropWeaponInput("D-Pad Horizontal");
            }
            else
                LockPlayer();
        }

        void ControlCameraState()
        {
            tpCamera.ChangeState("Default", true);
        }

        protected override void RotateWithCamera()
        {
            if (strafing && !actions && !lockPlayer && draggableBox == null)
            {
                // smooth align character with aim position
                if (tpCamera != null && tpCamera.lockTarget)
                {
                    Quaternion rot = Quaternion.LookRotation(tpCamera.lockTarget.position - transform.position);
                    Quaternion newPos = Quaternion.Euler(transform.eulerAngles.x, rot.eulerAngles.y, transform.eulerAngles.z);
                    transform.rotation = Quaternion.Slerp(transform.rotation, newPos, freeRotationSpeed * Time.fixedDeltaTime);
                }
                else if (input != Vector2.zero)
                {
                    if (!inAttack)
                    {
                        var diferenceRotation = cameraTransform.eulerAngles.y - transform.eulerAngles.y;
                        var eulerY = transform.eulerAngles.y;
                        if (diferenceRotation < 0 && canMoveLeft) eulerY = cameraTransform.eulerAngles.y;
                        else if ((diferenceRotation > 0 && canMoveRight)) eulerY = cameraTransform.eulerAngles.y;
                        Quaternion newPos = Quaternion.Euler(transform.eulerAngles.x, eulerY, transform.eulerAngles.z);
                        transform.rotation = Quaternion.Slerp(transform.rotation, newPos, draggableBox != null ? draggableBox.box.cameraSmoothRotation * Time.fixedDeltaTime : freeRotationSpeed * Time.fixedDeltaTime);
                    }
                }
            }
        }

        #region Locomotion Input

        void ControllerInput()
        {
            if (gameplayInputStyle == GameplayInputStyle.ClickAndMove)
            {
                if (topDownCursor == null)
                {
                    var go = GameObject.Instantiate(Resources.Load("TopDownCursor"), transform.position, Quaternion.identity) as GameObject;
                    topDownCursor = go.GetComponent<TopDownCursor>();
                }

                var dir = (topDownCursor.transform.position - transform.position).normalized;
                if (Input.GetMouseButton(0))
                {
                    if (Physics.Raycast(tpCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
                    {
                        topDownCursor.Enable();
                        topDownCursor.transform.position = hit.point;
                    }
                }
                if (!topDownCursor.Near(transform.position, 0.5f))
                    input = Vector2.Lerp(input, new Vector2(dir.x, dir.z), freeRotationSpeed * Time.deltaTime);
                else
                {
                    topDownCursor.Disable();
                    input = Vector2.Lerp(input, Vector3.zero, 20 * Time.deltaTime);
                }
            }
            else if (gameplayInputStyle == GameplayInputStyle.DirectionalInput)
            {
                var _input = Vector2.zero;
                if (inputType == InputType.Mobile)
                    _input = new Vector2(CrossPlatformInputManager.GetAxis("Horizontal"), CrossPlatformInputManager.GetAxis("Vertical"));
                else if (inputType == InputType.MouseKeyboard)
                    _input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                else if (inputType == InputType.Controler)
                {
                    float deadzone = 0.25f;
                    _input = new Vector2(Input.GetAxis("LeftAnalogHorizontal"), Input.GetAxis("LeftAnalogVertical"));
                    if (_input.magnitude < deadzone)
                        _input = Vector2.zero;
                    else
                        _input = _input.normalized * ((_input.magnitude - deadzone) / (1 - deadzone));
                }
                if (draggableBox != null)
                {
                    var dir = transform.InverseTransformDirection(new Vector3(_input.x, 0, _input.y));
                    input = new Vector2(dir.x, dir.z);
                }
                else
                {
                    input = _input;
                }
            }
        }

        void CameraInput()
        {
            if (inputType == InputType.Mobile)
            {
                tpCamera.RotateCamera(CrossPlatformInputManager.GetAxis("Mouse X"), CrossPlatformInputManager.GetAxis("Mouse Y"));
            }
            else if (inputType == InputType.MouseKeyboard)
            {
                tpCamera.RotateCamera(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                tpCamera.Zoom(Input.GetAxis("Mouse ScrollWheel"));
            }
            else if (inputType == InputType.Controler)
            {
                tpCamera.RotateCamera(Input.GetAxis("RightAnalogHorizontal"), Input.GetAxis("RightAnalogVertical"));
            }
        }

        void SprintInput()
        {
            // check if you select this action on the Action Input Painel
            if (!actionsController.Sprint.use) return;
            // check the input you select on the Action Input Painel 
            var _input = actionsController.Sprint.input.ToString();
            // mobile input
            if (inputType == InputType.Mobile)
            {
                if (CrossPlatformInputManager.GetButtonDown(_input) && currentStamina > 0 && input.sqrMagnitude > 0.1f)
                {
                    strafing = false;
                    if (onGround && !crouch)
                        canSprint = !canSprint;
                }
                else if (currentStamina <= 0 || input.sqrMagnitude < 0.1f || actions)
                    canSprint = false;
            }
            // keyboard or gamepad input
            else
            {
                if (Input.GetButtonDown(_input) && currentStamina > 0 && input.sqrMagnitude > 0.1f)
                {
                    if (onGround && !crouch)
                        canSprint = !canSprint;
                }
                else if (currentStamina <= 0 || input.sqrMagnitude < 0.1f || actions)
                    canSprint = false;
            }
            // check how much stamina this action will consume
            if (canSprint)
            {
                recoveryDelay = actionsController.Sprint.recoveryDelay;
                ReduceStamina(actionsController.Sprint.staminaCost, true);
            }
        }

        void CrouchInput()
        {
            // check if you select this action on the Action Input Painel
            if (!actionsController.Crouch.use) return;
            // check the input you select on the Action Input Painel 
            var _input = actionsController.Crouch.input.ToString();

            if (autoCrouch)
                crouch = true;
            else if (actionsController.Crouch.pressToCrouch)
            {
                if (inputType == InputType.Mobile)
                    crouch = (CrossPlatformInputManager.GetButton(_input) && onGround);
                else
                    crouch = Input.GetButton(_input) && onGround && !actions;
            }
            else
            {
                if (inputType == InputType.Mobile)
                {
                    if (CrossPlatformInputManager.GetButtonDown(_input) && onGround)
                        crouch = !crouch;
                }
                else
                {
                    if (Input.GetButtonDown(_input) && onGround && !actions)
                        crouch = !crouch;
                }
            }
        }

        void JumpInput()
        {
            // check if you select this action on the Action Input Painel
            if (!actionsController.Jump.use) return;
            // check the input you select on the Action Input Painel 
            var _input = actionsController.Jump.input.ToString();
            // know if has enough stamina to make this action
            bool staminaConditions = currentStamina > actionsController.Jump.staminaCost;
            // conditions to do this action
            bool jumpConditions = !crouch && onGround && !actions && staminaConditions && !inAttack && !jump && !isJumping;

            if (inputType == InputType.Mobile)
            {
                if (CrossPlatformInputManager.GetButtonDown(_input) && jumpConditions)
                {
                    jump = true;
                    ReduceStamina(actionsController.Jump.staminaCost, false);
                    recoveryDelay = actionsController.Jump.recoveryDelay;
                }
            }
            else
            {
                if (Input.GetButtonDown(_input) && jumpConditions)
                {
                    jump = true;
                    ReduceStamina(actionsController.Jump.staminaCost, false);
                    recoveryDelay = actionsController.Jump.recoveryDelay;
                }
            }
        }

        void StrafeInput()
        {
            // check if you select this action on the Action Input Painel
            if (!actionsController.Strafe.use) return;
            // check the input you select on the Action Input Painel 
            var _input = actionsController.Strafe.input.ToString();

            if (!locomotionType.Equals(LocomotionType.OnlyFree))
            {
                if (inputType == InputType.Mobile)
                {
                    if (CrossPlatformInputManager.GetButtonDown(_input) && !actions)
                    {
                        animator.SetFloat("Direction", 0f);
                        strafing = !strafing;
                    }
                }
                else
                {
                    if (Input.GetButtonDown(_input) && !actions)
                    {
                        animator.SetFloat("Direction", 0f);
                        strafing = !strafing;
                    }
                }
            }
        }

        void RollInput()
        {
            // check if you select this action on the Action Input Painel
            if (!actionsController.Roll.use) return;
            // check the input you select on the Action Input Painel 
            var _input = actionsController.Roll.input.ToString();

            if (inputType == InputType.Mobile)
            {
                if (CrossPlatformInputManager.GetButtonDown(_input) && !isRolling)
                    Rolling();
            }
            else
            {
                if (Input.GetButtonDown(_input) && !isRolling)
                    Rolling();
            }
        }

        void ExitLadderInput(GameObject other)
        {
            // if you are using the ladder and reach the exit from the bottom
            if (other.gameObject.CompareTag("ExitLadderBottom") && usingLadder)
            {
                if (inputType == InputType.Mobile)
                {
                    if (CrossPlatformInputManager.GetButtonDown("B") || speed <= -0.05f && !enterLadderBottom)
                        exitLadderBottom = true;
                }
                else
                {
                    if (Input.GetButtonDown("B") || speed <= -0.05f && !enterLadderBottom)
                        exitLadderBottom = true;
                }
            }
            // if you are using the ladder and reach the exit from the top
            if (other.gameObject.CompareTag("ExitLadderTop") && usingLadder && !enterLadderTop)
            {
                if (speed >= 0.05f)
                    exitLadderTop = true;
            }
        }

        // DO ACTIONS - WHITE raycast to check if there is anything interactable ahead             
        void InteractInput()
        {
            if (!actionsController.Interact.use) return;
            // trigger action input - default "A"
            var _input = actionsController.Interact.input.ToString();
            // dragBox input - default "B"       
            var _dbInput = actionsController.DragBox.input.ToString();

            var hitObject = CheckActionObject();
            if (hitObject != null)
            {
                try
                {
                    // -- here you can add new tags to trigger new actions --

                    if (hitObject.CompareTag("ClimbUp"))
                    {
                        ActionInput(hitObject, ref climbUp, _input);
                        CheckForDraggableObject(hitObject, _dbInput);
                    }
                    else if (hitObject.CompareTag("StepUp"))
                        ActionInput(hitObject, ref stepUp, _input);
                    else if (hitObject.CompareTag("JumpOver"))
                        ActionInput(hitObject, ref jumpOver, _input);
                    else if (hitObject.CompareTag("EnterLadderBottom") && !jump)
                        ActionInput(hitObject, ref enterLadderBottom, _input);
                    else if (hitObject.CompareTag("EnterLadderTop") && !jump)
                        ActionInput(hitObject, ref enterLadderTop, _input);
                    else if (hitObject.CompareTag("AutoCrouch"))
                        autoCrouch = true;
                    else if (hitObject.CompareTag("Weapon"))
                        PickUpMeleeWeaponInput(hitObject, "D-Pad Horizontal");
                }
                catch (UnityException e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
            else if (hud != null)
            {
                if (hud.showInteractiveText)
                    hud.HideActionText();
                if (hud.showEquipText)
                    hud.HideEquipText();
                if (hud.showDragBoxText)
                    hud.HideDragBoxText();
            }
        }

        #endregion

        #region Actions & DragBox

        void ActionInput(GameObject hitObject, ref bool action, string _input)
        {
            // check if the object has a TriggerAction script            
            var triggerAction = hitObject.transform.GetComponent<vTriggerAction>();

            if (!triggerAction)
            {
                Debug.LogWarning("Missing TriggerAction Component on " + hitObject.transform.name + "Object");
                return;
            }
            // check if you can use this action
            if (triggerAction.CanUse() && draggableBox == null)
            {
                if (hud != null && !triggerAction.autoAction) hud.ShowActionText(triggerAction.message, true);
            }
            else
            {
                if (hud != null && !triggerAction.autoAction) hud.ShowActionText("Can't " + triggerAction.message, false);
                return;
            }

            // input to call the action
            if (inputType == InputType.Mobile)
            {
                if (CrossPlatformInputManager.GetButtonDown(_input) && !actions || triggerAction.autoAction && !actions)
                    DoAction(triggerAction, hitObject, ref action);
            }
            else
            {
                if (Input.GetButtonDown(_input) && !actions || triggerAction.autoAction && !actions)
                    DoAction(triggerAction, hitObject, ref action);
            }
        }

        void DoAction(vTriggerAction triggerAction, GameObject hitObject, ref bool action)
        {
            // turn the action bool true and call the animation
            action = true;
            // disable the text and sprite 
            if (hud != null) hud.HideActionText();
            // find the cursorObject height to match with the character animation
            matchTarget = triggerAction.target;
            // align the character rotation with the object rotation
            var rot = hitObject.transform.rotation;
            transform.rotation = rot;
            // reset any other animation state to null
            animator.SetTrigger("ResetState");
        }

        void DragBoxInput(GameObject hitObject, vTriggerDragable triggerDraggable, string _input)
        {
            // input to call the action for mobile
            if (inputType == InputType.Mobile)
            {
                if (CrossPlatformInputManager.GetButtonDown(_input) && !actions && draggableBox == null)
                    DragBox(hitObject, triggerDraggable);
                else if (CrossPlatformInputManager.GetButtonDown(_input) && !actions && draggableBox != null)
                    DropBox();
            }
            // input to call the action for pc/gamepad
            else
            {
                if (Input.GetButtonDown(_input) && !actions && draggableBox == null)
                    DragBox(hitObject, triggerDraggable);
                else if (Input.GetButtonDown(_input) && !actions && draggableBox != null)
                    DropBox();
            }
        }

        void CheckForDraggableObject(GameObject hitObject, string _input)
        {
            // find the trigger components
            var triggerDraggable = hitObject.GetComponent<vTriggerDragable>();
            var triggerAction = hitObject.transform.GetComponent<vTriggerAction>();
            if (triggerDraggable == null) return;

            // display hud message & icon    
            if (triggerAction.CanUse())
            {
                if (hud != null) hud.ShowDragBoxText(triggerDraggable.message, true);
            }
            else
            {
                if (hud != null) hud.ShowDragBoxText("Can't " + triggerDraggable.message, false);
                return;
            }

            DragBoxInput(hitObject, triggerDraggable, _input);
        }

        #endregion

        #region Melee Combat

        void LockOnInput()
        {
            // check if you select this action on the Action Input Painel
            if (!actionsController.LockOn.use) return;
            // check the input you select on the Action Input Painel 
            var _input = actionsController.LockOn.input.ToString();
            // only do the lockon if the character is on Free Movement
            if (!locomotionType.Equals(LocomotionType.OnlyFree))
            {
                // mobile input
                if (inputType == InputType.Mobile)
                {
                    if (CrossPlatformInputManager.GetButtonDown(_input) && !actions)
                    {
                        lockIntoTarget = !lockIntoTarget;
                        tpCamera.gameObject.SendMessage("UpdateLockOn", lockIntoTarget, SendMessageOptions.DontRequireReceiver);
                    }
                }
                // keyboard & gamepad input
                else
                {
                    if (Input.GetButtonDown(_input) && !actions)
                    {
                        lockIntoTarget = !lockIntoTarget;
                        tpCamera.gameObject.SendMessage("UpdateLockOn", lockIntoTarget, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }

            SwitchTargets();
        }

        void SwitchTargets()
        {
            if (tpCamera.lockTarget)
            {
                // switch between targets using Keyboard
                if (inputType == InputType.MouseKeyboard)
                {
                    if (Input.GetKey(KeyCode.X))
                        tpCamera.gameObject.SendMessage("ChangeTarget", 1, SendMessageOptions.DontRequireReceiver);
                    else if (Input.GetKey(KeyCode.Z))
                        tpCamera.gameObject.SendMessage("ChangeTarget", -1, SendMessageOptions.DontRequireReceiver);
                }
                // switch between targets using GamePad
                else if (inputType == InputType.Controler)
                {
                    var value = Input.GetAxisRaw("RightAnalogHorizontal");
                    if (value == 1)
                        tpCamera.gameObject.SendMessage("ChangeTarget", 1, SendMessageOptions.DontRequireReceiver);
                    else if (value == -1f)
                        tpCamera.gameObject.SendMessage("ChangeTarget", -1, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        void AttackInput()
        {
            // check if you select this action on the Action Input Painel
            if (!actionsController.Attack.use) return;
            if (meleeManager == null) return;

            // check the input you select on the Action Input Painel 
            var _input = actionsController.Attack.input.ToString();

            // attack conditions
            bool weaponStaminaConditions = meleeManager.CurrentMeleeAttack() != null && currentStamina > meleeManager.CurrentMeleeAttack().staminaCost;
            bool attackConditions = (!actions || roll) && onGround && weaponStaminaConditions && !isJumping && !jump;

            // mobile input
            if (inputType == InputType.Mobile)
            {
                if (CrossPlatformInputManager.GetButtonDown(_input) && attackConditions)
                    animator.SetTrigger("MeleeAttack");
            }
            // keyboard & gamepad input
            else
            {
                if (Input.GetButtonDown(_input) && attackConditions)
                    animator.SetTrigger("MeleeAttack");
            }
        }

        void DefenseInput()
        {
            // check if you select this action on the Action Input Painel
            if (!actionsController.Defense.use) return;
            if (meleeManager == null) return;

            // check the input you select on the Action Input Painel 
            var _input = actionsController.Defense.input.ToString();

            // defense contitions    
            bool shieldStaminaConditions = meleeManager.CurrentMeleeDefense() != null;
            bool defenseConditions = (!actions || roll || hitRecoil || quickTurn) && onGround && shieldStaminaConditions && !inAttack;

            // mobile input
            if (inputType == InputType.Mobile)
            {
                if (CrossPlatformInputManager.GetButton(_input) && defenseConditions)
                    blocking = true;
                else
                    blocking = false;
            }
            // keyboard & gamepad input
            else
            {
                if (Input.GetButton(_input) && defenseConditions)
                    blocking = true;
                else
                    blocking = false;
            }
        }

        // the method InteractInput() will return a gameobject with type of CollectableMelee when you stay inside the Trigger area
        void PickUpMeleeWeaponInput(GameObject collectableItem, string _input)
        {
            #region Mobile Input
            if (inputType == InputType.Mobile)
            {
                // return the weapon and the left side to equip to the EquipMeleeWeapon    
                if (CrossPlatformInputManager.GetButtonDown("EquipLeftMobile") && !actions)
                {
                    if (isAxisInUse == false)
                    {
                        EquipMeleeWeapon(collectableItem, -1);
                        isAxisInUse = true;
                    }
                }
                else if (CrossPlatformInputManager.GetButtonUp("EquipLeftMobile"))
                    isAxisInUse = false;

                // return the weapon and the right side to equip to the EquipMeleeWeapon       
                if (CrossPlatformInputManager.GetButtonDown("EquipRightMobile") && !actions)
                {
                    if (isAxisInUse == false)
                    {
                        EquipMeleeWeapon(collectableItem, 1);
                        isAxisInUse = true;
                    }
                }
                else if (CrossPlatformInputManager.GetButtonUp("EquipRightMobile"))
                    isAxisInUse = false;
            }
            #endregion
            #region Keyboard & Gamepad
            else
            {
                // return the weapon and the right side to equip to the EquipMeleeWeapon            
                if (Input.GetAxisRaw(_input) == 1f && !actions)
                {
                    if (isAxisInUse == false)
                    {
                        EquipMeleeWeapon(collectableItem, 1);
                        isAxisInUse = true;
                    }
                }
                // return the weapon and the left side to equip to the EquipMeleeWeapon    
                else if (Input.GetAxisRaw(_input) == -1f && !actions)
                {
                    if (isAxisInUse == false)
                    {
                        EquipMeleeWeapon(collectableItem, -1);
                        isAxisInUse = true;
                    }
                }
                if (Input.GetAxisRaw(_input) == 0)
                    isAxisInUse = false;
            }
            #endregion
        }

        // the PickUpEquipmentInput will return the collectable and the side to equip 
        void EquipMeleeWeapon(GameObject collectableItem, int side)
        {
            // get the collectable melee component if you enter the trigger area 
            var collectable = collectableItem.GetComponent<vCollectableMelee>();
            // get the meleeWeapon component of the collectable
            var weapon = collectable.GetComponentInChildren<vMeleeWeapon>();

            if (meleeManager != null)
            {
                // equip weapon on the right handler
                if (side == 1 && CheckEquipConditions(1, weapon))
                    meleeManager.SetRightWeaponHandler(collectable);
                // drop current right weapon to equip the new one
                else if (side == 1 && meleeManager.currentMeleeWeaponRA != null)
                    meleeManager.DropRightWeapon();
                // equip weapon on the left handler
                else if (side == -1 && CheckEquipConditions(-1, weapon))
                    meleeManager.SetLeftWeaponHandler(collectable);
                // drop current left weapon to equip the new one
                else if (side == -1 && meleeManager.currentMeleeWeaponLA != null)
                    meleeManager.DropLeftWeapon();
            }

            if (hud != null) hud.HideEquipText();
            currentCollectable = null;
        }

        bool CheckEquipConditions(int side, vMeleeWeapon weapon)
        {
            if (side == 1)
            {
                if ((weapon.meleeType == vMeleeWeapon.MeleeType.All && (weapon.handEquip == vMeleeWeapon.HandEquip.BothHand || weapon.handEquip == vMeleeWeapon.HandEquip.RightHand)))
                    return true;
                if (weapon.meleeType == vMeleeWeapon.MeleeType.Attack)
                    return true;
            }
            else if (side == -1)
            {
                if (weapon.useTwoHand) return false;
                if ((weapon.meleeType == vMeleeWeapon.MeleeType.All && (weapon.handEquip == vMeleeWeapon.HandEquip.BothHand || weapon.handEquip == vMeleeWeapon.HandEquip.RightHand)))
                    return true;
                if (weapon.meleeType == vMeleeWeapon.MeleeType.Defense)
                    return true;
            }

            return false;
        }

        void DropWeaponInput(string _input)
        {
            #region Mobile Input
            if (inputType == InputType.Mobile)
            {
                if (CrossPlatformInputManager.GetButtonDown("EquipLeftMobile") && !actions)
                {
                    if (isAxisInUse == false)
                    {
                        if (meleeManager != null && meleeManager.CurrentMeleeDefense() != null)
                            meleeManager.DropLeftWeapon();
                        isAxisInUse = true;
                    }
                }
                else if (CrossPlatformInputManager.GetButtonUp("EquipLeftMobile"))
                    isAxisInUse = false;
                if (CrossPlatformInputManager.GetButtonDown("EquipRightMobile") && !actions)
                {
                    if (isAxisInUse == false)
                    {
                        if (meleeManager != null)
                            meleeManager.DropRightWeapon();

                        isAxisInUse = true;
                    }
                }
                else if (CrossPlatformInputManager.GetButtonUp("EquipRightMobile"))
                    isAxisInUse = false;
            }
            #endregion
            #region Keyboard & Gamepad
            else
            {
                if (Input.GetAxisRaw(_input) >= 1f)
                {
                    if (isAxisInUse == false)
                    {
                        if (meleeManager != null)
                            meleeManager.DropRightWeapon();

                        isAxisInUse = true;
                    }
                }
                else if (Input.GetAxisRaw(_input) <= -1f)
                {
                    if (isAxisInUse == false)
                    {
                        if (meleeManager != null && meleeManager.CurrentMeleeDefense() != null)
                            meleeManager.DropLeftWeapon();
                        isAxisInUse = true;
                    }
                }
                if (Input.GetAxisRaw(_input) == 0)
                    isAxisInUse = false;
            }
            #endregion
        }

        // run this method on a OnTriggerStay to check if it's a collectable weapon
        void CheckForCollectableWeapon(GameObject other)
        {
            if (other.gameObject.CompareTag("Weapon"))
            {
                var collectable = other.gameObject.GetComponent<vCollectableMelee>();
                if (collectable != null)
                {
                    currentCollectable = collectable.gameObject;
                    if (collectable._meleeWeapon.GetType().Equals(typeof(vMeleeWeapon)))
                    {
                        var _meleeWeapon = collectable.gameObject.GetComponent<vMeleeWeapon>();
                        if (_meleeWeapon == null)
                            _meleeWeapon = collectable.gameObject.GetComponentInChildren<vMeleeWeapon>();

                        // display weapon information on the hud
                        if (hud != null && _meleeWeapon.meleeType == vMeleeWeapon.MeleeType.Attack)
                            hud.ShowEquipText(collectable.message + " (" + _meleeWeapon.damage.value + " ATK)", _meleeWeapon.meleeType, _meleeWeapon.useTwoHand ? 1 : (int)_meleeWeapon.handEquip);
                        else if (hud != null && _meleeWeapon.meleeType == vMeleeWeapon.MeleeType.Defense)
                            hud.ShowEquipText(collectable.message + " (" + _meleeWeapon.defenseRate + " DEF)", _meleeWeapon.meleeType, (int)_meleeWeapon.handEquip);
                        else if (hud != null && _meleeWeapon.meleeType == vMeleeWeapon.MeleeType.All)
                            hud.ShowEquipText(collectable.message + " (" + _meleeWeapon.damage.value + " ATK)" + " (" + _meleeWeapon.defenseRate + " DEF)", _meleeWeapon.meleeType, _meleeWeapon.useTwoHand ? 1 : (int)_meleeWeapon.handEquip);
                    }
                }
            }
        }

        #endregion

        void OnTriggerStay(Collider other)
        {
            try
            {
                CheckForCollectableWeapon(other.gameObject);
                ExitLadderInput(other.gameObject);
            }
            catch (UnityException e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        void OnTriggerExit(Collider other)
        {
            // reset the currentCollectable to null if you exit the trigger area 
            if (other.gameObject.CompareTag("Weapon"))
            {
                var collectable = other.gameObject.GetComponent<vCollectableMelee>();
                if (collectable != null)
                {
                    currentCollectable = null;
                }
            }
        }

        void LockPlayer()
        {
            // lock the Player Input and reset some variables.
            input = Vector2.zero;
            speed = 0f;
            canSprint = false;
            if (hud != null) hud.HideActionText();
        }

        void CloseApp()
        {
            // just a example to quit the application 
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!Cursor.visible)
                    Cursor.visible = true;
                else
                    Application.Quit();
            }
        }
    }
}