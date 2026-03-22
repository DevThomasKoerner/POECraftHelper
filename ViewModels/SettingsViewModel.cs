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

    #endregion

    #region Binding Properties

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

    private SoundType m_selectedSound = SoundType.Divine;
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

    #endregion


    public SettingsViewModel (ISoundPlayerService x_soundPlayerService, ILoggingService x_loggingService, ISettingsService x_settingsService)
    {
      m_soundPlayerService = x_soundPlayerService;
      m_loggingService = x_loggingService;
      m_settingsService = x_settingsService;

      AddRegexCommand = new RelayCommand<RegexItem> (OnAddRegex);
      RemoveRegexCommand = new RelayCommand<RegexItem> (OnRemoveRegex);

      Initialize ();
    }

    private void Initialize ()
    {
      var craftHelperSettings = m_settingsService.LoadSettings ();

      SoundEnabled = craftHelperSettings.SoundEnabled;
      SoundVolume = craftHelperSettings.SoundVolume;
      SelectedSound = craftHelperSettings.SoundType;

      foreach (var regexPattern in craftHelperSettings.RegexPatterns)
      {
        var regexItem = new RegexItem (regexPattern.Key, regexPattern.Value);
        RegexItems.Add (regexItem);
      }
    }

    private void OnSoundChanged (SoundType x_selectedSoundType)
    {
      // Hier könnte die Logik implementiert werden, um den ausgewählten Sound zu speichern oder sofort abzuspielen.
      m_soundPlayerService.PlaySound (x_selectedSoundType);
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
