using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;


public class DeckScript : MonoBehaviour
{
    public Sprite[] cardSprites;
    int[] cardValues = new int[53];
    int currentIndex = 0;

    void Start()
    {
        GetCardValues();
        
    }

    // Update is called once per frame
    void GetCardValues()
    {
        for (int i = 0; i < cardSprites.Length; i++)
        {
            int num = i % 13; // 0–12 for each suit
            if (num == 0)       // Ace
            {
                cardValues[i] = 1;
            }
            else if (num > 10)  // Jack, Queen, King
            {
                cardValues[i] = 10;
            }
            else
            {
                cardValues[i] = num + 1; // 2–10
            }
        }

        currentIndex = 0; // reset deck index
    }

    public void Shuffle()
    {
        for (int i = cardSprites.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1); // random index from 0 to i inclusive

            // swap sprites
            Sprite tempSprite = cardSprites[i];
            cardSprites[i] = cardSprites[j];
            cardSprites[j] = tempSprite;

            // swap values
            int tempValue = cardValues[i];
            cardValues[i] = cardValues[j];
            cardValues[j] = tempValue;
        }

        currentIndex = 0; // reset dealing index after shuffle
    }

    public int DealCard(CardScript cardScript)
    {
        cardScript.SetSprite(cardSprites[currentIndex]);
        cardScript.setValueOfCard(cardValues[currentIndex++]);    
        return cardScript.getValueOfCard();
    }

    public Sprite GetCardBack()
    {
        return cardSprites[0];
    }
}
