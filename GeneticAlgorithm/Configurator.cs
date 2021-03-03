using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Configurator : MonoBehaviour
{
    [SerializeField] GameObject agentGO;
    public Dictionary<Agent, float> populationDict;
    [SerializeField] [Range(0f, 10f)] public float tempLimit;
    [SerializeField] [Range(0f, 10f)] public float cold;
    [SerializeField] [Range(0f, 100f)] public float startingBodyTemp;
    [SerializeField] [Range(0f, 10f)] public float densityEffect;
    [SerializeField] [Range(0f, 100f)] public int maxDeviation;

    UIController uc;

    [SerializeField] [Range(0, 100)] int initialAgents;
    int iteration;
    int agentsLeft;
    public float currentBestTime;
    public float overallBestTime;
    float avgSurvivalTime;

    public int minDensity;
    public int minTolerance;
    public int minInit;

    public float dShare;
    public float iShare;
    public float tShare;

    private void Awake()
    {
        uc = FindObjectOfType<UIController>();
        AssignFirstShare();
        populationDict = new Dictionary<Agent, float>();
        agentsLeft = initialAgents;
    }

    

    private void Start()
    {
        SpawnAgents();
    }

    private void SpawnAgents()
    {
        float xRange = 6.5f;
        float yRange = 3.5f;

        for (int i = 0; i < initialAgents; i++)
        {
            Instantiate(agentGO, new Vector3(Random.Range(-xRange, xRange), Random.Range(-yRange, yRange), 0f), Quaternion.identity);
        }

        agentsLeft = initialAgents;
    }

    private void Update()
    {
        RefreshUI();

        if (agentsLeft == 0)
        {
            EndIteration();
        }
    }

    private void AssignFirstShare()
    {
        dShare = Random.Range(maxDeviation * 0.5f, maxDeviation * 2);
        iShare = Random.Range(maxDeviation * 0.5f, maxDeviation * 2);
        tShare = Random.Range(maxDeviation * 0.5f, maxDeviation * 2);

        Debug.Log($"d: {dShare} i: {iShare} t: {tShare}");
    }

    private void EndIteration()
    {
        Time.timeScale = 0f;
        SortDictionary();
        DestroyAgents();

        currentBestTime = 0f;

        RunNextIteration();
    }

    private void SortDictionary()
    {
        var agList = new List<Agent>();

        foreach (var item in populationDict.OrderByDescending(key => key.Value)) { agList.Add(item.Key); }

        for (float i = agList.Count-1; i < agList.Count * 0.8f; i++) { agList.RemoveAt((int)i); }

        float _toAvg = new float();
        foreach (var item in agList) { _toAvg += item.lifeTime; }

        avgSurvivalTime = _toAvg / agList.Count;

        for (float i = 0; i < agList.Count * 0.2f; i++) { int a = agList[(int)i].GetAssessmentMethod(); }

        if (currentBestTime > overallBestTime)
        {
            overallBestTime = currentBestTime;
            uc.UpdateBest(false, overallBestTime);
        }

        UpdateMinValues(agList);
        CalculateShares(agList);
    }

    private void CalculateShares(List<Agent> input)
    {
        //Debug.Log($"SHARE BEFORE UPDATE: d: {dShare} i: {iShare} t: {tShare}");
        float i = new float();
        float _d = 0f;
        float _i = 0f;
        float _t = 0f;

        for (i = 0f; i < input.Count * 0.3f; i++)
        {
            _d += input[(int)i].GetDensity() * 2f;
            _i += input[(int)i].GetInitial() * 2f;
            _t += input[(int)i].GetTolerance() * 2f;
        }

        for (i = (int)i; i < input.Count * 0.5f; i++)
        {
            _d += input[(int)i].GetDensity();
            _i += input[(int)i].GetInitial();
            _t += input[(int)i].GetTolerance();
        }

        _d /= input.Count;
        _i /= input.Count;
        _t /= input.Count;

        dShare = dShare < _d ? _d : dShare;
        iShare = iShare < _i ? _i : iShare;
        tShare = tShare < _t ? _t : tShare;

        //Debug.Log($"calculated averages: density {_d} initial {_i} tolerance {_t}");
        //Debug.Log($"SHARE AFTER UPDATE: d: {dShare} i: {iShare} t: {tShare}");
    }

    private void UpdateMinValues(List<Agent> input)
    {
        int _d = 100;
        int _i = 100;
        int _t = 100;

        if (minDensity + minInit + minTolerance < 200)
        {
            foreach (var item in input)
            {
                int iD = item.GetDensity();
                int iI = item.GetInitial();
                int iT = item.GetTolerance();
                if (iD < _d) { _d = iD; }
                if (iI < _i) { _i = iI; }
                if (iT < _t) { _t = iT; }
            }

            if (minDensity < _d) { minDensity = _d; }
            if (minInit < _i) { minInit = _i; }
            if (minTolerance < _t) { minTolerance = _t; }
        }

        //Debug.Log($"MINIMUM VALUES: Density: {minDensity} Init temperature: {minInit} Tolerance: {minTolerance}");

        uc.minDens.text = minDensity.ToString();
        uc.minInit.text = minInit.ToString();
        uc.minTol.text = minTolerance.ToString();
    }

    private void RunNextIteration()
    {
        DestroyAgents();
        populationDict.Clear();
        iteration++;
        uc.ResetTime();
        Time.timeScale = 1f;
        SpawnAgents();
    }

    void DestroyAgents()
    {
        foreach (var item in GameObject.FindGameObjectsWithTag("Player")) { Destroy(item); }
    }

    private void RefreshUI()
    {
        uc.iteration.text = iteration.ToString();
        uc.agLeft.text = agentsLeft.ToString();
        uc.curBest.text = currentBestTime.ToString();
        uc.avgTime.text = avgSurvivalTime.ToString();
    }

    public void AgentDecrease() { agentsLeft--; }
}
