using UnityEngine;
using System.Collections;

public class vCollisionMessage : MonoBehaviour 
{	
	public Transform root;
    public bool sleeping;

    void Start()
    {
       root = transform.root;
    }

	void OnCollisionEnter(Collision other)
	{
        if (other != null )
		{           
            if(root)
            root.SendMessage("OnRagdollCollisionEnter", new vRagdollCollision(this.gameObject, other), SendMessageOptions.DontRequireReceiver);           
		}
	}

    void Sleep()
    {
        sleeping = false;
    }
}
