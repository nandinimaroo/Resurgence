
using UnityEngine;

public class GameStats 
{
    public static int CaptureScore {get;set;}
    public static int Lives {get;set;}
    public static float Health {get;set;}
    public static int ReloadsLeft {get;set;}
    public static float BulletsInGun {get;set;}
    public static int flag {get;set;}
    public static int ZombiesKilled {get;set;}

    static GameStats()
    {
        Lives=5;
    ZombiesKilled=0;
    }
}
