using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazorFix;
using PencilCase.Shared.Models.Notebooks;
using PencilCase.Web.Pages.Notebooks.Models;
using PencilCase.Web.Services;

namespace PencilCase.Web.Pages.Notebooks.Topics;

public partial class TopicView : ComponentBase
{
    [CascadingParameter(Name = "block")] protected BlockViewModel? Block { get; set; }
    
    private IEnumerable<BlockViewModel> _blockChildren = new List<BlockViewModel>();
    
    [Parameter] public IBlocksApi BlocksApi { get; set; } = null!;
    
    private bool _isLoading = false;
    
    [Parameter] public Action<BlockViewModel> RedirectTo { get; set; }
    
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
        if (Block is not null)
        {
            _blockChildren = await BlocksApi.LoadAllAsync(Block.ChildrenIds);
        }
    }

    private async Task ReloadCurrentBlock()
    {
        if(Block is not null)
            Block = await BlocksApi.GetBlock(Block.Id);
    }

    public async Task LoadDataFor(BlockViewModel block)
    {
        Block = block;
        await ReloadCurrentBlock();
        await _blocksTable.ReloadServerData();
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
    
    // Navigation
    
    private async Task OnBackPageClick()
    {
        if (Block is not null && Block.ParentId is not null)
        {
            Block = await BlocksApi.GetBlock((Guid)Block.ParentId!);
            await _blocksTable.ReloadServerData();
        }
    }
    
    private async Task OnDoubleClicked(BlockViewModel block)
    {
        RedirectTo(block);
        
        /*
        if (block.Type == BlockType.Topic || block.Type == BlockType.Source)
        {
            Block = block;
            await ReloadCurrentBlock();
            await _blocksTable.ReloadServerData();
        }
        else
        {
            await RedirectToBlock.InvokeAsync(block);
        }*/
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
    
    private int _selectedRowNumber = -1;

    private string SelectedRowClassFunc(BlockViewModel block, int rowNumber)
    {
        if (_selectedRowNumber == rowNumber)
        {
            _selectedRowNumber = -1;
            return string.Empty;
        }
        else if (_blocksTable.SelectedItem != null && _blocksTable.SelectedItem.Equals(block))
        {
            _selectedRowNumber = rowNumber;
            return "selected";
        }
        else
        {
            return string.Empty;
        }
    }
    
    // Editing blocks
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

    // Delete blocks
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

    // Add blocks
    private async Task AddNewBlockAndReload(BlockViewModel block)
    {
        var createdBlock = await BlocksApi.AddBlock(block);
        RedirectTo(createdBlock);
        
        /*
        if (block.Type == BlockType.Topic || block.Type == BlockType.Source)
        {
            await ReloadCurrentBlock();
            await _blocksTable.ReloadServerData();
        }
        else
        {
            await RedirectToBlock.InvokeAsync(createdBlock);
        }
        */
    }
    
    private async Task OnAddNotebookClicked()
    {
        _isLoading = true;
        var newNotebook = new BlockViewModel()
        {
            Name = $"Untitled - {DateTime.Now.ToLocalTime():g}",
            ParentId = Block!.Id,
            ChildrenIds = new List<Guid>(),
            Type = BlockType.Notebook
        };

        await AddNewBlockAndReload(newNotebook);
        _isLoading = false;
    }

    private async Task OnAddSourceClicked()
    {
        _isLoading = true;
        Snackbar.Add("Sorry! Adding Sources are not supported yet!", Severity.Error);
        _isLoading = false;
    }

    private async Task OnAddTopicClicked()
    {
        _isLoading = true;
        var newTopic = new BlockViewModel()
        {
            Name = $"Untitled - {DateTime.Now.ToLocalTime():g}",
            ParentId = Block!.Id,
            ChildrenIds = new List<Guid>(),
            Type = BlockType.Topic
        };
        
        await AddNewBlockAndReload(newTopic);
        _isLoading = false;
    }
}