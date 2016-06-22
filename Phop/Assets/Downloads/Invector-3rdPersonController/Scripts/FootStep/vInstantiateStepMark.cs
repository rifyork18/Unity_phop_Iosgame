using UnityEngine;

public class vInstantiateStepMark : MonoBehaviour
{
    public GameObject stepMark;
    public LayerMask stepLayer;
    public float timeToDestroy = 5f;

    void StepMark(Transform _transform)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + new Vector3(0, 0.1f, 0), -_transform.up, out hit, 1f, stepLayer))
        {
            var angle = Quaternion.FromToRotation(_transform.up, hit.normal);
            if (stepMark != null)
            {
                var step = Instantiate(stepMark, hit.point, angle * _transform.rotation) as GameObject;
                Destroy(step, timeToDestroy);
                //Destroy(gameObject, timeToDestroy);
            }
            else
                Destroy(gameObject, timeToDestroy);
        }
    }
}
