using UnityEngine;
using System.Collections;

public class vProjectile : MonoBehaviour
{
    public Vector3 move;
    public GameObject particleOnCollision;
    [HideInInspector]
    public HitProperties hitP;
    private Transform sender;

    void Start()
    {
        var coll = GetComponent<Collider>();
        //yield return new WaitForSeconds(0.1f);
        coll.isTrigger = false;
    }
   
    void Update()
    {
        transform.Translate(move * Time.deltaTime, Space.Self);
    }

    void OnCollisionEnter(Collision other)
    {
        if (particleOnCollision != null)
        {
            var go = Instantiate(particleOnCollision, transform.position, transform.rotation) as GameObject;
            go.SendMessage("SetHitProperties", hitP, SendMessageOptions.DontRequireReceiver);
            go.SendMessage("SetSender", sender, SendMessageOptions.DontRequireReceiver);
        }

        Destroy(gameObject);
    }

    public void SetHitProperties(HitProperties hitP)
    {
        this.hitP = hitP;
    }

    public void SetSender(Transform sender)
    {
        this.sender = sender;
    }
}
