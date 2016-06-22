using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Invector
{
    public abstract class vThirdPersonMotor : vCharacter
    {
        #region Variables

        #region Layers
        [Header("---! Layers !---")]
        [Tooltip("Layers that the character can walk on")]
        public LayerMask groundLayer = 1 << 0;
        [Tooltip("Distance to became not grounded")]
        [SerializeField] protected float groundCheckDistance = 0.5f;

        [Tooltip("What objects can make the character auto crouch")]
        public LayerMask autoCrouchLayer = 1 << 0;
        [Tooltip("[SPHERECAST] ADJUST IN PLAY MODE - White Spherecast put just above the head, this will make the character Auto-Crouch if something hit the sphere.")]
        public float headDetect = 0.95f;

        [Tooltip("Gameobjects that has the ActionTrigger component to trigger a action")]
        public LayerMask actionLayer;
        [Tooltip("[RAYCAST] Height of the ActionTriggers Raycast, normally at the knee height of your character works fine")]
        public float actionRayHeight = 0.5f;
        [Tooltip("[RAYCAST] Distance of the ActionTriggers Raycast, make sure that this raycast reachs the Trigger Collider")]
        public float actionRayDistance = 0.25f;

        [Tooltip("Select the layers the your character will stop moving when close to")]
        public LayerMask stopMoveLayer;
        [Tooltip("[RAYCAST] Stopmove Raycast Height")]
        public float stopMoveHeight = 0.65f;
        [Tooltip("[RAYCAST] Stopmove Raycast Distance")]
        public float stopMoveDistance = 0.5f;
        #endregion

        #region Character Variables

        public enum LocomotionType
        {
            FreeWithStrafe,
            OnlyStrafe,
            OnlyFree
        }

        [Header("--- Locomotion Setup ---")]

        public LocomotionType locomotionType = LocomotionType.FreeWithStrafe;
        [Tooltip("Add extra speed for the locomotion movement, keep this value at 0 if you want to use only root motion speed.")]
        [SerializeField] protected float extraMoveSpeed = 0f;
        [Tooltip("Add extra speed for the strafe movement, keep this value at 0 if you want to use only root motion speed.")]
        [SerializeField] protected float extraStrafeSpeed = 0f;
        [Tooltip("Use this to rotate the character using the World axis, or false to use the camera axis - CHECK for Isometric Camera")]
        [SerializeField] protected bool rotateByWorld = false;
        [Tooltip("Check if you want to use Turn on Spot animations")]
        [SerializeField] protected bool turnOnSpot = true;
        [Tooltip("Check if you want to use QuickStop animation")]
        [SerializeField] protected bool quickStopAnim = true;
        [Tooltip("Speed of the rotation on free directional movement")]
        [SerializeField] protected float freeRotationSpeed = 10f;
        [Tooltip("Speed of the rotation while strafe movement")]
        [SerializeField] protected float strafeRotationSpeed = 4f;
        [Tooltip("Put your Random Idle animations at the AnimatorController and select a value to randomize, 0 is disable.")]
        [SerializeField] protected float randomIdleTime = 0f;

        [Header("--- Grounded Setup ---")]

        protected float groundDistance;
        public RaycastHit groundHit;
        [Tooltip("ADJUST IN PLAY MODE - Offset height limit for sters - GREY Raycast in front of the legs")]
        public float stepOffsetEnd = 0.45f;
        [Tooltip("ADJUST IN PLAY MODE - Offset height origin for sters, make sure to keep slight above the floor - GREY Raycast in front of the legs")]
        public float stepOffsetStart = 0.05f;
        [Tooltip("Higher value will result jittering on ramps, lower values will have difficulty on steps")]
        public float stepSmooth = 2f;
        [Tooltip("Max angle to walk")]
        [SerializeField] protected float slopeLimit = 45f;
        [Tooltip("Apply extra gravity when the character is not grounded")]
        [SerializeField] protected float extraGravity = 4f;
        [Tooltip("Select a VerticalVelocity to trigger the Land High animation")]
        [SerializeField] protected float landHighVel = -5f;
        [Tooltip("Turn the Ragdoll On when falling at high speed (check VerticalVelocity) - leave the value with 0 if you don't want this feature")]
        [SerializeField] protected float ragdollVel = -8f;

        [Header("--- Head Track & IK---")]

        [Tooltip("The character Head will follow where you look at, leave with 0 if you are using TopDown or 2.5D or don't want to use")]
        [SerializeField] protected float headWeight = 1f;
        [Tooltip("Determines how much the body is involved in the LookAt when in Free Movement")]
        [SerializeField] protected float freeBodyWeight = 0.25f;
        [Tooltip("Determines how much the body is involved in the LookAt when Strafing")]
        [SerializeField] protected float strafeBodyWeight = 0.8f;
        [Tooltip("Max angle to affect the HeadTrack and Body")]
        [SerializeField] protected float maxAngle = 55f;
        [Tooltip("Speed to blend the HeadTrack and Body")]
        [SerializeField] protected float headTrackMultiplier = 1f;
        [HideInInspector] public float handIKWeight;

        [Header("--- Debug Info ---")]
        [SerializeField] protected bool debugWindow;
        [Range(0f, 1f)]
        public float timeScale = 1f;
        #endregion

        #region Protected Variables      
        protected CombatID combatID = new CombatID();
        protected GameObject currentCollectable;
        protected v_AIController targetEnemy;
        #endregion

        #region Actions

        // general bools of movement        
        protected bool
            onGround,   stopMove,       autoCrouch,
            quickStop,  quickTurn,      canSprint,
            crouch,     strafing,       landHigh,
            jump,       isJumping,      sliding,
            lockIntoTarget;        

        // actions bools, used to turn on/off actions animations *check ThirdPersonAnimator*	        
        protected bool
            jumpOver,
            stepUp,
            climbUp,
            roll,
            isRolling,
            enterLadderBottom,
            enterLadderTop,
            usingLadder,
            exitLadderBottom,
            exitLadderTop,
            inAttack,
            blocking,
            hitReaction,
            hitRecoil;

        protected bool canMoveForward, canMoveRight, canMoveLeft, canMoveBack;
        [HideInInspector] public vTriggerDragable draggableBox;
        protected Vector3 dragEuler;
        protected bool dragStart;

        // one bool to rule then all
        protected bool actions
        {
            get
            {
                return jumpOver || stepUp || climbUp || roll || usingLadder || quickStop || quickTurn || hitReaction || hitRecoil;
            }
        }

        /// <summary>
        ///  DISABLE ACTIONS - call this method in canse you need to turn every action bool false
        /// </summary>
        protected void DisableActions()
        {
            inAttack = false;
            blocking = false;
            quickTurn = false;
            hitReaction = false;
            hitRecoil = false;
            quickStop = false;
            canSprint = false;
            strafing = false;
            landHigh = false;
            jumpOver = false;
            roll = false;
            stepUp = false;
            climbUp = false;
            crouch = false;
            jump = false;
            isJumping = false;
        }

        protected void RemoveComponents()
        {
            if (_capsuleCollider != null) Destroy(_capsuleCollider);
            if (_rigidbody != null) Destroy(_rigidbody);
            if (animator != null) Destroy(animator);            
            var comps = GetComponents<MonoBehaviour>();
            foreach (Component comp in comps) Destroy(comp);
        }

        #endregion

        #region Camera Variables
        // acess camera info
        [HideInInspector]
        public v3rdPersonCamera tpCamera;
        // generic string to change the CameraState
        [HideInInspector]
        public string customCameraState;
        // generic string to change the CameraPoint of the Fixed Point Mode
        [HideInInspector]
        public string customlookAtPoint;
        // generic bool to change the CameraState
        [HideInInspector]
        public bool changeCameraState;
        // generic bool to know if the state will change with or without lerp
        [HideInInspector]
        public bool smoothCameraState;
        // generic variables to find the correct direction 
        [HideInInspector]
        public Quaternion freeRotation;
        // create a offSet base on the character hips 
        [HideInInspector]
        public float offSetPivot;
        // keep the current direction in case you change the cameraState
        [HideInInspector]
        public bool keepDirection;
        [HideInInspector]
        public Vector2 oldInput;
        [HideInInspector]
        public Vector3 cameraForward;
        [HideInInspector]
        public Vector3 cameraRight;
        // get the tpCamera transform
        public Transform cameraTransform
        {
            get
            {
                Transform cT = transform;
                if (Camera.main != null)
                    cT = Camera.main.transform;
                if (tpCamera)
                    cT = tpCamera.transform;
                if (cT == transform)
                {
                    Debug.LogWarning("Invector : Missing TPCamera or MainCamera");
                    this.enabled = false;
                }
                return cT;
            }
        }
        #endregion

        #region HUD variables
        // acess hud controller 
        [HideInInspector]
        public vHUDController hud;
        #endregion

        #region Components & Hide Variables
        // acess action controller information
        [HideInInspector] public vActionsController actionsController;
        // acess melee weapon information
        [HideInInspector] public vMeleeManager meleeManager;
        // physics material
        [HideInInspector] public PhysicMaterial frictionPhysics, slippyPhysics;
        // get capsule collider information
        [HideInInspector] public CapsuleCollider _capsuleCollider;
        // storage capsule collider extra information
        [HideInInspector] public float colliderRadius, colliderHeight;
        // storage the center of the capsule collider info
        [HideInInspector] public Vector3 colliderCenter;
        // access the rigidbody component
        [HideInInspector] public Rigidbody _rigidbody;
        // generate input for the controller
        [HideInInspector] public Vector2 input;
        // lock all the character locomotion 
        [HideInInspector] public bool lockPlayer;
        // general variables to the locomotion
        [HideInInspector] public float speed, direction, verticalVelocity;
        #endregion

        #endregion

        public void Init()
        {        
            // this method is called on the ThirdPersonController or TopDownController - Start     

            animator = GetComponent<Animator>();
            tpCamera = v3rdPersonCamera.instance;
            hud = vHUDController.instance;
            meleeManager = GetComponent<vMeleeManager>();
            // create a offset pivot to the character, to align camera position when transition to ragdoll
            var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            offSetPivot = Vector3.Distance(transform.position, hips.position);

            if (tpCamera != null)
            {
                tpCamera.offSetPlayerPivot = offSetPivot;
                tpCamera.target = transform;
            }

            if (hud == null) Debug.LogWarning("Invector : Missing HUDController, please assign on ThirdPersonController");

            // prevents the collider from slipping on ramps
            frictionPhysics = new PhysicMaterial();
            frictionPhysics.name = "frictionPhysics";
            frictionPhysics.staticFriction = 1f;
            frictionPhysics.dynamicFriction = 1f;

            // default physics 
            slippyPhysics = new PhysicMaterial();
            slippyPhysics.name = "slippyPhysics";
            slippyPhysics.staticFriction = 0f;
            slippyPhysics.dynamicFriction = 0f;

            // rigidbody info
            _rigidbody = GetComponent<Rigidbody>();

            // capsule collider 
            _capsuleCollider = GetComponent<CapsuleCollider>();

            // save your collider preferences 
            colliderCenter = GetComponent<CapsuleCollider>().center;
            colliderRadius = GetComponent<CapsuleCollider>().radius;
            colliderHeight = GetComponent<CapsuleCollider>().height;

            // health info
            currentHealth = maxHealth;
            currentHealthRecoveryDelay = healthRecoveryDelay;
            currentStamina = maxStamina;

            // stopmove info
            canMoveForward = true;
            canMoveRight = true;
            canMoveLeft = true;
            canMoveBack = true;

            if (hud == null)
                return;

            hud.damageImage.color = new Color(0f, 0f, 0f, 0f);

            cameraTransform.SendMessage("Init", SendMessageOptions.DontRequireReceiver);
            UpdateHUD();
        }

        protected void UpdateMotor()
        {
            CheckHealth();
            CheckGround();
            CheckRagdoll();
            ControlHeight();
            ControlLocomotion();
            StaminaRecovery();
            HealthRecovery();
            DragBoxRoutine();
        }

        #region HUD Health & Stamina

        /// <summary>
        /// APPLY DAMAGE - call this method by a SendMessage with the damage value
        /// </summary>
        /// <param name="damage"> damage to apply </param>
        public override void TakeDamage(Damage damage)
        {
            // don't apply damage if the character is rolling, you can add more conditions here
            if (!damage.ignoreDefense && roll || isDead)
                return;

            // defend attack behaviour
            if (BlockAttack(damage))
                return;

            // only trigger the hitReaction animation when the character is not doing an action, using ladder or jumping
            var hitReactionConditions = ((!actions || (actions && hitReaction)) || (usingLadder)) && !isJumping;
            if (hitReactionConditions)
            {
                // set the ID of the reaction based on the attack animation state of the attacker - Check the MeleeAttackBehaviour script
                animator.SetInteger("Recoil_ID", damage.recoil_id);
                // trigger hitReaction animation
                animator.SetTrigger("HitReaction");
            }

            // instantiate the hitDamage particle - check if your character has a HitDamageParticle component
            var hitrotation = Quaternion.LookRotation(new Vector3(transform.position.x, damage.hitPosition.y, transform.position.z) - damage.hitPosition);
            SendMessage("TriggerHitParticle", new HittEffectInfo(new Vector3(transform.position.x, damage.hitPosition.y, transform.position.z), hitrotation, damage.attackName), SendMessageOptions.DontRequireReceiver);

            // reduce the current health by the damage amount.
            currentHealth -= damage.value;
            currentHealthRecoveryDelay = healthRecoveryDelay;

            // trigger the hit sound 
            if (damage.sender != null)
                damage.sender.SendMessage("PlayHitSound", SendMessageOptions.DontRequireReceiver);

            // update the hud graphics 
            if (hud != null)
            {
                // set the damaged flag so the screen will flash.
                hud.damageImage.enabled = true;
                hud.damaged = true;
                // set the health bar's value to the current health.
                hud.healthSlider.value = currentHealth;
            }

            // apply vibration on the gamepad             
            transform.SendMessage("GamepadVibration", 0.25f, SendMessageOptions.DontRequireReceiver);
            // turn the ragdoll on if the weapon is checked with 'activeRagdoll' 
            if (damage.activeRagdoll)
                transform.SendMessage("ActivateRagdoll", SendMessageOptions.DontRequireReceiver);
        }

        void CheckHealth()
        {
            if (currentHealth <= 0 && !isDead)
            {
                transform.root.SendMessage("DropRightWeapon", SendMessageOptions.DontRequireReceiver);
                transform.root.SendMessage("DropLeftWeapon", SendMessageOptions.DontRequireReceiver);

                tpCamera.ClearTargetLockOn();
                isDead = true;

                if (hud != null)
                    hud.FadeText("You are Dead!", 2f, 0.5f);

                if (vGameController.instance != null && vGameController.instance.playerPrefab != null)
                    vGameController.instance.Invoke("Spawn", vGameController.instance.respawnTimer);
                else if (vGameController.instance != null)
                {
                    //if (vSpawnEnemies.instance != null)
                    //    vSpawnEnemies.instance.Invoke("ShowLoseWindow", vGameController.instance.respawnTimer);
                    //else
                    vGameController.instance.Invoke("ResetScene", vGameController.instance.respawnTimer);
                }
            }
        }

        void HealthRecovery()
        {
            if (currentHealth <= 0) return;
            if (currentHealthRecoveryDelay > 0)
            {
                currentHealthRecoveryDelay -= Time.deltaTime;
            }
            else
            {
                if (currentHealth > maxHealth)
                    currentHealth = maxHealth;
                if (currentHealth < maxHealth)
                    currentHealth = Mathf.Lerp(currentHealth, maxHealth, healthRecovery * Time.deltaTime);
            }
        }

        void StaminaRecovery()
        {
            if (recoveryDelay > 0)
            {
                recoveryDelay -= Time.deltaTime;
            }
            else
            {
                if (currentStamina > maxStamina)
                    currentStamina = maxStamina;
                if (currentStamina < maxStamina)
                    currentStamina += staminaRecovery;
            }
        }

        /// <summary>
        /// Call this method when executing a action to reduce the stamina 
        /// </summary>
        /// <param name="value"></param>
        protected void ReduceStamina(float value, bool accumulative)
        {
            if(accumulative) currentStamina -= value * Time.deltaTime; 
            else currentStamina -= value;
            if (currentStamina < 0)
                currentStamina = 0;
        }

        // sycronize the stamina value with the stamina slider and show/hide the damageHUD image effect        
        protected void UpdateHUD()
        {
            if (hud == null)
                return;

            hud.healthSlider.maxValue = maxHealth;
            hud.healthSlider.value = currentHealth;
            hud.staminaSlider.maxValue = maxStamina;
            hud.staminaSlider.value = currentStamina;

            if (hud.damaged)
                hud.damageImage.color = hud.flashColour;
            else
                hud.damageImage.color = Color.Lerp(hud.damageImage.color, Color.clear, hud.flashSpeed * Time.deltaTime);

            hud.damaged = false;
        }        

        /// <summary>
        /// Show a custom message if you have a HUD FadeText 
        /// </summary>
        /// <param name="message"></param>
        public void ShowText(string message)
        {
            hud.FadeText(message, 2f, 0.5f);
        }

        #endregion

        #region Locomotion 
        
        protected bool freeLocomotionConditions
        {
            get
            {
                if (locomotionType.Equals(LocomotionType.OnlyStrafe)) strafing = true;
                return !strafing && !usingLadder && !landHigh && !locomotionType.Equals(LocomotionType.OnlyStrafe) || locomotionType.Equals(LocomotionType.OnlyFree);
            }
        }

        void ControlLocomotion()
        {            
            if (lockPlayer || inAttack) return;
                        
            if (freeLocomotionConditions)                            
                FreeMovement();     // free directional movement

            else                            
                StrafeMovement();   // move forward, backwards, strafe left and right
        }

        void StrafeMovement()
        {            
            var _speed = Mathf.Clamp(input.y, canMoveBack ? -1f : 0, canMoveForward ? 1f : 0);
            var _direction = Mathf.Clamp(input.x, canMoveLeft ? -1f : 0, canMoveRight ? 1f : 0);
            speed = _speed;
            direction = _direction;
            if (canSprint) speed += canMoveForward ? 0.5f : 0;
        }
        
        void FreeMovement()
        {
            // set speed to both vertical and horizontal inputs               
            speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);
            speed = Mathf.Clamp(speed, 0, canMoveForward ? 1 : 0);
            // add 0.5f on sprint to change the animation on animator
            if (canSprint) speed += (canMoveForward ? 0.5f : 0);
            if (stopMove) speed = 0f;
            if (input == Vector2.zero && !quickTurn) direction = Mathf.Lerp(direction, 0f, 20f * Time.fixedDeltaTime);

            var conditions = (!actions || quickTurn || quickStop) && !inAttack;
            if (input != Vector2.zero && targetDirection.magnitude > 0.1f && conditions)
            {
                freeRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                Vector3 velocity = Quaternion.Inverse(transform.rotation) * targetDirection.normalized;
                var _direction = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;
                direction = Mathf.Clamp(_direction, canMoveLeft? -180 : 0, canMoveRight ? 180 : 0);
                
                // activate quickTurn180 based on the directional angle
                var quickTurnConditions = !crouch && direction >= 90 && !jump && onGround || !crouch && direction <= -90 && !jump && onGround;
                if (quickTurnConditions && turnOnSpot)
                    quickTurn = true;

                // apply free directional rotation while not turning180 animations
                if ((!quickTurn && !isJumping) || (isJumping && actionsController.Jump.jumpAirControl))
                {
                    Vector3 lookDirection = targetDirection.normalized;
                    freeRotation = Quaternion.LookRotation(lookDirection, transform.up);
                    var diferenceRotation = freeRotation.eulerAngles.y - transform.eulerAngles.y;
                    var eulerY = transform.eulerAngles.y;

                    if (diferenceRotation < 0 && canMoveLeft || diferenceRotation > 0 && canMoveRight) eulerY = freeRotation.eulerAngles.y;
                    var euler = new Vector3(0, eulerY, 0);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(euler), freeRotationSpeed * Time.fixedDeltaTime);
                }
                if (!keepDirection)
                    oldInput = input;
                if (Vector2.Distance(oldInput, input) > 0.9f && keepDirection)
                    keepDirection = false;
            }
        }
        
        void ControlHeight()
        {
            if (crouch || roll)
            {
                _capsuleCollider.center = colliderCenter / 1.4f;
                _capsuleCollider.height = colliderHeight / 1.4f;
            }
            else if (usingLadder)
            {
                _capsuleCollider.radius = colliderRadius / 1.25f;
            }
            else
            {
                // back to the original values
                _capsuleCollider.center = colliderCenter;
                _capsuleCollider.radius = colliderRadius;
                _capsuleCollider.height = colliderHeight;
            }
        }
        
        void CheckGround()
        {
            CheckGroundDistance();

            // change the physics material to very slip when not grounded
            _capsuleCollider.material = (onGround && GroundAngle() < slopeLimit) ? frictionPhysics : slippyPhysics;
            // we don't want to stick the character grounded if one of these bools is true
            bool groundStickConditions = !jumpOver && !stepUp && !climbUp && !usingLadder && !hitReaction;

            if (groundStickConditions)
            {
                // clear the checkground to free the character to attack on air
                if (inAttack) return;
                var onStep = StepOffset();

                if (groundDistance <= 0.05f)
                {
                    onGround = true;
                    Sliding();
                }
                else
                {
                    if (groundDistance >= groundCheckDistance)
                    {
                        onGround = false;
                        // check vertical velocity
                        verticalVelocity = _rigidbody.velocity.y;
                        // apply extra gravity when falling
                        if (!onStep && !roll)
                            transform.position -= Vector3.up * (extraGravity * Time.deltaTime);
                    }
                    else if (!onStep && !roll && !jump)
                        transform.position -= Vector3.up * (extraGravity * Time.deltaTime);
                }
            }
        }

        void Sliding()
        {
            var onStep = StepOffset();
            var groundAngleTwo = 0f;
            RaycastHit hitinfo;
            Ray ray = new Ray(transform.position, -transform.up);

            if (Physics.Raycast(ray, out hitinfo, 1f, groundLayer))
                groundAngleTwo = Vector3.Angle(Vector3.up, hitinfo.normal);

            if (GroundAngle() > slopeLimit + 1f && GroundAngle() <= 85 &&
                groundAngleTwo > slopeLimit + 1f && groundAngleTwo <= 85 &&
                groundDistance <= 0.05f && !onStep)
            {
                sliding = true;
                onGround = false;
                var slideVelocity = (GroundAngle() - slopeLimit) * 5f;
                slideVelocity = Mathf.Clamp(slideVelocity, 0, 10);
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, -slideVelocity, _rigidbody.velocity.z);
            }
            else
            {
                sliding = false;
                onGround = true;
            }
        }

        public void Rolling()
        {
            bool staminaCondition = currentStamina > actionsController.Roll.staminaCost;
            // conditions to roll when strafing
            bool strafeRoll = strafing && input != Vector2.zero || speed > 0.25f;
            // can roll even if it's on a quickturn or quickstop animation
            bool actionsRoll = !actions || (actions && (quickTurn || quickStop));
            // general conditions to roll
            bool rollConditions = actionsRoll && strafeRoll && onGround && staminaCondition && !isJumping && !jump && draggableBox == null;

            if (rollConditions)
            {
                roll = true;
                ReduceStamina(actionsController.Roll.staminaCost, false);
                recoveryDelay = actionsController.Roll.recoveryDelay;
            }
        }

        void CheckGroundDistance()
        {
            if (_capsuleCollider != null)
            {
                // radius of the SphereCast
                float radius = _capsuleCollider.radius * 0.9f;
                var dist = 10f;
                // position of the SphereCast origin starting at the base of the capsule
                Vector3 pos = transform.position + Vector3.up * (_capsuleCollider.radius);
                // ray for RayCast
                Ray ray1 = new Ray(transform.position + new Vector3(0, colliderHeight / 2, 0), Vector3.down);
                // ray for SphereCast
                Ray ray2 = new Ray(pos, -Vector3.up);
                // raycast for check the ground distance
                if (Physics.Raycast(ray1, out groundHit, 1f, groundLayer))
                    dist = transform.position.y - groundHit.point.y;

                // sphere cast around the base of the capsule to check the ground distance
                if (Physics.SphereCast(ray2, radius, out groundHit, 1f, groundLayer))
                {
                    // check if sphereCast distance is small than the ray cast distance
                    if (dist > (groundHit.distance - _capsuleCollider.radius * 0.1f))
                        dist = (groundHit.distance - _capsuleCollider.radius * 0.1f);
                }
                groundDistance = dist;
            }
        }
        
        bool StepOffset()
        {
            if (input.sqrMagnitude < 0.1 || !onGround) return false;

            var hit = new RaycastHit();
            Ray rayStep = new Ray((transform.position + new Vector3(0, stepOffsetEnd, 0) + transform.forward * ((_capsuleCollider).radius + 0.05f)), Vector3.down);

            if (Physics.Raycast(rayStep, out hit, stepOffsetEnd - stepOffsetStart, groundLayer))
            {
                if (!stopMove && hit.point.y >= (transform.position.y) && hit.point.y <= (transform.position.y + stepOffsetEnd))
                {
                    var heightPoint = new Vector3(transform.position.x, hit.point.y + 0.1f, transform.position.z);
                    transform.position = Vector3.Lerp(transform.position, heightPoint, (speed * stepSmooth) * Time.fixedDeltaTime);
                    //var heightPoint = new Vector3(_rigidbody.velocity.x, hit.point.y + 0.1f, _rigidbody.velocity.z);
                    //_rigidbody.velocity = heightPoint * 10f * Time.fixedDeltaTime;
                    return true;
                }
            }
            return false;
        }
        
        float GroundAngle()
        {
            var groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
            return groundAngle;
        }
        
        protected void StopMove()
        {
            if (input.sqrMagnitude < 0.1 || !onGround) return;

            RaycastHit hitinfo;
            Ray ray = new Ray(transform.position + new Vector3(0, stopMoveHeight, 0), transform.forward);

            if (Physics.Raycast(ray, out hitinfo, _capsuleCollider.radius + stopMoveDistance, stopMoveLayer) && !usingLadder)
            {
                var hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);

                if (hitinfo.distance <= stopMoveDistance && hitAngle > 85)
                    stopMove = true;
                else if (hitAngle >= slopeLimit + 1f && hitAngle <= 85)
                    stopMove = true;
            }
            else if (Physics.Raycast(ray, out hitinfo, 1f, groundLayer) && !usingLadder && draggableBox == null)
            {
                var hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);
                if (hitAngle >= slopeLimit + 1f && hitAngle <= 85)
                {
                    Debug.Log("opa");
                    stopMove = true;
                }
                    
            }
            else
                stopMove = false;
        }
        
        protected void CheckAutoCrouch()
        {
            // radius of SphereCast
            float radius = _capsuleCollider.radius * 0.9f;
            // Position of SphereCast origin stating in base of capsule
            Vector3 pos = transform.position + Vector3.up * ((colliderHeight * 0.5f) - colliderRadius);
            // ray for SphereCast
            Ray ray2 = new Ray(pos, Vector3.up);

            // sphere cast around the base of capsule for check ground distance
            if (Physics.SphereCast(ray2, radius, out groundHit, headDetect - (colliderRadius * 0.1f), autoCrouchLayer))
            {
                autoCrouch = true;
                crouch = true;
            }
            else
                autoCrouch = false;
        }
        
        public IEnumerator UpdateRaycast()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                CheckAutoCrouch();
                StopMove();
            }
        }
        
        protected GameObject CheckActionObject()
        {
            bool checkConditions = onGround && !landHigh && !actions && !inAttack;
            GameObject _object = null;

            if (checkConditions)
            {
                RaycastHit hitInfoAction;
                Vector3 yOffSet = new Vector3(0f, actionRayHeight, 0f);
                Vector3 fwd = transform.TransformDirection(Vector3.forward);

                if (Physics.Raycast(transform.position + yOffSet, fwd, out hitInfoAction, distanceOfRayActionTrigger, actionLayer))
                {
                    _object = hitInfoAction.transform.gameObject;
                }
            }
            return currentCollectable != null ? currentCollectable : _object;
        }

        public float distanceOfRayActionTrigger
        {
            get
            {
                if (_capsuleCollider == null) return 0f;
                var dist = _capsuleCollider.radius + actionRayDistance;
                return dist;
            }
        }
        
        protected virtual void RotateWithCamera()
        {
            if (strafing && !actions && !lockPlayer)
            {
                // smooth align character with aim position
                if (tpCamera != null && tpCamera.lockTarget)
                {
                    Quaternion rot = Quaternion.LookRotation(tpCamera.lockTarget.position - transform.position);                    
                    var newPos = new Vector3(transform.eulerAngles.x, rot.eulerAngles.y, transform.eulerAngles.z);
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newPos), strafeRotationSpeed * Time.fixedDeltaTime);                    
                }
                else //if(input != Vector2.zero)
                {
                    if (!inAttack)
                    {
                        var diferenceRotation = cameraTransform.eulerAngles.y - transform.eulerAngles.y;
                        var eulerY = transform.eulerAngles.y;
                        if (diferenceRotation < -5 && canMoveLeft) eulerY = cameraTransform.eulerAngles.y;
                        else if ((diferenceRotation > 5 && canMoveRight)) eulerY = cameraTransform.eulerAngles.y;                        
                        var newPos = new Vector3(transform.eulerAngles.x, eulerY, transform.eulerAngles.z);
                        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newPos), draggableBox != null ? draggableBox.box.cameraSmoothRotation * Time.fixedDeltaTime : strafeRotationSpeed * Time.fixedDeltaTime);                        
                    }
                }
            }
        }        

        protected Vector3 targetDirection
        {
            get
            {
                Vector3 refDir = Vector3.zero;
                cameraForward = keepDirection ? cameraForward : cameraTransform.TransformDirection(Vector3.forward);
                cameraForward.y = 0;

                if (tpCamera == null || !tpCamera.currentState.cameraMode.Equals(TPCameraMode.FixedAngle) || !rotateByWorld)
                {
                    //cameraForward = tpCamera.transform.TransformDirection(Vector3.forward);
                    cameraForward = keepDirection ? cameraForward : cameraTransform.TransformDirection(Vector3.forward);
                    cameraForward.y = 0; //set to 0 because of camera rotation on the X axis

                    //get the right-facing direction of the camera
                    cameraRight = keepDirection ? cameraRight : cameraTransform.TransformDirection(Vector3.right);

                    // determine the direction the player will face based on input and the camera's right and forward directions
                    refDir = input.x * cameraRight + input.y * cameraForward;
                }
                else
                {
                    refDir = new Vector3(input.x, 0, input.y);
                }
                return refDir;
            }
        }

        #endregion

        #region Ragdoll 

        void CheckRagdoll()
        {
            if (ragdollVel == 0) return;

            // check your verticalVelocity and assign a value on the variable RagdollVel at the Player Inspector
            if (verticalVelocity <= ragdollVel && groundDistance <= 0.1f)
                transform.SendMessage("ActivateRagdoll", SendMessageOptions.DontRequireReceiver);
        }

        public override void ResetRagdoll()
        {
            tpCamera.offSetPlayerPivot = offSetPivot;
            tpCamera.SetTarget(this.transform);
            lockPlayer = false;
            verticalVelocity = 0f;
            ragdolled = false;
        }

        public override void RagdollGettingUp()
        {
            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;
            _capsuleCollider.enabled = true;
        }

        public override void EnableRagdoll()
        {
            tpCamera.offSetPlayerPivot = 0f;
            tpCamera.SetTarget(animator.GetBoneTransform(HumanBodyBones.Hips));
            ragdolled = true;
            _capsuleCollider.enabled = false;
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
            lockPlayer = true;
        }

        #endregion

        #region Melee Combat

        public void SetCombatID(CombatID id)
        {
            combatID = id;
        }
        
        public void OnAttackEnter(HitboxFrom hitboxFrom)
        {
            ReduceStamina(meleeManager.CurrentMeleeAttack(hitboxFrom).staminaCost, false);
            recoveryDelay = meleeManager.CurrentMeleeAttack(hitboxFrom).a_staminaRecoveryDelay;
        }
        
        public void InAttacking()
        {
            inAttack = true;
        }
        
        public void OnAttackExit()
        {
            inAttack = false;
        }

        public void FindTargetLockOn(Transform target)
        {
            var _targetEnemy = target.GetComponent<v_AIController>();
            if (_targetEnemy != null)
                targetEnemy = _targetEnemy;
        }

        public void LostTargetLockOn()
        {
            targetEnemy = null;
            strafing = false;
        }

        int GetDamageResult(int damage, float defenseRate)
        {
            int result = (int)(damage - ((damage * defenseRate) / 100));
            return result;
        }
        
        bool BlockAttack(Damage damage)
        {
            // while the character is defending 
            var defenceRangeConditions = (meleeManager != null && meleeManager.CurrentMeleeDefense() != null ? meleeManager.CurrentMeleeDefense().AttackInDefenseRange(damage.sender) : false);
            if (blocking && currentStamina > damage.value && !damage.ignoreDefense && defenceRangeConditions)
            {
                var recoil_id = damage.recoil_id;
                var targetRecoil_id = (meleeManager.CurrentMeleeDefense() != null ? meleeManager.CurrentMeleeDefense().Recoil_ID : 0);
                // trigger a recoil animation based on the ID set by the attack of the attacker
                SendMessage("TriggerRecoil", recoil_id, SendMessageOptions.DontRequireReceiver);
                if (meleeManager.CurrentMeleeDefense().breakAttack && damage.sender != null)
                    damage.sender.SendMessage("TriggerRecoil", targetRecoil_id, SendMessageOptions.DontRequireReceiver);

                // reduce the current stamina based on the damage of the attack
                ReduceStamina(damage.value, false);
                recoveryDelay = meleeManager.CurrentMeleeDefense().d_staminaRecoveryDelay;

                // reduce the current health if your defense rate is lower then 100
                var damageResult = GetDamageResult(damage.value, meleeManager.CurrentMeleeDefense().defenseRate);
                currentHealth -= damageResult;

                // trigger the def sound of your defense weapon
                meleeManager.CurrentMeleeDefense().PlayDEFSound();
                if (damage.sender != null && damageResult > 0)
                    // play the hit sound based on the weapon that attacked you - check the weapon sounds
                    damage.sender.SendMessage("PlayHitSound", SendMessageOptions.DontRequireReceiver);

                // reduce the current health based on the damage of the weapon
                currentHealth -= damageResult;
                currentHealthRecoveryDelay = healthRecoveryDelay;
                return true;
            }
            return false;
        }

        #endregion

        #region Drag Box

        protected void DragBoxRoutine()
        {
            if (!draggableBox || !dragStart) return;

            // check if can move to all direction;
            // this work with a relative direction of character movement

            // update position of DraggableBox objet
            Ray fwr = new Ray(transform.position, transform.forward);

            var point = fwr.GetPoint(draggableBox.targetDistance);
            var position = new Vector3(point.x, draggableBox.box.transform.position.y, point.z);
            var direction = (Quaternion.Euler(transform.eulerAngles) * new Vector3(input.x, 0, input.y));
            var canMoveToDirection = draggableBox.box.CanMoveToDirection(transform, direction);

            canMoveForward = canMoveToDirection;
            canMoveBack = canMoveToDirection;
            canMoveRight = canMoveToDirection;
            canMoveLeft = canMoveToDirection;

            var X = draggableBox.box._rigidbody.position.x;
            var Z = draggableBox.box._rigidbody.position.z;
            var difX = Mathf.Clamp(X - position.x, -.1f, .1f);
            var difZ = Mathf.Clamp(Z - position.z, -.1f, .1f);
            var newPos = new Vector3(position.x + difX, draggableBox.box.transform.position.y, position.z + difZ);

            draggableBox.box._rigidbody.position = Vector3.Lerp(draggableBox.box.transform.position, newPos, 40 * Time.fixedDeltaTime);
            var euler = draggableBox.box.transform.eulerAngles;
            euler.y = (transform.eulerAngles + dragEuler).y;
            draggableBox.box.transform.eulerAngles = euler;
            
            var lHandPos = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
            var rHandPos = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
            var lDist = Vector3.Distance(draggableBox.IKLeftHand.position, lHandPos);
            var rDist = Vector3.Distance(draggableBox.IKRightHand.position, rHandPos);

            // lose box if the arms IK stretch too much
            if (handIKWeight >= 1 && (lDist >= 0.25f || rDist >= 0.25f))            
                DropBox();            
        }

        public void DragBox(GameObject hitObject, vTriggerDragable triggerDraggable)
        {
            // align the character rotation with the object rotation
            var rot = hitObject.transform.rotation;
            transform.rotation = rot;
            // reset any other animation state to null
            animator.SetTrigger("ResetState");
            // set the player to strafe mode
            strafing = true;
            // set the gameobject to drag
            triggerDraggable.box.SetInDrag(true);
            draggableBox = triggerDraggable;
        }

        public void DropBox()
        {            
            if (hud != null) hud.HideDragBoxText();
            strafing = false;
            dragStart = false;
            canMoveForward = true;
            canMoveRight = true;
            canMoveLeft = true;
            canMoveBack = true;
            draggableBox.box.SetInDrag(false);
            draggableBox = null;
        }

        #endregion
        
        protected void DebugMode()
        {
            Time.timeScale = timeScale;

            if (hud == null)
                return;

            if (hud.debugPanel != null)
            {
                if (debugWindow)
                {
                    hud.debugPanel.SetActive(true);
                    float delta = Time.smoothDeltaTime;
                    float fps = 1 / delta;

                    hud.debugPanel.GetComponentInChildren<Text>().text =
                        "FPS " + fps.ToString("#,##0 fps") + "\n" +
                        "CameraState = " + tpCamera.currentStateName.ToString() + "\n" +
                        "Input = " + Mathf.Clamp(input.sqrMagnitude, 0, 1f).ToString("0.0") + "\n" +
                        "Speed = " + speed.ToString("0.0") + "\n" +
                        "Direction = " + direction.ToString("0.0") + "\n" +
                        "VerticalVelocity = " + verticalVelocity.ToString("0.00") + "\n" +
                        "GroundDistance = " + groundDistance.ToString("0.00") + "\n" +
                        "GroundAngle = " + GroundAngle().ToString("0.00") + "\n" +
                        "\n" + "--- Movement Bools ---" + "\n" +
                        "LockPlayer = " + lockPlayer.ToString() + "\n" +
                        "StopMove = " + stopMove.ToString() + "\n" +
                        "Ragdoll = " + ragdolled.ToString() + "\n" +
                        "onGround = " + onGround.ToString() + "\n" +
                        "canSprint = " + canSprint.ToString() + "\n" +
                        "crouch = " + crouch.ToString() + "\n" +
                        "strafe = " + strafing.ToString() + "\n" +
                        "turn180 = " + quickTurn.ToString() + "\n" +
                        "quickStop = " + quickStop.ToString() + "\n" +
                        "landHigh = " + landHigh.ToString() + "\n" +
                        "jump = " + jump.ToString() + "\n" +
                        "\n" + "--- Actions Bools ---" + "\n" +
                        "roll = " + roll.ToString() + "\n" +
                        "stepUp = " + stepUp.ToString() + "\n" +
                        "jumpOver = " + jumpOver.ToString() + "\n" +
                        "climbUp = " + climbUp.ToString() + "\n" +
                        "usingLadder = " + usingLadder.ToString() + "\n" +
                        "blocking = " + blocking.ToString() + "\n" +
                        "InAttack = " + inAttack.ToString() + "\n";

                }
                else
                    hud.debugPanel.SetActive(false);
            }
        }
    }
}