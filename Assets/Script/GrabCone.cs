using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrabCone : MonoBehaviour
{
    [HideInInspector]
    public List<Grabbable> grabTargets = new List<Grabbable>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(grabTargets.Count > 0)
        {
            if(grabTargets[0] == null)
            {
                grabTargets.RemoveAt(0);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Grabbable target = other.GetComponent<Grabbable>();
        if (target != null && other.gameObject.layer == 11)
        {
            if (!grabTargets.Contains(target) && target.priority > 0)
                grabTargets.Add(target);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Grabbable target = other.GetComponent<Grabbable>();
        if (target != null)
        {
            if (grabTargets.Contains(target))
                grabTargets.Remove(target);
        }
    }
}
