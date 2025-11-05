using UnityEngine;
using System.Collections.Generic;

public class PlayerScript : MonoBehaviour
{
    public DeckScript deckScript;
    public GameObject[] hand;

    public int handValue = 0;
    public int cardIndex = 0;

    private List<CardScript> aceList = new List<CardScript>();
    private int money = 1000;

    // --- Deals the initial 2-card hand ---
    public void DealInitialHand()
    {
        cardIndex = 0;
        handValue = 0;
        aceList.Clear();

        // Reset visuals
        foreach (GameObject cardObj in hand)
        {
            CardScript cs = cardObj.GetComponent<CardScript>();
            cs.ResetCard();
            cs.GetComponent<Renderer>().enabled = false;
        }

        // Deal exactly 2 cards
        DealOneCard();
        DealOneCard();

        AceCheck();
    }

    // --- Deals one card for Hit ---
    public void HitOneCard()
    {
        if (cardIndex >= hand.Length)
        {
            Debug.Log("No more cards in hand!");
            return;
        }

        DealOneCard();
        AceCheck();
    }

    // --- Internal function to deal a single card ---
    private void DealOneCard()
    {
        Debug.Log("Dealing card at index: " + cardIndex);

        CardScript card = hand[cardIndex].GetComponent<CardScript>();
        int value = deckScript.DealCard(card);
        card.GetComponent<Renderer>().enabled = true;

        handValue += value;

        if (value == 1)
            aceList.Add(card);

        cardIndex++;
    }

    // --- Ace adjustment ---
    public void AceCheck()
    {
        foreach (CardScript ace in aceList)
        {
            if (handValue + 10 <= 21 && ace.getValueOfCard() == 1)
            {
                ace.setValueOfCard(11);
                handValue += 10;
            }
            else if (handValue > 21 && ace.getValueOfCard() == 11)
            {
                ace.setValueOfCard(1);
                handValue -= 10;
            }
        }
    }

    // --- Money management ---
    public void AdjustMoney(int amount) { money += amount; }
    public int GetMoney() { return money; }
}
