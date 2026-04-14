using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [Header("Windows")]
    public GameObject HomeWindow;
    public GameObject SkillWindow;
    public GameObject SkillEnforceWindow;
    public GameObject CrackOpenWindow;
    public GameObject ChWindow;
    public GameObject ChEnforceWindow;
    public GameObject PetWindow;
    public GameObject PetEnforceWindow;
    public GameObject SettingWindow;
    public GameObject NewIdWindow;

    [Header("CrackWindows")]
    public GameObject PigCrackWindow;
    public GameObject SlimeCrackWindow;
    public GameObject ExtremColdCrackWindow;
    public GameObject WolfCrackWindow;
    public GameObject MarionetteCrackWindow;

    [Header("StageWindows")]
    public GameObject PigStageWindow;
    public GameObject SlimeStageWindow;
    public GameObject ExtremColdStageWindow;
    public GameObject WolfStageWindow;
    public GameObject MarionetteStageWindow;

    [Header("Buttons")]
    public Button StageChallengeButton;
    public Button CharacterButton;
    public Button SkillButton;
    public Button HomeButton;
    public Button PetButton;
    public Button EnforceButton;
    public Button MenuButton;

    [Header("Texts")]
    public TextMeshProUGUI Gold;

    [SerializeField] private ButtonAudio ButtonAudio;



    public void Awake()
    {
        HomeWindow.SetActive(true);
        CrackOpenWindow.SetActive(false);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);
        ChWindow.SetActive(false);
        ChEnforceWindow.SetActive(false);
        PetWindow.SetActive(false);
        PetEnforceWindow.SetActive(false);
        SettingWindow.SetActive(false);
        NewIdWindow.SetActive(false);
        CrackWindowSetFalse();
        StageWindowSetFalse();


        StageChallengeButton.onClick.AddListener(StageChallengeButtonClick);
        HomeButton.onClick.AddListener(HomeButtonClick);
        SkillButton.onClick.AddListener(SkillButtonClick);
        CharacterButton.onClick.AddListener(CharacterButtonClick);
        PetButton.onClick.AddListener(PetButtonClick);
        MenuButton.onClick.AddListener(MenuButtonClick);

        StageChallengeButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        HomeButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        SkillButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        CharacterButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        PetButton.onClick.AddListener(ButtonAudio.PlayClickSound);
        MenuButton.onClick.AddListener(ButtonAudio.PlayClickSound);

        Gold.text = SaveLoadManager.Data.Gold.ToString();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            var data = SaveLoadManager.Data;
            if (data == null) return;

            int[] allSkills = { 3001, 3002, 3003, 3004, 3005, 3006, 3007, 3008, 3009, 3010, 3011, 3012, 3013, 3014, 3015 };
            int[] allPets = { 7001, 7002, 7003, 7004 };
            const int GoldGrant = 999999;

            if (data.OwnedSkillIds == null)
                data.OwnedSkillIds = new System.Collections.Generic.List<int>();
            if (data.EquipmentSkillIds == null)
                data.EquipmentSkillIds = new System.Collections.Generic.List<int>();

            var bestPerBase = new System.Collections.Generic.Dictionary<int, int>();
            foreach (var owned in data.OwnedSkillIds)
            {
                int baseId = owned > 9999 ? owned / 100 : owned;
                if (!bestPerBase.TryGetValue(baseId, out int cur) || owned > cur)
                    bestPerBase[baseId] = owned;
            }

            foreach (int baseId in allSkills)
            {
                if (!bestPerBase.ContainsKey(baseId))
                    bestPerBase[baseId] = baseId;
            }

            data.OwnedSkillIds.Clear();
            foreach (var kv in bestPerBase)
                data.OwnedSkillIds.Add(kv.Value);

            for (int i = data.EquipmentSkillIds.Count - 1; i >= 0; i--)
            {
                int eq = data.EquipmentSkillIds[i];
                int baseId = eq > 9999 ? eq / 100 : eq;
                if (bestPerBase.TryGetValue(baseId, out int best))
                    data.EquipmentSkillIds[i] = best;
                else
                    data.EquipmentSkillIds.RemoveAt(i);
            }

            if (data.OwnedPetIds == null)
                data.OwnedPetIds = new System.Collections.Generic.List<int>();
            foreach (int pid in allPets)
            {
                if (!data.OwnedPetIds.Contains(pid))
                    data.OwnedPetIds.Add(pid);
            }

            data.Gold += GoldGrant;
            if (Gold != null) Gold.text = data.Gold.ToString();

            SaveLoadManager.Save();
            Debug.Log($"[Cheat] Skills deduped & unlocked, pets unlocked, +{GoldGrant} gold. Total: {data.Gold}");
        }
    }

    public void StageChallengeButtonClick()
    {
        HomeWindow.SetActive(false);
        CrackOpenWindow.SetActive(true);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);
        ChWindow.SetActive(false);
        ChEnforceWindow.SetActive(false);
        PetWindow.SetActive(false);
        PetEnforceWindow.SetActive(false);
        SettingWindow.SetActive(false);
        NewIdWindow.SetActive(false);
        CrackWindowSetFalse();
        StageWindowSetFalse();
    }

    public void HomeButtonClick()
    {
        HomeWindow.SetActive(true);
        CrackOpenWindow.SetActive(false);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);
        ChWindow.SetActive(false);
        ChEnforceWindow.SetActive(false);
        PetWindow.SetActive(false);
        PetEnforceWindow.SetActive(false);
        SettingWindow.SetActive(false);
        NewIdWindow.SetActive(false);
        CrackWindowSetFalse();
        StageWindowSetFalse();
    }

    public void SkillButtonClick()
    {
        HomeWindow.SetActive(false);
        CrackOpenWindow.SetActive(false);
        SkillWindow.SetActive(true);
        SkillEnforceWindow.SetActive(false);
        ChWindow.SetActive(false);
        ChEnforceWindow.SetActive(false);
        PetWindow.SetActive(false);
        PetEnforceWindow.SetActive(false);
        SettingWindow.SetActive(false);
        NewIdWindow.SetActive(false);
        CrackWindowSetFalse();
        StageWindowSetFalse();
    }

    public void CharacterButtonClick()
    {
        HomeWindow.SetActive(false);
        CrackOpenWindow.SetActive(false);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);
        ChWindow.SetActive(true);
        ChEnforceWindow.SetActive(false);
        PetWindow.SetActive(false);
        PetEnforceWindow.SetActive(false);
        SettingWindow.SetActive(false);
        NewIdWindow.SetActive(false);
        CrackWindowSetFalse();
        StageWindowSetFalse();
    }

    public void PetButtonClick()
    {
        HomeWindow.SetActive(false);
        CrackOpenWindow.SetActive(false);
        SkillWindow.SetActive(false);
        SkillEnforceWindow.SetActive(false);
        ChWindow.SetActive(false);
        ChEnforceWindow.SetActive(false);
        PetWindow.SetActive(true);
        PetEnforceWindow.SetActive(false);
        SettingWindow.SetActive(false);
        NewIdWindow.SetActive(false);
        CrackWindowSetFalse();
        StageWindowSetFalse();
    }

    public void MenuButtonClick()
    {
        SettingWindow.SetActive(true);
    }



    private void CrackWindowSetFalse()
    {
        PigCrackWindow.SetActive(false);
        SlimeCrackWindow.SetActive(false);
        ExtremColdCrackWindow.SetActive(false);
        WolfCrackWindow.SetActive(false);
        MarionetteCrackWindow.SetActive(false);
    }

    private void StageWindowSetFalse()
    {
        PigStageWindow.SetActive(false);
        SlimeStageWindow.SetActive(false);
        ExtremColdStageWindow.SetActive(false);
        WolfStageWindow.SetActive(false);
        MarionetteStageWindow.SetActive(false);
    }
}