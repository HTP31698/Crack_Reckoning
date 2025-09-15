using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StageManager : MonoBehaviour
{
    private static readonly string MonsterTable = "MonsterTable";
    private static readonly string StageTable = "StageTable";
    private MonsterDataTable monsterDataTable;
    private StageDataTable StageDataTable;
    public GameObject monsterPrefab;
    public Transform target;

    private int currentStageIndex = 0;
    private int waveCount = 0;
    private StageData currentStage;

    private void Awake()
    {
        monsterDataTable = new MonsterDataTable();
        monsterDataTable.Load(MonsterTable);
        StageDataTable = new StageDataTable();
        StageDataTable.Load(StageTable);
    }
    private void Start()
    {
        StartStage(currentStageIndex);
    }
    private void StartStage(int stageIndex)
    {
        if (waveCount > 21)
        {
            Debug.Log("스테이지 순환 완료");
            waveCount = 0;
            return;
        }
        currentStage = StageDataTable.GetAtIndex(stageIndex);
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        if (currentStage.M1Num > 0)
        {
            StartCoroutine(SpawnMonsterGroup(currentStage.M1Id.GetValueOrDefault(), 
                currentStage.M1Num.GetValueOrDefault()));
        }
        if (currentStage.M2Num > 0)
        {
            StartCoroutine(SpawnMonsterGroup(currentStage.M2Id.GetValueOrDefault(),
                currentStage.M2Num.GetValueOrDefault()));
        }
        if (currentStage.M3Num > 0)
        {
            StartCoroutine(SpawnMonsterGroup(currentStage.M3Id.GetValueOrDefault(),
                currentStage.M3Num.GetValueOrDefault()));
        }
        yield return new WaitForSeconds(currentStage.WaveTime.GetValueOrDefault());

        Debug.Log($"Stage {currentStage.StageName} 완료!");
        currentStageIndex++;
        waveCount++;
        StartStage(currentStageIndex); // 다음 스테이지 시작
    }

    public IEnumerator SpawnMonsterGroup(int monsterid, int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnMonster(monsterid);
            yield return new WaitForSeconds(0.4f);
        }
    }

    public void SpawnMonster(int monsterId)
    {
        Vector3 spawnpos = new Vector3(Random.Range(-3.25f, 3.25f), 7, 0);
        GameObject obj = Instantiate(monsterPrefab, spawnpos, Quaternion.identity);
        Monster monster = obj.GetComponent<Monster>();
        monster.Init(monsterDataTable, monsterId);
        monster.SetTarget(target);
    }

}
