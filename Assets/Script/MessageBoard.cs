using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class MessageBoard : MonoBehaviour
{
    public TMP_Text text;
    public TMP_Text text2;

    public float readingY = 5.72f;
    Vector3 targetPosition;

    // Start is called before the first frame update
    void Start()
    {
        targetPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, 0.1f);
    }

    public void NewWave(int wave)
    {
        text.text = "Wave\n" + wave;
        RaiseUp();
        Invoke("BackDown", 4f);
    }

    void RaiseUp()
    {
        targetPosition = new Vector3(transform.position.x, readingY, transform.position.z);
    }

    void BackDown()
    {
        targetPosition = new Vector3(transform.position.x, -5f, transform.position.z);
    }

    public void Gameover(int score, bool player)
    {
        text.text = "Score - " + score;
        if (player)
        {
            text2.text = "Your player died";
        }
        else
        {
            text2.text = "Your castle died";
        }
        RaiseUp();
    }
}
