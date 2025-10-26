using System;
using UnityEngine;

[CreateAssetMenu(fileName = "QuantityBuff", menuName = "Buffs/QuantityBuff")]
[Serializable]
public class QuantityBuff : ScriptableBuff
{

    public string cropAffected;
    public float modifier;

    public override void Apply(GameObject target)
    {
        CropManager cropManager = target.GetComponent<CropManager>();
        CropInfo crop = cropManager.getCropInfo(cropAffected);
        cropManager.ApplySpecificQuantityBuff(crop, modifier);
        Debug.Log($"Crop '{cropAffected}' quantity is now '{cropManager.getCropQuantity(cropAffected)}'.");
    }

    public override void Remove(GameObject target)
    {
        throw new System.NotImplementedException();
    }
}

