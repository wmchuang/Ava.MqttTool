using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Threading;

namespace Ava.MqttTool;

class Program
{
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        //这行代码创建了一个互斥体对象 _mutex。
        //第一个参数为 true，表示如果当前线程成功创建了互斥体，则它将拥有该互斥体的初始所有权。
        //第二个参数为 nameof(Ava.MqttTool)，表示互斥体的名称。
        //第三个参数 out bool ret 是一个输出参数，用于指示互斥体是否被当前线程创建成功。如果成功创建，ret 的值将为 true，否则为 false。
        _ = new Mutex(true, nameof(MqttTool), out bool ret);
        if (!ret)
        {
            Environment.Exit(0);
        }

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}