using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Invector
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]

    public class vHitBox : MonoBehaviour
    {
        public HitBarPoints hitBarPoint;
        [HideInInspector]
        public vMeleeWeapon hitControl;
        private BoxCollider box;
        private Rigidbody rgd;
        //this variable is used to limit to one hit per activation        
        private List<GameObject> hitObjects;

        void Start()
        {
            hitObjects = new List<GameObject>();
            rgd = GetComponent<Rigidbody>();
            box = GetComponent<BoxCollider>();
            rgd.isKinematic = true;
            rgd.useGravity = false;
            rgd.constraints = RigidbodyConstraints.FreezeAll;
            box.isTrigger = true;
            this.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            if(hitObjects!=null)
                 hitObjects.Clear();
        }

        void OnDisable()
        {
            if (hitObjects != null)
                hitObjects.Clear();
        }

        void OnTriggerEnter(Collider other)
        {            
            if (hitControl != null && hitControl.isActive && !hitObjects.Contains(other.gameObject))
            {
                hitObjects.Add(other.gameObject);
                CheckHitProperties(other);
            }
            else if(this.gameObject.activeSelf) this.gameObject.SetActive(false);
        }  
              
        protected void CheckHitProperties(Collider other)
        {
            var inDamage = false;
            var inRecoil = false;
          
            if (hitControl.hitProperties.hitRecoilLayer == (hitControl.hitProperties.hitRecoilLayer | (1 << other.gameObject.layer)))
                inRecoil = true;

            if (hitControl.hitProperties.hitDamageTags == null || hitControl.hitProperties.hitDamageTags.Count == 0)
                inDamage = true;
            else if (hitControl.hitProperties.hitDamageTags.Contains(other.tag))
                inDamage = true;

            if (inDamage == true)
            {
                SendMessageUpwards("OnDamageHit", new HitInfo(other, transform.position, hitBarPoint), SendMessageOptions.DontRequireReceiver);
            }

            if (inRecoil == true)
            {
                if (hitBarPoint == HitBarPoints.Bottom || hitBarPoint == HitBarPoints.Center)
                {
                    SendMessageUpwards("OnRecoilHit", new HitInfo(other, transform.position, hitBarPoint), SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        [System.Serializable]
        public class HitInfo
        {
            public HitBarPoints hitBarPoint;
            public Vector3 hitPoint;
            public Collider hitCollider;
            public HitInfo(Collider hitCollider, Vector3 hitPoint, HitBarPoints hitBarPoint)
            {
                this.hitCollider = hitCollider;
                this.hitPoint = hitPoint;
                this.hitBarPoint = hitBarPoint;
            }
        }
    }
}