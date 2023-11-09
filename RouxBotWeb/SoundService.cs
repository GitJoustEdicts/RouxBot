using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace RouxBotWeb
{
    public class SoundService
    {
        private readonly IJSRuntime _jsRuntime;

        public SoundService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task PlaySoundAsync(string soundPath)
        {
            await _jsRuntime.InvokeVoidAsync("playSound", soundPath);
        }
    }
}