using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class UpdateScore : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale=1;
       if(SceneManager.GetActiveScene().name=="RetryL1")
        {
  TextMeshProUGUI Display=gameObject.GetComponent<TextMeshProUGUI>();
  
  int lives = GameStats.Lives;
    Display.text="You have "+lives.ToString()+" lives left.";
        }
      
    }

    // Update is called once per frame
    void Update()
    {
        if(SceneManager.GetActiveScene().name=="L1toL2")
        {
  TextMeshProUGUI Display=gameObject.GetComponent<TextMeshProUGUI>();
    Display.text="You Captured "+GameStats.CaptureScore+" Zombies";
        }
    


    }
}
