using UnityEngine;

public class DealerEmotionSystem : MonoBehaviour
{
    // All possible emotional states
    public enum EmotionState
    {
        Confident,
        Nervous,
        Neutral,
        Bluffing,
        Lying
    }

    public EmotionState currentState;

    // values that update dynamically
    private int dealerRealValue;
    private int dealerVisibleValue;
    private float bluffChance;
    private float lieChance;

   
    public void EvaluateInitialHand(int visibleCard, int hiddenCard)
    {
        dealerVisibleValue = visibleCard;
        dealerRealValue = visibleCard + hiddenCard;

        // Determine emotional state based on total
        if (dealerRealValue >= 18)
            currentState = EmotionState.Confident;

        else if (dealerRealValue <= 12)
            currentState = EmotionState.Nervous;

        else
            currentState = EmotionState.Neutral;

        // Probability values for bluffing/lying
        bluffChance = 0.20f; 
        lieChance = 0.10f;   
    }

  
    public string GetDealerStatement()
    {
        // check for lying behavior
        if (Random.value <= lieChance)
        {
            currentState = EmotionState.Lying;
            return GenerateLie();
        }

        // bluffing behavior
        if (Random.value <= bluffChance)
        {
            currentState = EmotionState.Bluffing;
            return GenerateBluff();
        }

        // True emotions
        switch (currentState)
        {
            case EmotionState.Confident:
                return "Dealer seems confident...";
            case EmotionState.Nervous:
                return "Dealer looks nervous.";
            default:
                return "Dealer is unreadable.";
        }
    }

    private string GenerateBluff()
    {
        return "Dealer smirks confidently.";
    }

    private string GenerateLie()
    {
        return "Dealer looks calm… but something feels off.";
    }
}
