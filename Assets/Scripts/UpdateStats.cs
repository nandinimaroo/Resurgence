using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
public class UpdateStats : MonoBehaviour
{
    public GameObject stats;
    public GameObject Player;
    public GameObject Spawner;
    // Start is called before the first frame update
    // void Start()
    // {

    // }
   void OnGUI()
   {
PlayerMovementScript script=Player.GetComponent<PlayerMovementScript>();
EnemySpawner spawner = Spawner.GetComponent<EnemySpawner>();
TextMeshProUGUI Display=stats.GetComponent<TextMeshProUGUI>();
if(spawner&&script&&Display)

    Display.text="Zombies Captured:"+script.ZombiesCaptured.ToString()+"/"+spawner.TotalZombies+"\nHealth:"+Math.Ceiling(script.health).ToString()+"/100";

   }
}
