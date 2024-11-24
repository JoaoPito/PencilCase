using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using MudBlazor.Extensions;
using PencilCase.Shared.Models.Notebooks;
using PencilCase.Web.Pages.Notebooks.Models;
using PencilCase.Web.Services;

namespace PencilCase.Web.Pages.Notebooks.Notebooks;

public partial class CellView : ComponentBase
{
    bool _isLoading = false;
    
    [Parameter] public BlockViewModel? Block { get; set; }
    [Parameter] public IBlocksApi BlocksApi { get; set; } = null!;
    [Parameter] public EventCallback? OnNewCellShortcut { get; set; }

    IEnumerable<BlockViewModel> _loadedChildren = new List<BlockViewModel>();
    BlockViewModel? _shownChild;

    MudTextField<string> _inputTextField = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadChildren();
        await base.OnInitializedAsync();
    }

    async Task OnInputValueChanged(string newValue)
    {
        if (Block is not null)
        {
            Block.Name = newValue.Trim();
            await UpdateChangesTo(Block);
        }
    }

    async Task OnKeyDown(KeyboardEventArgs args)
    {
        if (args.CtrlKey && args.Key=="Enter")
        {
            await _inputTextField.BlurAsync();
            await GenerateOutputIfPossible();
        }

        if (args.ShiftKey && args.Key == "Enter")
        {
            Console.WriteLine("Creating new cell");
            if(OnNewCellShortcut is not null) await OnNewCellShortcut.As<EventCallback>().InvokeAsync();
        }
    }

    async Task OnSubmitClick()
    {
        await _inputTextField.BlurAsync();
        await GenerateOutputIfPossible();
    }

    void OnArrowLeftClick()
    {
        
    }

    void OnArrowRightClick()
    {
        
    }

    void OnDeleteClick()
    {
        
    }
    
    async Task LoadChildren()
    {
        if (Block is not null && Block.ChildrenIds.Any())
        {
            _loadedChildren = await BlocksApi.GetChildren(Block.Id);
            _shownChild = _loadedChildren
                .OrderBy(c => c.Properties.Order)
                .First();
        }
    }
    
    int _exampleCounter = 0;
    async Task GenerateOutputIfPossible()
    {
        if (IsGenerator())
        {
            _isLoading = true;
            StateHasChanged();

            // Example generation for testing
            var exampleGeneration = new BlockViewModel()
            {
                Id = new Guid(),
                Name = $"{_exampleCounter++} - ASCII, short for American Standard Code for Information Interchange, is a character encoding standard used to represent text in computers and other devices that handle text.\nIt was developed in the 1960s and became widely adopted as a standard for communication between different systems.",
                ParentId = Guid.Parse("808d779b-c6a5-4a8b-aa4d-ea1b4a7f6ae1"),
                Properties = new BlockPropertiesViewModel()
                {
                    CellType = CellType.Text,
                    Order = _shownChild is null ? 0 : _shownChild.Properties.Order + 1
                }
            };
            Block!.ChildrenIds = Block!.ChildrenIds.Append(exampleGeneration.Id);
            _loadedChildren = _loadedChildren.Append(exampleGeneration);
            _shownChild = exampleGeneration;
            await Task.Delay(2000);
        
            _isLoading = false;
            StateHasChanged();
        }
    }

    bool IsGenerator()
    {
        return Block?.Properties.CellType is CellType.Question;
    }

    async Task UpdateChangesTo(BlockViewModel block)
    {
        await BlocksApi.UpdateBlock(block);
    }
}