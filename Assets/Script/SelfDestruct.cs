using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float time = 2f;

    void Start()
    {
        Destroy(gameObject, time);
    }
    
}
