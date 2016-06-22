using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Invector;

public class vMeleeWeapon : MonoBehaviour 
{
    #region variables
    public enum MeleeType
    {
        Attack,
        Defense,
        All
    }
    public enum HandEquip
    {
        LeftHand = -1,
        RightHand =1,
        BothHand =2
    }

    public MeleeType meleeType = MeleeType.All;
    public HandEquip handEquip = HandEquip.BothHand;
    [Header("Weapon Settings")]
    public bool useTwoHand;
    public int ATK_ID = 1;
    public int MoveSet_ID;
    public float distanceToAttack = 1f;
    
    public Damage damage;
    public DamagePercentage damagePercentage;

    [Tooltip("How much stamina the attack will consume")]
    public float staminaCost = 30f;
    [Tooltip("Time for the stamina to recover")]
    public float a_staminaRecoveryDelay = 1f;

    [Header("Hitbox Settings")]
    [Range(0.1f, 2.9f)]
    public float centerSize = 1f;
    [Range(-1.4f, 1.4f)]
    public float centerPos = 0f;
    public BoxCollider top, bottom, center;
    public bool showHitboxes;
    public HitProperties hitProperties { get; set; }
    protected float curCenterSize;
    [HideInInspector]
    public vHitBox hitTop, hitBottom, hitCenter;
    public bool lockHitBox = true;      
    
    [Header("Defense Attribute")]
    [Tooltip("Check your Animation State to match the ID")]
    public int DEF_ID = 1;
    public int Recoil_ID = 1;
    public float defenseRate = 100f;
    public float defenseRange = 90f;
    public float d_staminaRecoveryDelay = 1f;
    [Tooltip("Break the attack of the opponent and trigger a recoil animation")]
    public bool breakAttack;
    [Tooltip("Mirror the def animation if equipped on the left hand")]
    public bool mirrorAnimation;    
	public GameObject audioSource;
	public List<AudioClip> hitSounds;
	public List <AudioClip> recoilSounds;
    public List<GameObject> recoilParticles;
	public List<AudioClip> defSounds;
    public void SetActiveWeapon(bool value)
    {
        SetActiveHitBoxes(value);
      
        if (value == false )
			isHit = false;
        active = value;
    }

    private Transform root;
    protected bool active;
    protected bool isHit;
    #endregion   

    [System.Serializable]
    public class DamagePercentage
    {
        public float Top = 30f;
        public float Center = 70f;        
    }

    public void Init()
    {
        var meleeManager = GetComponentInParent<vMeleeManager>();
        if (meleeManager != null)
            root = meleeManager.transform;

        if (meleeType == MeleeType.Attack || meleeType == MeleeType.All)
        {
            hitTop = top.GetComponent<vHitBox>();
            hitBottom = bottom.GetComponent<vHitBox>();
            hitCenter = center.GetComponent<vHitBox>();

            hitTop.hitBarPoint = HitBarPoints.Top;
            hitCenter.hitBarPoint = HitBarPoints.Center;
            hitBottom.hitBarPoint = HitBarPoints.Bottom;

            hitTop.hitControl = this;
            hitBottom.hitControl = this;
            hitCenter.hitControl = this;            
        }
        else
        {
            if (hitTop == null) return;
            hitTop.gameObject.SetActive(false);
            hitBottom.gameObject.SetActive(false);
            hitCenter.gameObject.SetActive(false);
        }
    }   

    public bool isActive { get { return active; } }

    public struct DamageType
    {
        public Collider hitCollider;
        public HitBarPoints hitBarPoints;
	}

	public void PlayHitSound()
	{
		if (!isHit) 
		{
			isHit = true;
			if (audioSource != null && hitSounds.Count > 0) 
			{
				var clip = hitSounds[UnityEngine.Random.Range(0, hitSounds.Count)];
				var audioObj = Instantiate (audioSource, transform.position, transform.rotation) as GameObject;
				audioObj.GetComponent<AudioSource> ().PlayOneShot (clip);
			}
		}
	}

	public void PlayRecoilSound()
	{
		if (!isHit) 
		{
			isHit = true;
			if (audioSource != null && recoilSounds.Count > 0) 
			{
				var clip = recoilSounds[UnityEngine.Random.Range(0, recoilSounds.Count)];
				var audioObj = Instantiate (audioSource, transform.position, transform.rotation) as GameObject;
				audioObj.GetComponent<AudioSource> ().PlayOneShot (clip);
			}
        }
	}

    public void InstantiateParticle(Vector3 pos, Quaternion rot)
    {
        if (recoilParticles.Count > 0)
        {
            var particles = recoilParticles[UnityEngine.Random.Range(0, recoilParticles.Count)];
            if(particles != null)
                Instantiate(particles, pos, rot);
        }
    }
	
	public void PlayDEFSound()
	{
		if (audioSource != null && defSounds.Count > 0) 
		{
			var clip = defSounds[UnityEngine.Random.Range(0, defSounds.Count)];
			var audioObj = Instantiate (audioSource, transform.position, transform.rotation) as GameObject;
			audioObj.GetComponent<AudioSource> ().PlayOneShot (clip);
		}
	}

	void InstantiateSoundObject(List<HitSoundObject> hitSoundObjects,string tag)
	{
		var clips = hitSoundObjects.Find (x => x.tagName.Equals (tag));
		if (clips == null)
			return;
		var clip =  clips.sounds[UnityEngine.Random.Range (0, clips.sounds.Count-1)];
		if (clip) 
		{
			var audioObj = Instantiate (audioSource, transform.position, transform.rotation) as GameObject;
			audioObj.GetComponent<AudioSource> ().PlayOneShot (clip);
		}
	}

    public bool AttackInDefenseRange(Transform damageSender)
    {
        if (damageSender == null) return true;
        var localTarget = root.InverseTransformPoint(damageSender.position);
        var angle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        if (angle <= defenseRange && angle >= -defenseRange) return true;
        return false;
    }

    void SetActiveHitBoxes(bool value)
    {        
        hitBottom.gameObject.SetActive(value);
        hitCenter.gameObject.SetActive(value);
        hitTop.gameObject.SetActive(value);
    }
}

[System.Serializable]
public class HitSoundObject
{
	public string tagName;
	public List<AudioClip> sounds;
}