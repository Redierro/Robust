using UnityEngine;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    [Header("All Registered Items")]
    [SerializeField] private List<Item> availableItems;

    public static ItemManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // prevent duplicates
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Retrieve an item ScriptableObject by name.
    /// </summary>
    public Item GetItemByName(string itemName)
    {
        return availableItems.Find(item => item.itemName == itemName);
    }

    /// <summary>
    /// Optionally expose list for indexing if preferred.
    /// </summary>
    public Item GetItemByIndex(int index)
    {
        if (index >= 0 && index < availableItems.Count)
            return availableItems[index];
        return null;
    }
}
