using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class StageManager : MonoBehaviour
{
    private static readonly string MonsterTable = "MonsterTable";
    private static readonly string StageTable = "StageTable";
    public GameObject monsterPrefab;
    public Transform target;

    private StageData currentStageData;
    private int currentStage = 1;
    private int currentWave = 1;
    private Coroutine spawnCoroutine;


    private void Start()
    {
        StartStage(currentStage, currentWave);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log($"{currentStageData.StageName}스킵");
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
            spawnCoroutine = StartCoroutine(SpawnWave(currentWave + 1)); // 스킵 시 다음 웨이브
        }
    }

    private void StartStage(int stageNumber, int waveNumber)
    {
        if(currentWave > 20)
        {
            currentWave = 1;
            return;
        }
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        spawnCoroutine  = StartCoroutine(SpawnWave(currentWave));
    }
    private IEnumerator SpawnWave(int startWave)
    {
        int totalWaves = 21;
        for (int wave = startWave; wave <= totalWaves; wave++)
        {
            currentWave = wave;
            currentStageData = DataTableManager.Get<StageTable>(StageTable).Get(currentStage, currentWave);

            if (currentStageData.M1Num > 0)
                yield return StartCoroutine(SpawnMonsterGroup(currentStageData.M1Id.GetValueOrDefault(), currentStageData.M1Num.GetValueOrDefault()));
            if (currentStageData.M2Num > 0)
                yield return StartCoroutine(SpawnMonsterGroup(currentStageData.M2Id.GetValueOrDefault(), currentStageData.M2Num.GetValueOrDefault()));
            if (currentStageData.M3Num > 0)
                yield return StartCoroutine(SpawnMonsterGroup(currentStageData.M3Id.GetValueOrDefault(), currentStageData.M3Num.GetValueOrDefault()));
            Debug.Log($"Wave {currentStageData.StageName} 몬스터 소환완료!");
            yield return new WaitForSeconds(currentStageData.WaveTime.GetValueOrDefault());

        }
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
        Vector3 spawnpos = new Vector3(Random.Range(-3f, 3f), 7, 0);
        var monsterTable = DataTableManager.Get<MonsterTable>(MonsterTable);
        GameObject obj = Instantiate(monsterPrefab, spawnpos, Quaternion.identity);
        Monster monster = obj.GetComponent<Monster>();
        monster.Init(monsterTable, monsterId);
        monster.SetTarget(target);
    }

}
