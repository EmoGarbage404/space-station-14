using Content.Client.Examine;
using Content.Client.Gameplay;
using Content.Client.Interaction;
using Content.Client.Storage.Systems;
using Content.Client.UserInterface.Systems.Hotbar.Widgets;
using Content.Client.UserInterface.Systems.Storage.Controls;
using Content.Client.Verbs.UI;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Storage;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Timing;

namespace Content.Client.UserInterface.Systems.Storage;

public sealed class StorageUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<StorageSystem>
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IInputManager _input = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public readonly DragDropHelper<ItemGridPiece> _menuDragHelper;
    private ItemGridPiece? _draggedPiece;
    private StorageContainer? _container;

    private HotbarGui? Hotbar => UIManager.GetActiveUIWidgetOrNull<HotbarGui>();

    public bool IsDragging => _menuDragHelper.IsDragging;
    public ItemGridPiece? CurrentlyDragging => _menuDragHelper.Dragged;

    public StorageUIController()
    {
        _menuDragHelper = new DragDropHelper<ItemGridPiece>(OnMenuBeginDrag, OnMenuContinueDrag, OnMenuEndDrag);
    }

    public override void Initialize()
    {
        base.Initialize();

        //EntityManager.EventBus.SubscribeLocalEvent<StorageComponent, ComponentShutdown>(OnStorageShutdown);
    }

    private void OnStorageShutdown(EntityUid uid, StorageComponent component, ComponentShutdown args)
    {
        //todo: close the storage window nerd
    }

    public void OnSystemLoaded(StorageSystem system)
    {
        system.StorageOpened += OnStorageOpened;
        system.StorageClosed += OnStorageClosed;
        system.StorageUpdated += OnStorageUpdated;
    }

    public void OnSystemUnloaded(StorageSystem system)
    {
        system.StorageOpened -= OnStorageOpened;
        system.StorageClosed -= OnStorageClosed;
        system.StorageUpdated -= OnStorageUpdated;
    }

    public void OnStateEntered(GameplayState state)
    {

    }

    private void OnStorageOpened(EntityUid uid, StorageComponent component)
    {
        if (_container == null)
            return;

        _container.UpdateContainer((uid, component));
        _container.Visible = true;
    }

    private void OnStorageClosed(EntityUid uid, StorageComponent component)
    {
        if (_container == null)
            return;

        _container.Visible = false;
    }

    private void OnStorageUpdated(EntityUid uid, StorageComponent component)
    {
        if (_container == null)
            return;

        if (_container.StorageEntity != uid)
            return;

        _container.BuildItemPieces((uid, component));
    }

    public void RegisterStorageContainer(StorageContainer container)
    {
        if (_container != null)
        {
            container.OnPiecePressed -= OnPiecePressed;
            container.OnPieceUnpressed -= OnPieceUnpressed;
        }

        _container = container;
        container.OnPiecePressed += OnPiecePressed;
        container.OnPieceUnpressed += OnPieceUnpressed;
    }

    private void OnPiecePressed(GUIBoundKeyEventArgs args, ItemGridPiece control)
    {
        if (args.Function == EngineKeyFunctions.UIClick)
        {
            _menuDragHelper.MouseDown(control);
            _menuDragHelper.Update(0f);

            args.Handle();
        }
        else if (args.Function == EngineKeyFunctions.UIRightClick &&
                 _container?.StorageEntity != null &&
                 _player.LocalEntity.HasValue)
        {
            _entity.RaisePredictiveEvent(new StorageInteractWithItemEvent(
                _entity.GetNetEntity(control.Entity),
                _entity.GetNetEntity(_container.StorageEntity.Value)));
            args.Handle();
        }
        else if (args.Function == ContentKeyFunctions.ExamineEntity)
        {
            _entity.System<ExamineSystem>()
                .DoExamine(control.Entity);
        }
        else if (args.Function == EngineKeyFunctions.UseSecondary)
        {
            UIManager.GetUIController<VerbMenuUIController>().OpenVerbMenu(control.Entity);
        }
        else if (args.Function == ContentKeyFunctions.ActivateItemInWorld)
        {
            _entity.EntityNetManager?.SendSystemNetworkMessage(
                new InteractInventorySlotEvent(_entity.GetNetEntity(control.Entity), altInteract: false));
        }
        else if (args.Function == ContentKeyFunctions.AltActivateItemInWorld)
        {
            _entity.RaisePredictiveEvent(new InteractInventorySlotEvent(_entity.GetNetEntity(control.Entity), altInteract: true));
        }
    }

    private void OnPieceUnpressed(GUIBoundKeyEventArgs args, ItemGridPiece control)
    {
        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        if (_menuDragHelper.Dragged != null)
        {
            _menuDragHelper.EndDrag();
        }
        args.Handle();
    }

    private bool OnMenuBeginDrag()
    {
        if (_menuDragHelper.Dragged is not { } dragged)
            return false;

        _draggedPiece = new ItemGridPiece((dragged.Entity, _entity.GetComponent<ItemComponent>(dragged.Entity)),
            dragged.Location,
            _entity);
        _draggedPiece.MouseFilter = Control.MouseFilterMode.Ignore;
        _draggedPiece.Visible = true;
        _draggedPiece.Orphan();

        UIManager.PopupRoot.AddChild(_draggedPiece);
        LayoutContainer.SetPosition(_draggedPiece, UIManager.MousePositionScaled.Position / 2 - _draggedPiece.GetCenterOffset());
        return true;
    }

    private bool OnMenuContinueDrag(float frameTime)
    {
        if (_draggedPiece == null)
            return false;

        // I don't know why it divides the position by 2. Hope this helps! -emo
        LayoutContainer.SetPosition(_draggedPiece, UIManager.MousePositionScaled.Position / 2 - _draggedPiece.GetCenterOffset());
        return true;
    }

    private void OnMenuEndDrag()
    {
        if (_draggedPiece == null)
            return;
        _draggedPiece.Visible = false;
        _draggedPiece = null;
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        _menuDragHelper.Update(args.DeltaSeconds);
    }
}