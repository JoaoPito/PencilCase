using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using MudBlazor.Extensions;
using PencilCase.LLM.Agents.Providers.Gemini;
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
    
    [Parameter] public BlockViewModel? Block { get; set; }
    [Parameter] public IBlocksApi BlocksApi { get; set; } = null!;
    [Parameter] public EventCallback? OnNewCellShortcut { get; set; }
    [Inject] public ILlmApi LlmApi { get; set; }

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
        if (_shownChild is not null)
        {
            var nextChild = _loadedChildren
                .LastOrDefault(c => c.Properties.CreatedOn < _shownChild.Properties.CreatedOn);
            SwapShownChildAndUpdate(nextChild);
        }
    }

    void OnArrowRightClick()
    {
        if (_shownChild is not null)
        {
            var nextChild = _loadedChildren
                .FirstOrDefault(c => c.Properties.CreatedOn > _shownChild.Properties.CreatedOn);
            SwapShownChildAndUpdate(nextChild);
        }
            
    }

    void OnDeleteClick()
    {
        
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
                    .First();
            }
            catch (Exception)
            {
                _childError = true;
            }
        }
    }
    
    int _exampleCounter = 0;

    async Task GenerateOutputIfPossible()
    {
        if (IsGenerator() && !string.IsNullOrWhiteSpace(Block!.Name))
        {
            _isLoading = true;
            _childError = false;
            StateHasChanged();

            try
            {
                var genResult = await GenerateAnswersTo(Block!.Name);
                
                foreach (var block in genResult)
                {
                    Block!.ChildrenIds = Block!.ChildrenIds.Append(block.Id);
                    _loadedChildren = _loadedChildren.Append(block);
                
                    SwapShownChildAndUpdate(block);
                }
            }
            catch (Exception)
            {
                _childError = true;
                throw;
            }
            _isLoading = false;
        }
    }

    async Task<List<BlockViewModel>> GenerateAnswersTo(string query)
    {
        if (Block!.ParentId is null)
            throw new ArgumentException("Cannot generate LLM result on root block!");

        var parentBlock = await BlocksApi.GetBlock(Block!.ParentId ?? new Guid());
        
        if (parentBlock!.ParentId is null)
            throw new ArgumentException("Cannot generate LLM result on root block!");
        
        var parentIds = new List<Guid> { parentBlock!.ParentId ?? new Guid() };
        Console.WriteLine($"Sending query to RAG with query {query} and parentIds {parentIds.First()}");
        
        var ragResults = await LlmApi.QueryRagDocumentsAsync(query,parentIds);
        Console.WriteLine($"Got {ragResults.Count()} results from RAG. ");
        
        var llmResults = await LlmApi.InvokeLlmAgentAsync(
            BuildChatMessagesWithRag(Block!.Name, ragResults.ToList()));
        
        return llmResults.Select(r => new BlockViewModel()
        {
            Id = new Guid(),
            Name = r.Content,
            ParentId = Block!.Id,
            Type = BlockType.Cell,
            Properties = new BlockPropertiesViewModel()
            {
                CellType = CellType.Text,
                CreatedOn = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Order = _shownChild is null ? 0 : _shownChild.Properties.Order + 1
            }
        }).ToList();
    }

    List<LlmMessage> BuildChatMessagesWithRag(string prompt, List<RagDocument>? docs)
    {
        var chatMessages = new List<LlmMessage>();
        
        // Append messages for other blocks in notebook
        
        if (docs is not null)
        {
            string docsMessage = "";
        
            for(int i = 0; i < docs!.Count(); i++)
            {
                if(!string.IsNullOrWhiteSpace(docs[i].Content))
                {
                    docsMessage += $"# CHUNK {i+1}\n{docs[i].Content}\n";
                    Console.WriteLine($"docsMessage: {docsMessage}");
                }
            }
            if(!string.IsNullOrWhiteSpace(docsMessage))
                chatMessages = chatMessages.Append(new LlmMessage()
                {
                    Role = "user",
                    Content = docsMessage,
                }).ToList();
        }
        return chatMessages.Append(new LlmMessage()
        {
            Role = "user",
            Content = prompt,
        }).ToList();
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