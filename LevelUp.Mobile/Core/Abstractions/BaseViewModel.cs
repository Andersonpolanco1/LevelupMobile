using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using LevelUp.Mobile.Infrastructure.Api;

namespace LevelUp.Mobile.Core.Abstractions
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private Dictionary<string, string> fieldErrors = [];

        // ========================= 
        // SNACKBAR OPTIONS         
        // =========================

        private static readonly SnackbarOptions ErrorOptions = new()
        {
            BackgroundColor = Color.FromArgb("#EF4444"),
            TextColor = Color.FromArgb("#FFFFFF"),
            ActionButtonTextColor = Color.FromArgb("#FFFFFF"),
            CornerRadius = new CornerRadius(16),
            Font = Microsoft.Maui.Font.SystemFontOfSize(14)
        };

        private static readonly SnackbarOptions SuccessOptions = new()
        {
            BackgroundColor = Color.FromArgb("#22C55E"),
            TextColor = Color.FromArgb("#FFFFFF"),
            ActionButtonTextColor = Color.FromArgb("#FFFFFF"),
            CornerRadius = new CornerRadius(16),
            Font = Microsoft.Maui.Font.SystemFontOfSize(14)
        };

        private static readonly SnackbarOptions WarningOptions = new ()
        {
            BackgroundColor = Color.FromArgb("#F59E0B"),
            TextColor = Color.FromArgb("#FFFFFF"),
            ActionButtonTextColor = Color.FromArgb("#FFFFFF"),
            CornerRadius = new CornerRadius(16),
            Font = Microsoft.Maui.Font.SystemFontOfSize(14)
        };

        // ========================= 
        // SNACKBAR METHODS         
        // =========================

        protected static async Task ShowErrorAsync(string message) =>
            await Snackbar.Make(message, duration: TimeSpan.FromSeconds(4), visualOptions: ErrorOptions).Show();

        protected static async Task ShowSuccessAsync(string message) =>
            await Snackbar.Make(message, duration: TimeSpan.FromSeconds(3), visualOptions: SuccessOptions).Show();

        protected static async Task ShowWarningAsync(string message) =>
            await Snackbar.Make(message, duration: TimeSpan.FromSeconds(3), visualOptions: WarningOptions).Show();

        // ========================= 
        // FIELD ERRORS             
        // =========================

        protected bool HasFieldError(string field) => FieldErrors.ContainsKey(field);
        protected string? GetFieldError(string field) => FieldErrors.GetValueOrDefault(field);
        protected void ClearErrors() => FieldErrors = [];

        protected virtual void NotifyFieldErrorsChanged() { }

        // ========================= 
        // RUN ASYNC                
        // =========================

        protected async Task RunAsync(Func<Task> action)
        {
            ClearErrors();
            IsBusy = true;
            try
            {
                await action();
            }
            catch (ApiException e)
            {
                if (e.FieldErrors?.Count > 0)
                {
                    FieldErrors = e.FieldErrors.ToDictionary(
                        f => char.ToUpper(f.Field[0]) + f.Field[1..],
                        f => f.Message);
                    NotifyFieldErrorsChanged();
                }
                else
                {
                    await ShowErrorAsync(e.Message ?? "Error inesperado");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[STACK] {ex.StackTrace}");
                if (ex.InnerException is not null)
                    System.Diagnostics.Debug.WriteLine($"[RunAsync INNER] {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                await ShowErrorAsync("Ocurrió un error inesperado");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}