using UnityEngine;
using System.Collections;

namespace Invector
{
    public class v_AIController : v_AIAnimator
    {
        protected virtual void Start()
        {
            Init();
            StartCoroutine(StateRoutine());
            StartCoroutine(OffMeshLinkRoutine());
        }

        protected virtual void Update()
        {            
            ControlLocomotion();
            CheckGroundDistance();
            HealthRecovery();
        }

        #region AI Target
        
        protected void SetTarget()
        {          
            if (currentHealth > 0 && sphereSensor != null)
            {               
                if(target == null || (sphereSensor.getFromDistance))
                {
                    var vChar = sphereSensor.GetTargetvCharacter();
                    if (vChar != null && vChar.currentHealth > 0)
                        target = vChar.GetTransform;
                }
                               
                if (!CheckTargetIsAlive() || TargetDistance > distanceToLostTarget)
                {                    
                    target = null;
                }                                        
            }
            else if (currentHealth <= 0f)
            {
                destination = transform.position;
                target = null;
            }
        }

        bool CheckTargetIsAlive()
        {
            if (target == null) return false;

            var vChar = target.GetComponent<vCharacter>();
            if (vChar == null) return false;
            else if(vChar.currentHealth > 0)            
                return true;
            
            return false;
        }

        #endregion

        #region AI Locomotion

        void ControlLocomotion()
        {
            if (AgentDone())
            {
                agent.speed = 0f;
                combatMovement = Vector3.zero;
            }
            if (agent.isOnOffMeshLink)
            {
                float speed = (method == OffMeshLinkMoveMethod.Action) ? 0f : agent.desiredVelocity.magnitude;
                UpdateAnimator(AgentDone() ? 0f : speed, direction);
            }
            else
            {
                if (OnStrafeArea)
                {
                    quickTurn = false;
                    var destin = transform.InverseTransformDirection(agent.desiredVelocity).normalized;
                    combatMovement = Vector3.Lerp(combatMovement, destin, 2f * Time.deltaTime);
                    UpdateAnimator(AgentDone() ? 0f : combatMovement.z, combatMovement.x);
                }
                else
                {
                    float speed = agent.desiredVelocity.magnitude;
                    combatMovement = Vector3.zero;

                    Vector3 velocity = Quaternion.Inverse(transform.rotation) * AgentDirection();
                    direction = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;

                    var condition = !(AgentDone() && target == null);
                    // turn on spot
                    if (agent.pathStatus != NavMeshPathStatus.PathInvalid && condition && !inAttack)
                    {
                        if ((direction >= 45 || direction <= -45) && !actions && onGround && !crouch)
                            quickTurn = true;
                    }
                    else
                        quickTurn = false;

                    UpdateAnimator(AgentDone() ? 0f : speed, direction);
                }
            }
        }

        Vector3 AgentDirection()
        {
            var forward = AgentDone() ? (target != null && OnStrafeArea && canSeeTarget() ?
                         (new Vector3(destination.x, transform.position.y, destination.z) - transform.position) :
                         transform.forward) : agent.desiredVelocity;

            fwd = Vector3.Lerp(fwd, forward, 20 * Time.deltaTime);
            return fwd;
        }

        protected virtual void UpdateDestination(Vector3 position)
        {
            #region destination routine
            NavMeshHit hit;
            if (agent.enabled && agent.isOnNavMesh && NavMesh.SamplePosition(position, out hit, 0.5f, groundLayer))
            {
                if (agent.enabled && agent.isOnNavMesh && agent.CalculatePath(hit.position, agentPath) || !agent.pathPending)
                {
                    if (agent.enabled && agentPath.status != NavMeshPathStatus.PathInvalid && agentPath.corners.Length > 0)
                        agent.destination = (agentPath.corners[agentPath.corners.Length - 1]);
                    else if (agent.enabled && agent.isOnNavMesh && !agent.isOnOffMeshLink) agent.ResetPath();
                }
                else if (agent.enabled && agent.hasPath && agent.path.status == NavMeshPathStatus.PathInvalid)
                {

                    if (agent.FindClosestEdge(out hit))
                        agent.destination = hit.position;
                    else
                        agent.ResetPath();
                }
            }
            #region debug Path
            if (agent.enabled && agent.hasPath)
            {
                if (drawAgentPath)
                {
                    Debug.DrawLine(transform.position, position, Color.red, 0.5f);
                    var oldPos = transform.position;
                    for (int i = 0; i < agent.path.corners.Length; i++)
                    {
                        var pos = agent.path.corners[i];
                        Debug.DrawLine(oldPos, pos, Color.green, 0.5f);
                        oldPos = pos;
                    }
                }
            }
            #endregion
            #endregion
        }

