
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStats 
{
        private static bool isInitialized = false;

    public static int CaptureScore {get;set;}
    public static int Lives {get;set;}
    public static float Health {get;set;}
    public static int ReloadsLeft {get;set;}
    public static float BulletsInGun {get;set;}
    public static int flag {get;set;}
    public static int ZombiesKilled {get;set;}
    public static int ZombiesTreated {get;set;}

    static GameStats()
    {
        Lives=5;
    ZombiesKilled=0;
    ReloadsLeft=10;
    BulletsInGun=5;
    ZombiesTreated=0;
    CaptureScore=0;
            // if (SceneManager.GetActiveScene().name=="Level1")
        if (!isInitialized)
{
    Health=100;
isInitialized=true;
}
    }
}
