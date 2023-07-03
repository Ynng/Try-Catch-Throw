
using UnityEngine;

public class Tesseract : MonoBehaviour
{
    bool activated = false;
    bool reachedTarget = false;

    float targetHeight = 90f;
    Vector3 targetPosition;
    LightManager lightManager;
    public Transform explosionEffect;
    public AudioClip explosionSound;
    public Light light;
    public float timeToRaise = 3f;
    float timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        lightManager = GameObject.Find("GameManagerer").GetComponent<LightManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < 1)
        {
            activated = true;
            Activate();
        }
        if (activated && !reachedTarget)
        {
            timer += Time.deltaTime;
            if(GetComponent<Rigidbody>().velocity.y < 50f)
                GetComponent<Rigidbody>().AddForce(new Vector3(0, 30, 0));

            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(3f, 3f, 3f), 0.005f);

            //transform.position = Vector3.Lerp(transform.position, targetPosition, timer / timeToRaise);
        }
        if (transform.position.y > targetHeight-10f && !reachedTarget)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            reachedTarget = true;
            light.range = 40f;
            light.intensity = 20f;
            timer = -1f;
        }
        if(activated && reachedTarget)
        {
            timer += Time.deltaTime;

            if (GetComponent<Rigidbody>().velocity.y > -50f && timer > 0f)
                GetComponent<Rigidbody>().AddForce(new Vector3(0, -10, 0));
        }
        if (activated && reachedTarget && transform.position.y < 2f)
        {
            Explode();
        }
    }

    void Explode()
    {
        lightManager.GoToDay(2f);
        Instantiate(explosionEffect, transform.position, transform.rotation);
        AudioSource.PlayClipAtPoint(explosionSound, transform.position, 0.8f);

        Collider[] colliders = Physics.OverlapSphere(transform.position, 50f);
        foreach (Collider nearby in colliders)
        {
            DamageAbsorber da = nearby.GetComponent<DamageAbsorber>();
            if (da != null)
            {
                if (da.type > 1)
                {
                    da.ReceiveDamage(1000f);
                    Instantiate(explosionEffect, da.transform.position, da.transform.rotation);
                }
            }
            Rigidbody rb = nearby.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(500f, transform.position, 100f);
                rb.AddForce(new Vector3(0, 100f, 0));
            }
        }

        Destroy(gameObject);
    }

    void Activate()
    {
        GetComponent<Grabbable>().priority = -10;
        timer = timeToRaise;
        targetPosition = new Vector3(transform.position.x, targetHeight, transform.position.z);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        lightManager.GoToNight();
    }
}
