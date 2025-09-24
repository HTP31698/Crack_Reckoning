using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadPlayerData : MonoBehaviour
{
    private void Awake()
    {
        if (!SaveLoadManager.Load(0))
        {
            var skillTable = DataTableManager.SkillTable;
            SaveLoadManager.Data.PlayerID = 11001;
            SaveLoadManager.Data.Gold = 0;
            SaveLoadManager.Data.OwnedSkillIds = skillTable.GetSkillsWithoutPlus();
            SaveLoadManager.Data.EquipmentSkillIds.Add(31001);
            SaveLoadManager.Data.StageClear.Add(1, false);
            SaveLoadManager.Save(0);
        }
        else
        {
            SaveLoadManager.Load(0);
        }
    }

    private void Update()
    {
        if (Input.touchCount == 1)
        {
            const string scenePath = "Assets/Scenes/LobbyScene.unity";
            int index = SceneUtility.GetBuildIndexByScenePath(scenePath);

            if (index < 0)
            {
                Debug.LogError($"Scene not in Build Settings: {scenePath}");
                return;
            }
            SceneManager.LoadScene(index);
        }
    }
}