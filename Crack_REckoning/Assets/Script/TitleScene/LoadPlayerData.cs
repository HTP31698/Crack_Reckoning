using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadPlayerData : MonoBehaviour
{
    public TextMeshProUGUI title;
    public float interval = 0.5f;

    private void Awake()
    {
        if (!SaveLoadManager.Load(0))
        {
            var skillTable = DataTableManager.SkillTable;
            SaveLoadManager.Data.PlayerID = 11001;
            SaveLoadManager.Data.Gold = 0;
            SaveLoadManager.Data.OwnedSkillIds = skillTable.GetSkillsWithoutPlus();
            SaveLoadManager.Data.EquipmentSkillIds.Add(3001);
            SaveLoadManager.Data.StageClear.Add(1, false);
            for(int i = 0; i < 25; i++)
            {
                SaveLoadManager.Data.StageClear[i] = false;
            }
            SaveLoadManager.Data.CurrentCrack = 1;

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
        if (title)
        {
            float safe;
            if (interval > 0f)
                safe = interval;
            else
                safe = 0.001f;

            int block = Mathf.FloorToInt(Time.unscaledTime / safe);
            bool even = (block % 2) == 0;
            title.enabled = even;
        }



    }
}