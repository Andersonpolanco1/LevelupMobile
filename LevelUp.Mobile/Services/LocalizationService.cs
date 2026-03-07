using LevelUp.Mobile.Core.Enums;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace LevelUp.Mobile.Services;

public class LocalizationService : INotifyPropertyChanged
{
    private static LocalizationService? _instance;
    public static LocalizationService Instance => _instance ??= new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private CultureInfo _currentCulture = CultureInfo.CurrentUICulture;

    private static readonly ResourceManager _resourceManager =
        new("LevelUp.Mobile.Resources.Strings.AppStrings",
            typeof(LocalizationService).Assembly);

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        private set
        {
            if (_currentCulture.Name == value.Name) return;
            _currentCulture = value;

            // Cambiar cultura del hilo principal
            CultureInfo.CurrentCulture = value;
            CultureInfo.CurrentUICulture = value;

            // Notificar todos los bindings con indexer
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }

    public string this[string key] =>
        _resourceManager.GetString(key, _currentCulture) ?? $"[{key}]";

    public void SetLanguage(Language language)
    {
        var culture = language switch
        {
            Language.Spanish => new CultureInfo("es"),
            Language.English => new CultureInfo("en"),
            _ => new CultureInfo("en")
        };
        CurrentCulture = culture;
    }

    public string GetAcceptLanguageHeader() =>
        _currentCulture.TwoLetterISOLanguageName;
}