using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using OpenSilver.WebAssembly;
using System.Threading.Tasks;

namespace ScratchApplication.Browser.Pages;

[Route("/")]
public class Index : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder __builder)
    {
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await Runner.RunApplicationAsync<ScratchContent.App>();
    }
}
