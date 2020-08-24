using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Catapult : MonoBehaviour
{

    bool relaxed = true;
    public Animator axelAnimator;
    public Transform weight;
    public bool debug = false;
    public float animationDelay = 0.416f;
    Vector3 weightRotation;
    public GameManager gameManager;

    public Transform thrownPrefab;
    public Transform thrownParent;
    GameObject thrownObject;
    bool spawned = false;

    public float airtimeMultiplier = 1.5f;
    float airtime;

    public float groundY = 0.17f;

    public float turnMinTime = 2f;
    public float turnMaxTime = 4f;
    public float throwMinTime = 3f;
    public float throwMaxTime = 6f;

    public float rangeMin = 10f;
    public float rangeMax = 20f;

    public float objectGravity = -5f;

    public float objectLetGoTime = 0.1f;

    public float maxDistanceTravel = 5f;

    public float totalHealth = 200f;
    float health;
    public AudioClip deathSound;
    public AudioClip damageSound;

    public Slider healthSlider;
    public Image healthSliderBackground;

    Vector3 originalPosition;
    Vector3 targetPosition;
    Vector2 targetPosition2D;
    Quaternion targetRotation;
    Quaternion originalRotation;


    int taskCounter = 0;
    float timer, totalTime;

    Transform enemyTarget;

    NavMeshAgent agent;
    NavMeshHit closestHit;

    bool dead = false;


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManagerer").GetComponent<GameManager>();

        health = totalHealth;
        agent = GetComponent<NavMeshAgent>();
        if (NavMesh.SamplePosition(transform.position, out closestHit, 500, NavMesh.AllAreas))
        {
            transform.position = closestHit.position;
            agent.enabled = true;
        }
        else
        {
            Debug.Log("Where's the navmesh?");
        }
            
        if(weight != null)
            weightRotation = weight.rotation.eulerAngles;
        spawned = true;
    }

    private Vector3 velocity = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameover) dead = true;
        if (dead)
        {
            healthSliderBackground.color = new Color(healthSliderBackground.color.r, healthSliderBackground.color.g, healthSliderBackground.color.b, healthSliderBackground.color.a - 0.02f);
            return;
        }

        if(weight != null)
            weight.rotation = Quaternion.Euler(weightRotation.x, weight.rotation.eulerAngles.y, weightRotation.z);
        if (spawned)
        {
            timer -= Time.deltaTime;
            if(taskCounter == 2)
            {   
                if(agent.velocity.magnitude < 0.001f && timer < -1f)
                {
                    taskCounter++;
                    HandleTask();
                }
            }
            else if (timer < 0)
            {
                taskCounter++;
                HandleTask();
            }
        }

        switch (taskCounter)
        {
            case 1:
            case 3:
                transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, (totalTime - timer) / totalTime);
                break;
            //case 2:
            //    //transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, totalTime);
            //    transform.position = Vector3.Slerp(originalPosition, targetPosition, (totalTime - timer) / totalTime);
            //    break;
        }

        if (debug)
        {
            ReceiveDamage(10);
            debug = false;
            //Shoot();
        }
    }

    void ChooseTarget()
    {
        enemyTarget = gameManager.GetEnemyTarget();
        targetPosition2D = new Vector2(Random.Range(-0.7f, 0.7f), Random.Range(0, -1f));
        targetPosition2D = targetPosition2D.normalized;
        targetPosition2D = targetPosition2D * Random.Range(rangeMin, rangeMax);
        targetPosition2D = targetPosition2D + new Vector2(0, -8);
        targetPosition2D = targetPosition2D + new Vector2(enemyTarget.transform.position.x, enemyTarget.transform.position.z);
        if (targetPosition2D.x > 10 && targetPosition2D.x > -15) targetPosition2D.x = 10;

         Vector2 currentPos = new Vector2(transform.position.x, transform.position.z);
        if ((targetPosition2D - currentPos).magnitude > maxDistanceTravel)
            targetPosition2D = currentPos + ((targetPosition2D - currentPos).normalized * maxDistanceTravel);

        targetPosition = new Vector3(targetPosition2D.x, groundY, targetPosition2D.y);
    }

    void HandleTask()
    {
        switch (taskCounter)
        {
            case 1:

                //spinning to the target to prepare to move
                totalTime = timer = Random.Range(turnMinTime, turnMaxTime);

                ChooseTarget();

                targetPosition = new Vector3(targetPosition2D.x, groundY, targetPosition2D.y);
                targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
                originalRotation = transform.rotation;
                break;
            case 2:
                //moving to the target location
                velocity = Vector3.zero;
                timer = 0;
                //totalTime = timer = Random.Range(moveMinTime, moveMaxTime);
                transform.rotation = originalRotation = targetRotation;
                originalPosition = transform.position;
                agent.SetDestination(targetPosition);
                break;
            case 3:
                totalTime = timer = Random.Range(turnMinTime, turnMaxTime);
                targetRotation = Quaternion.LookRotation( (new Vector3 (enemyTarget.position.x, groundY, enemyTarget.position.z)) - transform.position);
                originalRotation = transform.rotation;
                break;
            case 4:
                totalTime = timer = Random.Range(throwMinTime, throwMaxTime);
                Shoot();
                break;
        }
        if (taskCounter == 4) taskCounter = 0;
    }

    void Shoot()
    {
        axelAnimator.SetTrigger("Shoot");
        thrownObject = Instantiate(thrownPrefab, thrownParent).gameObject;
        thrownObject.GetComponent<Rigidbody>().isKinematic = true;
        Invoke("ReleaseObject", animationDelay);
        //axelAnimation;
        //axelAnimation.PlayQueued();
    }

    public void ReleaseObject()
    {
        thrownObject.transform.parent = null;

        airtime = (new Vector2(thrownObject.transform.position.x, thrownObject.transform.position.z) - new Vector2(enemyTarget.position.x, enemyTarget.position.z)).magnitude * airtimeMultiplier / 10f;

        thrownObject.GetComponent<Rigidbody>().isKinematic = false;
        thrownObject.GetComponent<Grabbable>().setTarget(enemyTarget, airtime, objectGravity, objectLetGoTime);
        
        if(thrownObject.GetComponent<Grenade>() != null)
        {
            thrownObject.GetComponent<Grenade>().Activate(airtime + Random.Range(0.2f, 1f));
        }
    }

    public void ReceiveDamage(float damage)
    {
        health -= damage;
        if (health < 10)
        {
            health = 0;
            healthSlider.value = (health / totalHealth);
            Death();
        }
        else
        {
            healthSlider.value = (health / totalHealth);
            AudioSource.PlayClipAtPoint(damageSound, transform.position);
        }
    }
    
    public void Death()
    {
        health = 0;
        AudioSource.PlayClipAtPoint(deathSound, transform.position);
        agent.enabled = false;
        dead = true;
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

        Destroy(gameObject, Random.Range(2f, 4f));
        gameManager.ReportDeath();
    }
}
