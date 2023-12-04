using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
public class UpdateStats2 : MonoBehaviour
{
    public GameObject stats;
    public GameObject Player;
    // public GameObject Spawner;
    // Start is called before the first frame update
    // void Start()
    // {

    // }
   void OnGUI()
   {
PlayerMovementScript script=Player.GetComponent<PlayerMovementScript>();
// EnemySpawner spawner = Spawner.GetComponent<EnemySpawner>();
TextMeshProUGUI Display=stats.GetComponent<TextMeshProUGUI>();
// GameStats.CaptureScore=script.ZombiesCaptured;
if(script&&Display)

    // Display.text="Zombies Captured:"+script.ZombiesCaptured.ToString()+"/"+spawner.TotalZombies+"\nHealth:"+Math.Ceiling(script.health).ToString()+"/100\nReloads Left:"+GameStats.ReloadsLeft+"\nBullets In The Gun:"+GameStats.BulletsInGun;
    Display.text="Zombies Treated:"+GameStats.ZombiesTreated+"/"+GameStats.CaptureScore+"\nHealth:"+Math.Ceiling(GameStats.Health).ToString()+"/100\nReloads Left:"+GameStats.ReloadsLeft+"\nBullets In The Gun:"+GameStats.BulletsInGun;

   }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
