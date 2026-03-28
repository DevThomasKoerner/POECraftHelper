using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using POECraftHelper.Core;
using POECraftHelper.Dialogs;
using POECraftHelper.Models;
using POECraftHelper.Services;

namespace POECraftHelper.ViewModels
{
  public class SettingsViewModel : DialogViewModelBase
  {
    #region Services

    private readonly ISoundPlayerService m_soundPlayerService;

    private readonly ILoggingService m_loggingService;

    private readonly ISettingsService m_settingsService;

    private readonly IDialogService m_dialogService;

    #endregion

    #region Binding Properties

    private Double m_windowHeight;
    public Double WindowHeight
    {
      get => m_windowHeight;
      set
      {
        m_windowHeight = value;
        OnPropertyChanged (nameof (WindowHeight));
      }
    }

    private Double m_windowWidth;
    public Double WindowWidth
    {
      get => m_windowWidth;
      set
      {
        m_windowWidth = value;
        OnPropertyChanged (nameof (WindowWidth));
      }
    }

    private Double m_windowLeft;
    public Double WindowLeft
    {
      get => m_windowLeft;
      set
      {
        m_windowLeft = value;
        OnPropertyChanged (nameof (WindowLeft));
      }
    }

    private Double m_windowTop;
    public Double WindowTop
    {
      get => m_windowTop;
      set
      {
        m_windowTop = value;
        OnPropertyChanged (nameof (WindowTop));
      }
    }

    private Boolean m_soundEnabled;
    public Boolean SoundEnabled
    {
      get => m_soundEnabled;
      set
      {
        if (m_soundEnabled != value)
        {
          m_soundEnabled = value;
          OnPropertyChanged (nameof (SoundEnabled));
          OnPropertyChanged (nameof (CanChangeSoundVolume));
          OnPropertyChanged (nameof (CanSelectSound));
        }
      }
    }

    private Int32 m_soundVolume;
    public Int32 SoundVolume
    {
      get => m_soundVolume;
      set
      {
        if (m_soundVolume != value)
        {
          m_soundVolume = value;
          OnPropertyChanged (nameof (SoundVolume));
        }
      }
    }

    private Boolean m_isEditingTitle;
    public Boolean IsEditingTitle
    {
      get => m_isEditingTitle;
      set
      {
        m_isEditingTitle = value;
        OnPropertyChanged (nameof (IsEditingTitle));
      }
    }


    public ObservableCollection<SoundType> AvailableSounds { get; } = new ObservableCollection<SoundType> (Enum.GetValues<SoundType> ());

    private SoundType m_selectedSound;

    public SoundType SelectedSound
    {
      get => m_selectedSound;
      set
      {
        if (m_selectedSound != value)
        {
          m_selectedSound = value;
          OnPropertyChanged (nameof (SelectedSound));
          OnSoundChanged (SoundVolume);
        }
      }
    }

    public ObservableCollection<RegexItem> RegexItems { get; } = new ObservableCollection<RegexItem> ();

    #endregion

    #region Can Properties

    public Boolean CanChangeSoundVolume => (SoundEnabled == true);

    public Boolean CanSelectSound => (SoundEnabled == true);

    #endregion

    #region Commands
    public ICommand SaveCommand { get; }
    public ICommand SliderChangedCommand { get; }
    public ICommand AddRegexCommand { get; }
    public ICommand RemoveRegexCommand { get; }
    public ICommand CopyRegexCommand { get; }
    #endregion


    public SettingsViewModel (ISoundPlayerService x_soundPlayerService, ILoggingService x_loggingService, ISettingsService x_settingsService, IDialogService x_dialogService)
    {
      m_soundPlayerService = x_soundPlayerService ?? throw new ArgumentNullException (nameof (x_soundPlayerService));
      m_loggingService = x_loggingService ?? throw new ArgumentNullException (nameof (x_loggingService));
      m_settingsService = x_settingsService ?? throw new ArgumentNullException (nameof (x_settingsService));
      m_dialogService = x_dialogService ?? throw new ArgumentNullException (nameof (x_dialogService));

      SaveCommand = new RelayCommand (OnSave);
      SliderChangedCommand = new RelayCommand<Double> (OnSoundChanged);
      AddRegexCommand = new RelayCommand (OnAddRegex);
      RemoveRegexCommand = new RelayCommand<RegexItem> (OnRemoveRegex);
      CopyRegexCommand = new RelayCommand<RegexItem> (OnCopyRegex);

      InitializeSettings ();
      InitializeWindow ();
    }

