using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BattleRam : MonoBehaviour
{

    bool relaxed = true;
    public Animator axelAnimator;
    public bool debug = false;
    public float animationDelay = 1f;
    public GameManager gameManager;

    bool spawned = false;

    public float spawnDelay = 5f;
    public float groundY = 0.17f;

    public float attackMinTime = 3f;
    public float attackMaxTime = 6f;

    public float rangeMin = 10f;
    public float rangeMax = 20f;

    public float totalHealth = 200f;
    float health;
    public AudioClip deathSound;
    public AudioClip damageSound;
    public AudioClip attackSound;

    public Slider healthSlider;
    public Image healthSliderBackground;

    Vector3 targetPosition;

    Transform enemyTarget;

    NavMeshAgent agent;
    NavMeshHit closestHit;

    float ramDamage = 300f;

    bool dead = false;
    bool reachedTarget = false;

    float timer = 2f;


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

        spawned = true;
        Invoke("FinishSpawning", spawnDelay);

        enemyTarget = gameManager.GetRamTarget();
        targetPosition = new Vector3(enemyTarget.position.x, 0, enemyTarget.position.z);
        agent.SetDestination(targetPosition);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameover) dead = true;
        if (dead)
        {
            healthSliderBackground.color = new Color(healthSliderBackground.color.r, healthSliderBackground.color.g, healthSliderBackground.color.b, healthSliderBackground.color.a - 0.02f);
            return;
        }

        if (reachedTarget)
        {
            timer -= Time.deltaTime;
            if(timer < 0)Attack();
        }

        if (debug)
        {
            ReceiveDamage(10);
            debug = false;
            //Shoot();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == enemyTarget)
        {
            agent.isStopped = true;
            reachedTarget = true;
        }
    }

    void Attack()
    {
        timer = Random.Range(attackMinTime, attackMaxTime);
        axelAnimator.SetTrigger("Strike");
        Invoke("DealDamage", animationDelay);
    }

    public void DealDamage()
    {
        gameManager.ReceiveDamage(ramDamage, 1, transform.position);
        AudioSource.PlayClipAtPoint(attackSound, transform.position);
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
