using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameHandler : MonoBehaviour
{
    [SerializeField]float startTime, numberPlayers;
    [SerializeField] TMP_Text timerText, positionDisplay;

    float currentTime, currentPosition = 0;

    bool timerStarted = false;

    void Start()
    {
        currentTime = startTime;
        timerStarted = true;
        timerText.text = currentTime.ToString();
        positionDisplay.text = currentPosition + " / " + numberPlayers;
    }

    // Update is called once per frame
    void Update()
    {
        if (timerStarted)
        {
            currentTime -= Time.deltaTime;
            if(currentTime <= 0)
            {
                timerStarted = false;
                currentTime = 0;
            }
            timerText.text = currentTime.ToString("f1");

        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentPosition++;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentPosition--;
        }

        positionDisplay.text = currentPosition + " / " + numberPlayers;

    }
}
