using UnityEngine;
using System.Collections;


[System.Serializable]
public class StagePack  {

    [SerializeField]
    public int fromStage;
    [SerializeField]
    public int toStage;

    public bool allStageUnlocked = false;

    public int StageCount
    {
        get
        {
            return toStage - fromStage;
        }
    }

    [SerializeField]
    bool[] unlocks;

    public bool[] Unlocks
    {
        get { return unlocks; }
        set { unlocks = value; }
    }

    public bool isUnlocked;

    public int GetStageID(int index)
    {
        return fromStage + index;
    }

    public bool IsStageUnlocked(int index)
    {
        return unlocks[index];
    }

    
    void Awake()
    {
        
        if (unlocks == null) 
        { 
            unlocks = new bool[StageCount];
            unlocks[0] = isUnlocked;

            if (allStageUnlocked)
            {
                for (int i = 1; i < StageCount; i++)
                {
                    unlocks[i] = true;
                }
            }
        }

    }
        
   

}
