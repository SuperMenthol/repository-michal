using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Linq;

public class ScoreHolder : MonoBehaviour
{
    public List<recStruct> shRec;
    public List<recStruct> SortedList;

    public static ScoreHolder sh;
    void Awake()
    {
        if (sh != null && sh != this) { Destroy(this); }
        else { sh = this; }

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        shRec = new List<recStruct>(11);
        SortedList = new List<recStruct>(10);
        Empty();

        NewLoad();
        GameObject.Find("UIController").GetComponent<UIController>().RefreshScore();
    }

    private void Empty()
    {
        for (int i = 0; i < 10; i++)
        {
            recStruct eRec = new recStruct();
            eRec.pos = i + 1;
            eRec.name = " ";
            eRec.score = 0;

            shRec.Add(eRec);
            SortedList.Add(eRec);
        }
    }

    public void UpdateScore(string name, int score)
    {
        var nScore = new recStruct();
        nScore.pos = 11;
        nScore.name = name;
        nScore.score = score;

        shRec.Add(nScore);
        shRec = shRec.OrderByDescending(o => o.score).ToList();
        shRec.Remove(shRec[shRec.Count - 1]);

        for (int i = 0; i < 10; i++)
        {
            shRec[i].pos = i + 1;
            SortedList[i] = shRec[i];
        }

        NewSave();
    }

    public void NewSave()
    {
        string destination = Application.persistentDataPath + "/save.dat";
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(destination, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

        ScoreContainer sc = new ScoreContainer();

        for (int i = 0; i < 10; i++) { sc.recArray.Add(SortedList[i]); }

        bf.Serialize(file, sc);
        file.Close();
    }

    public void NewLoad()
    {
        string destination = Application.persistentDataPath + "/save.dat";
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;

        if (File.Exists(destination))
        {
            file = File.Open(destination, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            ScoreContainer sc = (ScoreContainer)bf.Deserialize(file);

            for (int i = 0; i < 10; i++)
            {
                shRec[i] = sc.recArray[i];
                SortedList[i] = sc.recArray[i];
            }

            file.Close();
            file.Dispose();
        }
        else
        {
            for (int i = 0; i < 10; i++)
            {
                recStruct eRec = new recStruct();
                eRec.pos = i + 1;
                eRec.name = " ";
                eRec.score = 0;

                shRec.Add(eRec);
                SortedList.Add(eRec);
            }
        }

        //GameObject.Find("UIController").GetComponent<UIController>().RefreshScore();
    }
}

[Serializable]
public class recStruct
{
    public int pos;
    public string name;
    public int score;
}

[Serializable]
public class ScoreContainer
{
    public List<recStruct> recArray = new List<recStruct>(10);
}
