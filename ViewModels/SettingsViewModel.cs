using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using POECraftHelper.Core;
using POECraftHelper.Models;
using POECraftHelper.Services;

namespace POECraftHelper.ViewModels
{
  public class SettingsViewModel : ObservableObject
  {
    #region Services

    private readonly ISoundPlayerService m_soundPlayerService;

    private readonly ILoggingService m_loggingService;

    private readonly ISettingsService m_settingsService;

    private readonly IWindowService m_windowService;

    #endregion

    #region Private Fields



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
          OnSoundChanged (m_selectedSound);
        }
      }
    }

    public ObservableCollection<RegexItem> RegexItems { get; } = new ObservableCollection<RegexItem> ();

    #endregion

    #region Commands

    public ICommand AddRegexCommand { get; }
    public ICommand RemoveRegexCommand { get; }

    public ICommand SaveCommand { get; }

    #endregion


    public SettingsViewModel (ISoundPlayerService x_soundPlayerService, ILoggingService x_loggingService, ISettingsService x_settingsService, IWindowService x_windowService)
    {
      m_soundPlayerService = x_soundPlayerService;
      m_loggingService = x_loggingService;
      m_settingsService = x_settingsService;
      m_windowService = x_windowService;

      AddRegexCommand = new RelayCommand<RegexItem> (OnAddRegex);
      RemoveRegexCommand = new RelayCommand<RegexItem> (OnRemoveRegex);
      SaveCommand = new RelayCommand (OnSave);

      Initialize ();
    }

    private void Initialize ()
    {
      var currentSettings = m_settingsService.LoadSettings<CraftHelperSettings> ();

      m_soundEnabled = currentSettings.SoundEnabled;
      m_soundVolume = currentSettings.SoundVolume;
      m_selectedSound = currentSettings.SoundType;

      foreach (var regexPattern in currentSettings.RegexPatterns)
      {
        var regexItem = new RegexItem (regexPattern.Key, regexPattern.Value);
        RegexItems.Add (regexItem);
      }

      OnPropertyChanged (nameof (SoundEnabled));
      OnPropertyChanged (nameof (SoundVolume));
      OnPropertyChanged (nameof (SelectedSound));
    }

    private void OnSoundChanged (SoundType x_selectedSoundType)
    {
      // Hier könnte die Logik implementiert werden, um den ausgewählten Sound zu speichern oder sofort abzuspielen.
      m_soundPlayerService.PlaySound (x_selectedSoundType);
    }

    private void OnSave ()
    {
      var craftHelperSettings = new CraftHelperSettings ();
      craftHelperSettings.SoundEnabled = SoundEnabled;
      craftHelperSettings.SoundVolume = SoundVolume;
      craftHelperSettings.SoundType = SelectedSound;
      craftHelperSettings.RegexPatterns = RegexItems.ToDictionary (item => item.RegexName, item => item.RegexPattern);
      m_settingsService.SaveSettings (craftHelperSettings);

      var windowSettings = new WindowSettings ();
      windowSettings.SettingsWindowHeight = WindowHeight;
      windowSettings.SettingsWindowWidth = WindowWidth;
      m_settingsService.SaveSettings (windowSettings);

      m_windowService.CloseSettings ();
    }

    private void OnAddRegex (RegexItem x_regexItem)
    {
      if (x_regexItem == null || String.IsNullOrWhiteSpace (x_regexItem.RegexName) || String.IsNullOrWhiteSpace (x_regexItem.RegexPattern))
        return;

      RegexItems.Add (x_regexItem);
    }

    private void OnRemoveRegex (RegexItem x_regexItem)
    {
      if (x_regexItem == null)
        return;

      RegexItems.Remove (x_regexItem);
    }

  }
}
