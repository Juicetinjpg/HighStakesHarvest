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
        public string buffId; // cached id for quick lookup
    }

    private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();
    private HashSet<string> activeBuffIds = new HashSet<string>(); // canonical ownership

    [Header("Target Configuration")]
    [SerializeField]
    private GameObject cropManagerObject; // Object with CropManager component

    [Header("Buff Display (Optional)")]
    [SerializeField]
    private GameObject buffUIContainer;

    private void Start()
    {
        // Force auto-locate every time, ignore inspector value to avoid stale prefab refs
        cropManagerObject = null;
        StartCoroutine(FindCropManagerNextFrame());
        Debug.Log($"BuffManager Start — activeBuffs count: {activeBuffs.Count}");
    }

    private IEnumerator FindCropManagerNextFrame()
    {
        yield return null; // wait one frame for all objects to initialize

        CropManager cm = FindFirstObjectByType<CropManager>();
        if (cm != null)
        {
            cropManagerObject = cm.gameObject;
            Debug.Log("BuffManager: Auto-found CropManager on " + cropManagerObject.name);
        }
        else
        {
            Debug.LogError("BuffManager: Could not find CropManager in scene!");
        }
    }

    private void Update()
    {
        // Update timed buff durations
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

    // Add buff by asset. duration <= 0 makes it permanent
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

        // Determine canonical id to use for identity checks.
        // Prefer BuffID if present; otherwise fall back to BuffName (ensure unique names)
        string id = !string.IsNullOrEmpty(buff.BuffID) ? buff.BuffID : buff.BuffName;

        // Check if buff is already active via the ID set
        if (activeBuffIds.Contains(id))
        {
            // find existing and refresh duration if applicable
            ActiveBuff existing = activeBuffs.Find(ab => ab.buffId == id);
            if (existing != null)
            {
                if (!existing.isPermanent && duration > 0f)
                {
                    existing.timeRemaining = duration;
                    Debug.Log($"Buff '{buff.BuffName}' duration refreshed to {duration}s");
                }
                else
                {
                    Debug.LogWarning($"Buff '{buff.BuffName}' is already active.");
                }
            }
            else
            {
                // Shouldn't happen, but keep consistent
                Debug.LogWarning($"BuffManager state mismatch: ID '{id}' present but no ActiveBuff found.");
            }
            return;
        }

        // Add new active buff record
        ActiveBuff newBuff = new ActiveBuff
        {
            buff = buff,
            timeRemaining = duration,
            isPermanent = duration <= 0f,
            buffId = id
        };

        activeBuffs.Add(newBuff);
        activeBuffIds.Add(id);

        // Apply buff effects to crop manager (ScriptableBuff should not change its own "owned" flags)
        buff.Apply(cropManagerObject);

        string durationText = newBuff.isPermanent ? "permanent" : $"{duration}s";
        Debug.Log($"Buff '{buff.BuffName}' (id: {id}) added ({durationText}) to {cropManagerObject.name}");
    }

    private void RemoveBuffAtIndex(int index)
    {
        if (index < 0 || index >= activeBuffs.Count) return;
        if (cropManagerObject == null) return;

        ActiveBuff buffToRemove = activeBuffs[index];
        buffToRemove.buff.Remove(cropManagerObject);

        // keep sets consistent
        if (!string.IsNullOrEmpty(buffToRemove.buffId))
            activeBuffIds.Remove(buffToRemove.buffId);

        activeBuffs.RemoveAt(index);

        Debug.Log($"Buff '{buffToRemove.buff.BuffName}' removed.");
    }

    public void RemoveBuff(ScriptableBuff buff)
    {
        if (buff == null) return;
        string id = !string.IsNullOrEmpty(buff.BuffID) ? buff.BuffID : buff.BuffName;

        for (int i = 0; i < activeBuffs.Count; i++)
        {
            if (activeBuffs[i].buffId == id)
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
        activeBuffIds.Clear();
        Debug.Log("All buffs removed.");
    }

    // Robust HasBuff: checks canonical ID set
    public bool HasBuff(ScriptableBuff buff)
    {
        if (buff == null) return false;
        string id = !string.IsNullOrEmpty(buff.BuffID) ? buff.BuffID : buff.BuffName;
        return activeBuffIds.Contains(id);
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
        if (buff == null) return 0f;
        string id = !string.IsNullOrEmpty(buff.BuffID) ? buff.BuffID : buff.BuffName;
        ActiveBuff active = activeBuffs.Find(ab => ab.buffId == id);
        return active != null ? active.timeRemaining : 0f;
    }

    public static BuffManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}