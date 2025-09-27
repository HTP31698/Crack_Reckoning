using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectWindow : MonoBehaviour
{
    private const string GameScene = "GameScene";

    private Animator animator;

    [Header("CrackWindows")]
    //public GameObject Crack1;
    //public GameObject Crack2;
    //public GameObject Crack3;
    //public GameObject Crack4;
    //public GameObject Crack5;


    [Header("Buttons")]
    public Button[] StageButton;

    private int CrackCount = 1;
    private int StageCount = 1;

    private void Awake()
    {
        //Crack1.SetActive(false);
        //Crack2.SetActive(false);
        //Crack3.SetActive(false);
        //Crack4.SetActive(false);
        //Crack5.SetActive(false);

        animator = GetComponent<Animator>();
        var data = SaveLoadManager.Data;


        for(int i = 0; i < StageButton.Length; i++)
        {
            StageButton[i].interactable = false;
        }
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
        SetupUI();
    }

    private void SetupUI()
    {
        var data = SaveLoadManager.Data;
        for (int i = 0; i < data.StageClear.Count; i++)
        {
            if (GetStageClear(i)) StageCount++;
        }
        if (GetStageClear(4)) CrackCount++;
        if (GetStageClear(9)) CrackCount++;
        if (GetStageClear(14)) CrackCount++;
        if (GetStageClear(19)) CrackCount++;

        if (StageCount > StageButton.Length) StageCount = StageButton.Length;
        if (StageCount < 1) StageCount = 1;
        if (CrackCount < 1) CrackCount = 1;

        data.CurrentCrack = CrackCount;

        for (int i = 0; i < StageCount && i < StageButton.Length; i++)
        {
            StageButton[i].onClick.RemoveAllListeners();
            int idx = i;
            StageButton[i].interactable = true;
            StageButton[i].onClick.AddListener(() => StageButtonClick(idx));
        }

        SaveLoadManager.Save();
    }



    private bool GetStageClear(int index)
    {
        bool v;
        if (SaveLoadManager.Data.StageClear.TryGetValue(index, out v))
            return v;
        return false;
    }

    private void CrackButtonClick(int crack)
    {
        //switch (crack)
        //{
        //    case 0:
        //        Crack1.SetActive(true);
        //        break;
        //    case 1:
        //        Crack2.SetActive(true);
        //        break;
        //    case 2:
        //        Crack3.SetActive(true);
        //        break;
        //    case 3:
        //        Crack4.SetActive(true);
        //        break;
        //    case 4:
        //        Crack5.SetActive(true);
        //        break;
        //}
    }

    private void StageButtonClick(int stage)
    {
        PlaySetting.SetSelectStage(stage + 1);

        SceneManager.LoadScene(GameScene);
    }
}