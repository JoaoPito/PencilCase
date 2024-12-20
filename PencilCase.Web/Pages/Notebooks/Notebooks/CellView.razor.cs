using Microsoft.AspNetCore.Components;
using MudBlazor;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;
using PencilCase.Web.Pages.Notebooks.Models;
using PencilCase.Web.Services.LLM;
using PencilCase.Web.Services.Notebooks;

namespace PencilCase.Web.Pages.Notebooks.Notebooks;

public partial class CellView : ComponentBase
{
    bool _isLoading = false;
    bool _childError = false;
    
    public Guid? TopicId { get; set; }
    [Parameter] public BlockViewModel? Block { get; set; }
    [Parameter] public IBlocksApi BlocksApi { get; set; } = null!;
    [Parameter] public Func<string, Task<IEnumerable<RagDocument>>> RagSearchAsync { get; set; } = null!;
    [Parameter] public Func<string, IEnumerable<RagDocument>, Task<IEnumerable<LlmMessage>>> InvokeLlmAsync { get; set; } = null!;
    [Parameter] public EventCallback? OnNewCellShortcut { get; set; }

    IEnumerable<BlockViewModel> _loadedChildren = new List<BlockViewModel>();
    BlockViewModel? _shownChild;

    MudTextField<string> _inputTextField = null!;
    private string? _cellMsg;

    protected override async Task OnInitializedAsync()
    {
        await LoadChildren();
        await base.OnInitializedAsync();
    }
    
    async Task LoadChildren()
    {
        _childError = false;
        if (Block is not null && Block.ChildrenIds.Any())
        {
            try
            {
                _loadedChildren = await BlocksApi.GetChildren(Block.Id);
                _loadedChildren = _loadedChildren.OrderBy(c => c.Properties.CreatedOn);
                _shownChild = _loadedChildren
                    .OrderBy(c => c.Properties.Order)
                    .Last();
            }
            catch (Exception)
            {
                _childError = true;
            }
        }
    }

    async Task SubmitCell()
    {
        if (IsGenerator() && !string.IsNullOrWhiteSpace(Block!.Name))
        {
            var answers = await TrySearchAndGenerateAnswers();
            await AddNewAnswers(answers);
        }
    }

    async Task<List<BlockViewModel>> TrySearchAndGenerateAnswers()
    {
        _childError = false;
        try
        {
            return await SearchAndGenerateAnswersTo(Block!);
        }
        catch (Exception)
        {
            _childError = true;
            throw;
        }
    }

    async Task<List<BlockViewModel>> SearchAndGenerateAnswersTo(BlockViewModel query)
    {
        if (Block!.ParentId is null)
            throw new ArgumentException("Cannot generate LLM result on root block!");

        _cellMsg = "Searching for related information...";
        var docs = await RagSearchAsync(Block!.Name);
        _cellMsg = $"Found {docs.ToList().Count()} documents. Generating answer...";
        var messages =  await InvokeLlmAsync(Block!.Name, docs);

        return messages.Select(ConvertLlmMessageToBlock).ToList();
    }

    private BlockViewModel ConvertLlmMessageToBlock(LlmMessage msg)
    {
        return new BlockViewModel()
        {
            Id = Guid.NewGuid(),
            Name = msg.Content,
            ParentId = Block!.Id,
            Properties = new BlockPropertiesViewModel()
            {
                CellType = CellType.Text,
                CreatedOn = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Order = 0
            }
        };
    }

    async Task AddNewAnswers(List<BlockViewModel> answers)
    {
        if (answers.Any())
        {
            foreach (var answer in answers)
            {
                Block!.ChildrenIds = Block!.ChildrenIds.Append(answer.Id);
                await BlocksApi.AddBlock(answer);
            }
            if (_shownChild != null)
            {
                _shownChild.Properties.Order = 0;
                await BlocksApi.UpdateBlock(_shownChild);
            }
            var lastChild = answers.Last();
            lastChild.Properties.Order = 1;
            await BlocksApi.UpdateBlock(lastChild);
            _shownChild = lastChild;
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

    void SwapShownChildAndUpdate(BlockViewModel? nextChild)
    {
        if(nextChild is not null) _shownChild = nextChild;
        StateHasChanged();
    }
}