using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ReloadingObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
         GameObject objectToReload = CarryOverObject.objectToReload;

        // Check if the object to reload exists
        if (objectToReload != null)
        {
            // Reload the object in the current scene (you may need to instantiate it or reset its state)
            Instantiate(objectToReload, transform.position, transform.rotation);

            // Optionally, you can unload the previous scene
            SceneManager.UnloadSceneAsync("Level1");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
