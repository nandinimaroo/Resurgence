using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class typewriter : MonoBehaviour
{
    

	TextMeshProUGUI txt;
	public string story ;

	void Awake () 
	{

		txt = GetComponent<TextMeshProUGUI>();
		story = txt.text;
		txt.text = "";

		// TODO: add optional delay when to start
		StartCoroutine ("PlayText");
	}

	IEnumerator PlayText()
	{
		foreach (char c in story) 
		{
			txt.text += c;
			yield return new WaitForSeconds (0.05f);
		}
	}


}
