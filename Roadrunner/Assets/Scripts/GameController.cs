using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    ScoreHolder sh;
    GameObject player;
    PlayerController pScr;

    public KeyCode[] keys = { KeyCode.A, KeyCode.D, KeyCode.S };
    GameObject bottomBg;
    GameObject bg;
    GameObject cnv;

    int currentScore;
    TextMeshProUGUI cScr;
    TextMeshProUGUI hScr;
    GameObject[] lives = new GameObject[3];

    //initalization of the game, UI
    bool started;
    public bool paused;
    bool ended;

    int cNum;
    GameObject cDown;
    TextMeshProUGUI cText;
    TextMeshProUGUI cTime;
    CanvasGroup cAlpha;
    TextMeshProUGUI cSpeed;
    GameObject liveUI;
    GameObject pauseUI;
    [SerializeField]GameObject lifeImg;
    [SerializeField] AudioClip crash;

    //camera manipulation
    Camera mCam;
    [SerializeField][Range(10f,60f)]float panout = 40f;

    float[] lanes = { 0.4f, 1f, 1.6f, 2.2f, 2.82f, 3.45f };
    [SerializeField] GameObject[] barPrefabs = new GameObject[3];
    [SerializeField] Sprite[] carSprites = new Sprite[7];
    [SerializeField] GameObject carObject;
    [SerializeField] GameObject ExplObj;
    [SerializeField] public AudioClip[] horns;
    [SerializeField] public AudioClip[] engines;

    [SerializeField] public float objectSlower = 300f;
    float barrInterval = 2f;
    float carInterval = 0.5f;
    float second = 1f;
    
    //timer
    float minutes;
    float seconds;
    float miliseconds;

    float startTimer;
    float barrTimer;
    float carTimer;

    public float lowPoint = -18f;

    private void Awake() { paused = false; }
    void Start()
    {
        cnv = GameObject.Find("CnvMain");
        cDown = GameObject.Find("CnvCountdown");
        cText = cDown.GetComponentInChildren<TextMeshProUGUI>();
        cAlpha = cDown.GetComponent<CanvasGroup>();
        cScr = GameObject.Find("CScr").GetComponent<TextMeshProUGUI>();
        cTime = GameObject.Find("TScr").GetComponent<TextMeshProUGUI>();
        cSpeed = GameObject.Find("VScr").GetComponent<TextMeshProUGUI>();
        liveUI = GameObject.Find("BgLives");
        pauseUI = GameObject.Find("CnvPause");

        sh = FindObjectOfType<ScoreHolder>();

        mCam = Camera.main;
        
        player = GameObject.FindGameObjectWithTag("Player");
        pScr = player.GetComponent<PlayerController>();

        bottomBg = GameObject.FindGameObjectWithTag("Background");
        bg = Instantiate(bottomBg.gameObject, new Vector3(0f, 10.3f, 0.5f), Quaternion.identity);
        Physics.gravity = new Vector3(0f, 0f, 0f);

        barrTimer = barrInterval;
        carTimer = carInterval;
        startTimer = second;

        currentScore = 0;
        cNum = 3;

        Instantiate(barPrefabs[2], new Vector3(0f, 14f, -0.1f), Quaternion.identity);

        for (int i = 0; i < 3; i++)
        {
            lives[i] = Instantiate(lifeImg, new Vector3(i * 20f, liveUI.transform.position.y - 20f, 0f), Quaternion.identity, liveUI.transform);
        }

        hScr = GameObject.Find("HScr").GetComponent<TextMeshProUGUI>();
        hScr.text = sh.SortedList[0].score.ToString();
    }

    public void PauseGame()
    {
        if (!paused)
        {
            Time.timeScale = 0f;
            pauseUI.GetComponent<CanvasGroup>().alpha = 1f;
            paused = true;
        }
        else
        {
            Time.timeScale = 1f;
            pauseUI.GetComponent<CanvasGroup>().alpha = 0f;
            paused = false;
        }
    }

    private void Update() { if (Input.GetKeyDown(KeyCode.Escape)) { PauseGame(); } }

    void FixedUpdate()
    {
        //initial countdown, UI manipulation
        if (!started)
        {
            cnv.GetComponent<CanvasGroup>().alpha = 0f;

            startTimer -= Time.deltaTime;
            cAlpha.alpha = startTimer;

            cText.text = cNum > 0 ? cNum.ToString() : "Go!";
            
            if (startTimer < 0)
            {
                cNum--;
                cAlpha.alpha = 1f;
                startTimer = 1f;

                startTimer -= Time.deltaTime;
                cAlpha.alpha = startTimer;

                if (cText.text == "Go!") { started = true; }

                if (startTimer < 0)
                {
                    cAlpha.alpha = 1f;
                    startTimer = 1f;

                    startTimer -= Time.deltaTime;
                    cAlpha.alpha = startTimer;
                }
            }
        }
        else
        {
            cAlpha.alpha = 0f;
            cnv.GetComponent<CanvasGroup>().alpha = 1f;

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
            cTime.text = timeStr;
        }

        cSpeed.text = Mathf.Round(pScr.currentSpeed).ToString();

        //camera manipulation
        if (cText.text == "2")
        {
            mCam.orthographicSize -= Time.deltaTime * 2;
            mCam.transform.position -= new Vector3(0f,Time.deltaTime,0f) *2;
        }
        if (pScr.currentSpeed > 125f && pScr.currentSpeed < 300f)
        {
            mCam.orthographicSize += Time.deltaTime/panout;
            mCam.transform.position += new Vector3(0f, Time.deltaTime/panout, 0f);
        }

        //spawning and despawning objects on road
        barrTimer -= Time.deltaTime;
        carTimer -= Time.deltaTime;

        if (bottomBg.transform.position.y < lowPoint)
        {
            Destroy(bottomBg);
            bottomBg = bg;
            bg = Instantiate(bottomBg.gameObject, new Vector3(0f, 10.3f, 0.5f), Quaternion.identity);
            bg.name = "BgObj";
        }
        
        bottomBg.transform.position -= new Vector3(0f, pScr.currentSpeed / 1000f, 0f);
        bg.transform.position -= new Vector3(0f, pScr.currentSpeed / 1000f, 0f);

        foreach (GameObject el in GameObject.FindGameObjectsWithTag("Barrier"))
        {
            el.transform.position -= new Vector3(0f, pScr.currentSpeed / objectSlower, 0f);

            if (el.transform.position.y < lowPoint * 2) { Destroy(el); }
        }

        foreach (GameObject el in GameObject.FindGameObjectsWithTag("Car")) { if (el.transform.position.y < lowPoint) { Destroy(el.gameObject); } }

        if (barrTimer < 0f)
        {
            int rn = UnityEngine.Random.Range(0, barPrefabs.Length-1);

            if (!Physics2D.Raycast(new Vector2(0f, 7f), Vector2.zero) || Physics2D.Raycast(new Vector2(0f, 7f), Vector2.zero).collider.tag == "Gradient") { SpawnBarrier(rn); }

            barrTimer = barrInterval;
        }

        if (carTimer < 0f)
        {
            carTimer = carInterval;
            int rn = UnityEngine.Random.Range(0, carSprites.Length-1);

            SpawnCar(rn);
        }
    }

    private void SpawnBarrier(int rn)
    {
        var nBarrier = Instantiate(barPrefabs[rn], new Vector3(0f, 7f, -0.1f), Quaternion.identity);
    }

    private void SpawnCar(int rn)
    {
        int side = UnityEngine.Random.Range(0,10) < 6 ? -1 : 1;

        float myLane = lanes[UnityEngine.Random.Range(0, lanes.Length)] * side;
        float mySpeed = UnityEngine.Random.Range(40f, 140f);

        GameObject nCar = Instantiate(carObject, new Vector3(myLane, 7f, -0.1f), Quaternion.identity);
        nCar.name = "Car";

        nCar.GetComponent<CarScript>().speed = mySpeed;
        nCar.GetComponent<SpriteRenderer>().sprite = carSprites[rn];
        nCar.GetComponent<BoxCollider2D>().size = nCar.GetComponent<SpriteRenderer>().size * 0.9f;
    }

    public void AddPoints(int spDif, float closeMult, float pow)
    {
        currentScore += (int)Mathf.Abs((spDif*closeMult*pow));
        cScr.text = currentScore.ToString();

        if (currentScore > int.Parse(hScr.text)) { cScr.color = Color.yellow; }
    }

    public void Lose(Vector3 pos)
    {
        int lInd = Array.FindLastIndex(lives, x => x != null);
        if (lInd > 0)
        { 
            Destroy(lives[lInd]);
            lives[lInd] = null;

            player.GetComponent<AudioSource>().PlayOneShot(crash);
        }
        else
        {
            player.GetComponent<AudioSource>().PlayOneShot(crash);

            Destroy(lives[0]);
            Time.timeScale = 0f;

            foreach (AudioSource el in FindObjectsOfType<AudioSource>()) { el.Stop(); }

            if (currentScore > sh.SortedList[9].score) { HighScore(true); }
            else { HighScore(false); }
        }

        Instantiate(ExplObj, pos, Quaternion.identity);
    }

    void HighScore(bool mode)
    {
        var cHs = GameObject.Find("CnvHs");
        if (!mode)
        { 
            GameObject.Find("TextHs").GetComponent<TextMeshProUGUI>().text = "GAME OVER";
            GameObject.Find("TextName").GetComponent<TextMeshProUGUI>().text = " ";
            var nf = GameObject.Find("NameField");
            nf.GetComponent<TMP_InputField>().interactable = false;
            nf.GetComponent<CanvasGroup>().alpha = 0f;
        }

        GameObject.Find("TextScore").GetComponent<TextMeshProUGUI>().text = currentScore.ToString();
        var cCgr = cHs.GetComponent<CanvasGroup>();
        cCgr.alpha = 1f;
        cCgr.interactable = true;
        cCgr.blocksRaycasts = true;
    }
}