        protected void CheckIsOnNavMesh()
        {
            // check if the AI is on a valid Navmesh, if not he dies
            if (!agent.isOnNavMesh && agent.enabled && !ragdolled)
            {
                Debug.LogWarning("Missing NavMesh Bake, character will die - Please Bake your navmesh again!");
                currentHealth = 0;
            }
        }

        #endregion

        #region AI States

        protected IEnumerator StateRoutine()
        {
            while (this.enabled)
            {
                CheckIsOnNavMesh();
                CheckAutoCrouch();
                SetTarget();
                yield return new WaitForEndOfFrame();
                switch (currentState)
                {
                    case AIStates.Idle:
                        yield return StartCoroutine(Idle());
                        break;
                    case AIStates.Chase:
                        CheckActions();
                        yield return StartCoroutine(Chase());
                        break;
                    case AIStates.PatrolSubPoints:
                        CheckActions();
                        yield return StartCoroutine(PatrolSubPoints());
                        break;
                    case AIStates.PatrolWaypoints:
                        CheckActions();
                        yield return StartCoroutine(PatrolWaypoints());
                        break;
                }
            }
        }

        protected IEnumerator Idle()
        {
            while (!agent.enabled) yield return null;
            if (canSeeTarget())
                currentState = AIStates.Chase;
            if (agent.enabled && Vector3.Distance(transform.position, startPosition) > agent.stoppingDistance && !((pathArea && pathArea.waypoints.Count > 0)))
                currentState = AIStates.PatrolWaypoints;
            else if ((pathArea && pathArea.waypoints.Count > 0))
                currentState = AIStates.PatrolWaypoints;
            else
                agent.speed = Mathf.Lerp(agent.speed, 0f, 2f * Time.deltaTime);
        }

        protected IEnumerator Chase()
        {
            while (!agent.enabled || currentHealth <= 0) yield return null;
            agent.speed = Mathf.Lerp(agent.speed, chaseSpeed, 2f * Time.deltaTime);
            agent.stoppingDistance = chaseStopDistance;
            distanceToAttack = meleeManager != null && meleeManager.CurrentMeleeAttack() != null ? meleeManager.CurrentMeleeAttack().distanceToAttack : 1f;
            if (!blocking && !tryingBlock) StartCoroutine(CheckChanceToBlock(chanceToBlockInStrafe, lowerShield));

            if (target == null || !agressive)
                currentState = AIStates.Idle;
            if (OnStrafeArea)
            {
                if (ActionIsEnabled(AIActions.ClimbUp))
                    SetActiveAction(AIActions.ClimbUp, false);
                if (ActionIsEnabled(AIActions.StepUp))
                    SetActiveAction(AIActions.StepUp, false);
                if (ActionIsEnabled(AIActions.JumpOver))
                    SetActiveAction(AIActions.JumpOver, false);
            }

            // begin the Attack Routine when close to the Target
            if (TargetDistance <= distanceToAttack + agent.stoppingDistance && meleeManager != null && canAttack)
            {
                canAttack = false;
                if (meleeManager.CurrentMeleeAttack() != null)
                    yield return StartCoroutine(MeleeAttackRotine());
            }
            if (attackCount <= 0 && !inResetAttack && !inAttack)
            {
                StartCoroutine(ResetAttackCount());
                yield return null;
            }
            // strafing while close to the Target
            if (OnStrafeArea && strafeSideways)
            {
                //Debug.DrawRay(transform.position, dir * 2, Color.red, 0.2f);
                if (strafeSwapeFrequency <= 0)
                {
                    sideMovement = GetRandonSide();
                    strafeSwapeFrequency = Random.Range(minStrafeSwape, maxStrafeSwape);
                }
                else
                {
                    strafeSwapeFrequency -= Time.deltaTime;
                }
                fwdMovement = (TargetDistance < distanceToAttack + agent.stoppingDistance) ? (strafeBackward ? -1 : 0) : TargetDistance > distanceToAttack + agent.stoppingDistance ? 1 : 0;
                var dir = ((transform.right * sideMovement) + (transform.forward * fwdMovement));
                Ray ray = new Ray(new Vector3(transform.position.x, target != null ? target.position.y : transform.position.y, transform.position.z), dir);
                destination = OnStrafeArea ? ray.GetPoint(agent.stoppingDistance + 0.5f) : target.position;
            }
            // chase Target
            else
            {
                if (!OnStrafeArea && target != null)
                    destination = target.position;
                else
                {
                    fwdMovement = (TargetDistance < distanceToAttack + agent.stoppingDistance) ? (strafeBackward ? -1 : 0) : TargetDistance > distanceToAttack + agent.stoppingDistance ? 1 : 0;
                    Ray ray = new Ray(transform.position, transform.forward * fwdMovement);
                    destination = (fwdMovement != 0) ? ray.GetPoint(agent.stoppingDistance + ((fwdMovement > 0) ? TargetDistance : 1f)) : transform.position;
                }
            }
            UpdateDestination(destination);
        }

