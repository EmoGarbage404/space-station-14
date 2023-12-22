using Content.Client.UserInterface.Controls;
using Robust.Client.UserInterface;

namespace Content.Client.UserInterface.Systems.Inventory.Controls;

public sealed class ItemSlotClothingContainer : ItemSlotButtonContainer
{
    public int MaxColumns;

    public override SlotControl? AddButton(SlotControl newButton)
    {
        if (!InventoryController.TryGetPlayerSlotData(newButton.SlotName, out var slotData))
            return base.AddButton(newButton);

        if (Children.Contains(newButton) || newButton.Parent != null || newButton.SlotName == "")
            return AddButtonToDict(newButton);

        var targetIndex = slotData.ButtonOffset.Y * MaxColumns + slotData.ButtonOffset.X;

        var indexDif = targetIndex - ChildCount;
        for (var i = 0; i <= indexDif; i++)
        {
            AddChild(new Control
            {
                MinSize = newButton.Size
            });
        }

        RemoveChild(GetChild(targetIndex));
        AddChild(newButton);
        newButton.SetPositionInParent(targetIndex);

        return AddButtonToDict(newButton);
    }
}
