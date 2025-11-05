using System;
using System.Collections.Generic;
using UnityEngine;

public class CropManager : MonoBehaviour
{

    //public TextAsset jsonFile;
    public Dictionary<string, CropInfo> cropInfoDictionary = new Dictionary<string, CropInfo>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //string jsonString = jsonFile.text;

        setDebugDictionaryValues();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void setDebugDictionaryValues()
    {
        // name, plural vers, value, growth, quantity, type
        cropInfoDictionary.Add("Potato", new CropInfo("Potato", "Potatoes", 10, 3, 3, "Vegetable"));
        cropInfoDictionary.Add("Blueberry", new CropInfo("Blueberry", "Blueberries", 5, 3, 6, "Fruit"));
        cropInfoDictionary.Add("Pumpkin", new CropInfo("Pumpkin", "Pumpkins", 15, 3, 1, "Fruit"));
    }

    // getters

    public CropInfo getCropInfo(string name)
    {
        return cropInfoDictionary[name];
    }

    public int getCropValue(string name)
    {
        return cropInfoDictionary[name].value;
    }

    public int getCropGrowth(string name)
    {
        return cropInfoDictionary[name].growth;
    }

    public int getCropQuantity(string name)
    {
        return cropInfoDictionary[name].quantity;
    }

    public void ApplySpecificValueBuff(CropInfo crop, float modifier)
    {
        crop.value = Mathf.CeilToInt(crop.value * modifier);

        Debug.Log("New value of "+crop.name+"is "+crop.value);
    }

    public void ApplySpecificGrowthDecrease(CropInfo crop, int decrease)
    {
        crop.growth -= decrease;

        Debug.Log("New growth of " + crop.name + "is " + crop.growth);
    }

    public void ApplySpecificQuantityBuff(CropInfo crop, float modifier)
    {
        crop.quantity = Mathf.CeilToInt(crop.quantity * modifier);

        Debug.Log("New quantity of " + crop.name + "is " + crop.quantity);
    }




}