        protected IEnumerator PatrolSubPoints()
        {
            while (!agent.enabled) yield return null;

            if (targetWaypoint)
            {
                if (targetPatrolPoint == null || !targetPatrolPoint.isValid)
                {
                    targetPatrolPoint = GetPatrolPoint(targetWaypoint);
                }
                else
                {
                    agent.speed = Mathf.Lerp(agent.speed, (agent.hasPath && targetPatrolPoint.isValid) ? patrolSpeed : 0, 2f * Time.deltaTime);
                    agent.stoppingDistance = patrollingStopDistance;
                    destination = targetPatrolPoint.isValid ? targetPatrolPoint.position : transform.position;
                    if (Vector3.Distance(transform.position, destination) < targetPatrolPoint.areaRadius && targetPatrolPoint.CanEnter(transform) && !targetPatrolPoint.IsOnWay(transform))
                    {
                        targetPatrolPoint.Enter(transform);
                        wait = targetPatrolPoint.timeToStay;
                        visitedPatrolPoint.Add(targetPatrolPoint);
                    }
                    else if (Vector3.Distance(transform.position, destination) < targetPatrolPoint.areaRadius && (!targetPatrolPoint.CanEnter(transform) || !targetPatrolPoint.isValid)) targetPatrolPoint = GetPatrolPoint(targetWaypoint);

                    if (targetPatrolPoint != null && (targetPatrolPoint.IsOnWay(transform) && Vector3.Distance(transform.position, destination) < distanceToChangeWaypoint))
                    {
                        if (wait <= 0 || !targetPatrolPoint.isValid)
                        {
                            wait = 0;
                            if (visitedPatrolPoint.Count == pathArea.GetValidSubPoints(targetWaypoint).Count)
                            {
                                currentState = AIStates.PatrolWaypoints;
                                targetWaypoint.Exit(transform);
                                targetPatrolPoint.Exit(transform);
                                targetWaypoint = null;
                                targetPatrolPoint = null;
                                visitedPatrolPoint.Clear();
                            }
                            else
                            {
                                targetPatrolPoint.Exit(transform);
                                targetPatrolPoint = GetPatrolPoint(targetWaypoint);
                            }
                        }
                        else if (wait > 0)
                        {
                            if (agent.desiredVelocity.magnitude == 0)
                                wait -= Time.deltaTime;
                        }
                    }
                }
            }
            if (canSeeTarget())
                currentState = AIStates.Chase;
            UpdateDestination(destination);
        }

        protected IEnumerator PatrolWaypoints()
        {
            while (!agent.enabled) yield return null;

            if (pathArea != null && pathArea.waypoints.Count > 0)
            {
                if (targetWaypoint == null || !targetWaypoint.isValid)
                {
                    targetWaypoint = GetWaypoint();
                }
                else
                {
                    agent.speed = Mathf.Lerp(agent.speed, (agent.hasPath && targetWaypoint.isValid) ? patrolSpeed : 0, 2f * Time.deltaTime);
                    agent.stoppingDistance = patrollingStopDistance;

                    destination = targetWaypoint.position;
                    if (Vector3.Distance(transform.position, destination) < targetWaypoint.areaRadius && targetWaypoint.CanEnter(transform) && !targetWaypoint.IsOnWay(transform))
                    {
                        targetWaypoint.Enter(transform);
                        wait = targetWaypoint.timeToStay;
                    }
                    else if (Vector3.Distance(transform.position, destination) < targetWaypoint.areaRadius && (!targetWaypoint.CanEnter(transform) || !targetWaypoint.isValid))
                        targetWaypoint = GetWaypoint();

                    if (targetWaypoint != null && targetWaypoint.IsOnWay(transform) && Vector3.Distance(transform.position, destination) < distanceToChangeWaypoint)
                    {
                        if (wait <= 0 || !targetWaypoint.isValid)
                        {
                            wait = 0;
                            if (targetWaypoint.subPoints.Count > 0)
                                currentState = AIStates.PatrolSubPoints;
                            else
                            {
                                targetWaypoint.Exit(transform);
                                visitedPatrolPoint.Clear();
                                targetWaypoint = GetWaypoint();
                            }
                        }
                        else if (wait > 0)
                        {
                            if (agent.desiredVelocity.magnitude == 0)
                                wait -= Time.deltaTime;
                        }
                    }
                }
                UpdateDestination(destination);
            }
            else if (Vector3.Distance(transform.position, startPosition) > patrollingStopDistance)
            {
                agent.speed = Mathf.Lerp(agent.speed, patrolSpeed, 2f * Time.deltaTime);
                agent.stoppingDistance = patrollingStopDistance;
                UpdateDestination(startPosition);
            }
            if (canSeeTarget())
                currentState = AIStates.Chase;
        }

