using OVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Transform bomb;
    public Transform grenade;
    public Transform cube;
    public Camera player;

    public List<Transform> ramTargets;
    public List<Transform> potentialTargets;

    public Slider castleHealthSlider;
    public Slider playerHealthSlider;

    public float castleTotalHealth = 1000f;
    public float playerTotalHealth = 500f;
    float castleHealth;
    float playerHealth;

    public AudioClip towerDamageSound;
    public AudioClip playerDamageSound;
    public AudioClip getPointSound;

    public List<Transform> enemies;
    public int waveCount = 0;
    public int waveEnemyCount = 0;
    public int aliveEnemyCount = 0;

    public Image redOverlay;

    float redFlashTimer = 0;
    float lastPlayerDamage;

    public TMP_Text scoreText;
    public int score = 0;

    public int enemyToSpawn = 0;
    float enemySpawnTimer = 5;

    public float enemySpawnDelay = 5f;
    public float newWaveDelay = 5f;


    public Transform enemySpawnLocation;
    public AudioSource getPointAudioSource;
    public AudioSource warhornAudioSource;

    public MessageBoard messageBoard;
    public MessageBoard gameoverBoard;


    public Transform shopSpawnPosition;

    public AudioClip successSound;
    public AudioClip errorSound;

    public bool gameover = false;
    public bool infiniteMoney = false;

    // Start is called before the first frame update
    void Start()
    {
        castleHealth = castleTotalHealth;
        playerHealth = playerTotalHealth;
        if(scoreText!=null)
            scoreText.text = "Current Score: " + score;
    }

    // Update is called once per frame
    void Update()
    {
        if (infiniteMoney && score < 100) score = 100;
        if(redFlashTimer > 0)
        {
            redOverlay.color = new Color(255, 0, 0, -1 * (float)Math.Pow(0.5f * (redFlashTimer - 2f),2f) + lastPlayerDamage / 150);
            redFlashTimer -= Time.deltaTime;
        }else if(redFlashTimer <= 0)
        {
            redFlashTimer = 0;
            redOverlay.color = new Color(0, 0, 0, 0);
        }

        if (gameover) return;
        if(aliveEnemyCount == 0 && enemyToSpawn == 0)
        {
            NewWave();
        }
        enemySpawnTimer -= Time.deltaTime;
        if ((enemySpawnTimer < 0) && enemyToSpawn > 0){
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (gameover) return;
        enemySpawnTimer = enemySpawnDelay;
        int selection = UnityEngine.Random.Range(0, Math.Min(enemies.Count, enemyToSpawn));
        if(selection+1 == 3)
        {
            if(enemyToSpawn == 3)
            {
                selection--;
            }
        }
        enemyToSpawn -= (selection + 1);
        Instantiate(enemies[selection], enemySpawnLocation.position, enemySpawnLocation.rotation);
        aliveEnemyCount++;
    }

    public void NewWave()
    {
        if (gameover) return;
        waveCount++;
        enemyToSpawn = waveEnemyCount = waveCount;
        enemySpawnTimer = 2 * enemySpawnDelay + newWaveDelay;
        Invoke("NewWaveFeedback", newWaveDelay);
    }

    void NewWaveFeedback()
    {
        if (gameover) return;
        warhornAudioSource.Play();
        messageBoard.NewWave(waveCount);
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void DebugSpawnGrenade()
    {
        Instantiate(grenade, new Vector3(0, 2, 0), Quaternion.Euler(0,1,0));
    }

    public void DebugSpawnBomb()
    {
        Instantiate(bomb, new Vector3(0, 2, 0), Quaternion.Euler(0, 1, 0));
    }

    public Transform GetEnemyTarget()
    {
        return potentialTargets[UnityEngine.Random.Range(0, potentialTargets.Count)];
    }

    public Transform GetRamTarget()
    {
        return ramTargets[UnityEngine.Random.Range(0, ramTargets.Count)];
    }

    public void ReceiveDamage(float damage, int type, Vector3 position)
    {
        if (type == 0)
        {
            lastPlayerDamage = damage;
            playerHealth -= damage;
            if (playerHealth < 10)
            {
                playerHealth = 0;
            }
            AudioSource.PlayClipAtPoint(playerDamageSound, position, 3f);
            if(playerHealthSlider!=null)
                playerHealthSlider.value = (playerHealth / playerTotalHealth);
            FlashRedOverlay();
            if (playerHealth <=0)
            {
                GameOver(true);
            }
        }
        else if(type == 1)
        {
            castleHealth -= damage;
            if (castleHealth < 10)
            {
                castleHealth = 0;
            }
            if (castleHealth <= 0)
            {
                GameOver(false);
            }
            AudioSource.PlayClipAtPoint(towerDamageSound, position, 3f);
            if (castleHealthSlider != null)
                castleHealthSlider.value = (castleHealth / castleTotalHealth);
        }
    }

    void GameOver(bool player)
    {
        gameover = true;
        if(gameoverBoard!=null)
            gameoverBoard.Gameover(score, player);
    }

    void FlashRedOverlay()
    {
        redFlashTimer = 2f;
    }

    public void ReportDeath()
    {
        aliveEnemyCount--;
        score++;
        //getPointAudioSource.Play();
        UpdateScoreText();
        //AudioSource.PlayClipAtPoint(getPointSound, player.transform.position);
    }

    void UpdateScoreText()
    {
        if(scoreText!=null)
            scoreText.text = "Current Score: " + score;
    }

    void PlayErrorSound()
    {
        AudioSource.PlayClipAtPoint(errorSound, shopSpawnPosition.position, 1);
    }

    void PlaySuccessSound()
    {
        AudioSource.PlayClipAtPoint(successSound, shopSpawnPosition.position, 1f);
    }

    public void BuyBomb()
    {
        if(score >= 1)
        {
            score--;
            UpdateScoreText();
            Instantiate(bomb, shopSpawnPosition.position, shopSpawnPosition.rotation);
            PlaySuccessSound();
        }
        else
        {
            PlayErrorSound();
        }
    }


    public void BuyGrenade()
    {
        if (score >= 2)
        {
            score-=2;
            UpdateScoreText();
            Instantiate(grenade, shopSpawnPosition.position, shopSpawnPosition.rotation);
            PlaySuccessSound();
        }
        else
        {
            PlayErrorSound();
        }
    }


    public void BuyCube()
    {
        if (score >= 5)
        {
            score-=5;
            UpdateScoreText();
            Instantiate(cube, shopSpawnPosition.position, shopSpawnPosition.rotation);
            PlaySuccessSound();
        }
        else
        {
            PlayErrorSound();
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}
