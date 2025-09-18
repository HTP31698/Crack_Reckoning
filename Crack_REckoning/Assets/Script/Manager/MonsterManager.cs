using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager
{
    private static List<MonsterBase> monsters = new List<MonsterBase>();


    public static void AddMonster(MonsterBase m)
    {
        if (!monsters.Contains(m))
        {
            monsters.Add(m);
        }
    }

    public static void RemoveMonster(MonsterBase m)
    {
        if (monsters.Contains(m))
        {
            monsters.Remove(m);
        }
    }

    public static MonsterBase nearMonster(Vector3 fromPosition, float maxRange = Mathf.Infinity)
    {
        MonsterBase closemonster = null;
        float closemonterSqr = maxRange * maxRange;

        foreach (MonsterBase m in monsters)
        {
            if (m == null)
                continue;
            float sqrDist = (m.transform.position - fromPosition).sqrMagnitude;
            if(sqrDist < closemonterSqr)
            {
                closemonterSqr = sqrDist;
                closemonster = m;
            }
        }

        return closemonster;
    }
    // 랜덤으로 살아있는 몬스터 반환
    public static MonsterBase GetRandomMonster()
    {
        List<MonsterBase> aliveMonsters = monsters.FindAll(m => !m.isdead);

        if (aliveMonsters.Count == 0)
            return null;

        int randIndex = Random.Range(0, aliveMonsters.Count);
        return aliveMonsters[randIndex];
    }
}
