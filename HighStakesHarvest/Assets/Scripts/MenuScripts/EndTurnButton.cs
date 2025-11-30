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

        if (TurnManager.Instance.IsTurnActive())
        {
            float timeRemaining = TurnManager.Instance.GetTurnTimeRemaining();

            Debug.Log("EndTurnButton: Forcing turn to end early. Time left: " + timeRemaining);

            // Log the early turn usage
            TurnManager.Instance.LogEarlyTurnEnd(timeRemaining);

            // Immediately set timer to 0 via reflection
            typeof(TurnManager)
                .GetField("currentTurnTimeRemaining",
                           System.Reflection.BindingFlags.NonPublic |
                           System.Reflection.BindingFlags.Instance)
                .SetValue(TurnManager.Instance, 0f);

            // End the turn normally
            TurnManager.Instance.EndTurn();
        }
        else
        {
            Debug.Log("EndTurnButton: Tried to end turn but no turn is active.");
        }
    }

}
