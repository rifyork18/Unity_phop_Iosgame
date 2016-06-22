using UnityEngine;
using System.Collections;

public class vExplosionDamage : MonoBehaviour
{
    public int recoil_ID;
    public bool oneShot;
    public Damage damage;
    public float radius;
    public float damageFrequence;
    public LayerMask layer;
    private Collider[] colls;
    [HideInInspector]
    public HitProperties hitP;

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        damage.recoil_id = recoil_ID;
        //damage.sender = this.gameObject.transform;
        damage.hitPosition = transform.position;

        if (oneShot)
        {
            colls = Physics.OverlapSphere(transform.position, radius, layer);
            foreach (Collider coll in colls)
            {
                if (hitP.hitDamageTags.Contains(coll.tag))
                    coll.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
        }
        else
        {
            while (true)
            {
                colls = Physics.OverlapSphere(transform.position, radius, layer);
                foreach (Collider coll in colls)
                {
                    if(hitP.hitDamageTags.Contains(coll.tag))
                        coll.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                }
                
                yield return new WaitForSeconds(damageFrequence >= 0.1f ? damageFrequence : 0.5f);
            }
        }
    }

    public void SetHitProperties(HitProperties hitP)
    {
        this.hitP = hitP;        
    }

    public void SetSender(Transform sender)
    {
        damage.sender = sender;
    }
}
