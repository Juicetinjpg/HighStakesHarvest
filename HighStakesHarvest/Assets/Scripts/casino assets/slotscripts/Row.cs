using UnityEngine;
using System.Collections;

public class Row : MonoBehaviour
{
    private int randomValue;
    private float timeInterval;
    public bool rowStopped;
    public string stoppedSlot;

    // Your spacing: -3, -1, 1, 3, 5 (intervals of 2)
    private float symbolSpacing = 2f;
    private float wrapTopPosition = 5f;
    private float wrapBottomPosition = -3f;
    private float detectionPosition = -1f; // Detection at Y = -1

    private void Start()
    {
        rowStopped = true;
        SlotController.HandlePulled += StartRotating;
    }

    private void StartRotating()
    {
        stoppedSlot = "";
        StartCoroutine("Rotate");
    }

    private IEnumerator Rotate()
    {
        rowStopped = false;
        timeInterval = .025f;

        // Initial fast spin
        for (int i = 0; i < 30; i++)
        {
            MoveSymbols(-symbolSpacing);
            yield return new WaitForSeconds(timeInterval);
        }

        randomValue = Random.Range(60, 100);
        switch (randomValue % 5)
        {
            case 1:
                randomValue += 2;
                break;
            case 2:
                randomValue += 1;
                break;
        }

        // Gradual slowdown
        for (int i = 0; i < randomValue; i++)
        {
            MoveSymbols(-symbolSpacing);

            if (i > Mathf.RoundToInt(randomValue * .25f))
                timeInterval = .05f;
            if (i > Mathf.RoundToInt(randomValue * .5f))
                timeInterval = .1f;
            if (i > Mathf.RoundToInt(randomValue * .75f))
                timeInterval = .15f;
            if (i > Mathf.RoundToInt(randomValue * .95f))
                timeInterval = .2f;

            yield return new WaitForSeconds(timeInterval);
        }

        // Find the symbol at Y = -1 (or closest to it)
        Transform centerSymbol = GetCenterSymbol();
        if (centerSymbol != null)
        {
            stoppedSlot = centerSymbol.name;
            Debug.Log($"Stopped on: {stoppedSlot} at position Y={centerSymbol.localPosition.y}");
        }
        else
        {
            stoppedSlot = "Cherry";
            Debug.LogWarning("No center symbol found!");
        }

        rowStopped = true;
    }

    private void MoveSymbols(float distance)
    {
        // Move all child symbols
        foreach (Transform child in transform)
        {
            Vector3 newPos = child.localPosition;
            newPos.y += distance;

            // Wrap around: if symbol goes below -3, move it to 5
            if (newPos.y < wrapBottomPosition)
            {
                newPos.y = wrapTopPosition + (newPos.y - wrapBottomPosition);
            }
            // Wrap around: if symbol goes above 5, move it to -3
            else if (newPos.y > wrapTopPosition)
            {
                newPos.y = wrapBottomPosition + (newPos.y - wrapTopPosition);
            }

            child.localPosition = newPos;
        }
    }

    private Transform GetCenterSymbol()
    {
        Transform closest = null;
        float minDistance = float.MaxValue;

        // Find the symbol closest to Y = -1 (the detection position)
        foreach (Transform child in transform)
        {
            float distance = Mathf.Abs(child.localPosition.y - detectionPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = child;
            }
        }

        return closest;
    }

    private void OnDestroy()
    {
        SlotController.HandlePulled -= StartRotating;
    }
}