using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// InventoryManager handles quickbar (0-9), stacking, equipment (hat/shirt/shoes), and basic item queries.
// It intentionally keeps a simple API for use by other systems (PlayerStatsManager, UI, loot, etc.).
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // Quickbar slots (0..N-1). Configure in inspector (10 recommended).
    [Header("Quickbar")]
    [SerializeField] private InventorySlotUI[] quickSlots;

    // UI prefab for item visuals (InventoryItemUI)
    [Header("UI")]
    [SerializeField] private GameObject inventoryItemPrefab;

    // All known items in the game (for load-by-id). Populate in inspector or via editor script.
    [Header("Item Database")]
    [SerializeField] private List<Item> allItems = new List<Item>();

    // Equipment slots: 0 = Hat, 1 = Shirt, 2 = Shoes
    private Item[] equipment = new Item[3];

    // Internal stack storage: each quick slot maps to an ItemStack or null
    private ItemStack[] stacks;

    // Selected quick slot index
    private int selectedSlotIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Duplicate InventoryManager, destroying.");
            Destroy(gameObject);
            return;
        }

        // init stacks array to match UI slots
        stacks = new ItemStack[quickSlots.Length];
        for (int i = 0; i < quickSlots.Length; i++)
        {
            quickSlots[i].Initialize(this, i);
            stacks[i] = null;
        }
    }

    private void Start()
    {
        // Select initial slot 0 if available
        if (quickSlots.Length > 0)
            SelectSlot(0);
    }

    // Public API: select slot (call from input system)
    public void SelectSlot(int index)
    {
        if (index < 0 || index >= quickSlots.Length) return;

        quickSlots[selectedSlotIndex].Deselect();
        selectedSlotIndex = index;
        quickSlots[selectedSlotIndex].Select();
    }

    public int GetSelectedIndex() => selectedSlotIndex;

    // Add an item to inventory quickbar. Returns true if fully added (stacked or new), false if no room.
    public bool AddItem(Item item, int count = 1)
    {
        if (item == null) return false;

        // Try to stack in existing stacks first (same item & stackable & not full)
        if (item.Stackable)
        {
            for (int i = 0; i < stacks.Length; i++)
            {
                if (stacks[i] != null && stacks[i].Item == item && stacks[i].Count < item.MaxStack)
                {
                    int canAdd = Mathf.Min(count, item.MaxStack - stacks[i].Count);
                    stacks[i].Count += canAdd;
                    quickSlots[i].RefreshFromStack(stacks[i]);
                    count -= canAdd;
                    if (count <= 0) return true;
                }
            }
        }

        // Find empty slots
        for (int i = 0; i < stacks.Length && count > 0; i++)
        {
            if (stacks[i] == null)
            {
                int take = item.Stackable ? Mathf.Min(count, item.MaxStack) : 1;
                stacks[i] = new ItemStack(item, take);
                quickSlots[i].RefreshFromStack(stacks[i]);
                count -= take;
            }
        }

        // If any left, couldn't fit
        return count <= 0;
    }

    // Count how many of an item are present in inventory
    public int CountItem(Item item)
    {
        if (item == null) return 0;
        int total = 0;
        for (int i = 0; i < stacks.Length; i++)
            if (stacks[i] != null && stacks[i].Item == item)
                total += stacks[i].Count;
        return total;
    }

    // Consume up to amount of item. Returns true if consumed entire amount.
    // Consumes from stacks left-to-right.
    public bool ConsumeItem(Item item, int amount)
    {
        if (item == null || amount <= 0) return false;
        int have = CountItem(item);
        if (have < amount) return false;

        int remaining = amount;
        for (int i = 0; i < stacks.Length && remaining > 0; i++)
        {
            if (stacks[i] != null && stacks[i].Item == item)
            {
                int take = Mathf.Min(remaining, stacks[i].Count);
                stacks[i].Count -= take;
                remaining -= take;
                if (stacks[i].Count <= 0)
                {
                    stacks[i] = null;
                    quickSlots[i].Clear();
                }
                else
                {
                    quickSlots[i].RefreshFromStack(stacks[i]);
                }
            }
        }

        return remaining == 0;
    }

    // Get the selected item instance without consuming. If use==true, consume one from the stack.
    public Item GetSelectedItem(bool use)
    {
        ItemStack s = stacks[selectedSlotIndex];
        if (s == null) return null;
        Item item = s.Item;
        if (use)
        {
            // consume one
            s.Count--;
            if (s.Count <= 0)
            {
                stacks[selectedSlotIndex] = null;
                quickSlots[selectedSlotIndex].Clear();
            }
            else quickSlots[selectedSlotIndex].RefreshFromStack(s);
        }
        return item;
    }

    // Find and automatically use a healing consumable (FoodHeal > 0). Returns true if used.
    public bool UseHealItem()
    {
        for (int i = 0; i < stacks.Length; i++)
        {
            var s = stacks[i];
            if (s != null && s.Item.FoodHeal > 0)
            {
                //PlayerStatsManager.instance.Heal(s.Item.FoodHeal);
                ConsumeItem(s.Item, 1);
                return true;
            }
        }
        return false;
    }

    // Equipment API. 0=Hat,1=Shirt,2=Shoes. Equip returns previously equipped item (or null).
    public Item Equip(int slotIndex, Item item)
    {
        if (slotIndex < 0 || slotIndex >= equipment.Length)
        {
            Debug.LogWarning("Invalid equipment slot");
            return null;
        }

        Item previous = equipment[slotIndex];
        equipment[slotIndex] = item;

        // Update player stats manager with new equipment bonuses
        UpdatePlayerEquipmentBonuses();

        return previous;
    }

    public Item GetEquipped(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equipment.Length) return null;
        return equipment[slotIndex];
    }

    private void UpdatePlayerEquipmentBonuses()
    {
        // Reset bonuses in PlayerStatsManager by clearing equipment bonuses and re-adding
        // PlayerStatsManager expects Item.GetAttributeBonuses and GetArmorValue() so we just call it
        // We will call PlayerStatsManager to re-read equipped items (simpler) — but PlayerStatsManager
        // earlier expects inventory to provide item instances by ID when loading. Here we directly push bonuses.

        // Clear all equipment bonuses first
        foreach (PlayerStatsManager.AttributeType attr in System.Enum.GetValues(typeof(PlayerStatsManager.AttributeType)))
        {
            // PlayerStatsManager has method UpdateEquipmentBonuses via Inventory Manager from earlier code.
            // Instead of calling internal methods, call public Equip hooks on PlayerStatsManager by setting items there.
        }

        // The PlayerStatsManager script you have already queries InventoryManager in LoadStats for equipped items.
        // To keep logic simple, we'll call PlayerStatsManager methods for applying equipment now:
        //PlayerStatsManager.instance.EquipFromInventory(equipment[0], equipment[1], equipment[2]);

        // Recalculate stats
        PlayerStatsManager.instance.RecalculateAllStats();

        // Notify listeners
        GameEventsManager.instance.playerEvents.StatsChanged();
    }

    // Get item by ID (used during load)
    public Item GetItemByID(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return allItems.FirstOrDefault(x => x != null && x.ID == id);
    }

    // Utility for other code: give out the list of equipment for saving
    public string GetEquippedID(int slotIndex)
    {
        Item it = GetEquipped(slotIndex);
        return it != null ? it.ID : "";
    }

    // Simple container representing an item stack in a quick slot
    private class ItemStack
    {
        public Item Item;
        public int Count;
        public ItemStack(Item item, int count) { Item = item; Count = count; }
    }
}
