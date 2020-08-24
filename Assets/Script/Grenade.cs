using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public Transform explosionEffect;
    public Transform smokeTrail;
    public Transform smokePosition;
    public AudioClip explosionSound;
    public AudioClip pinSound;

    public float explodeDelay = 4f;
    public float explosionRadius = 2f;
    public float explosionForce = 50f;


    public GameObject cap;
    GameObject smokeObject;
    bool activated = false;

    public GameManager gameManager;


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManagerer").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (gameManager.gameover) Destroy(gameObject);
    }

    public void Activate(float explodeDelay)
    {
        if (activated) return;
        Grabbable grabbable = GetComponent<Grabbable>();
        if (grabbable != null)
        {
            grabbable.priority = 20;
        }
        activated = true;
        if (cap != null)
        {
            cap.transform.parent = null;
            cap.transform.position = transform.position;
            cap.GetComponent<Rigidbody>().isKinematic = false;
            cap.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            cap.GetComponent<Rigidbody>().AddForce(cap.transform.up * 2f);
        }
        smokeObject = Instantiate(smokeTrail, transform.position, transform.rotation).gameObject;
        smokeObject.transform.parent = smokePosition;

        if(pinSound == null)
        {
            GetComponent<AudioSource>().Play();
        }
        else
        {
            AudioSource.PlayClipAtPoint(pinSound, smokePosition.position);
        }

        Invoke("Explode", explodeDelay);
    }

    public void Activate()
    {
        Activate(explodeDelay);
    }

    public void Explode()
    {
        Instantiate(explosionEffect, transform.position, transform.rotation);
        AudioSource.PlayClipAtPoint(explosionSound, transform.position, 0.8f);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        RaycastHit hit;
        foreach (Collider nearby in colliders)
        {

            //if(Physics.Raycast(transform.position, (nearby.transform.position - transform.position), out hit, explosionRadius))
            //{
            //    if(hit.collider == nearby)
            //    {
            DamageAbsorber da = nearby.GetComponent<DamageAbsorber>();
            if(da != null)
            {
                da.ReceiveDamage(explosionForce * ((2 * explosionRadius - (nearby.transform.position - transform.position).magnitude) / explosionRadius));
            }
            //    }
            //}
            Rigidbody rb = nearby.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        if(cap!=null)
            Destroy(cap);
        Destroy(smokeObject);
        Destroy(gameObject);
    }
}
