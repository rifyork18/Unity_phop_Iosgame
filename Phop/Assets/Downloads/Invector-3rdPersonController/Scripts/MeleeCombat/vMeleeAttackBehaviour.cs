using UnityEngine;
using System.Collections;
using Invector;
public class vMeleeAttackBehaviour : StateMachineBehaviour
{       
    [Tooltip("normalizedTime of Active Damage")]   
    public float startDamage = 0.05f;
    [Tooltip("normalizedTime of Disable Damage")]
    public float endDamage = 0.9f;
    [Tooltip("Allow the player to move again")]
    public float allowMovementAt = 0.9f;
    public float damageMultiplier =1;
    [Tooltip("Set the reaction/recoil animation for the target if the defense is not check with BreakAttack")]
    public int recoil_ID;
    [Tooltip("Set what limb the attack will come from")]
    public HitboxFrom hitboxFrom;
    [Tooltip("Check this bool on every LAST attack state animation to prevent attacking again right after the last attack")]
    public bool resetTrigger;    
    public string attackName;
	[HideInInspector]
	public bool isActive;
    public bool debug;

    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (debug) Debug.Log(animator.name+" attack " + attackName + " stateEnter");
        animator.gameObject.SendMessage("OnAttackEnter", hitboxFrom, SendMessageOptions.DontRequireReceiver);
    }        

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.SendMessage("InAttacking", SendMessageOptions.DontRequireReceiver);

		if (stateInfo.normalizedTime >= startDamage && stateInfo.normalizedTime <= endDamage && !isActive)
        {
            if (debug) Debug.Log(animator.name + " attack " + attackName + " enable damage in " + System.Math.Round(stateInfo.normalizedTime,2));
            isActive = true;
			animator.gameObject.SendMessage("EnableDamage", new AttackObject(hitboxFrom, recoil_ID, damageMultiplier, true, attackName), SendMessageOptions.DontRequireReceiver);
        }
		else if(stateInfo.normalizedTime > endDamage && isActive )			
		{
            if (debug) Debug.Log(animator.name +" attack " + attackName + " disable damage in " + System.Math.Round(stateInfo.normalizedTime, 2));
            isActive = false;
            animator.gameObject.SendMessage("FinishAttack", SendMessageOptions.DontRequireReceiver);
            animator.gameObject.SendMessage("EnableDamage", new AttackObject(hitboxFrom, recoil_ID, 0, false,attackName), SendMessageOptions.DontRequireReceiver);
        }
        if (stateInfo.normalizedTime > allowMovementAt)
        {
            animator.gameObject.SendMessage("OnAttackExit", SendMessageOptions.DontRequireReceiver);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {      
        animator.gameObject.SendMessage("OnAttackExit", SendMessageOptions.DontRequireReceiver);

        if(resetTrigger)
            animator.gameObject.SendMessage("ResetTrigger", SendMessageOptions.DontRequireReceiver);

        if(isActive)
        {
            if (debug) Debug.Log(animator.name + " attack " + attackName + " disable damage in " + System.Math.Round(stateInfo.normalizedTime, 2));
            isActive = false;
            animator.gameObject.SendMessage("FinishAttack", SendMessageOptions.DontRequireReceiver);
            animator.gameObject.SendMessage("EnableDamage", new AttackObject(hitboxFrom, recoil_ID,0, false, attackName), SendMessageOptions.DontRequireReceiver);
        }

        if (debug) Debug.Log(animator.name + " attack " + attackName + " stateExit");
    }
}