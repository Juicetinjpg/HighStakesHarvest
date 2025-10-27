// Justin Gabon
// 10/12/2025
// Simple item icon database - assign sprites in Inspector

using UnityEngine;
using System.Collections.Generic;

public class SimpleItemIcons : MonoBehaviour
{
    public static SimpleItemIcons Instance { get; private set; }
    
    [System.Serializable]
    public class ItemIcon
    {
        public string itemName;
        public Sprite icon;
    }
    
    [Header("Item Icons")]
    [SerializeField] private List<ItemIcon> itemIcons = new List<ItemIcon>();
    
    private Dictionary<string, Sprite> iconDictionary;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Build dictionary for fast lookup
        BuildDictionary();
    }
    
    private void BuildDictionary()
    {
        iconDictionary = new Dictionary<string, Sprite>();
        foreach (var item in itemIcons)
        {
            if (!string.IsNullOrEmpty(item.itemName) && item.icon != null)
            {
                iconDictionary[item.itemName] = item.icon;
            }
        }
    }
    
    /// <summary>
    /// Gets the sprite for an item by name
    /// </summary>
    public Sprite GetIcon(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;
        
        if (iconDictionary != null && iconDictionary.ContainsKey(itemName))
        {
            return iconDictionary[itemName];
        }
        
        return null;
    }
    
    /// <summary>
    /// Checks if an icon exists for an item
    /// </summary>
    public bool HasIcon(string itemName)
    {
        return iconDictionary != null && iconDictionary.ContainsKey(itemName);
    }
}