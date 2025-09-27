using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrackOpen : MonoBehaviour
{
    private Animator animator;

    [Header("Cracks")]
    public GameObject PigCrack;
    public GameObject SlimeCrack;
    public GameObject ExtremColdCrack;
    public GameObject WolfCrack;
    public GameObject MarionetteCrack;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        PigCrack.SetActive(false);
        SlimeCrack.SetActive(false);
        ExtremColdCrack.SetActive(false);
        WolfCrack.SetActive(false);
        MarionetteCrack.SetActive(false);
    }

    private void OnEnable()
    {
        StartCoroutine(WaitAnimator());
    }
    private IEnumerator WaitAnimator()
    {
        var info = animator.GetCurrentAnimatorStateInfo(0);
        while (info.normalizedTime < 1f)
        {
            yield return null;
            info = animator.GetCurrentAnimatorStateInfo(0);
        }
        SetOpenCurrentCrack(SaveLoadManager.Data.CurrentCrack);
    }
    public void SetOpenCurrentCrack(int Crack)
    {
        switch (Crack)
        {
            case 1:
                PigCrack.SetActive(true);
                gameObject.SetActive(false);
                break;
            case 2:
                SlimeCrack.SetActive(true);
                gameObject.SetActive(false);
                break;
            case 3:
                ExtremColdCrack.SetActive(true);
                gameObject.SetActive(false);
                break;
            case 4:
                WolfCrack.SetActive(true);
                gameObject.SetActive(false);
                break;
            case 5:
                MarionetteCrack.SetActive(true);
                gameObject.SetActive(false);
                break;
        }
    }
}
