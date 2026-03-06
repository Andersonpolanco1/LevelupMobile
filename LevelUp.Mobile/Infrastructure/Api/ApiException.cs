namespace LevelUp.Mobile.Infrastructure.Api
{
    public class ApiException : Exception
    {
        public string? Code { get; }
        public List<FieldError>? FieldErrors { get; }

        public ApiException(string message, string? code = null, List<FieldError>? fieldErrors = null)
            : base(message)
        {
            Code = code;
            FieldErrors = fieldErrors;
        }
    }
}
