using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    private List<ScriptableBuff> activeBuffs = new List<ScriptableBuff>();

    public void AddBuff(ScriptableBuff buff)
    {
        if (buff == null) return;

        // Prevent adding the same permanent buff multiple times
        if (!activeBuffs.Contains(buff))
        {
            activeBuffs.Add(buff);
            buff.Apply(transform.parent.gameObject);
            Debug.Log($"Buff '{buff.BuffName}' added");
        }
        else
        {
            Debug.LogWarning($"Buff '{buff.BuffName}' is already active on {gameObject.name}.");
        }
    }

    public void RemoveBuff(ScriptableBuff buff)
    {
        if (buff == null) return;

        if (activeBuffs.Contains(buff))
        {
            buff.Remove(gameObject);
            activeBuffs.Remove(buff);
            Debug.Log($"Buff '{buff.BuffName}' removed from {gameObject.name}.");
        }
        else
        {
            Debug.LogWarning($"Buff '{buff.BuffName}' not found on {gameObject.name}.");
        }
    }

}
