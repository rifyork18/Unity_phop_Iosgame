using UnityEngine;
using System.Collections;
using System;

namespace Invector
{
    public class v_AIMotor : vCharacter
    {
        #region Variables

        #region Layers
        [Header("---! Layers !---")]
        [Tooltip("Layers that the character can walk on")]
        public LayerMask groundLayer = 1 << 0;
        [Tooltip("Distance to became not grounded")]
        [SerializeField]
        protected float groundCheckDistance = 0.5f;

        [Tooltip("What objects can make the character auto crouch")]
        public LayerMask autoCrouchLayer = 1 << 0;
        [Tooltip("[SPHERECAST] ADJUST IN PLAY MODE - White Spherecast put just above the head, this will make the character Auto-Crouch if something hit the sphere.")]
        public float headDetect = 0.95f;

        [Tooltip("Gameobjects that has the ActionTrigger component to trigger a action")]
        public LayerMask actionLayer;
        [Tooltip("[RAYCAST] Height of the ActionTriggers Raycast, normally at the knee height of your character works fine")]
        public float actionRayHeight = 0.5f;
        [Tooltip("[RAYCAST] Distance of the ActionTriggers Raycast, make sure that this raycast reachs the Trigger Collider")]
        public float actionRayDistance = 0.8f;
        #endregion

        #region AI variables
        [Header("--- Locomotion Speed ---")]
        [Tooltip("Use to limit your locomotion animation, if you want to patrol walking set this value to 0.5f")]
        [Range(0f, 1.5f)]
        public float patrolSpeed = 0.5f;
        [Tooltip("Use to limit your locomotion animation, if you want to chase the target walking set this value to 0.5f")]
        [Range(0f, 1.5f)]
        public float chaseSpeed = 1f;
        [Tooltip("Use to limit your locomotion animation, if you want to strafe the target walking set this value to 0.5f")]
        [Range(0f, 1.5f)]
        public float strafeSpeed = 1f;

        //[Header("--- Sensors ---")]        
        public v_AISphereSensor sphereSensor;
        public AIStates currentState = AIStates.PatrolSubPoints;
        public bool drawAgentPath = false;
        public bool displayGizmos;
        [Range(0f, 180f)]
        public float fieldOfView = 95f;
        [Tooltip("Max Distance to detect the Target with FOV")]
        public float maxDetectDistance = 10f;
        [Tooltip("Min Distance to noticed the Target without FOV")]
        public float minDetectDistance = 4f;
        [Tooltip("Distance to lost the Target")]
        public float distanceToLostTarget = 20f;
        [Tooltip("Distance to stop when chasing the Player")]
        public float chaseStopDistance = 1f;

        [Header("--- Head Track ---")]
        [Tooltip("Look at the target using AnimatorIK headtrack")]
        public float headWeight = 1f;
        [Tooltip("Determines how much the body is involved in the LookAt when in Free Movement")]
        [SerializeField]
        protected float freeBodyWeight = 0.1f;
        [Tooltip("Determines how much the body is involved in the LookAt when Strafing")]
        [SerializeField]
        protected float strafeBodyWeight = 0.5f;
        [Tooltip("Speed to blend the HeadTrack and Body")]
        [SerializeField]
        protected float headTrackMultiplier = 1f;

        [Header("--- Strafe ---")]
        [Tooltip("Strafe around the target")]
        public bool strafeSideways = true;
        [Tooltip("Strafe a few steps backwards")]
        public bool strafeBackward = true;
        [Tooltip("Distance to switch to the strafe locomotion, leave with 0 if you don't want your character to strafe")]
        public float strafeDistance = 3f;
        [Tooltip("Min time to change the strafe direction")]
        public float minStrafeSwape = 2f;
        [Tooltip("Max time to change the strafe direction")]
        public float maxStrafeSwape = 5f;
        [Tooltip("Velocity to rotate the character while strafing")]
        public float strafeRotationSpeed = 5f;

