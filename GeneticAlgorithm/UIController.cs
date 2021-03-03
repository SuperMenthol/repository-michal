using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIController : MonoBehaviour
{
    float miliseconds;
    int seconds;
    int minutes;

    [SerializeField] public Text iteration;
    [SerializeField] public Text agLeft;
    [SerializeField] public Text curBest;
    [SerializeField] public Text ovBest;
    [SerializeField] public Text avgTime;
    [SerializeField] public Text minDens;
    [SerializeField] public Text minInit;
    [SerializeField] public Text minTol;
    [SerializeField] public Text timeText;

    void Update()
    {
        miliseconds += Time.deltaTime * 1000;
        miliseconds = Mathf.Round(miliseconds);
        if (miliseconds > 1000)
        {
            miliseconds = 0;
            seconds++;
        }
        if (seconds > 59)
        {
            seconds = 0;
            minutes++;
        }
        string mString = miliseconds == 1000 ? "000" : miliseconds > 100 ? miliseconds.ToString() : miliseconds == 0 ? "000" : "0" + miliseconds.ToString();

        string timeStr = $"{minutes}:{seconds}.{mString}";
        timeText.text = timeStr;
    }

    public void ResetTime()
    {
        miliseconds = 0f;
        timeText.text = "";
    }

    public void UpdateBest(bool currentOrBest, float time)
    {
        var timeStr = time.ToString();
        timeText.text = timeStr;

        if (currentOrBest)
        {
            curBest.text = timeStr;
        }
        else
        {
            ovBest.text = timeStr;
        }
    }
}
