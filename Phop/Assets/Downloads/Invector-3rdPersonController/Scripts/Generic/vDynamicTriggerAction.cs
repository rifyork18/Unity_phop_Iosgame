using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class vDynamicTriggerAction : vTriggerAction
{
    [HideInInspector]
    public vBoxTrigger[] boxTriggers;
    public Transform rootTransform;
    public void Start()
    {
        if (rootTransform)
        {
            var colliders = rootTransform.GetComponentsInChildren<Collider>();
            foreach (Collider coll in colliders)
            {
                foreach (vBoxTrigger boxTrigger in boxTriggers)
                {
                    var triggerColl = boxTrigger.GetComponent<Collider>();
                    if (triggerColl != coll)
                    {
                        Physics.IgnoreCollision(triggerColl, coll);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if CanUse this Trigger
    /// Work with trigger box cast
    /// </summary>
    /// <returns></returns>
    public override bool CanUse()
    {
        for (int i = 0; i < boxTriggers.Length; i++)
        {
            if (BoxCast(boxTriggers[i]))
                return false;
        }

        return true;
    }

    bool BoxCast(vBoxTrigger boxCast)
    {
       // Debug.Log("box " + boxCast.name + " use incollision = " + boxCast.inCollision);
        return boxCast.inCollision;
    }
}
