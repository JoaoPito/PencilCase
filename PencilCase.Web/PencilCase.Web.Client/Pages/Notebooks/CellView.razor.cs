using Microsoft.AspNetCore.Components;
using MudBlazor;
using PencilCase.Shared.Models.LLM.Agents;
using PencilCase.Shared.Models.LLM.RAG;
using PencilCase.Shared.Models.Notebooks;
using PencilCase.Web.Client.Services.Blocks;
using PencilCase.Web.Client.ViewModels;

namespace PencilCase.Web.Client.Pages.Notebooks;

public partial class CellView : ComponentBase
{
    bool _isLoading = false;
    bool _childError = false;
    
    public Guid? TopicId { get; set; }
    [Parameter] public BlockViewModel? Block { get; set; }
    [Parameter] public IBlocksApi BlocksApi { get; set; } = null!;
    [Parameter] public Func<string, Task<IEnumerable<RagDocument>>> RagSearchAsync { get; set; } = null!;
    [Parameter] public Func<BlockViewModel, IEnumerable<RagDocument>, Task<IEnumerable<LlmMessage>>> InvokeLlmAsync { get; set; } = null!;
    [Parameter] public EventCallback OnNewCellShortcut { get; set; }

    IEnumerable<BlockViewModel> _loadedChildren = new List<BlockViewModel>();
    BlockViewModel? _shownChild;

    MudTextField<string> _inputTextField = null!;
    private string? _cellMsg;
    private List<RagDocument> _docsRetrieved = [];

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
                LoadShownChild();
            }
            catch (Exception)
            {
                _childError = true;
            }
        }
    }

    void LoadShownChild()
    {
        _shownChild = _loadedChildren
            .SingleOrDefault(
                c => c!.Id == Block!.Properties.CellShownAnswerId,
                _loadedChildren.LastOrDefault()
            );
        StateHasChanged();
    }
    
    async Task UpdateShownChildTo(BlockViewModel shownChild)
    {
        Block!.Properties.CellShownAnswerId = shownChild.Id;
        LoadShownChild();
    }

    async Task SubmitCell()
    {
        if (IsGenerator() && !string.IsNullOrWhiteSpace(Block!.Name))
        {
            var answers = await TrySearchAndGenerateAnswers();
            await AddAnswerList(answers);
            await UpdateChangesTo(Block!);
        }
    }

    async Task<List<BlockViewModel>> TrySearchAndGenerateAnswers()
    {
        _childError = false;
        try
        {
            return await SearchAndGenerateAnswersTo(Block!);
        }
        catch (Exception ex)
        {
            _childError = true;
            throw;
        }
    }

    async Task<List<BlockViewModel>> SearchAndGenerateAnswersTo(BlockViewModel query)
    {
        if (Block!.ParentId is null)
            throw new ArgumentException("Cannot generate LLM result on root block!");

        _isLoading = true;
        _cellMsg = "Searching for related information...";
        _docsRetrieved = new();
        StateHasChanged();
        var docs = await RagSearchAsync(Block!.Name);
        
        _cellMsg = $"Found {docs.ToList().Count()} related documents. Generating answer...";
        _docsRetrieved = docs.ToList();
        StateHasChanged();
        var messages = await InvokeLlmAsync(Block!, docs);
        ValidateLlmResponse(messages);
        
        _isLoading = false;
        return messages.Select(ConvertLlmMessageToBlock).ToList();
    }

    private void ValidateLlmResponse(IEnumerable<LlmMessage> messages)
    {
        if(!messages.Any())
            throw new ArgumentException("LLM did not generate any response.");
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

    async Task AddAnswerList(List<BlockViewModel> answers)
    {
        foreach (var answer in answers)
        {
            await AddAnswer(answer);
        }
    }

    async Task AddAnswer(BlockViewModel answer)
    {
        var addedBlock = await BlocksApi.AddBlock(answer);
        if(addedBlock is not null) 
            _loadedChildren = _loadedChildren.Append(addedBlock);
        if(addedBlock is not null) await UpdateShownChildTo(addedBlock);
        Block!.ChildrenIds = Block!.ChildrenIds.Append(addedBlock.Id);
    }
    
    bool IsGenerator()
    {
        return Block?.Properties.CellType is CellType.Question;
    }

    async Task UpdateChangesTo(BlockViewModel block)
    {
        await BlocksApi.UpdateBlock(block);
    }

    async Task SwapShownChildAndUpdate(BlockViewModel? nextChild)
    {
        if (nextChild is not null)
        {
            await UpdateShownChildTo(nextChild);
        }
        StateHasChanged();
        await UpdateChangesTo(Block!);
    }
}