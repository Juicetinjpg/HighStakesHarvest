using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotController : MonoBehaviour
{
    public static event Action HandlePulled = delegate { };

    [SerializeField]
    private Text prizeText;

    [SerializeField]
    private Row[] rows;

    [SerializeField]
    private Transform handle;

    private int prizeValue;
    private bool resultsChecked = false;

    private void Start()
    {
        // Validate that all rows are assigned
        if (rows == null || rows.Length < 3)
        {
            Debug.LogError("SlotController: rows array is not properly assigned! Need 3 rows.");
            return;
        }

        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] == null)
            {
                Debug.LogError($"SlotController: Row {i} is null!");
            }
        }
    }

    private void Update()
    {
        // Add null checks
        if (rows == null || rows.Length < 3) return;
        if (rows[0] == null || rows[1] == null || rows[2] == null) return;

        if (!rows[0].rowStopped || !rows[1].rowStopped || !rows[2].rowStopped)
        {
            prizeValue = 0;
            if (prizeText != null)
                prizeText.enabled = false;
            resultsChecked = false;
        }

        if (rows[0].rowStopped && rows[1].rowStopped && rows[2].rowStopped && !resultsChecked)
        {
            CheckResults();
            if (prizeText != null)
            {
                prizeText.enabled = true;
                prizeText.text = "Prize: " + prizeValue;
            }
        }
    }

    private void OnMouseDown()
    {
        if (rows == null || rows.Length < 3) return;
        if (rows[0] == null || rows[1] == null || rows[2] == null) return;

        if (rows[0].rowStopped && rows[1].rowStopped && rows[2].rowStopped)
        {
            StartCoroutine("PullHandle");
        }
    }

    private IEnumerator PullHandle()
    {
        for (int i = 0; i < 15; i += 5)
        {
            if (handle != null)
                handle.Rotate(0f, 0f, i);
            yield return new WaitForSeconds(.1f);
        }

        HandlePulled();

        for (int i = 0; i < 15; i += 5)
        {
            if (handle != null)
                handle.Rotate(0f, 0f, -i);
            yield return new WaitForSeconds(.1f);
        }
    }

    private Dictionary<string, int> threeMatchPrizes = new Dictionary<string, int>()
    {
        { "Cherry", 200 },
        { "Bell", 400 },
        { "Bar", 600 },
        { "Seven", 1000 }
    };

    private Dictionary<string, int> twoMatchPrizes = new Dictionary<string, int>()
    {
        { "Cherry", 100 },
        { "Bell", 200 },
        { "Bar", 300 },
        { "Seven", 500 }
    };

    private void CheckResults()
    {
        if (rows == null || rows.Length < 3) return;
        if (rows[0] == null || rows[1] == null || rows[2] == null) return;

        string slot0 = rows[0].stoppedSlot;
        string slot1 = rows[1].stoppedSlot;
        string slot2 = rows[2].stoppedSlot;

        // Debug to see what stopped
        Debug.Log($"Results: {slot0}, {slot1}, {slot2}");

        // Check for three matching
        if (slot0 == slot1 && slot1 == slot2 && !string.IsNullOrEmpty(slot0))
        {
            if (threeMatchPrizes.ContainsKey(slot0))
                prizeValue = threeMatchPrizes[slot0];
            resultsChecked = true;
            return;
        }

        // Check for two matching
        string matchedSymbol = null;
        if (slot0 == slot1 && !string.IsNullOrEmpty(slot0))
            matchedSymbol = slot0;
        else if (slot0 == slot2 && !string.IsNullOrEmpty(slot0))
            matchedSymbol = slot0;
        else if (slot1 == slot2 && !string.IsNullOrEmpty(slot1))
            matchedSymbol = slot1;

        if (matchedSymbol != null && twoMatchPrizes.ContainsKey(matchedSymbol))
        {
            prizeValue = twoMatchPrizes[matchedSymbol];
        }

        resultsChecked = true;
    }
}