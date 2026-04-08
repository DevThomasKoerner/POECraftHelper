using System;
using System.IO;

namespace POECraftHelper.Services
{
  public interface ILoggingService
  {
    void Log (string message);
  }

  public class LoggingService : ILoggingService, IDisposable
  {
    private readonly string m_logDir;
    private readonly object m_lock = new object();
    private StreamWriter m_writer;
    private DateTime m_currentLogDate;

    public LoggingService () : this ("POECraftHelper") { }

    /// <summary>
    /// Erstellt einen LoggingService, der Logs täglich rotiert:
    /// LocalApplicationData\<appName>\logs\app_yyyy-MM-dd.log
    /// </summary>
    public LoggingService (string x_appName)
    {
      if (string.IsNullOrWhiteSpace (x_appName))
        throw new ArgumentException ("AppName darf nicht leer sein", nameof (x_appName));

      m_logDir = Path.Combine (
        Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData),
        x_appName,
        "logs");

      Directory.CreateDirectory (m_logDir);
      OpenWriterForDate (DateTime.Today);
    }

    /// <summary>
    /// Schreibt eine Nachricht in die tagesaktuelle Logdatei mit Zeitstempel.
    /// Bei Datumswechsel wird automatisch eine neue Datei geöffnet.
    /// </summary>
    public void Log (string message)
    {
      lock (m_lock)
      {
        if (m_writer == null)
          throw new ObjectDisposedException (nameof (LoggingService));

        // Datumswechsel → neue Datei öffnen
        if (DateTime.Today != m_currentLogDate)
          RotateLog ();

        m_writer.WriteLine ($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}");
      }
    }

    public void Dispose ()
    {
      lock (m_lock)
      {
        m_writer?.Dispose ();
        m_writer = null;
      }
    }

    // -------------------------------------------------------------------------

    private void OpenWriterForDate (DateTime date)
    {
      m_currentLogDate = date;
      string logFilePath = Path.Combine(m_logDir, $"app_{date:yyyy-MM-dd}.log");
      m_writer = new StreamWriter (logFilePath, append: true) { AutoFlush = true };
    }

    private void RotateLog ()
    {
      m_writer?.Dispose ();
      OpenWriterForDate (DateTime.Today);
    }
  }
}