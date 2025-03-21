namespace Flycatcher.Models.Results
{
    public class Result(bool success, string? errorMessage = null)
    {
        public bool Success { get; set; } = success;
        public string? ErrorMessage { get; set; } = errorMessage;
    }
}
