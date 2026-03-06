using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelUp.Mobile.Infrastructure.Api;

namespace LevelUp.Mobile.Core.Abstractions
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string? errorMessage = null;
        [ObservableProperty] private Dictionary<string, string> fieldErrors = [];

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        partial void OnErrorMessageChanged(string? value) => OnPropertyChanged(nameof(HasError));

        [RelayCommand]
        private void ClearError()
        {
            ErrorMessage = null;
            FieldErrors = [];
        }

        protected bool HasFieldError(string field) => FieldErrors.ContainsKey(field);
        protected string? GetFieldError(string field) => FieldErrors.GetValueOrDefault(field);

        protected void ClearErrors()
        {
            ErrorMessage = null;
            FieldErrors = [];
        }

        protected async Task RunAsync(Func<Task> action)
        {
            ClearErrors();
            IsBusy = true;
            try
            {
                await action();
            }
            catch (ApiException ex)
            {
                if (ex.FieldErrors?.Count > 0)
                {
                    // Los field errors de la API vienen en minúscula, normalizamos a PascalCase
                    FieldErrors = ex.FieldErrors.ToDictionary(
                        f => char.ToUpper(f.Field[0]) + f.Field[1..],
                        f => f.Message);
                    NotifyFieldErrorsChanged();
                }
                else
                {
                    ErrorMessage = ex.Message;
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Ocurrió un error inesperado";
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Cada ViewModel sobreescribe esto para notificar sus propiedades de error
        protected virtual void NotifyFieldErrorsChanged() { }
    }
}