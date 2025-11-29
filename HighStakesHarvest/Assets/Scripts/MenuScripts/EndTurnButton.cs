using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public void ForceEndTurn()
    {
        if (TurnManager.Instance == null)
        {
            Debug.LogWarning("EndTurnButton: No TurnManager instance found!");
            return;
        }

        // Only end turn if one is active
        if (TurnManager.Instance.IsTurnActive())
        {
            // Immediately set timer to 0
            Debug.Log("EndTurnButton: Forcing turn to end early.");
            typeof(TurnManager)
                .GetField("currentTurnTimeRemaining",
                           System.Reflection.BindingFlags.NonPublic |
                           System.Reflection.BindingFlags.Instance)
                .SetValue(TurnManager.Instance, 0f);

            // Call EndTurn normally
            TurnManager.Instance.EndTurn();
        }
        else
        {
            Debug.Log("EndTurnButton: Tried to end turn but no turn is active.");
        }
    }
}