        [Header("--- Combat ---")]
        [Tooltip("Check if you want the Enemy to chase the Target at first sight")]
        public bool agressive = true;
        [Tooltip("Velocity to rotate the character while attacking")]
        public float attackRotationSpeed = 0.5f;
        [Tooltip("Delay to trigger the first attack when close to the target")]
        public float firstAttackDelay = 0f;
        [Tooltip("Min frequency to attack")]
        public float minTimeToAttack = 4f;
        [Tooltip("Max frequency to attack")]
        public float maxTimeToAttack = 6f;
        [Tooltip("How many attacks the AI will make on a combo")]
        public int maxAttackCount = 3;
        [Tooltip("Randomly attacks based on the maxAttackCount")]
        public bool randomAttackCount = true;
        [Range(0f, 1f)]
        public float chanceToRoll = .1f;
        [Range(0f, 1f)]
        public float chanceToBlockInStrafe = .1f;
        [Range(0f, 1f)]
        public float chanceToBlockAttack = 0f;
        [Tooltip("How much time the character will stand up the shield")]
        public float raiseShield = 4f;
        [Tooltip("How much time the character will lower the shield")]
        public float lowerShield = 2f;

        [Header("--- Waypoint ---")]
        [Tooltip("Max Distance to change waypoint")]
        [Range(0.5f, 2f)]
        public float distanceToChangeWaypoint = 1f;
        [Tooltip("Min Distance to stop when Patrolling through waypoints")]
        [Range(0.5f, 2f)]
        public float patrollingStopDistance = 0.5f;
        public vWaypointArea pathArea;
        public bool randomWaypoints;

        public vFisherYatesRandom randomWaypoint = new vFisherYatesRandom();
        public vFisherYatesRandom randomPatrolPoint = new vFisherYatesRandom();
        [HideInInspector]
        public CapsuleCollider _capsuleCollider;
        // there is a prefab of health hud example that you can drag and drop into the head bone of your character
        [HideInInspector]
        public v_SpriteHealth healthSlider;
        // attach a meleeManager component to create new hitboxs and set up different weapons
        [HideInInspector]
        public vMeleeManager meleeManager;
        // check your MeleeWeapon Inspector, each weapon can set up different distances to attack
        [HideInInspector]
        public float distanceToAttack;
        [HideInInspector]
        public OffMeshLinkMoveMethod method = OffMeshLinkMoveMethod.Grounded;

        public enum OffMeshLinkMoveMethod
        {
            Teleport,
            DropDown,
            JumpAcross,
            Action,
            Grounded
        }

        public enum AIStates
        {
            Idle,
            PatrolSubPoints,
            PatrolWaypoints,
            Chase
        }
        #endregion

        #region Protected Variables
        protected CombatID combatID;
        protected Transform target;
        protected Vector3 targetPos;
        protected Vector3 destination;
        protected Vector3 fwd;
        protected bool onGround;
        protected bool strafing;
        protected bool inAttack;
        protected bool inResetAttack;
        protected bool firstAttack = true;
        protected int attackCount;
        protected int currentWaypoint;
        protected int currentPatrolPoint;
        protected float direction;
        protected float timer, wait;
        protected float fovAngle;
        protected float sideMovement, fwdMovement = 0;
        protected float strafeSwapeFrequency;
        protected float groundDistance;
        protected bool useJumpOver;
        protected bool useClimbUp;
        protected bool useStepUp;
        protected bool JumpOverIsActive;
        protected bool ClimbUpIsActive;
        protected bool StepUpIsActive;
        protected Vector3 startPosition;
        protected float timeToEnableAction;
        protected RaycastHit groundHit;
        protected NavMeshAgent agent;
        protected NavMeshPath agentPath;
        protected Quaternion freeRotation;
        protected Quaternion desiredRotation;
        protected Vector3 oldPosition;
        protected Vector3 combatMovement;
        protected Vector3 rollDirection;
        protected Rigidbody _rigidbody;
        protected PhysicMaterial frictionPhysics;
        protected Transform head;
        protected Collider colliderTarget;
        protected vWaypoint targetWaypoint;
        protected vPoint targetPatrolPoint;
        protected System.Collections.Generic.List<vPoint> visitedPatrolPoint = new System.Collections.Generic.List<vPoint>();
        protected System.Collections.Generic.List<vWaypoint> visitedWaypoint = new System.Collections.Generic.List<vWaypoint>();
        private int jumpOverLayer, climbUpLayer, stepUpLayer;
        #endregion

