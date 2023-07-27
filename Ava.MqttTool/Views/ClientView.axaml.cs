using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ava.MqttTool.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MQTTnet;
using MQTTnet.Client;

namespace Ava.MqttTool.Views;

public partial class ClientView : Window
{
    private IMqttClient _mqttClient;

    public ClientView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        var mqttFactory = new MqttFactory();

        _mqttClient = mqttFactory.CreateMqttClient();
        _mqttClient.ConnectedAsync += args =>
        {
            Log("客户端已成功连接");
            return Task.CompletedTask;
        };

        _mqttClient.ApplicationMessageReceivedAsync += args =>
        {
            var str = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment.ToArray());
            Log($"收到消息 {str}");
            return Task.CompletedTask;
        };
    }

    private void Start_OnClick(object? sender, RoutedEventArgs e)
    {
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(Ip.Text)
            .WithoutPacketFragmentation()
            .WithClientId(ClientId.Text)
            .Build();

        TaskClient.Run(async () =>
        {
            if (!_mqttClient.IsConnected)
            {
                await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            }
        });
    }

    private void Stop_OnClick(object? sender, RoutedEventArgs e)
    {
        TaskClient.Run(async () =>
        {
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
            }
        });
    }

    private void Subscribe_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = TaskClient.Run(async () =>
        {
            if (_mqttClient.IsConnected)
            {
                var topic = string.Empty;
                Dispatcher.UIThread.Invoke(() => { topic = Topic.Text; });
                await _mqttClient.SubscribeAsync(topic);
            }
        });
    }

    private void Log(string message)
    {
        var str = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} >>> {message} {Environment.NewLine}";
        Dispatcher.UIThread.Invoke(() => { this.BoxMessage.Text += str; });
    }

    private void Publish_OnClick(object? sender, RoutedEventArgs e)
    {
        var topic = string.Empty;
        var message = string.Empty;
        Dispatcher.UIThread.Invoke(() =>
        {
            topic = PTopic.Text;
            message = PMessage.Text;
        });
        if (_mqttClient.IsConnected)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .Build();

            TaskClient.Run(async () => { await _mqttClient.PublishAsync(applicationMessage); });
        }
    }
}