using NUnit.Framework.Internal;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager
{
    private static List<MonsterBase> monsters = new List<MonsterBase>();

    private static readonly List<MonsterBase> aliveBuf = new List<MonsterBase>(400);
    private static readonly List<MonsterBase> poolBuf = new List<MonsterBase>(400);
    private static readonly List<MonsterBase> fallBuf = new List<MonsterBase>(400);

    public static void AddMonster(MonsterBase m)
    {
        if (m != null && !monsters.Contains(m))
            monsters.Add(m);
    }

    public static void RemoveMonster(MonsterBase m)
    {
        if (m != null)
            monsters.Remove(m);
    }

    public static MonsterBase nearMonster(Vector3 fromPosition, float maxRange = Mathf.Infinity)
    {
        MonsterBase closest = null;
        float bestSqr = maxRange * maxRange;

        foreach (MonsterBase m in monsters)
        {
            if (m == null || m.isdead) continue;

            float sqrDist = (m.transform.position - fromPosition).sqrMagnitude;
            if (sqrDist < bestSqr)
            {
                bestSqr = sqrDist;
                closest = m;
            }
        }
        return closest;
    }

    public static MonsterBase GetRandomMonster()
    {
        aliveBuf.Clear();
        foreach (var m in monsters)
        {
            if (m != null && !m.isdead)
                aliveBuf.Add(m);
        }

        if (aliveBuf.Count == 0) return null;
        int randIndex = Random.Range(0, aliveBuf.Count);
        return aliveBuf[randIndex];
    }

    public static MonsterBase GetNearestAliveWithin(Vector3 fromPosition, float range)
    {
        MonsterBase best = null;
        MonsterBase second = null;

        float maxSqr = float.IsInfinity(range) ? float.PositiveInfinity : range * range;
        float bestSqr = float.PositiveInfinity;
        float secondSqr = float.PositiveInfinity;

        foreach (var m in monsters)
        {
            if (m == null || m.isdead) continue;

            float d2 = (m.transform.position - fromPosition).sqrMagnitude;
            if (d2 > maxSqr) continue;

            if (d2 < bestSqr)
            {
                second = best; secondSqr = bestSqr;
                best = m; bestSqr = d2;
            }
            else if (m != best && d2 < secondSqr)
            {
                second = m; secondSqr = d2;
            }
        }

        return second ?? best;
    }

    public static MonsterBase GetRandomAliveWithin(Vector3 fromPosition, float range, HashSet<MonsterBase> exclude = null)
    {
        float rangeSqr = range * range;
        poolBuf.Clear();
        fallBuf.Clear();

        foreach (var m in monsters)
        {
            if (m == null || m.isdead) continue;
            float sqrDist = (m.transform.position - fromPosition).sqrMagnitude;
            if (sqrDist <= rangeSqr)
            {
                if (exclude != null && exclude.Contains(m))
                    fallBuf.Add(m);
                else
                    poolBuf.Add(m);
            }
        }

        List<MonsterBase> list = (poolBuf.Count > 0) ? poolBuf : fallBuf;
        if (list.Count == 0) return null;

        int idx = Random.Range(0, list.Count);
        return list[idx];
    }

    public static MonsterBase GetAttackingWithin(Vector3 from, float range)
    {
        float rangeSqr = range * range;
        MonsterBase best = null;
        float bestSqr = float.MaxValue;

        foreach (var m in monsters)
        {
            if (m == null || m.isdead) continue;
            if (!m.isattack) continue;

            float sqr = (m.transform.position - from).sqrMagnitude;
            if (sqr > rangeSqr) continue;

            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = m;
            }
        }
        return best;
    }

    public static bool HasMonster()
    {
        foreach (var m in monsters)
        {
            if (m == null || m.isdead) continue;

            return true;
        }

        return false;
    }
}
