using System;
using NLog;

namespace Ava.MqttTool.Utils;

public static class LoggerClient
{
    private static readonly ILogger Current;

    static LoggerClient()
    {
       
        var logger = LogManager.GetCurrentClassLogger();

        Current = logger;
    }
  
    public static void Error(Exception exception)
    {
        Current.Error(exception);
    }

    public static void Warn(string data)
    {
        Current.Warn(data);
    }

    public static void Info(string data)
    {
        Current.Info(data);
    }
}