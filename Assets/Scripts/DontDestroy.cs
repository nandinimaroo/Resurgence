using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroy : MonoBehaviour
{
    void Start()
    {
        Time.timeScale=1;
    }
    void Awake()
    {
        // Get all root objects in the active scene
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        // Iterate through the root objects and make sure they don't get destroyed on scene load
        foreach (GameObject gameObject in rootObjects)
        {
            if(gameObject.tag=="Player")
            DontDestroyOnLoad(gameObject);
        }
    }
}
