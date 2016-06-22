using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vHitDamageParticle : MonoBehaviour
{
    public GameObject defaultHitEffect;
    public List<HitEffect> customHitEffects = new List<HitEffect>();

    /// <summary>
    /// Raises the hit event.
    /// </summary>
    /// <param name="hitEffectInfo">Hit effect info.</param>
    public void TriggerHitParticle(HittEffectInfo hitEffectInfo)
    {
        if (string.IsNullOrEmpty(hitEffectInfo.hitName))        
            Instantiate(defaultHitEffect, hitEffectInfo.position, hitEffectInfo.rotation);        
        else
        {
            var hitEffect = customHitEffects.Find(effect => effect.hitName.Equals(hitEffectInfo.hitName) && effect.hitPrefab != null);

            if (hitEffect != null)
                Instantiate(hitEffect.hitPrefab, hitEffectInfo.position, hitEffectInfo.rotation);
            else if (defaultHitEffect != null)            
                Instantiate(defaultHitEffect, hitEffectInfo.position, hitEffectInfo.rotation);            
        }
    }
}

public class HittEffectInfo
{
    public Vector3 position;
    public Quaternion rotation;
    public string hitName;
    public HittEffectInfo(Vector3 position, Quaternion rotation, string hitName = "")
    {
        this.position = position;
        this.rotation = rotation;
        this.hitName = hitName;
    }
}