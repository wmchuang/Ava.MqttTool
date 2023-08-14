using System;
using System.Collections.Generic;
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
using SkiaSharp;

namespace Ava.MqttTool.Views;

public partial class ClientView : Window
{
    private IMqttClient _mqttClient;

    private List<string> AllTopics = new();

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
        _mqttClient.DisconnectedAsync += args =>
        {
            Log("客户端已断开连接");
            return Task.CompletedTask;
        };

        _mqttClient.ApplicationMessageReceivedAsync += args =>
        {
            var str = args.ApplicationMessage.ConvertPayloadToString();
            Log($"收到消息 {str}");
            Log($"  Qos: {args.ApplicationMessage.QualityOfServiceLevel.ToString()}");
            return Task.CompletedTask;
        };
    }

    private void Start_OnClick(object? sender, RoutedEventArgs e)
    {
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(Ip.Text,Convert.ToInt32(Port.Text))
            .WithoutPacketFragmentation()
            .WithClientId(ClientId.Text)
            .WithCredentials(UserName.Text,Password.Text)
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

                AllTopics.Add(topic);
                Dispatcher.UIThread.Invoke(() => {  this.AllTopic.Text = string.Join(',',AllTopics); });
               
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

    private void Clear_OnClick(object? sender, RoutedEventArgs e)
    {
        this.BoxMessage.Text = string.Empty;
    }

    private void UnSubscribe_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = TaskClient.Run(async () =>
        {
            if (_mqttClient.IsConnected)
            {
                var topic = string.Empty;
                Dispatcher.UIThread.Invoke(() => { topic = Topic.Text; });
                await _mqttClient.UnsubscribeAsync(topic);

                AllTopics.Remove(topic);
                Dispatcher.UIThread.Invoke(() => {  this.AllTopic.Text = string.Join(',',AllTopics); });
            }
        });
    }
}