using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MonsterSorting : MonoBehaviour
{
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        // ���� SpriteRenderer�� �ڽ� ������Ʈ�� �ִٸ� GetComponentInChildren ���
    }

    void LateUpdate()
    {
        // Y��ǥ�� �������� ������ ���̰�
        sr.sortingOrder = -(int)(transform.position.y * 100);
    }
}