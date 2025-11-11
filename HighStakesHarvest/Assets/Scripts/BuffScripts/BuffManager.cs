using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    [System.Serializable]
    private class ActiveBuff
    {
        public ScriptableBuff buff;
        public float timeRemaining;
        public bool isPermanent;
    }

    private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

    [Header("Target Configuration")]
    [SerializeField]
    private GameObject cropManagerObject; // Object with CropManager component

    [Header("Buff Display (Optional)")]
    [SerializeField]
    private GameObject buffUIContainer;

    private void Start()
    {
        // Auto-find CropManager if not assigned
        if (cropManagerObject == null)
        {
            CropManager cropManager = Object.FindFirstObjectByType<CropManager>();
            if (cropManager != null)
            {
                cropManagerObject = cropManager.gameObject;
                Debug.Log("BuffManager: Auto-found CropManager on " + cropManagerObject.name);
            }
            else
            {
                Debug.LogError("BuffManager: Could not find CropManager in scene!");
            }
        }
    }

    private void Update()
    {
        // Update buff durations
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            if (!activeBuffs[i].isPermanent)
            {
                activeBuffs[i].timeRemaining -= Time.deltaTime;

                if (activeBuffs[i].timeRemaining <= 0)
                {
                    RemoveBuffAtIndex(i);
                }
            }
        }
    }

    public void AddBuff(ScriptableBuff buff, float duration = -1f)
    {
        if (buff == null)
        {
            Debug.LogWarning("Attempted to add null buff");
            return;
        }

        if (cropManagerObject == null)
        {
            Debug.LogError("Cannot add buff: CropManager object not found!");
            return;
        }

        // Check if buff is already active
        ActiveBuff existingBuff = activeBuffs.Find(ab => ab.buff == buff);

        if (existingBuff != null)
        {
            // Refresh duration if it's a temporary buff
            if (!existingBuff.isPermanent && duration > 0)
            {
                existingBuff.timeRemaining = duration;
                Debug.Log($"Buff '{buff.BuffName}' duration refreshed to {duration}s");
            }
            else
            {
                Debug.LogWarning($"Buff '{buff.BuffName}' is already active.");
            }
            return;
        }

        // Add new buff
        ActiveBuff newBuff = new ActiveBuff
        {
            buff = buff,
            timeRemaining = duration,
            isPermanent = duration <= 0
        };

        activeBuffs.Add(newBuff);
        buff.Apply(cropManagerObject);

        string durationText = newBuff.isPermanent ? "permanent" : $"{duration}s";
        Debug.Log($"Buff '{buff.BuffName}' added ({durationText}) to {cropManagerObject.name}");
    }

    private void RemoveBuffAtIndex(int index)
    {
        if (index < 0 || index >= activeBuffs.Count) return;
        if (cropManagerObject == null) return;

        ActiveBuff buffToRemove = activeBuffs[index];
        buffToRemove.buff.Remove(cropManagerObject);
        activeBuffs.RemoveAt(index);

        Debug.Log($"Buff '{buffToRemove.buff.BuffName}' expired and removed.");
    }

    public void RemoveBuff(ScriptableBuff buff)
    {
        if (buff == null) return;

        for (int i = 0; i < activeBuffs.Count; i++)
        {
            if (activeBuffs[i].buff == buff)
            {
                RemoveBuffAtIndex(i);
                return;
            }
        }

        Debug.LogWarning($"Buff '{buff.BuffName}' not found.");
    }

    public void RemoveAllBuffs()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            RemoveBuffAtIndex(i);
        }
        Debug.Log("All buffs removed.");
    }

    public bool HasBuff(ScriptableBuff buff)
    {
        return activeBuffs.Exists(ab => ab.buff == buff);
    }

    public List<ScriptableBuff> GetActiveBuffs()
    {
        List<ScriptableBuff> buffs = new List<ScriptableBuff>();
        foreach (var activeBuff in activeBuffs)
        {
            buffs.Add(activeBuff.buff);
        }
        return buffs;
    }

    public float GetBuffTimeRemaining(ScriptableBuff buff)
    {
        ActiveBuff activeBuff = activeBuffs.Find(ab => ab.buff == buff);
        return activeBuff != null ? activeBuff.timeRemaining : 0f;
    }
}