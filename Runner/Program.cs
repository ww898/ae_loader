using System;
using ww898.AELauncher;

namespace ww898.Runner
{
  internal static class Program
  {
    private static int Main(string[] args)
    {
      try
      {
        Console.WriteLine("AERunner v{0} Copyright © 2020 Mikhail Pilin", typeof(Program).Assembly.GetName().Version.ToString(3));
        if (args.Length < 1)
        {
          Console.WriteLine("Usage: <executable> [<arg0> [<arg1> [...]]]");
          return 256;
        }
        var arguments = new string[args.Length - 1];
        Array.Copy(args, 1, arguments, 0, arguments.Length);
        AEProcess.Start(args[0], arguments);
        return 0;
      }
      catch (Exception e)
      {
        Console.Error.WriteLine("ERROR: {0}", e);
        return 1;
      }
    }
  }
}