        #region Actions
        protected bool
           quickTurn,
           jumpOver,
           stepUp,
           climbUp,
           hitReaction,
           hitRecoil,
           crouch,
           canAttack,
           blocking,
           tryingBlock,
           rolling;

        // one bool to rule then all
        public bool actions { get { return stepUp || climbUp || jumpOver || quickTurn || hitReaction || hitRecoil || rolling; } }

        /// <summary>
        ///  DISABLE ACTIONS - call this method in canse you need to turn every action bool false
        /// </summary>
        protected void DisableActions()
        {
            // blocking = false;
            strafing = false;
            jumpOver = false;
            stepUp = false;
            crouch = false;
            climbUp = false;
            quickTurn = false;
            rolling = false;
        }
        #endregion

        #endregion

        public void Init()
        {
            if (randomWaypoint == null)
                randomWaypoint = new vFisherYatesRandom();
            if (randomPatrolPoint == null)
                randomPatrolPoint = new vFisherYatesRandom();
            currentWaypoint = -1;
            currentPatrolPoint = -1;
            fwd = transform.forward;
            destination = transform.position;
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agentPath = new NavMeshPath();
            sphereSensor = GetComponentInChildren<v_AISphereSensor>();
            if (sphereSensor)
                sphereSensor.SetColliderRadius(maxDetectDistance);
            animator = GetComponent<Animator>();
            meleeManager = GetComponent<vMeleeManager>();
            canAttack = true;
            attackCount = 0;
            sideMovement = GetRandonSide();
            destination = transform.position;

            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = false;
            _rigidbody.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            _capsuleCollider = GetComponent<CapsuleCollider>();

            healthSlider = GetComponentInChildren<v_SpriteHealth>();
            head = animator.GetBoneTransform(HumanBodyBones.Head);
            oldPosition = transform.position;
            currentHealth = maxHealth;

            method = OffMeshLinkMoveMethod.Grounded;

            // set up Actions
            jumpOverLayer = NavMesh.GetAreaFromName("JumpOver");
            stepUpLayer = NavMesh.GetAreaFromName("StepUp");
            climbUpLayer = NavMesh.GetAreaFromName("ClimbUp");
            useJumpOver = ActionIsEnabled(AIActions.JumpOver);
            useStepUp = ActionIsEnabled(AIActions.StepUp);
            useClimbUp = ActionIsEnabled(AIActions.ClimbUp);
            startPosition = transform.position;
        }

        #region AI Locomotion

        public bool OnCombatArea
        {
            get
            {
                if (target == null) return false;
                var inFloor = Vector3.Distance(new Vector3(0, transform.position.y, 0), new Vector3(0, target.position.y, 0)) < 1.5f;
                return (inFloor && agressive && TargetDistance <= strafeDistance && !agent.isOnOffMeshLink);
            }
        }

        public bool OnStrafeArea
        {
            get
            {
                if (!canSeeTarget())
                {
                    strafing = false;
                    return false;
                }

                if (target == null || !agressive) return false;
                if (currentState.Equals(AIStates.PatrolSubPoints)) return false;

                var inFloor = Vector3.Distance(new Vector3(0, transform.position.y, 0), new Vector3(0, target.position.y, 0)) < 1.5f;
                // exit strafe 
                if (strafing)
                    strafing = TargetDistance < (strafeDistance + 2f);
                // enter strafe                 
                else
                    strafing = OnCombatArea;

                return inFloor ? strafing : false;
            }
        }

        public bool AgentDone()
        {
            if (!agent.enabled)
                return true;
            return !agent.pathPending && AgentStopping();
        }

        public bool AgentStopping()
        {
            if (!agent.enabled || !agent.isOnNavMesh)
                return true;
            return agent.remainingDistance <= agent.stoppingDistance;
        }

