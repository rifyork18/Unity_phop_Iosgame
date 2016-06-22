using UnityEngine;
using System.Collections;

public class vCollectableMelee : MonoBehaviour
{
    public bool destroyOnDrop;   
    public string message = "Pick up Weapon";
    public string handler = "handler@weaponName";

    private bool usingPhysics;
    SphereCollider _sphere { get {return GetComponent<SphereCollider>(); } set { _sphere = value; } }
    Collider _collider { get { return GetComponent<Collider>(); } set { _collider = value; } }
    Rigidbody _rigidbody { get { return GetComponent<Rigidbody>(); } set { _rigidbody = value; } }

    [HideInInspector] public vMeleeWeapon _meleeWeapon;

	void Start ()
    {
        usingPhysics = true;
        if (_meleeWeapon == null)
            _meleeWeapon = GetComponentInChildren<vMeleeWeapon>();
    }	
	
	void Update ()
    {
        if (_rigidbody.IsSleeping() && usingPhysics)
        {
            usingPhysics = false;
            _collider.enabled = false;
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
        }
	}

    public void EnableMeleeItem()
    {        
        _sphere.enabled = false;
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        _collider.enabled = false;
        _collider.isTrigger = true;
    }

    public void DisableMeleeItem()
    {
        _sphere.enabled = true;
        _collider.enabled = true;
        _collider.isTrigger = false;
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
        usingPhysics = true;
        _meleeWeapon.SetActiveWeapon(false);        
    }
}
