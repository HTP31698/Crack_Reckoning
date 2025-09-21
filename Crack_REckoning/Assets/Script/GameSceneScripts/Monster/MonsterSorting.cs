using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MonsterSorting : MonoBehaviour
{
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        // 만약 SpriteRenderer가 자식 오브젝트에 있다면 GetComponentInChildren 사용
    }

    void LateUpdate()
    {
        // Y좌표가 낮을수록 앞으로 보이게
        sr.sortingOrder = -(int)(transform.position.y * 100);
    }
}