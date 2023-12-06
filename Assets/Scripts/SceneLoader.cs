using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
         GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
         
   foreach (GameObject gameObject in rootObjects)
        {
            // DontDestroyOnLoad(gameObject);
        }

            if(SceneManager.GetActiveScene().name=="Level1"||SceneManager.GetActiveScene().name=="Level2")
        {
        Cursor.lockState = CursorLockMode.None;
Cursor.visible = true;
Time.timeScale = 1.0f;
        }
        if (SceneManager.GetActiveScene().name=="RetryL1")
        {
            GameStats.Lives--;

            GameStats.Health=100;
            GameStats.BulletsInGun=5;
            GameStats.ReloadsLeft=10;
            GameStats.CaptureScore=0;
            GameStats.ZombiesKilled=0;
            GameStats.ZombiesTreated=0;

        }
 if (SceneManager.GetActiveScene().name=="RetryL12")
        {
            GameStats.Lives--;

            GameStats.Health=100;
            GameStats.BulletsInGun=5;
            GameStats.ReloadsLeft=10;
            GameStats.CaptureScore=0;
            GameStats.ZombiesKilled=0;
            GameStats.ZombiesTreated=0;

        }

        // Iterate through the root objects and make sure they don't get destroyed on scene load
                    SceneManager.LoadScene(sceneName);


    
        Debug.Log(sceneName);
    }

    public void PlaySound(AudioSource h)
    {
        if(!h.isPlaying)
        h.Play();
    }
}
