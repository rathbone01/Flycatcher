namespace Flycatcher.Models.Results
{
    public class LoginResult : Result
    {
        public LoginResult(bool success, string? errorMessage = null) : base(success, errorMessage) {}

        public LoginResult(int UserId)
        {
            this.UserId = UserId;
            Success = true;
        }

        public int? UserId { get; set; }
    }
}
