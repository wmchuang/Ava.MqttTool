using System;
using System.Threading.Tasks;

namespace Ava.MqttTool.Utils;

public static class TaskClient
{
    /// <summary>
    /// 线程中开启一个后台线程
    /// </summary>
    /// <param name="function"></param>
    /// <param name="catchDo"></param>
    /// <returns></returns>
    public static Task Run(Func<Task> function, Action catchDo = null)
    {
        return Task.Run(() =>
        {
            var task = function();
            task.Execute(catchDo);
        });
    }
    
    public static Task Run(Action action)
    {
        var task = Task.Run(action);
        task.Execute();
        return task;
    }

    private static void Execute(this Task task, Action catchDo = null)
    {
        task.ContinueWith(t =>
        {
            t?.Exception?.Handle(ex =>
            {
                catchDo?.Invoke();
                return true;
            });
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}