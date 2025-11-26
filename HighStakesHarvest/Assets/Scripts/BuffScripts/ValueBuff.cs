using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ValueBuff", menuName = "Buffs/ValueBuff")]
[Serializable]
public class ValueBuff : ScriptableBuff
{
    public string cropAffected; // e.g., "Tomato", "Vegetables", "Fruits", "All"
    public float modifier;

    public override void Apply(GameObject target)
    {
        CropManager cropManager = target.GetComponent<CropManager>();
        if (cropManager == null)
        {
            Debug.LogError("CropManager not found on target!");
            return;
        }

        foreach (var kvp in cropManager.cropInfoDictionary)
        {
            CropInfo crop = kvp.Value;
            bool apply = false;

            if (cropAffected.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                apply = true;
            }
            else if (cropAffected.Equals("Vegetables", StringComparison.OrdinalIgnoreCase) && crop.type.Equals("Vegetable", StringComparison.OrdinalIgnoreCase))
            {
                apply = true;
            }
            else if (cropAffected.Equals("Fruits", StringComparison.OrdinalIgnoreCase) && crop.type.Equals("Fruit", StringComparison.OrdinalIgnoreCase))
            {
                apply = true;
            }
            else if (crop.name.Equals(cropAffected, StringComparison.OrdinalIgnoreCase))
            {
                apply = true;
            }

            if (apply)
            {
                cropManager.ApplySpecificValueBuff(crop, modifier);
                Debug.Log($"Crop '{crop.name}' value is now '{cropManager.getCropValue(crop.name)}'.");
            }
        }
    }

    public override void Remove(GameObject target)
    {
        throw new System.NotImplementedException();
    }
}
