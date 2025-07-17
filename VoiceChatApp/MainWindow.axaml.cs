using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace VoiceChatApp;

public partial class MainWindow : Window
{
    private VoiceChatService? _service;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnStart(object? sender, RoutedEventArgs e)
    {
        try
        {
            string remoteIp = RemoteIpBox.Text ?? "";
            int remotePort = int.Parse(RemotePortBox.Text ?? "0");
            int localPort = int.Parse(LocalPortBox.Text ?? "0");
            _service = new VoiceChatService(remoteIp, remotePort, localPort);
            _service.Start();
            StatusText.Text = "Running";
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
        }
    }

    private void OnStop(object? sender, RoutedEventArgs e)
    {
        _service?.Dispose();
        _service = null;
        StatusText.Text = "Stopped";
        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;
    }
}
