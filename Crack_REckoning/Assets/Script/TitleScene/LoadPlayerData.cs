using UnityEngine;

public class LoadPlayerData : MonoBehaviour
{
    private void Awake()
    {
        if(!SaveLoadManager.Load(0))
        {
            SaveLoadManager.Data.PlayerID = 11001;
            SaveLoadManager.Data.Gold = 0;

            
            SaveLoadManager.Data.OwnedSkillIds.Add(31001);

            SaveLoadManager.Data.EquipmentSkillIds.Add(31001);
            SaveLoadManager.Data.StageClear.Add(1, false);
            SaveLoadManager.Save();
        }
    }



    private 
}