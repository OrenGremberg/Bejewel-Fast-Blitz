using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{

       float currentTime = 60f;
       float startingTime = 20f;

       [SerializeField] Text countdownText;

       void start()
       {
            currentTime = startingTime;
       }

       void Update()
       {
            currentTime -= 1 * Time.deltaTime;
            countdownText.text = currentTime.ToString("0");

            if (currentTime <= 0)
            {
                currentTime = 0;
				 	#if UNITY_EDITOR
						UnityEditor.EditorApplication.isPlaying = false;
					#else
         				Application.Quit();
					#endif
            }
       }
	  

}
