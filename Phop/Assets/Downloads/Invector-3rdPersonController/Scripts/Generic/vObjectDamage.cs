using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vObjectDamage : MonoBehaviour 
{    
    public Damage damage;    
    [Tooltip("List of tags that can be hit")]
    public List<string> tags;    
    public bool useTrigger, useCollision;

	void OnCollisionEnter(Collision hit)
	{
        if (!useCollision)
            return;
		if(tags.Contains(hit.transform.tag))
		{
            damage.sender = transform;
			damage.hitPosition = hit.contacts[0].point;
            // apply damage to Character            
            hit.transform.SendMessage ("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
		}
	}

    void OnTriggerEnter(Collider hit)
    {
        if (!useTrigger)
            return;
        if (tags.Contains(hit.transform.tag))
        {
            damage.sender = transform;
			damage.hitPosition = transform.position;
            // apply damage to Character
            hit.transform.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
    }
}

[System.Serializable]
public class Damage
{
    [Tooltip("Apply damage to the Character Health")]
    public int value = 15;
    [Tooltip("Apply damage even if the Character is blocking")]
    public bool ignoreDefense;
    [Tooltip("Activated Ragdoll when hit the Character")]
    public bool activeRagdoll;
    [HideInInspector] public Transform sender;
	[HideInInspector] public Vector3 hitPosition;
    [HideInInspector] public int recoil_id =1;
   	public string attackName;

	public Damage (int value, bool ignoreDefense, bool activeRagdoll, Transform sender, int recoil_ID,Vector3 hitPosition , string attackName = "")
    {
        this.value = value;
        this.ignoreDefense = ignoreDefense;
        this.activeRagdoll = activeRagdoll;
        this.sender = sender;
        this.recoil_id = recoil_ID;
        this.attackName = attackName;
		this.hitPosition = hitPosition;
    }

    public Damage(Damage damage)
    {
        this.value = damage.value;
        this.ignoreDefense = damage.ignoreDefense;
        this.activeRagdoll = damage.activeRagdoll;
        this.sender = damage.sender;
        this.recoil_id = damage.recoil_id;
		this.attackName = damage.attackName;
		this.hitPosition = damage.hitPosition;
    }
}