using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazorFix;
using PencilCase.Shared.Models.Notebooks;
using PencilCase.Web.Pages.Notebooks.Models;
using PencilCase.Web.Services;

namespace PencilCase.Web.Pages.Notebooks.Topics;

public partial class TopicView : ComponentBase
{
    private IEnumerable<BlockViewModel> _blockChildren = new List<BlockViewModel>();
    [Parameter] public BlockViewModel? Block { get; set; }
    [Parameter] public IBlocksApi BlocksApi { get; set; } = null!;
    [Parameter] public EventCallback<BlockViewModel> RedirectToBlock { get; set; }
    private MudTable<BlockViewModel> _blocksTable = new();

    private BlockViewModel _rowBeforeEditing = new();

    private record RowClickRecord(DateTime ClickTimestamp, BlockViewModel? RowClicked);

    private RowClickRecord? _lastRowClicked = null;
    
    public async Task<TableData<BlockViewModel>> ServerReload(TableState state, CancellationToken token)
    {
        await LoadChildren();
        var blockViewModels = _blockChildren.ToList();
        var totalItems = blockViewModels.Count();
        blockViewModels = SortData(state, blockViewModels);
        return new TableData<BlockViewModel>() { TotalItems = totalItems, Items = blockViewModels };
    }

    private async Task LoadChildren()
    {
        if (Block is not null && !(_blockChildren.Any() && _blockChildren.First().Id == Block.Id))
        {
            _blockChildren = await BlocksApi.LoadAllAsync(Block.ChildrenIds);
        }
    }

    private List<BlockViewModel> SortData(TableState state, List<BlockViewModel> data)
    {
        data = state.SortLabel switch
        {
            "name" => data.OrderByDirection(state.SortDirection, o => o.Name).ToList(),
            "type" => data.OrderByDirection(state.SortDirection, o => o.Type).ToList(),
            "modified-at" => data.OrderByDirection(state.SortDirection, o => o.Properties.LastModified).ToList(),
            _ => data
        };
        return data;
    }
    
    private async Task OnDoubleClicked(BlockViewModel block)
    {
        await RedirectToBlock.InvokeAsync(block);
    }

    private async Task RowClickEvent(TableRowClickEventArgs<BlockViewModel> tableRowClickEventArgs)
    {
        var block = tableRowClickEventArgs.Item;
        var clickRecord = new RowClickRecord(DateTime.Now, block);

        if (block != null && _lastRowClicked != null && _lastRowClicked.RowClicked == clickRecord.RowClicked)
        {
            var clickDuration = clickRecord.ClickTimestamp - _lastRowClicked.ClickTimestamp;
            if (clickDuration > TimeSpan.FromMilliseconds(100) && clickDuration < TimeSpan.FromMilliseconds(500))
            {
                await OnDoubleClicked(block);
            }
        }

        _lastRowClicked = clickRecord;
    }

    private void BackupItemBeforeEditing(Object obj)
    {
        var blockViewModel = (BlockViewModel)obj;
        _rowBeforeEditing = new BlockViewModel()
        {
            Id = blockViewModel.Id,
            Name = blockViewModel.Name,
            Properties = new BlockPropertiesViewModel()
            {
                CreatedOn = blockViewModel.Properties.CreatedOn,
                LastModified = blockViewModel.Properties.LastModified,
                Order = blockViewModel.Properties.Order
            },
            Type = blockViewModel.Type,
            ChildrenIds = blockViewModel.ChildrenIds,
            ParentId = blockViewModel.ParentId
        };
    }

    private void ResetRowToOriginalValues(Object row)
    {
        ((BlockViewModel)row).Id = _rowBeforeEditing.Id;
        ((BlockViewModel)row).Name = _rowBeforeEditing.Name;
        ((BlockViewModel)row).Type = _rowBeforeEditing.Type;
        ((BlockViewModel)row).Properties = new BlockPropertiesViewModel()
        {
            CreatedOn = _rowBeforeEditing.Properties.CreatedOn,
            LastModified = _rowBeforeEditing.Properties.LastModified,
            Order = _rowBeforeEditing.Properties.Order
        };
        ((BlockViewModel)row).ChildrenIds = _rowBeforeEditing.ChildrenIds;
        ((BlockViewModel)row).ParentId = _rowBeforeEditing.ParentId;
    }

    private void OnRowEditCommit(Object row)
    {
        var blockViewModel = (BlockViewModel)row;
        BlocksApi.UpdateBlock(blockViewModel);
    }

    private async Task OnDeleteClicked(EditButtonContext context)
    {
        var block = (BlockViewModel?)context.Item;
        if (block is not null)
        {
            await ShowDeleteDialog(block, DeleteBlock);
        }
    }

    private async Task ShowDeleteDialog(BlockViewModel block, Func<Guid,Task> onAccept)
    {
        var options = new DialogOptions()
        {
            Position = DialogPosition.TopCenter
        };
        
        var parameters = new DialogParameters<DeleteDialog>
        {
            { x => x.OnAcceptAsync, onAccept },
            { x => x.Block, block }
        };

        await DialogService.ShowAsync<DeleteDialog>($"Delete topic", parameters, options);
    }

    private async Task DeleteBlock(Guid id)
    {
        var childrenList = Block?.ChildrenIds.ToList();
        childrenList!.Remove(id);
        Block!.ChildrenIds = childrenList;
        
        await BlocksApi.DeleteBlock(id);
        
        await _blocksTable.ReloadServerData();
    }

    // Add
    private async Task OnAddNotebookClicked()
    {
        Snackbar.Add("Sorry! Adding Notebooks ources are not supported yet!");
    }

    private async Task OnAddSourceClicked()
    {
        Snackbar.Add("Sorry! Adding sources are not supported yet!");
    }

    private async Task OnAddTopicClicked()
    {
        var newTopic = new BlockViewModel()
        {
            Name = $"Untitled - {DateTime.Now.ToLocalTime():f}",
            ParentId = Block!.Id,
            ChildrenIds = new List<Guid>(),
            Type = BlockType.Topic
        };
        
        await BlocksApi.AddBlock(newTopic);
        await _blocksTable.ReloadServerData();
    }
}