using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POECraftHelper.Core;
using POECraftHelper.Services;

namespace POECraftHelper.ViewModels
{
  public class SettingsViewModel : ObservableObject
  {
    #region Services
    private readonly ISoundPlayerService m_soundPlayerService;
    #endregion

    public ObservableCollection<String> AvailableSounds { get; } = new ObservableCollection<String> ();

    private String m_selectedSound = "Divine Sound";
    public String SelectedSound
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

    public SettingsViewModel (ISoundPlayerService x_soundPlayerService)
    {
      m_soundPlayerService = x_soundPlayerService;

      // Hier könnten die verfügbaren Sounds dynamisch geladen werden, z.B. aus einem Verzeichnis oder einer Konfigurationsdatei.
      AvailableSounds.Add ("Divine Sound");
      AvailableSounds.Add ("Chaos Sound");
      AvailableSounds.Add ("Exalt Sound");

      SelectedSound = AvailableSounds.FirstOrDefault ();
    }

    private void OnSoundChanged (String x_selectedSound)
    {
      // Hier könnte die Logik implementiert werden, um den ausgewählten Sound zu speichern oder sofort abzuspielen.
      m_soundPlayerService.PlaySound (x_selectedSound);

    }
  }
}
