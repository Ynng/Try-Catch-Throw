using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageAbsorber : MonoBehaviour
{
    public int type = 0;
    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ReceiveDamage(float damage)
    {
        if(type == 2)
        {
            GetComponent<Catapult>().ReceiveDamage(damage);
        }
        else if (type == 3)
        {
            GetComponent<BattleRam>().ReceiveDamage(damage);
        }
        else
        {
            gameManager.ReceiveDamage(damage, type, transform.position);
        }
    }
}
