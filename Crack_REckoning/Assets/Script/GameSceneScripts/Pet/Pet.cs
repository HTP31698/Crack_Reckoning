using UnityEditor.U2D.Animation;
using UnityEngine;

public class Pet : MonoBehaviour
{
    private static readonly string PetTable = "PetTable";
    public int PetID { get; private set; }
    public string PetName { get; private set; }
    public float GoldUp { get; private set; }
    public float WaveTime { get; private set; }
    public int WallHpUp { get; private set; }
    public int AttBuff { get; private set; }

    private PetData petData;

    private Sprite sprite;
    private SpriteRenderer spriteRenderer;
    private RuntimeAnimatorController runanimator;
    private Animator animator;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    public void Init(int id)
    {
        PetID = id;
        var table = DataTableManager.Get<PetTable>(PetTable);
        petData = table.Get(id);

        if (petData != null)
        {
            PetName = petData.PetName;
            GoldUp = petData.GoldUp.GetValueOrDefault();
            WaveTime = petData.WaveTime.GetValueOrDefault();
            WallHpUp = petData.WallHpUp.GetValueOrDefault();
            AttBuff = petData.AttBuff.GetValueOrDefault();

            sprite = petData.sprite;
            runanimator = petData.AnimatorController;

            if(sprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
            }
            if(animator != null && runanimator != null)
            {
                animator.runtimeAnimatorController = runanimator;
            }
        }
    }

    public float GetGoldUp()
    {
        if(GoldUp > 0)
        {
            return GoldUp;
        }
        else
        {
            return 1f;
        }
    }
    
    public float GetWaveTime()
    {
        if(WaveTime > 0)
        {
            return WaveTime;
        }
        else
        {
            return 0f;
        }
    }

    public int GetWallHpUp()
    {
        if(WallHpUp> 0)
        {
            return WallHpUp;
        }
        else
        {
            return 0;
        }
    }

    public int GetAttBuff()
    {
        if(AttBuff > 0)
        {
            return AttBuff;
        }
        else
        {
            return 0;
        }
    }
}