        #endregion

        #region AI Waypoint & PatrolPoint

        vWaypoint GetWaypoint()
        {
            var waypoints = pathArea.GetValidPoints();

            if (randomWaypoints) currentWaypoint = randomWaypoint.Next(waypoints.Count);
            else currentWaypoint++;

            if (currentWaypoint >= waypoints.Count) currentWaypoint = 0;
            if (waypoints.Count == 0)
            {
                agent.Stop();
                return null;
            }
            if (visitedWaypoint.Count == waypoints.Count) visitedWaypoint.Clear();

            if (visitedWaypoint.Contains(waypoints[currentWaypoint])) return null;

            agent.Resume();
            return waypoints[currentWaypoint];
        }

        vPoint GetPatrolPoint(vWaypoint waypoint)
        {
            var subPoints = pathArea.GetValidSubPoints(waypoint);
            if (waypoint.randomPatrolPoint) currentPatrolPoint = randomPatrolPoint.Next(subPoints.Count);
            else currentPatrolPoint++;

            if (currentPatrolPoint >= subPoints.Count) currentPatrolPoint = 0;
            if (subPoints.Count == 0)
            {
                agent.Stop();
                return null;
            }
            if (visitedPatrolPoint.Contains(subPoints[currentPatrolPoint])) return null;
            agent.Resume();
            return subPoints[currentPatrolPoint];
        }

        #endregion

        #region AI Melee Combat        

        protected IEnumerator MeleeAttackRotine()
        {           
            if (!meleeManager.applyDamage && !actions && attackCount > 0)
            {
                sideMovement = GetRandonSide();
                agent.stoppingDistance = distanceToAttack;
                attackCount--;
                MeleeAttack();
                yield return null;
            }
            //else if (!actions && attackCount > 0) canAttack = true;
        }

        public void FinishAttack()
        {
          //  if(attackCount > 0)
                canAttack = true;
        }

        IEnumerator ResetAttackCount()
        {
           
            inResetAttack = true;
            canAttack = false;
            var value = 0f;
            if (firstAttack)
            {
                firstAttack = false;
                value = firstAttackDelay;
            }
            else value = Random.Range(minTimeToAttack, maxTimeToAttack);
            yield return new WaitForSeconds(value);
            attackCount = randomAttackCount ? Random.Range(1, maxAttackCount + 1) : maxAttackCount;
            canAttack = true;
            inResetAttack = false;
        }

        #endregion

        #region AI Actions

        protected void CheckActions()
        {
            if (timeToEnableAction <= 0 && !OnStrafeArea)
            {
                if (!ActionIsEnabled(AIActions.ClimbUp) && useClimbUp && !actions)
                    SetActiveAction(AIActions.ClimbUp, true);
                if (!ActionIsEnabled(AIActions.StepUp) && useStepUp && !actions)
                    SetActiveAction(AIActions.StepUp, true);
                if (!ActionIsEnabled(AIActions.JumpOver) && useJumpOver && !actions)
                    SetActiveAction(AIActions.JumpOver, true);
            }
            else if (timeToEnableAction > 0 && !actions)
                timeToEnableAction -= Time.deltaTime;
        }

