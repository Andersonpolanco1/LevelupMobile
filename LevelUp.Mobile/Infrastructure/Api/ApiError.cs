namespace LevelUp.Mobile.Infrastructure.Api
{
    public class ApiError
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
        public List<FieldError>? FieldErrors { get; set; }

    }

    public class FieldError
    {
        public string Field { get; set; } = default!;
        public string Message { get; set; } = default!;
    }
}
