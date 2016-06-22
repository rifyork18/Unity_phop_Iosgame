using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

namespace Invector
{
    [System.Serializable]
    public abstract class vCharacter : MonoBehaviour
    {
        #region Character Variables

	    [Header("--- Health & Stamina ---")]
	    public float maxHealth = 100f;
	    public float currentHealth;
	    public float healthRecovery = 0f;
	    public float healthRecoveryDelay = 0f;
        public float maxStamina = 100f;
        public float staminaRecovery = 1.2f;

        protected float recoveryDelay;
        protected bool recoveringStamina;
        protected bool canRecovery;
        protected float currentStamina;
        protected float currentHealthRecoveryDelay;

        protected bool isDead;
        public enum DeathBy
        {
            Animation,
            AnimationWithRagdoll,
            Ragdoll
        }

        public DeathBy deathBy = DeathBy.Animation;

        // get the animator component of character
        [HideInInspector] public Animator animator;
        // know if the character is ragdolled or not
        [HideInInspector] public bool ragdolled { get; set; }

        #endregion

		public Transform GetTransform
		{
			get{ return transform; }
		}
 
        public virtual void ResetRagdoll()
        {

        }

        public virtual void RagdollGettingUp()
        {

        }

        public virtual void EnableRagdoll()
        {

        }

        public virtual void TakeDamage(Damage damage)
        {

        }
    }

}