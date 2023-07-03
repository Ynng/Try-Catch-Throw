using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    public float cloudPlacementRandomness = 10f;

    public float speed = 3f;
    public float limit = 200f;
    public float farLimit = 1000f;
    public float speedRandomness = 0.5f;

    private float realSpeed;
    private void Start()
    {
        speed += Random.Range(-speedRandomness, speedRandomness);
        realSpeed = speed;
        foreach (Transform child in transform){
            child.position += new Vector3(Random.Range(-cloudPlacementRandomness, cloudPlacementRandomness), 0, Random.Range(-cloudPlacementRandomness, cloudPlacementRandomness));
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += new Vector3(-realSpeed, 0, 0) * Time.fixedDeltaTime;
        if(transform.position.x < -1 * limit)
        {
            if(transform.position.x > -1 * farLimit)
            {
                realSpeed *= 1.05f;
            }
            else
            {
                transform.position += new Vector3(farLimit * 2, 0, 0);
            }
        }
        else
        {
            if(realSpeed > speed)
            {
                realSpeed /= 1.05f;
            }
        }
    }
}
