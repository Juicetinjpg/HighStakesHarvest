using UnityEngine;

/// <summary>
/// Base class for all items in the game. All specific item types should inherit from this.
/// </summary>
public abstract class ItemData : ScriptableObject
{
    [Header("Base Item Properties")]
    public string itemName;
    public string description;
    public Sprite icon;
    public int stackSize = 10;
    public bool isTradeable = true;
    public ItemType itemType;
    
    [Header("Economic Properties")]
    public int basePrice;
    public float sellPriceMultiplier = 0.5f; // Sell for 50% of buy price by default
    
    /// <summary>
    /// Provides the sprite used to visually represent the item.
    /// </summary>
    public virtual Sprite GetIcon()
    {
        return icon;
    }
    
    /// <summary>
    /// Gets the selling price of this item
    /// </summary>
    public virtual int GetSellPrice()
    {
        return Mathf.FloorToInt(basePrice * sellPriceMultiplier);
    }
    
    /// <summary>
    /// Gets the buying price of this item
    /// </summary>
    public virtual int GetBuyPrice()
    {
        return basePrice;
    }
    
    /// <summary>
    /// Called when the item is used. Override in derived classes for specific behavior.
    /// </summary>
    public abstract bool Use(GameObject user);
    
    /// <summary>
    /// Gets a display-friendly description of the item
    /// </summary>
    public virtual string GetDisplayInfo()
    {
        return $"{itemName}\n{description}\nValue: ${GetSellPrice()}";
    }
}

/// <summary>
/// Defines the different categories of items in the game
/// </summary>
public enum ItemType
{
    Seed,
    Crop,
    Tool,
    Resource,
    Consumable,
    Special
}
