using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    public float priority = 10f;
    float originalMass;
    int originalLayer;
    float finalVerticalVel = 0.5f;
    Vector3 targetPosition;
    float targetTime = 0;
    float timeRemaining;
    Transform targetTransform;
    float gravity = -9.8f;

    float velX, velY, velZ;
    float letGoTime;

    Rigidbody rigidbody;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        originalMass = rigidbody.mass;
        originalLayer = gameObject.layer;
    }

    // Update is called once per frame
    void Update()
    {
        timeRemaining = targetTime - Time.time;
        
        if (timeRemaining > letGoTime)
        {
            targetPosition = targetTransform.position;
            velX = (targetPosition.x - transform.position.x) / timeRemaining;
            velY = (targetPosition.y - transform.position.y + 0.05f) / timeRemaining - (gravity * timeRemaining) / 2;
            velZ = (targetPosition.z - transform.position.z) / timeRemaining;
            rigidbody.velocity = new Vector3(velX, velY, velZ);
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public float TransferMass()
    {
        SetLayerRecursively(gameObject,12);
        originalMass = rigidbody.mass;
        rigidbody.mass = 0;
        rigidbody.isKinematic = true;
        return originalMass;
    }

    public void RecoverMasss(Vector3 velocity, Vector3 angularVelocity)
    {
        rigidbody.mass = originalMass;
        rigidbody.isKinematic = false;
        rigidbody.velocity = velocity;
        rigidbody.angularVelocity = angularVelocity;
        Invoke("ResetLayer", 0.2f);
    }

    private void ResetLayer()
    {
        SetLayerRecursively(gameObject, originalLayer);
    }

    public void setTarget(Transform targetTransform, float forceGrabTime)
    {
        setTarget(targetTransform, forceGrabTime, -9.8f);
    }

    public void setTarget(Transform targetTransform, float forceGrabTime, float gravity)
    {
        setTarget(targetTransform, forceGrabTime, gravity, 0.3f);
    }

    public void setTarget(Transform targetTransform, float forceGrabTime, float gravity, float letGoTime)
    {
        this.targetTransform = targetTransform;
        targetTime = Time.time + forceGrabTime;
        this.gravity = gravity;
        this.letGoTime = letGoTime;
        //rigidbody.velocity = new Vector3((target.x - transform.position.x) / timeToPlayer, (target.y - transform.position.y) / timeToPlayer - (- 9.8f * timeToPlayer) / 2, (target.z - transform.position.z) / timeToPlayer) ;
        //rigidbody.velocity = rigidbody.velocity + new Vector3(0,(float)System.Math.Pow(9.8f, timeToPlayer) ,0);
    }

}