        public int GetRandonSide()
        {
            var side = UnityEngine.Random.Range(-1, 1);
            if (side < 0)
            {
                side = -1;
            }
            else side = 1;
            return side;
        }

        protected void CheckGroundDistance()
        {
            if (_capsuleCollider != null && _rigidbody.useGravity)
            {
                // radius of the SphereCast
                float radius = _capsuleCollider.radius * 0.9f;
                var dist = 10f;
                // position of the SphereCast origin starting at the base of the capsule
                Vector3 pos = transform.position + Vector3.up * (_capsuleCollider.radius);
                // ray for RayCast
                Ray ray1 = new Ray(transform.position + new Vector3(0, _capsuleCollider.height / 2, 0), Vector3.down);
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

                if (!agent.enabled && !actions && !rolling && groundDistance < 0.3f)
                {
                    _rigidbody.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                    _rigidbody.useGravity = false;
                    if (currentHealth > 0)
                        agent.enabled = true;
                }
            }
        }

        public void CheckAutoCrouch()
        {
            // radius of SphereCast
            float radius = _capsuleCollider.radius * 0.9f;
            // Position of SphereCast origin stating in base of capsule
            Vector3 pos = transform.position + Vector3.up * ((_capsuleCollider.height * 0.5f) - _capsuleCollider.radius);
            // ray for SphereCast
            Ray ray2 = new Ray(pos, Vector3.up);
            RaycastHit groundHit;
            // sphere cast around the base of capsule for check ground distance
            //if (Physics.SphereCast(ray2, radius, out groundHit, _capsuleCollider.bounds.max.y - (_capsuleCollider.radius * 0.1f), groundLayer))
            if (Physics.SphereCast(ray2, radius, out groundHit, headDetect - (_capsuleCollider.radius * 0.1f), autoCrouchLayer))
                crouch = true;
            else
                crouch = false;
        }
        
        /// <summary>
        /// Waypoint WaitTime
        /// </summary>
        /// <param name="waitTime"></param>
        public void ReturnWaitTime(float waitTime)
        {
            wait = waitTime;
        }

        #endregion

        #region Check Target       

        /// <summary>
        /// Calculate Fov Angle
        /// </summary>
        /// <returns></returns>
        public bool onFovAngle()
        {
            if (target == null) return false;
            var freeRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
            var newAngle = freeRotation.eulerAngles - transform.eulerAngles;
            fovAngle = newAngle.NormalizeAngle().y;

            if (fovAngle < fieldOfView && fovAngle > -fieldOfView)            
                return true;                            

            return false;
        }
        
        /// <summary>
        /// Target Detection
        /// </summary>
        /// <param name="_target"></param>
        /// <returns></returns>
        public bool canSeeTarget()
        {
            if (target == null || !agressive)
                return false;
            if (TargetDistance > maxDetectDistance)
                return false;
            if (colliderTarget == null) colliderTarget = target.GetComponent<Collider>();
            if (colliderTarget == null) return false;
            var top = new Vector3(colliderTarget.bounds.center.x, colliderTarget.bounds.max.y, colliderTarget.bounds.center.z);
            var bottom = new Vector3(colliderTarget.bounds.center.x, colliderTarget.bounds.min.y, colliderTarget.bounds.center.z);
            var offset = Vector3.Distance(top, bottom) * 0.15f;
            top.y -= offset;
            bottom.y += offset;

            if (!onFovAngle() && TargetDistance > minDetectDistance)
                return false;

            //Debug.DrawLine(head.position, top, Color.blue, .1f);
            //Debug.DrawLine(head.position, bottom, Color.blue, .1f);
            //Debug.DrawLine(head.position, colliderTarget.bounds.center, Color.blue, .1f);

            RaycastHit hit;
            if (Physics.Linecast(head.position, top, out hit, sphereSensor.obstacleLayer) &&
            Physics.Linecast(head.position, bottom, out hit, sphereSensor.obstacleLayer) &&
            Physics.Linecast(head.position, colliderTarget.bounds.center, out hit, sphereSensor.obstacleLayer))            
                return false;

            return true;
        }               

