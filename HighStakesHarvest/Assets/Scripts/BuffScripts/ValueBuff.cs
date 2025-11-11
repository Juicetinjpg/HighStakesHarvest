using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ValueBuff", menuName = "Buffs/ValueBuff")]
[Serializable]
public class ValueBuff : ScriptableBuff
{

    public string cropAffected;
    public float modifier;
    
    public override void Apply(GameObject target)
    {
        CropManager cropManager = target.GetComponent<CropManager>();  
        CropInfo crop = cropManager.getCropInfo(cropAffected);
        cropManager.ApplySpecificValueBuff(crop, modifier);
        Debug.Log($"Crop '{cropAffected}' value is now '{cropManager.getCropValue(cropAffected)}'.");
    }

    public override void Remove(GameObject target)
    {
        throw new System.NotImplementedException();
    }
}
