using UnityEngine;
using UnityEngine.UIElements;

public class MonsterSpawner : MonoBehaviour
{
    private static readonly string MonsterTable = "MonsterTable";
    [SerializeField] private MonsterDataTable monsterDataTable;
    [SerializeField] private GameObject monsterPrefab;

    public int monsterId = 21001;
    public Transform target;

    private void Awake()
    {
        monsterDataTable =new MonsterDataTable();
        monsterDataTable.Load(MonsterTable);
        Vector3 spawnpos = new Vector3(Random.Range(-5, 5), 7 , 0);
        SpawnMonster(spawnpos, monsterId);
    }


    public void SpawnMonster(Vector3 position, int monsterId)
    {
        GameObject obj = Instantiate(monsterPrefab, position, Quaternion.identity);
        Monster monster = obj.GetComponent<Monster>();
        monster.Init(monsterDataTable, monsterId);
        monster.SetTarget(target);
    }
}
