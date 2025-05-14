using Flycatcher.Classes;
using Microsoft.JSInterop;

namespace Flycatcher.Services
{
    public class BrowserService
    {
        private readonly IJSRuntime _js;

        public BrowserService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<BrowserDimension> GetDimensionsAsync()
        {
            return await _js.InvokeAsync<BrowserDimension>("getDimensions");
        }
    }
}