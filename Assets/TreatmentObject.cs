using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TreatmentObject : MonoBehaviour
{
    public GameObject player;
    public float proximityRange = 5f;
    public float countdownTime;
    public int totalZombies; // Number of zombies to be treated
    private TextMesh textMesh;

    private bool isInRange = false;
    private bool isTreatmentInProgress = false;
    private float remainingTime;
    public int z = 0;
    private Light spotlight;
private bool alltreated=false;
    void Start()
    {
        
        totalZombies = GameStats.CaptureScore;
        textMesh = GetComponent<TextMesh>();
         GameObject objectWithTag = GameObject.FindWithTag("spot");

        spotlight=objectWithTag.GetComponent<Light>();

        UpdateText();
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if(totalZombies<5)
        countdownTime = Random.Range(7,10);
        else
                countdownTime = Random.Range(5,7);

        if (distance <= proximityRange)
        {
            if (!isInRange)
            {
                isInRange = true;
                UpdateText();
            }

            if (isInRange && Input.GetKeyDown(KeyCode.B)&&!alltreated)
            {
                if (!isTreatmentInProgress)
                {
                    StartCoroutine(StartTreatment());
                }
            }
        }
        else
        {
            if (isInRange)
            {
                isInRange = false;
                StopTreatment();
                UpdateText();
            }
        }
     

    }

    IEnumerator StartTreatment()
    {
        spotlight.intensity=5;
        isTreatmentInProgress = true;

       
            float timer = remainingTime > 0f ? remainingTime : countdownTime;

            while (timer > 0f)
            {
                if (!isInRange)
                {
                    if(spotlight)
                                    spotlight.intensity=0;

                    StopTreatment();
                    UpdateText();
                    yield break;
                }

                textMesh.text = $"Time Left: {Mathf.CeilToInt(timer)}s";
                yield return new WaitForSeconds(1f);
                timer--;
            }
            z++;

            // Zombie treated
            GameStats.ZombiesTreated = z;

        

        // All zombies treated
        if(spotlight)
                        spotlight.intensity=0;

        StopTreatment();
        UpdateText();
        if (alltreated)
    {
        yield return new WaitForSeconds(2f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("GameEnd");
    }
    }

    void StopTreatment()
    {

        isTreatmentInProgress = false;
        remainingTime = 0f;
    }

    void UpdateText()
    {
        if (isInRange)
        {
            if (isTreatmentInProgress)
            {
                textMesh.text = $"Time left: {countdownTime}s";
            }
            else if (z >= totalZombies)
            {
                textMesh.text = "All Zombies \nSuccessfully Treated!";
                alltreated=true;


            }
            else
            {
                textMesh.text = "B -> Start Treatment";
            }
        }
        else
        {
            textMesh.text = "Out Of Range";
        }
    }
}
