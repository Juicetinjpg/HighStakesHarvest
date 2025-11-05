using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;
public class CardScript : MonoBehaviour
{
    public int value = 0;

    public int getValueOfCard()
    {
        return value;
    }

    public void setValueOfCard(int newvalue)
    {
        value = newvalue;
    }

    public string getSpriteName()
    {
        return GetComponent<SpriteRenderer>().sprite.name;

    }

    public void SetSprite(Sprite newSprite)
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = newSprite;
    }

    public void ResetCard()
    {
        Sprite Back = GameObject.Find("Deck").GetComponent<DeckScript>().GetCardBack();
        gameObject.GetComponent<SpriteRenderer>().sprite = Back;
        value= 0;
    }
}