        protected void CheckForwardAction()
        {
            var hitObject = CheckActionObject();
            if (hitObject != null)
            {
                try
                {
                    if (hitObject.CompareTag("ClimbUp"))
                        DoAction(hitObject, ref climbUp, AIActions.ClimbUp);
                    else if (hitObject.CompareTag("StepUp"))
                        DoAction(hitObject, ref stepUp, AIActions.StepUp);
                    else if (hitObject.CompareTag("JumpOver"))
                        DoAction(hitObject, ref jumpOver, AIActions.JumpOver);
                }
                catch (UnityException e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
            else
            {
                if (agent.enabled)
                {
                    agent.ResetPath();
                    method = OffMeshLinkMoveMethod.Grounded;
                }
            }
        }

        void DoAction(GameObject hitObject, ref bool action, AIActions aiAction)
        {
            var triggerAction = hitObject.transform.GetComponent<vTriggerAction>();
            if (!triggerAction)
            {
                Debug.LogWarning("Missing TriggerAction Component on " + hitObject.transform.name + "Object");
                return;
            }

            if (!actions)
            {
                matchTarget = triggerAction.target;
                var rot = hitObject.transform.rotation;
                transform.rotation = rot;
                animator.SetTrigger("ResetState");
                action = true;
                SetActiveAction(aiAction, false);
            }
        }       

        IEnumerator CheckOffMeshLink()
        {
            do
            {
                //Debug.Log(Vector3.Distance(transform.eulerAngles, desiredRotation.eulerAngles));
                method = OffMeshLinkMoveMethod.Grounded;
                yield return new WaitForEndOfFrame();
            }
            while (quickTurn);

            yield return new WaitForEndOfFrame();

            OffMeshLinkData data = agent.currentOffMeshLinkData;
            OffMeshLinkType linkType = data.linkType;
            string offMeshLinkTag = string.Empty;
            if (data.offMeshLink)
                offMeshLinkTag = data.offMeshLink.tag;

            switch (linkType)
            {
                case OffMeshLinkType.LinkTypeDropDown:
                    agent.autoTraverseOffMeshLink = false;
                    method = OffMeshLinkMoveMethod.DropDown;
                    break;
                case OffMeshLinkType.LinkTypeJumpAcross:
                    agent.autoTraverseOffMeshLink = false;
                    method = OffMeshLinkMoveMethod.JumpAcross;
                    break;
                case OffMeshLinkType.LinkTypeManual:
                    switch (offMeshLinkTag)
                    {
                        case "StepUp":
                            agent.autoTraverseOffMeshLink = false;
                            method = OffMeshLinkMoveMethod.Action;
                            break;
                        case "JumpOver":
                            agent.autoTraverseOffMeshLink = false;
                            method = OffMeshLinkMoveMethod.Action;
                            break;
                        case "ClimbUp":
                            agent.autoTraverseOffMeshLink = false;
                            method = OffMeshLinkMoveMethod.Action;
                            break;
                        default:
                            agent.autoTraverseOffMeshLink = false;
                            method = OffMeshLinkMoveMethod.Grounded;
                            break;
                    }
                    break;
            }
        }

        protected IEnumerator OffMeshLinkRoutine()
        {
            while (true)
            {
                if (agent.enabled && agent.isOnOffMeshLink)
                {
                    yield return StartCoroutine(CheckOffMeshLink());
                    yield return new WaitForEndOfFrame();

                    if (method == OffMeshLinkMoveMethod.DropDown)
                        yield return StartCoroutine(Parabola(agent, 1.0f, 0.5f));
                    else if (method == OffMeshLinkMoveMethod.JumpAcross)
                        yield return StartCoroutine(Parabola(agent, 1.0f, 0.5f));
                    else if (method == OffMeshLinkMoveMethod.Action)
                        CheckForwardAction();

                    while (agent.enabled == false)
                        yield return new WaitForEndOfFrame();
                }
                else
                    method = OffMeshLinkMoveMethod.Grounded;
                yield return null;
            }
        }

        IEnumerator NormalSpeed(NavMeshAgent agent)
        {
            OffMeshLinkData data = agent.currentOffMeshLinkData;
            Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
            while (agent.transform.position != endPos)
            {
                //Debug.DrawLine(transform.position, endPos, Color.red);
                transform.position = Vector3.MoveTowards(transform.position, endPos, 6f * Time.deltaTime);
                agent.enabled = false;
                yield return new WaitForEndOfFrame();
            }
            if (agent.enabled && !agent.autoTraverseOffMeshLink)
                agent.CompleteOffMeshLink();
            agent.enabled = true;
        }

        IEnumerator Parabola(NavMeshAgent agent, float height, float duration)
        {
            OffMeshLinkData data = agent.currentOffMeshLinkData;
            Vector3 startPos = agent.transform.position;
            Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;

            float normalizedTime = 0.0f;
            while (normalizedTime < 1.0f)
            {
                float yOffset = height * 4.0f * (normalizedTime - normalizedTime * normalizedTime);
                agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
                normalizedTime += Time.deltaTime / duration;

                yield return null;
            }
            if (agent.enabled && !agent.autoTraverseOffMeshLink)
                agent.CompleteOffMeshLink();
        }

        #endregion       
    }
}