    private void InitializeSettings ()
    {
      var currentSettings = m_settingsService.LoadSettings<UserSettings> ();

      m_soundEnabled = currentSettings.SoundEnabled;
      m_soundVolume = currentSettings.SoundVolume;
      m_selectedSound = currentSettings.SoundType;

      foreach (var regexPattern in currentSettings.RegexPatterns)
      {
        var regexItem = new RegexItem (regexPattern.Key, regexPattern.Value);
        RegexItems.Add (regexItem);
      }

      // Falls keine Regex-Items vorhanden sind, dann ein Beispiel-Item hinzufügen, damit der Benutzer eine Vorlage hat.
      if (RegexItems.Any () == false)
      {
        RegexItems.Add (new RegexItem ("Body Armour", "\"^(Prime)\\sConquest Lamellar|^Conquest Lamellar\" \"Conquest Lamellar\\s|Conquest Lamellar$\""));
      }

      OnPropertyChanged (nameof (SoundEnabled));
      OnPropertyChanged (nameof (SoundVolume));
      OnPropertyChanged (nameof (SelectedSound));
    }

    private void InitializeWindow ()
    {
      var windowSettings = m_settingsService.LoadSettings<SettingsWindowSettings> ();
      m_windowHeight = windowSettings.SettingsWindowHeight;
      m_windowWidth = windowSettings.SettingsWindowWidth;
      m_windowLeft = windowSettings.SettingsWindowLeft;
      m_windowTop = windowSettings.SettingsWindowTop;

      OnPropertyChanged (nameof (WindowHeight));
      OnPropertyChanged (nameof (WindowWidth));
      OnPropertyChanged (nameof (WindowLeft));
      OnPropertyChanged (nameof (WindowTop));
    }

    private void OnSoundChanged (Double x_soundVolume)
    {
      m_soundPlayerService.PlaySound (SelectedSound, x_soundVolume);
    }

    private void OnAddRegex ()
    {
      var viewModel = m_dialogService.ShowDialog<RegexCreationViewModel, RegexCreationDialog> ();
      if (viewModel.DialogResult == false)
        return;

      if (String.IsNullOrWhiteSpace (viewModel.RegexPattern) == true ||
          String.IsNullOrWhiteSpace (viewModel.RegexPattern) == true)
        return;

      var newRegexItem = new RegexItem (viewModel.RegexTitle, viewModel.RegexPattern);
      if (RegexItems.Any (x => x.RegexTitle.Equals (newRegexItem.RegexTitle)) == false)
        RegexItems.Add (newRegexItem);
    }

    private void OnRemoveRegex (RegexItem x_itemToRemove)
    {
      if (x_itemToRemove == null)
        return;

      if (RegexItems.Contains (x_itemToRemove) == false)
        return;

      RegexItems.Remove (x_itemToRemove);
    }

    private async void OnCopyRegex (RegexItem x_itemToCopy)
    {
      if (x_itemToCopy == null)
        return;

      Clipboard.SetText (x_itemToCopy.RegexPattern);

      x_itemToCopy.IsCopiedToClipboard = true;

      // nach kurzer Zeit zurücksetzen
      await Task.Delay (1500);

      x_itemToCopy.IsCopiedToClipboard = false;
    }

    private void OnSave ()
    {
      try
      {
        var craftHelperSettings = new UserSettings ();
        craftHelperSettings.SoundEnabled = SoundEnabled;
        craftHelperSettings.SoundVolume = SoundVolume;
        craftHelperSettings.SoundType = SelectedSound;
        craftHelperSettings.RegexPatterns = RegexItems.ToDictionary (item => item.RegexTitle, item => item.RegexPattern);
        m_settingsService.SaveSettings (craftHelperSettings);

        var windowSettings = new SettingsWindowSettings ();
        windowSettings.SettingsWindowHeight = WindowHeight;
        windowSettings.SettingsWindowWidth = WindowWidth;
        windowSettings.SettingsWindowLeft = WindowLeft;
        windowSettings.SettingsWindowTop = WindowTop;
        m_settingsService.SaveSettings (windowSettings);
      }
      catch (Exception ex)
      {
        m_loggingService.Log ("Error saving settings: " + ex.Message);
        DialogResult = false;
        Close (false);
        return;
      }

      DialogResult = true;
      Close (true);
    }
  }
}
