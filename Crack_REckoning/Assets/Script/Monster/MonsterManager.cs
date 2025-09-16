using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager
{
    private static List<Monster> monsters = new List<Monster>();

    public static void AddMonster(Monster m)
    {
        if (!monsters.Contains(m))
        {
            monsters.Add(m);
        }
    }

    public static void RemoveMonster(Monster m)
    {
        if (monsters.Contains(m))
        {
            monsters.Remove(m);
        }
    }

    public static Monster nearMonster(Vector3 fromPosition, float maxRange = Mathf.Infinity)
    {
        Monster closemonster = null;
        float closemonterSqr = maxRange * maxRange;

        foreach (Monster m in monsters)
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
}
