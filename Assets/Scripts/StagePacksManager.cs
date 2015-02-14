using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[ExecuteInEditMode]
public class StagePacksManager : MonoBehaviour {

    static StagePacksManager instace;
    public static StagePacksManager globalInstance
    {
        get
        {
            if (StagePacksManager.instace == null) instace = FindObjectOfType<StagePacksManager>();
            return StagePacksManager.instace;
        }
    }

    public List<StagePack> packs = new List<StagePack>();
    static string SAVE_FILE_NAME = "/Data.sav";
    void Awake()
    {
        LoadData();
    }

    void LoadData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream f;
        
        try
        {
            f = File.OpenRead(Application.persistentDataPath + SAVE_FILE_NAME);
            if (f == null)
            {
                f = File.Create(Application.persistentDataPath + SAVE_FILE_NAME);
            }
            packs = bf.Deserialize(f) as List<StagePack>;
            f.Close();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Exception! " + e);
        }

    }

    void SaveData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream f;

        try
        {
            f = File.OpenWrite(Application.persistentDataPath + SAVE_FILE_NAME);
            if (f == null)
            {
                f = File.Create(Application.persistentDataPath + SAVE_FILE_NAME);
            }
            bf.Serialize(f, packs);
            f.Close();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Exception! " + e);
        }
    }

    void OnDestroy()
    {
        SaveData();
    }
}
