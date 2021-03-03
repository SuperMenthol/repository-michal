using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public float lifeTime;
    Configurator cfg;
    [SerializeField] float bodyTemp;
    float limitTemp;
    [SerializeField]int geneAssessment;

    float rot;
    int maxDeviation;

    [SerializeField] int density;
    [SerializeField] int tempTolerance;
    [SerializeField] int initTemp;
    [SerializeField] float temperatureLosing;
    [SerializeField] int pointsLeft;

    void Awake()
    {
        cfg = FindObjectOfType<Configurator>();
        maxDeviation = cfg.maxDeviation;
        geneAssessment = Random.Range(0, 5);
        AssessVitals();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        lifeTime += Time.deltaTime;
        bodyTemp -= temperatureLosing * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0f,0f,transform.rotation.z + rot);

        if (bodyTemp < limitTemp) { PassAgent(); }
    }

    private void PassAgent()
    {
        cfg.populationDict.Add(this, lifeTime);
        cfg.AgentDecrease();

        if (lifeTime > cfg.currentBestTime) { cfg.currentBestTime = lifeTime; }
        GetComponent<SpriteRenderer>().color = Color.black;

        Destroy(gameObject);
    }

    void AssignSingleVital(int variable, int pointsToAdd)
    {
        switch (variable) //0 - density, 1 - initial, 2 - tolerance
        {
            case 0: density = Random.Range(cfg.minDensity, 100); break;
            case 1: initTemp = Random.Range(cfg.minInit, 100); break;
            case 2: tempTolerance = Random.Range(cfg.minTolerance, 100); break;
        }
    }

    private void AssessVitals()
    {
        bodyTemp = cfg.startingBodyTemp;
        int pointsPool = 200;
        int rng;

        density += (int)cfg.dShare;
        initTemp += (int)cfg.iShare;
        tempTolerance += (int)cfg.tShare;
        pointsPool -= (density + initTemp + tempTolerance);

        switch (geneAssessment)
        {
            case 0:
                rng = Mathf.Clamp(Random.Range(0, maxDeviation), 0, pointsPool);
                density += rng;
                pointsPool -= rng;

                rng = Mathf.Clamp(Random.Range(0, maxDeviation), 0, pointsPool);
                initTemp += rng;
                pointsPool -= rng;

                tempTolerance = Mathf.Clamp(tempTolerance + pointsPool, 0, 100);
                break;
            case 1:
                rng = Mathf.Clamp(Random.Range(0, maxDeviation), 0, pointsPool);
                density += rng;
                pointsPool -= rng;

                rng = Mathf.Clamp(Random.Range(0, maxDeviation), 0, pointsPool);
                tempTolerance += rng;
                pointsPool -= rng;

                initTemp = Mathf.Clamp(initTemp + pointsPool, 0, 100);
                break;
            case 2:
                rng = Mathf.Clamp(Random.Range(0, maxDeviation), 0, pointsPool);
                tempTolerance += rng;
                pointsPool -= rng;

                rng = Mathf.Clamp(Random.Range(0, maxDeviation), 0, pointsPool);
                density += rng;
                pointsPool -= rng;

                initTemp = Mathf.Clamp(initTemp + pointsPool, 0, 100);
                break;
            case 3:
                rng = Mathf.Clamp(Random.Range(0, maxDeviation), 0, pointsPool);
                tempTolerance += rng;
                pointsPool -= rng;

                rng = Mathf.Clamp(Random.Range(0, maxDeviation), 0, pointsPool);
                initTemp += rng;
                pointsPool -= rng;

                density = Mathf.Clamp(density + pointsPool, 0, 100);
                break;
            case 4:
                rng = Mathf.Clamp(Random.Range(0, maxDeviation), 0, pointsPool);
                initTemp += rng;
                pointsPool -= rng;

                rng = Mathf.Clamp(Random.Range(0, maxDeviation), 0, pointsPool);
                density += rng;
                pointsPool -= rng;

                tempTolerance = Mathf.Clamp(tempTolerance + pointsPool, 0, 100);
                break;
            case 5:
                rng = Mathf.Clamp(Random.Range(0, maxDeviation), 0, pointsPool);
                initTemp += rng;
                pointsPool -= rng;

                rng = Mathf.Clamp(Random.Range(0, maxDeviation), 0, pointsPool);
                tempTolerance += rng;
                pointsPool -= rng;

                density = Mathf.Clamp(density + pointsPool, 0, 100);
                break;
        }

        pointsPool = 200 - (density + initTemp + tempTolerance);

        limitTemp -= tempTolerance * 0.01f;
        bodyTemp += initTemp * 0.1f;
        gameObject.transform.localScale += new Vector3(density * 0.01f, 0f, 0f); //getting more swole with higher density
        GetComponent<SpriteRenderer>().color += new Color(0f, -initTemp * 0.01f, -initTemp * 0.01f, 1f); //getting more red with higher initial temperature
        rot = tempTolerance; //spinning faster with lower temperature limit

        pointsLeft = pointsPool;

        temperatureLosing = cfg.cold * (1 - (density * 0.008f));
    }

    public int GetDensity() { return density; }
    public int GetInitial() { return initTemp; }
    public int GetTolerance() { return tempTolerance; }
    public int GetAssessmentMethod() { return geneAssessment; }
}
