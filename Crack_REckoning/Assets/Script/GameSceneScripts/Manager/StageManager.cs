using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    private static readonly string StageTable = "StageTable";
    private static readonly string PrefabMonster = "Prefabs/Monster";
    private static readonly string PrefabBoss = "Prefabs/Boss";
    private static readonly string MonsterTable = "MonsterTable";

    public Transform target;

    private GameObject monsterPrefab;
    private GameObject bossPrefab;

    private StageData currentStageData;
    public int currentStage { get; private set; }
    public int currentWave { get; private set; }
    private Coroutine spawnCoroutine;

    private float StageAddMHp;
    private float StageAddMAtt;

    public TextMeshProUGUI settingStageName;
    public TextMeshProUGUI StageName;
    public TextMeshProUGUI ClearOrFailed;
    public TextMeshProUGUI ClearWindowCurrentStageName;
    public Image[] monsterSlots;

    public GameObject ClearWindow;
    public bool wave20Spawned { get; set; } = false;

    private List<int> currentMonsterIds = new List<int>();

    private void Awake()
    {
        monsterPrefab = Resources.Load<GameObject>(PrefabMonster);
        bossPrefab = Resources.Load<GameObject>(PrefabBoss);
        ClearWindow.gameObject.SetActive(false);
    }

    private void Start()
    {
        currentStage = PlaySetting.SelectStage;
        currentWave = 1;
        StartStage(currentStage, currentWave);
    }

    private void StartStage(int stageNumber, int waveNumber)
    {
        if (currentWave > 20)
        {
            currentWave = 1;
            return;
        }
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        spawnCoroutine = StartCoroutine(SpawnWave(currentWave));
    }
    private IEnumerator SpawnWave(int startWave)
    {
        int totalWaves = 20;
        for (int wave = startWave; wave <= totalWaves; wave++)
        {
            currentWave = wave;
            currentStageData = DataTableManager.Get<StageTable>(StageTable).Get(currentStage, currentWave);

            settingStageName.text = currentStageData.StageName;
            StageName.text = currentStageData.StageName;
            ClearWindowCurrentStageName.text = currentStageData.StageName;
            StageAddMHp = currentStageData.StageAddMHp.GetValueOrDefault();
            StageAddMAtt = currentStageData.StageAddMAtt.GetValueOrDefault();

            // UI 갱신
            currentMonsterIds.Clear();
            if (currentStageData.M1Num > 0) currentMonsterIds.Add(currentStageData.M1Id ?? 0);
            if (currentStageData.M2Num > 0) currentMonsterIds.Add(currentStageData.M2Id ?? 0);
            if (currentStageData.M3Num > 0) currentMonsterIds.Add(currentStageData.M3Id ?? 0);
            UpdateMonsterUI(currentMonsterIds);

            if (currentStageData.M1Num > 0)
                StartCoroutine(SpawnMonsterGroup(currentStageData.M1Id.GetValueOrDefault(), currentStageData.M1Num.GetValueOrDefault()));

            if (currentStageData.M2Num > 0)
                StartCoroutine(SpawnMonsterGroup(currentStageData.M2Id.GetValueOrDefault(), currentStageData.M2Num.GetValueOrDefault()));
            if (currentStageData.M3Num > 0)
                StartCoroutine(SpawnMonsterGroup(currentStageData.M3Id.GetValueOrDefault(), currentStageData.M3Num.GetValueOrDefault()));

            if (currentStageData.MiniBossID.HasValue && currentStageData.MiniBossNum.GetValueOrDefault() > 0)
                StartCoroutine(SpawnBossGroup(currentStageData.MiniBossID.Value, currentStageData.MiniBossNum.Value));

            if (currentStageData.MainBossID > 0)
                SpawnBoss(currentStageData.MainBossID.Value);
            yield return new WaitForSeconds(currentStageData.WaveTime.GetValueOrDefault());
        }
    }

    public IEnumerator SpawnMonsterGroup(int monsterid, int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnMonster(monsterid, StageAddMHp, StageAddMAtt);
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void SpawnMonster(int monsterId, float addHp, float addAtt)
    {
        float posx = GetSpawnPositionX(monsterPrefab);
        Vector3 spawnPos = new Vector3(posx, 7, 0);
        GameObject obj = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
        Monster monster = obj.GetComponent<Monster>();
        monster.Init(monsterId);
        monster.SetTarget(target);

            // 스테이지 보정
            monster.maxHp = (int)(monster.maxHp * addHp);
        monster.currentHp = monster.maxHp;
        monster.damage = (int)(monster.damage * addAtt);
    }

    public IEnumerator SpawnBossGroup(int bossId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnBoss(bossId);
            yield return new WaitForSeconds(1f);
        }
    }

    public void SpawnBoss(int bossId)
    {
        Vector3 spawnPos = new Vector3(0, 7, 0);
        GameObject obj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        Boss boss = obj.GetComponent<Boss>();
        boss.Init(bossId);
        boss.SetTarget(target);
    }

    private float GetSpawnPositionX(GameObject prefab, float prefabZ = 0f)
    {
        Rect safe = Screen.safeArea;
        float zDist = Mathf.Abs(Camera.main.transform.position.z - prefabZ);
        Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(safe.xMin, safe.yMin, zDist));
        Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(safe.xMax, safe.yMax, zDist));

        if (prefab == null)
            return (bottomLeft.x + topRight.x) / 2f;

        SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>();
        float halfWidth = 0f;
        if (sr != null && sr.sprite != null)
            halfWidth = sr.sprite.bounds.extents.x * prefab.transform.localScale.x;

        float margin = (topRight.x - bottomLeft.x) * 0.05f;
        return Random.Range(bottomLeft.x + halfWidth + margin,
                            topRight.x - halfWidth - margin);
    }


    private void UpdateMonsterUI(List<int> monsterIds)
    {
        for (int i = 0; i < monsterSlots.Length; i++)
        {
            if (i < monsterIds.Count && monsterIds[i] > 0)
            {
                Sprite monsterSprite = null;
                var monsterData = DataTableManager.Get<MonsterTable>(MonsterTable).Get(monsterIds[i]);
                if (monsterData != null) monsterSprite = monsterData.sprite;
                monsterSlots[i].rectTransform.sizeDelta = new Vector2(240, 240);
                monsterSlots[i].sprite = monsterSprite;
                monsterSlots[i].enabled = monsterSprite != null;
            }
            else
            {
                monsterSlots[i].sprite = null;
                monsterSlots[i].enabled = false;
            }
        }
    }
    public void ShowClearWindow()
    {
        ClearOrFailed.text = "Stage Clear!!!";
        ClearWindow.gameObject.SetActive(true);
    }
    public void ShowFailedWindow()
    {
        ClearOrFailed.text = "Stage Failed...";
        ClearWindow.gameObject.SetActive(true);
    }
}
