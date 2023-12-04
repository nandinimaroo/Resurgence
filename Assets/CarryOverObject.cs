using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryOverObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
     public static GameObject objectToReload;

    void Awake()
    {
        // Mark this GameObject as "Don't Destroy On Load"
        DontDestroyOnLoad(this.gameObject);

        // Set the objectToReload variable
        objectToReload = this.gameObject;
    }
}
