using Content.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Systems.Inventory.Controls;

[Virtual]
public class ItemSlotButtonContainer : ItemSlotUIContainer<SlotControl>
{
    protected readonly InventoryUIController InventoryController;
    private string _slotGroup = "";

    public string SlotGroup
    {
        get => _slotGroup;
        set
        {
            InventoryController.RemoveSlotGroup(SlotGroup);
            _slotGroup = value;
            InventoryController.RegisterSlotGroupContainer(this);
        }
    }

    public ItemSlotButtonContainer()
    {
        InventoryController = UserInterfaceManager.GetUIController<InventoryUIController>();
    }

    ~ItemSlotButtonContainer()
    {
        InventoryController.RemoveSlotGroup(SlotGroup);
    }
}
