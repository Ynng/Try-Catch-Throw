using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRControllerModel : MonoBehaviour
{
    [HideInInspector]
    public VRController parent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Floating()
    {
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponentInChildren<GrabCone>().enabled = false;

        transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
        transform.GetChild(3).GetComponent<MeshRenderer>().enabled = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        parent.Collided(collision.relativeVelocity.magnitude);
    }
}
