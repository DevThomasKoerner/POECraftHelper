using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using POECraftHelper.Models;

namespace POECraftHelper.Services
{

  public interface ISoundPlayerService
  {
    void PlaySound (SoundType x_soundType, Double x_volume);
  }

  public class SoundPlayerService : ISoundPlayerService
  {
    public void PlaySound (SoundType x_soundType, Double x_volume)
    {
      var assemblyDirectory = System.IO.Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);

      String filePath = x_soundType switch
      {
        SoundType.Divine => System.IO.Path.Combine (assemblyDirectory, "Sounds", "Divine.wav"),
        SoundType.Chaos  => System.IO.Path.Combine (assemblyDirectory, "Sounds", "Chaos.wav"),
        SoundType.Exalt  => System.IO.Path.Combine (assemblyDirectory, "Sounds", "Exalt.wav"),
        _ => throw new ArgumentException($"Sound type {x_soundType} is not supported.")
      };

      var audioFile = new AudioFileReader (filePath);
      audioFile.Volume = Math.Clamp ((float)x_volume / 100f, 0f, 1f);

      var output = new WaveOutEvent();
      output.Init (audioFile);
      output.Play ();

      output.PlaybackStopped += (s, e) =>
      {
        output.Dispose ();
        audioFile.Dispose ();
      };
    }
  }


}