        public float TargetDistance
        {
            get
            {
                if (target != null)
                    return Vector3.Distance(transform.position, target.position);
                return maxDetectDistance + 1f;
            }
        }

        public Transform headTarget
        {
            get
            {
                if (target != null && target.GetComponent<Animator>() != null)
                    return target.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
                else
                    return null;
            }
        }

        #endregion       

        #region AI Health

        public void CheckHealth()
        {
            // If the player has lost all it's health and the death flag hasn't been set yet...
            if (currentHealth <= 0 && !isDead)
            {
                //if (vSpawnEnemies.instance != null) vSpawnEnemies.instance.CheckEnemyAlive(this);
                isDead = true;
                DisableActions();
            }
        }

        public void HealthRecovery()
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

        protected void RemoveComponents()
        {
            if (_capsuleCollider != null) Destroy(_capsuleCollider);
            if (_rigidbody != null) Destroy(_rigidbody);
            if (animator != null) Destroy(animator);
            if (agent != null) Destroy(agent);
            var comps = GetComponents<MonoBehaviour>();
            foreach (Component comp in comps) Destroy(comp);
        }

        /// <summary>
        /// APPLY DAMAGE - call this method by a SendMessage with the damage value
        /// </summary>
        /// <param name="damage"> damage to apply </param>
        public override void TakeDamage(Damage damage)
        {
            // become agressive and starts to chase the target
            SetAgressive(true);
            // ignore damage if the character is rolling
            if (rolling || currentHealth <= 0) return;
            if (canSeeTarget() && !damage.ignoreDefense && !actions && CheckChanceToRoll()) return;
            // change to block an attack
            StartCoroutine(CheckChanceToBlock(chanceToBlockAttack, 0));
            // defend attack behaviour
            if (canSeeTarget() && BlockAttack(damage)) return;
            // instantiate hit particle 
            var hitRotation = Quaternion.LookRotation(new Vector3(transform.position.x, damage.hitPosition.y, transform.position.z) - damage.hitPosition);
            SendMessage("TriggerHitParticle", new HittEffectInfo(new Vector3(transform.position.x, damage.hitPosition.y, transform.position.z), hitRotation, damage.attackName), SendMessageOptions.DontRequireReceiver);
            // apply damage to the health
            currentHealth -= damage.value;
            currentHealthRecoveryDelay = healthRecoveryDelay;
            // apply tag from the character that hit you and start chase
            if (!sphereSensor.passiveToDamage && damage.sender != null)
            {
                target = damage.sender;
                currentState = AIStates.Chase;
                sphereSensor.SetTagToDetect(damage.sender);
                if (meleeManager != null)
                    meleeManager.SetTagToDetect(damage.sender);
            }
            // trigger hit sound 
            if (damage.sender != null)
                damage.sender.SendMessage("PlayHitSound", SendMessageOptions.DontRequireReceiver);
            // update the HUD display
            if (healthSlider != null)
                healthSlider.Damage(damage.value);
            // trigger the HitReaction when the AI take the damage
            var hitReactionConditions = stepUp || climbUp || jumpOver || quickTurn || rolling;
            if (animator != null && animator.enabled && !damage.activeRagdoll && !hitReactionConditions)
            {
                animator.SetInteger("Recoil_ID", damage.recoil_id);
                animator.SetTrigger("HitReaction");
            }
            // turn the ragdoll on if the weapon is checked with 'activeRagdoll' 
            if (damage.activeRagdoll)
                transform.SendMessage("ActivateRagdoll", SendMessageOptions.DontRequireReceiver);

            CheckHealth();
        }

        #endregion

        #region AI Actions

