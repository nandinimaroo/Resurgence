using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;


public class Enemy : MonoBehaviour
{
    private Transform player;
    public int xPos;
    public int zPos;
    public float TimeLeft;
    public float timer;
    public float attackRange=50;
    public float distanceFromPlayer;
private float movementSpeed=0.1f;
private Animator animator;
public NavMeshAgent enemy;
public string tag1;
public TextMesh countdown;
public float damage_per_attack;
private GameObject Player;
public PlayerMovementScript script;
public int r1;
public int r2;



    private void Start()
    {
        gameObject.tag="TreatableZombie";
        enemy=GetComponent<NavMeshAgent>();
        TimeLeft = UnityEngine.Random.Range(r1,r2);
        timer = TimeLeft;
        animator=gameObject.GetComponent<Animator>();
               animator.SetTrigger("IsWalking");
                       Player = GameObject.FindWithTag("Player");

               script=Player.GetComponent<PlayerMovementScript>();
        player=Player.transform;


    }
 private void OnGUI(){
		// if(!countdown){
		// 	try{
		// 		countdown = GameObject.Find("Enemy").GetComponent<TextMesh>();
		// 	}
		// 	catch(System.Exception ex){
		// 		print("Couldnt find the Enemy ->" + ex.StackTrace.ToString());
		// 	}
		// }
		if(countdown)
        {
            if(timer>0)
            {
			countdown.text = Math.Ceiling(timer)+"s";

            }
            else 
            			countdown.text = "TOXIC";

        }


	}
    private void RunTowardsPlayer()
    {
                animator.ResetTrigger("IsWalking");
        animator.ResetTrigger("IsAttacking");

        gameObject.tag="UntreatableZombie";
movementSpeed=0.5f;
        animator.SetTrigger("IsRunning");
        enemy.SetDestination(player.position);
       

    }
private void StopAndAttack()
    {
        animator.ResetTrigger("IsRunning");
        animator.ResetTrigger("IsWalking");
        enemy.Stop();

if(gameObject.tag=="UntreatableZombie")
{
            animator.SetTrigger("IsAttacking");
script.takeDamage(damage_per_attack*Time.deltaTime);

}
    }
         bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {

        Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range; //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) //documentation: https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
        { 
            //the 1.0f is the max distance from the random point to a point on the navmesh, might want to increase if range is big
            //or add a for loop like in the documentation
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
 
       public void DestroyZombie()
       {
        Destroy(gameObject);
       }
    
     private void Update()
    {
        
            if(timer<=0)
        {
        gameObject.tag="UntreatableZombie";
        }
                        enemy.SetDestination(player.position);


        timer-=Time.deltaTime;
        distanceFromPlayer = Vector3.Distance(transform.position, player.position);



        if(timer<=0 &&distanceFromPlayer>attackRange)
        {
            RunTowardsPlayer();
        }
        else
        if(distanceFromPlayer<=attackRange)
        {
            StopAndAttack();
        }
    
       
    }
}