using UnityEngine;
using System.Collections;
using Invector;

public class vHealthItem : MonoBehaviour 
{
    [Tooltip("How much health will be recovery")]
	public float value;
	
	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.tag.Equals("Player")) 
		{
            // access the basic character information
			var iChar = other.GetComponent<vCharacter>();
            // apply value plus the current health
			var targetHealth = iChar.currentHealth + value;
            // heal only if the character's health isn't full
			if(iChar.currentHealth < iChar.maxHealth)
			{
                // limit healing to the max health
                iChar.currentHealth = Mathf.Clamp(targetHealth, 0, iChar.maxHealth);
				Destroy(gameObject);	
			}
            else
            {
                // show message if the character's health is full
                other.SendMessage("ShowText", "Health is Full", SendMessageOptions.DontRequireReceiver);
            }
		}
	}
}