        public GameObject CheckActionObject()
        {
            GameObject _object = null;

            RaycastHit hitInfoAction;
            Vector3 yOffSet = new Vector3(0f, actionRayHeight, 0f);
            OffMeshLinkData data = agent.currentOffMeshLinkData;
            Ray ray = new Ray(transform.position + yOffSet, new Vector3(data.endPos.x, transform.position.y, data.endPos.z) - transform.position);

            //Debug.DrawRay(transform.position + yOffSet, ray.direction*distanceOfRayActionTrigger, Color.blue, 0.1f);
            if (Physics.Raycast(ray, out hitInfoAction, distanceOfRayActionTrigger, actionLayer))
            {
                _object = hitInfoAction.transform.gameObject;
            }

            return _object;
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

        /// <summary>
        /// Sets the active action with delay tme
        /// </summary>
        /// <returns>The active action.</returns>
        /// <param name="aiAction">Ai action.</param>
        /// <param name="value">If set to <c>true</c> value.</param>
        /// <param name="delay">Delay.</param>
        protected IEnumerator SetActiveAction(AIActions aiAction, bool value, float delay)
        {
            timeToEnableAction = delay;
            yield return new WaitForSeconds(delay);

            SetActiveAction(aiAction, value);
        }

        /// <summary>
        /// Sets the active action.
        /// </summary>
        /// <param name="aiAction">Ai action.</param>
        /// <param name="value">If set to <c>true</c> value.</param>
        protected void SetActiveAction(AIActions aiAction, bool value)
        {
            var layer = -1;
            switch (aiAction)
            {
                case AIActions.JumpOver:
                    layer = jumpOverLayer;
                    if (value == true && !useJumpOver)
                        return;
                    if (value == true && layer > 1 && !JumpOverIsActive)
                        agent.areaMask = agent.areaMask | (1 << layer);
                    else if (value == false && layer > 1 && JumpOverIsActive)
                        agent.areaMask ^= (1 << layer);
                    JumpOverIsActive = value;
                    break;
                case AIActions.StepUp:
                    layer = stepUpLayer;
                    if (value == true && !useStepUp)
                        return;
                    if (value == true && layer > 1 && !StepUpIsActive)
                        agent.areaMask = agent.areaMask | (1 << layer);
                    else if (value == false && layer > 1 && StepUpIsActive)
                        agent.areaMask ^= (1 << layer);
                    StepUpIsActive = value;
                    break;
                case AIActions.ClimbUp:
                    layer = climbUpLayer;
                    if (value == true && layer > 1 && !ClimbUpIsActive)
                        agent.areaMask = agent.areaMask | (1 << layer);
                    else if (value == false && layer > 1 && ClimbUpIsActive)
                        agent.areaMask ^= (1 << layer);
                    if (value == true && !useClimbUp)
                        return;
                    ClimbUpIsActive = value;
                    break;
            }
        }

        protected bool ActionIsEnabled(AIActions aiAction)
        {
            var value = false;
            switch (aiAction)
            {
                case AIActions.JumpOver:
                    value = JumpOverIsActive;
                    break;
                case AIActions.StepUp:
                    value = StepUpIsActive;
                    break;
                case AIActions.ClimbUp:
                    value = ClimbUpIsActive;
                    break;
            }
            return value;
        }

        protected void DisableAction(AIActions aiAction)
        {
            switch (aiAction)
            {
                case AIActions.ClimbUp:
                    if (climbUp == true)
                        StartCoroutine(SetActiveAction(aiAction, true, 8f));
                    climbUp = false;
                    break;
                case AIActions.JumpOver:
                    if (jumpOver == true)
                        StartCoroutine(SetActiveAction(aiAction, true, 5f));
                    jumpOver = false;
                    break;
                case AIActions.StepUp:
                    if (stepUp == true)
                        StartCoroutine(SetActiveAction(aiAction, true, 5f));
                    stepUp = false;
                    break;
            }
            _capsuleCollider.isTrigger = false;
            _rigidbody.useGravity = true;
            agent.enabled = true;
            quickTurn = false;
        }

        protected enum AIActions
        {
            JumpOver, ClimbUp, StepUp
        }        

        #endregion

        #region AI Melee Combat

        /// <summary>
        /// knows when the character is making an attack
        /// </summary>
        public void InAttacking()
        {
            inAttack = true;
        }

        /// <summary>
        /// knows when you have exit the Attack state
        /// </summary>
        public void OnAttackExit()
        {
            inAttack = false;
        }

        protected bool CheckChanceToRoll()
        {
            if (inAttack) return false;
            System.Random random = new System.Random();

            var randomRoll = (float)random.NextDouble();
            if (randomRoll < chanceToRoll && randomRoll > 0 && target != null)
            {
                sideMovement = GetRandonSide();
                Ray ray = new Ray(target.position, target.right * sideMovement);
                ray = new Ray(transform.position, ray.GetPoint(2f) - transform.position);

                rollDirection = ray.GetPoint(10f);
                //Debug.DrawLine(transform.position, rollDirection, Color.red, 10f);
                rolling = true;
                return true;
            }
            return false;
        }

        protected IEnumerator CheckChanceToBlock(float chance, float timeToEnter)
        {
            tryingBlock = true;
            System.Random random = new System.Random();

            var randomBlock = (float)random.NextDouble();
            if (randomBlock < chance && randomBlock > 0 && !blocking)
            {
                if (timeToEnter > 0)
                    yield return new WaitForSeconds(timeToEnter);
                blocking = target == null || (actions && !quickTurn) || inAttack ? false : true;
                StartCoroutine(ResetBlock());
                tryingBlock = false;
            }
            else
            {
                tryingBlock = false;
            }
        }

        protected IEnumerator ResetBlock()
        {
            yield return new WaitForSeconds(target == null ? 0 : raiseShield);
            blocking = false;
        }

        /// <summary>
        /// Find the combat ID
        /// </summary>
        /// <param name="id"></param>
        public void SetCombatID(CombatID id)
        {
            combatID = id;
        }

        protected virtual void SetAgressive(bool value)
        {
            agressive = value;
        }

        bool BlockAttack(Damage damage)
        {
            // trigger the hitReaction animation if the character is blocking
            var defenseRangeConditions = meleeManager != null ? (meleeManager.CurrentMeleeDefense() != null ? meleeManager.CurrentMeleeDefense().AttackInDefenseRange(damage.sender) : false) : false;

            if (blocking && (animator != null && animator.enabled) && !damage.ignoreDefense && defenseRangeConditions)
            {
                var recoil_id = damage.recoil_id;
                var targetRecoil_id = (meleeManager.CurrentMeleeDefense() != null ? meleeManager.CurrentMeleeDefense().Recoil_ID : 0);
                var hitRecoilContidions = stepUp || climbUp || jumpOver || quickTurn || rolling;

                if (!hitRecoilContidions)
                    SendMessage("TriggerRecoil", recoil_id, SendMessageOptions.DontRequireReceiver);
                if (meleeManager.CurrentMeleeDefense().breakAttack && !hitRecoilContidions && damage.sender != null)
                    damage.sender.SendMessage("TriggerRecoil", targetRecoil_id, SendMessageOptions.DontRequireReceiver);

                var damageResult = GetDamageResult(damage.value, meleeManager.CurrentMeleeDefense().defenseRate);
                currentHealth -= damageResult;

                meleeManager.CurrentMeleeDefense().PlayDEFSound();
                if (damage.sender != null && damageResult > 0)
                    damage.sender.SendMessage("PlayHitSound", SendMessageOptions.DontRequireReceiver);

                if (healthSlider != null)
                    healthSlider.Damage(damageResult);
                currentHealthRecoveryDelay = healthRecoveryDelay;
                CheckHealth();
                return true;
            }
            return false;
        }

        int GetDamageResult(int damage, float defenseRate)
        {
            int result = (int)(damage - ((damage * defenseRate) / 100));
            return result;
        }

        #endregion

        #region Ragdoll

        public override void ResetRagdoll()
        {
            oldPosition = transform.position;
            ragdolled = false;
        }

        public override void EnableRagdoll()
        {
            agent.enabled = false;
            ragdolled = true;
            _capsuleCollider.isTrigger = true;

        }

        public override void RagdollGettingUp()
        {
            agent.enabled = true;
            _capsuleCollider.isTrigger = false;
        }

        #endregion
    }
}
