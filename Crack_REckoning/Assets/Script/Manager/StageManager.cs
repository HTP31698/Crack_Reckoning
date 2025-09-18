using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class StageManager : MonoBehaviour
{
    private static readonly string StageTable = "StageTable";
    private static readonly string PrefabMonster = "Prefabs/Monster";
    private static readonly string PrefabBoss = "Prefabs/Boss";
    public Transform target;

    private StageData currentStageData;
    private int currentStage = 1;
    private int currentWave = 1;
    private Coroutine spawnCoroutine;

    private float StageAddMHp;
    private float StageAddMAtt;


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
        int totalWaves = 21;
        for (int wave = startWave; wave <= totalWaves; wave++)
        {
            currentWave = wave;
            currentStageData = DataTableManager.Get<StageTable>(StageTable).Get(currentStage, currentWave);
            StageAddMHp = currentStageData.StageAddMHp.GetValueOrDefault();
            StageAddMAtt = currentStageData.StageAddMAtt.GetValueOrDefault();

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
        float posx = GetSpawnPositionX(PrefabMonster);
        Vector3 spawnPos = new Vector3(posx, 7, 0);
        GameObject obj = Instantiate(Resources.Load<GameObject>(PrefabMonster), spawnPos, Quaternion.identity);
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
        GameObject obj = Instantiate(Resources.Load<GameObject>(PrefabBoss), spawnPos, Quaternion.identity);
        Boss boss = obj.GetComponent<Boss>();
        boss.Init(bossId);
        boss.SetTarget(target);
    }

    private float GetSpawnPositionX(string prefabPath)
    {
        Rect safe = Screen.safeArea;

        // SafeArea → 월드 좌표 변환 (Z는 카메라와 오브젝트 위치에 맞춰줌)
        float zDist = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(safe.xMin, safe.yMin, zDist));
        Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(safe.xMax, safe.yMax, zDist));

        // 프리팹 크기 계산
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        float halfWidth = 0f;

        if (prefab != null)
        {
            GameObject temp = Instantiate(prefab);
            SpriteRenderer sr = temp.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                halfWidth = sr.bounds.extents.x;
            Destroy(temp); // 임시 오브젝트 삭제
        }

        // 화면 안쪽에서만 랜덤
        float margin = 0.15f;
        float x = Random.Range(bottomLeft.x + halfWidth + margin, topRight.x - halfWidth - margin);
        return x;
    }


}
