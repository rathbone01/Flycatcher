namespace Flycatcher.Services
{

    public class MessageValidationService
    {
        public const int MaxMessageLength = 4000;
        public const int MinMessageLength = 1;

        public bool IsMessageValid(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            var length = content.Length;
            return length >= MinMessageLength && length <= MaxMessageLength;
        }

        public string? GetValidationError(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "Message cannot be empty.";

            if (content.Length > MaxMessageLength)
                return "Message cannot exceed 4000 characters.";

            return null;
        }

        public int GetCharacterCount(string? content)
        {
            return content?.Length ?? 0;
        }

        public double GetCharacterPercentage(string? content)
        {
            var count = GetCharacterCount(content);
            return (double)count / MaxMessageLength * 100.0;
        }
    }
}
