using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POECraftHelper.Services
{
  public interface ILoggingService
  {
    void Log (String message);
  }

  public class LoggingService : ILoggingService
  {
    public void Log (String message)
    {
      Console.WriteLine (message);
    }
  }
}
