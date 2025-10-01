using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentStat : MonoBehaviour
{
    [Header("SpriteRenderers")]
    public Image CharacterImage;
    public Image SkillImage;
    public Image PetImage;

    public TextMeshProUGUI StatText;

    private void OnEnable()
    {
        var data = SaveLoadManager.Data;
        var Stable = DataTableManager.SkillTable;
        var Ctable = DataTableManager.CharacterTable;
        var Ptable = DataTableManager.PetTable;

        var sdata = Stable.Get(data.EquipmentSkillIds[0]);
        var cdata = Ctable.Get(data.PlayerID);
        var pdata = Ptable.Get(data.EquipmentPetId);

        CharacterImage.sprite = cdata.sprite;
        SkillImage.sprite = sdata.sprite;
        PetImage.sprite = pdata.sprite;

        var sb = new StringBuilder();
        sb.AppendLine($"¿ë »ç: {cdata.ChName}");
        sb.AppendLine($"½º Å³: {sdata.SkillName}");
        sb.AppendLine($"Æê: {pdata.PetName}");

        StatText.text = sb.ToString();
    }
}