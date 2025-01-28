using Microsoft.AspNetCore.Components;
using PencilCase.Web.Pages.Notebooks.Models;
using PencilCase.Web.Services.Notebooks;

namespace PencilCase.Web.Pages.Notebooks.Sources;

public partial class SourceView : ComponentBase
{
    [CascadingParameter(Name = "block")] protected BlockViewModel? Block { get; set; }
    [Parameter] public IBlocksApi BlocksApi { get; set; } = null!;
    private List<BlockViewModel> _children = [];
    private bool _loading = true;

    protected override async Task OnParametersSetAsync()
    {
        await LoadChildren();
    }

    private async Task LoadChildren()
    {
        _loading = true;
        StateHasChanged();
        
        if (Block != null) _children = await BlocksApi.GetChildren(Block.Id);
        _children = _children.OrderBy(c => c.Properties.Order).ToList();
        
        _loading = false;
        StateHasChanged();
    }
}