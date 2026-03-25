using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace NetworkPortScanner;

public partial class MainWindow : Window
{
    private ObservableCollection<Potok> _potoks = [];

    public MainWindow()
    {
        InitializeComponent();
        _potoks.Add(new Potok("192.168.2.18", 2980, 6000, 10));
        _potoks.Add(new Potok("127.0.0.1", 1, 1024, 5));
        PotokiListBox.ItemsSource = _potoks;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        _potoks.Add(new Potok(IpBox.Text, int.Parse(Port1Box.Text), int.Parse(Port2Box.Text),
            int.Parse(PotokiCountBox.Text)));
    }

    private async void StartButton_OnClick(object? sender, RoutedEventArgs e)
    {
        IsEnabled = false;
        await RunAsync();
        IsEnabled = true;
    }

    private async Task RunAsync()
    {
        List<Task> tasks = [];

        foreach (Potok potok in _potoks)
        {
            for (int port = potok.Port1; port < potok.Port2 + 1; port += potok.Count)
            {
                List<Task> limited = [];
                for (int j = 0; j < potok.Count; j++)
                {
                    if (port + j > potok.Port2)
                        break;

                    limited.Add(TryConnect(potok.Ip, port + j, potok));
                }
                tasks.Add(Task.WhenAll(limited));
            }
        }

        try
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                TextBlock.Text = i.ToString();
                await tasks[i];
            }
        }
        catch (Exception e)
        {
            await MessageBoxManager.GetMessageBoxStandard("Ошибка", e.Message, ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Error, null, WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
        }
    }

    private async Task TryConnect(string host, int port, Potok potok)
    {
        TcpClient client = new TcpClient();

        var connectTask = client.ConnectAsync(host, port);
        var timeoutTask = Task.Delay(1000);

        var completedTask = await Task.WhenAny(connectTask, timeoutTask);

        bool connected = completedTask == connectTask && connectTask.IsCompletedSuccessfully;
        
        potok.ProgressBarValue += 1;
        
        if (connected)
        {
            potok.FindedPorts += port + " ";
        }
        else
        {
            Console.WriteLine(port + " is not connected");
        }
        
        PotokiListBox.ItemsSource = new ObservableCollection<Potok>(_potoks);
    }
}