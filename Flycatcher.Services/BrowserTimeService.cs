using Microsoft.JSInterop;

namespace Flycatcher.Services
{
    public class BrowserTimeService
    {
        private readonly IJSRuntime _jsRuntime;

        public BrowserTimeService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string> FormatUtcToLocalAsync(DateTime utcDateTime)
        {
            try
            {
                var utcString = utcDateTime.ToString("o"); // ISO 8601
                return await _jsRuntime.InvokeAsync<string>("timeUtils.formatUtcToLocal", utcString);
            }
            catch
            {
                return utcDateTime.ToString("MMM d, yyyy h:mm tt") + " UTC";
            }
        }

        public async Task<string> FormatUtcToLocalShortAsync(DateTime utcDateTime)
        {
            try
            {
                var utcString = utcDateTime.ToString("o");
                return await _jsRuntime.InvokeAsync<string>("timeUtils.formatUtcToLocalShort", utcString);
            }
            catch
            {
                return utcDateTime.ToString("MMM d h:mm tt") + " UTC";
            }
        }

        public async Task<string> GetUserTimeZoneAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("timeUtils.getUserTimeZone");
            }
            catch
            {
                return "UTC";
            }
        }
    }
}
