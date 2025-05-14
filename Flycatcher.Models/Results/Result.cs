namespace Flycatcher.Models.Results
{
    public class Result
    {
        public Result() { }
        public Result(bool success, string? errorMessage = null)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
