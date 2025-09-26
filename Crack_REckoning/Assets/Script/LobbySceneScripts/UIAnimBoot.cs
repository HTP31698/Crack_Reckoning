using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UIAnimBoot : MonoBehaviour
{
    public string stateName = "StageStart"; // �⺻ ����� ������Ʈ �̸�

    void OnEnable()
    {
        var anim = GetComponent<Animator>();
        if (!anim) return;

        anim.updateMode = AnimatorUpdateMode.UnscaledTime; // timeScale=0�̾ ���
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate; // ������ũ��/��Ȱ�� �θ𿩵� ����
        anim.Rebind();               // ���ε� ����
        anim.Update(0f);             // ��� �ݿ�
        anim.speed = 1f;
        anim.Play(stateName, 0, 0f); // ���� �����Ӻ��� ���
    }
}