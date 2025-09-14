using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MonsterManager : MonoBehaviour
{
    private static readonly string MonsterTable = "MonsterTable";
    private static readonly string StageTable = "StageTable";
    private MonsterDataTable monsterDataTable;
    private StageDataTable StageDataTable;
    public GameObject monsterPrefab;
    public Transform target;

    private int currentStageIndex = 0;
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
        if (stageIndex > 21)
        {
            Debug.Log("모든 스테이지 클리어!");
            return;
        }
        currentStage = StageDataTable.GetAtIndex(stageIndex);
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        if(currentStage.M1Num > 0)
        {
            for (int i = 0; i < currentStage.M1Num; i++)
            {
                SpawnMonster(currentStage.M1Id.GetValueOrDefault());
                Debug.Log("1솬");
                yield return new WaitForSeconds(0.4f);
            }
        }
        if(currentStage.M2Num > 0)
        {
            for(int i = 0;i < currentStage.M2Num;i++)
            {
                SpawnMonster(currentStage.M2Id.GetValueOrDefault());
                Debug.Log("2솬");
                yield return new WaitForSeconds(0.4f);
            }
        }
        if(currentStage.M3Num > 0)
        {
            for(int i = 0; i <currentStage.M3Num;i++)
            {
                SpawnMonster(currentStage.M3Num.GetValueOrDefault());
                Debug.Log("3솬");
                yield return new WaitForSeconds(0.4f);
            }
        }
        yield return new WaitForSeconds(currentStage.WaveTime.GetValueOrDefault());

        Debug.Log($"Stage {currentStage.StageName} 완료!");
        currentStageIndex++;
        StartStage(currentStageIndex); // 다음 스테이지 시작
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
