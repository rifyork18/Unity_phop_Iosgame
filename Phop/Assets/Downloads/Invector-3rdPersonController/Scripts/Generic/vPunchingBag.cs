using UnityEngine;
using System.Collections;
using Invector;

public class vPunchingBag : MonoBehaviour
{
    public Rigidbody _rigidbody;
    public float forceMultipler = 0.5f;
    public SpringJoint joint;
    public vCharacter character;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        character = GetComponent<vCharacter>();
    }

    public void TakeDamage(Damage damage)
    {
        var point = damage.hitPosition;
        var relativePoint = transform.position;
        relativePoint.y = point.y;
        var forceForward = relativePoint - point;

        if (character != null && joint != null && character.currentHealth < 0)
        {
            joint.connectedBody = null;
            foreach (MonoBehaviour mono in character.gameObject.GetComponentsInChildren<MonoBehaviour>())
                if (mono != this)
                    Destroy(mono);
        }

        if (_rigidbody != null)
        {
            _rigidbody.AddForce(forceForward * (damage.value * forceMultipler), ForceMode.Impulse);
        }
    }
}
