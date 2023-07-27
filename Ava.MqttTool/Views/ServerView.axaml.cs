using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Ava.MqttTool.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MQTTnet;
using MQTTnet.Server;

namespace Ava.MqttTool.Views;

public partial class ServerView : Window
{
    private MqttServer _mqttServer;

    private List<string> Clients = new();
    private List<string> Topics = new();

    public ServerView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void Start_OnClick(object? sender, RoutedEventArgs e)
    {
        var mqttFactory = new MqttFactory();

        var ip = IPAddress.Parse(Ip.Text);

        var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointBoundIPAddress(ip)
            .WithDefaultEndpointPort(Convert.ToInt32(Port.Text))
            .Build();
        _mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);
        _mqttServer.StartedAsync += args =>
        {
            Log("服务端启动");
            return Task.CompletedTask;
        };

        _mqttServer.StoppedAsync += args =>
        {
            Log("服务端关闭");
            return Task.CompletedTask;
        };

        _mqttServer.ClientConnectedAsync += args =>
        {
            Log($"客户端{args.ClientId}已连接");

            if (!Clients.Contains(args.ClientId))
            {
                Clients.Add(args.ClientId);
                Dispatcher.UIThread.Invoke(() => { ClientsTextBox.Text = String.Join(Environment.NewLine, Clients); });
            }

            return Task.CompletedTask;
        };
        _mqttServer.ClientDisconnectedAsync += args =>
        {
            Log($"客户端{args.ClientId}已断开");
            if (Clients.Contains(args.ClientId))
            {
                Clients.Remove(args.ClientId);
                Dispatcher.UIThread.Invoke(() => { ClientsTextBox.Text = String.Join(Environment.NewLine, Clients); });
            }

            return Task.CompletedTask;
        };
        _mqttServer.ClientSubscribedTopicAsync += args =>
        {
            if (!Topics.Contains(args.TopicFilter.Topic))
            {
                Topics.Add(args.TopicFilter.Topic);
                Dispatcher.UIThread.Invoke(() => { TopicTextBox.Text = String.Join(Environment.NewLine, Topics); });
            }
            return Task.CompletedTask;
        };
        
        _mqttServer.ClientUnsubscribedTopicAsync += args =>
        {
            if (!Topics.Contains(args.TopicFilter))
            {
                Topics.Add(args.TopicFilter);
                Dispatcher.UIThread.Invoke(() => { TopicTextBox.Text = String.Join(Environment.NewLine, Topics); });
            }
            return Task.CompletedTask;
        };

        // _mqttServer.ValidatingConnectionAsync += args =>
        // {
        //     if(args.UserName != UserName.Text && args.Password != Password.Text)
        //         args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
        //     return Task.CompletedTask;
        // };

        TaskClient.Run(async () => { await _mqttServer.StartAsync(); });
    }

    private void Stop_OnClick(object? sender, RoutedEventArgs e)
    {
        TaskClient.Run(async () => { await _mqttServer.StopAsync(); });
    }

    private void Log(string message)
    {
        var str = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} >>> {message} {Environment.NewLine}";
        Dispatcher.UIThread.Invoke(() => { this.BoxMessage.Text += str; });
    }

    private void NewClient_OnClick(object? sender, RoutedEventArgs e)
    {
        var client = new ClientView();
        client.Show();
    }
}