using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIController : MonoBehaviour
{
    bool ingame;
    GameController gc;
    ScoreHolder sh;

    private void Awake() { sh = GameObject.Find("ScoreHolder").GetComponent<ScoreHolder>(); }
    void Start()
    {
        sh = FindObjectOfType<ScoreHolder>();
        ingame = false;
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1) { gc = GameObject.Find("GameController").GetComponent<GameController>(); }
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1) { RefreshScore(); }

    public void Continue()
    {
        gc.paused = false;
        gc.PauseGame();
    }

    public void Restart()
    {
        if (EventSystem.current.currentSelectedGameObject.transform.parent.name == "BgHs")
        {
            var cHs = EventSystem.current.currentSelectedGameObject.transform.parent;
            var inField = GameObject.Find("NameText");

            int mScr = int.Parse(cHs.Find("TextScore").GetComponent<TextMeshProUGUI>().text);
            string mStr = inField.GetComponent<TextMeshProUGUI>().text;
            sh.UpdateScore(mStr,mScr);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    public void QuitButton()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "MainMenu":
                Application.Quit(); break;
            case "SampleScene":
                if (EventSystem.current.currentSelectedGameObject.transform.parent.name == "BgHs")
                {
                    var cHs = EventSystem.current.currentSelectedGameObject.transform.parent;
                    var inField = GameObject.Find("NameText");

                    int mScr = int.Parse(cHs.Find("TextScore").GetComponent<TextMeshProUGUI>().text);
                    string mStr = inField.GetComponent<TextMeshProUGUI>().text;
                    sh.UpdateScore(mStr, mScr);
                }

                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
                SceneManager.sceneLoaded += SceneManager_sceneLoaded;
                break;
        }
    }

    public void RefreshScore()
    {
        var source = sh.SortedList;

        for (int i = 0; i < 10; i++)
        {
            GameObject.Find($"HSRow ({i})").transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = (i+1).ToString();
            GameObject.Find($"HSRow ({i})").transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = source[i].name;
            GameObject.Find($"HSRow ({i})").transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().text = source[i].score > 0 ? source[i].score.ToString() : " ";
        }
    }
}
