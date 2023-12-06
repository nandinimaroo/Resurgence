using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    public GameObject Enemy;
 
    [SerializeField]
    private float Interval ;
 public float spawnRange;
 public int TotalZombies;
 public int counter;
public Transform player;
public GameObject EnemyPrefab;
    // Start is called before the first frame update
    void Start()
    {
counter=TotalZombies;
        InvokeRepeating("spawnEnemy",0.8f,Interval);
    }
   bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {

        Vector3 randomPoint = center + Random.insideUnitSphere * range; //random point in a sphere 
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas)) //documentation: https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
        { 
            //the 1.0f is the max distance from the random point to a point on the navmesh, might want to increase if range is big
            //or add a for loop like in the documentation
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
    void Update()
    {
        if(GameStats.CaptureScore+GameStats.ZombiesKilled==TotalZombies)
        {
             Cursor.lockState = CursorLockMode.None;
Cursor.visible = true;
Time.timeScale = 1.0f;
if(GameStats.CaptureScore>0)
            SceneManager.LoadScene("L1toL2");
            else
            SceneManager.LoadScene("RetryL12");
        }
    }
 
    void spawnEnemy()
    {
        Vector3 spawnPosition;
        for(int i=0;i<20;++i)
        {
    if(RandomPoint(player.position,spawnRange,out spawnPosition))
        {
    if(Vector3.Distance(player.position,spawnPosition)>8);
    {
 if(--counter==0) CancelInvoke("spawnEnemy");
        GameObject EnemySpawned = Instantiate( Enemy,spawnPosition, Quaternion.identity);
        
        break;
        }
    }
    
    
        }

    

    }
 
}