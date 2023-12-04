using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Button exitButton;

    void Start()
    {
        // Attach the ExitGame method to the button click event
        exitButton.onClick.AddListener(ExitGame);
    }

    // Call this method when you want to exit the game
    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
