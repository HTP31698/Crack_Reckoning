using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UIAnimBoot : MonoBehaviour
{
    public string stateName = "StageStart"; // 기본 재생할 스테이트 이름

    void OnEnable()
    {
        var anim = GetComponent<Animator>();
        if (!anim) return;

        anim.updateMode = AnimatorUpdateMode.UnscaledTime; // timeScale=0이어도 재생
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate; // 오프스크린/비활성 부모여도 갱신
        anim.Rebind();               // 바인딩 갱신
        anim.Update(0f);             // 즉시 반영
        anim.speed = 1f;
        anim.Play(stateName, 0, 0f); // 시작 프레임부터 재생
    }
}