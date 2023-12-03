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
GameStats.CaptureScore=script.ZombiesCaptured;
if(spawner&&script&&Display)

    Display.text="Zombies Captured:"+script.ZombiesCaptured.ToString()+"/"+spawner.TotalZombies+"\nHealth:"+Math.Ceiling(script.health).ToString()+"/100\nReloads Left:"+GameStats.ReloadsLeft+"\nBullets In The Gun:"+GameStats.BulletsInGun;

   }
}

/*
 if (beingHeld)
    {
        for (var i : int = 1; i <= currentClip; i++)
        {
            GUI.DrawTexture(Rect(ammoStartX + ((i - 1) * (ammoSize.x + ammoSpacing)), ammoY, ammoSize.x, ammoSize.y), bulletHudTexture);
        }
        GUI.Label(ammoCountRect, currentExtraAmmo.ToString());
        if (ammoDecorationTexture)
            GUI.DrawTexture( ammoDecorationHudRect, ammoDecorationTexture);
    }

	public TextMeshProUGUI HUD_bullets;

    void OnGUI(){
		if(!HUD_bullets){
			try{
				HUD_bullets = GameObject.Find("HUD_bullets").GetComponent<TextMesh>();
			}
			catch(System.Exception ex){
				// Debug.Log("here");
				print("Couldnt find the HUD_Bullets ->" + ex.StackTrace.ToString());
			}
		}
		if(mls && HUD_bullets)
			HUD_bullets.text = bulletsIHave.ToString() + " - " + bulletsInTheGun.ToString();
				// Debug.Log(bulletsIHave);

		DrawCrosshair();
	}

    	public var HUD_bullets;
	 
public var bulletHudTexture : Texture;

	void OnGUI(){
		Rect ammoCountRect =new  Rect(25,25,50,25);
int ammoStartX  = 100;
int ammoY  = 25;
Vector2 ammoSize =new Vector2(10,25);
int ammoSpacing = 4;

		if(!HUD_bullets){
			try{
				HUD_bullets = GameObject.Find("HUD_bullets").GetComponent<TextMesh>();
			}
			catch(System.Exception ex){
				// Debug.Log("here");
				print("Couldnt find the HUD_Bullets ->" + ex.StackTrace.ToString());
			}
		}
		if(mls && HUD_bullets)
		{
			for (var i : int = 1; i <= bulletsInTheGun; i++)
        {
            GUI.DrawTexture(Rect(ammoStartX + ((i - 1) * (ammoSize.x + ammoSpacing)), ammoY, ammoSize.x, ammoSize.y), bulletHudTexture);
        }
        GUI.Label(ammoCountRect, currentExtraAmmo.ToString());
       

		DrawCrosshair();
	}
    */