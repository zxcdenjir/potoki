using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace NetworkPortScanner;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<Potok> _potoks = [];

    public MainWindow()
    {
        InitializeComponent();
        _potoks.Add(new Potok("192.168.2.18", 1, 10000, 100));
        _potoks.Add(new Potok("127.0.0.1", 1, 1024, 50));
        PotokiListBox.ItemsSource = _potoks;
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            _potoks.Add(new Potok(IpBox.Text!, int.Parse(Port1Box.Text!), int.Parse(Port2Box.Text!),
                int.Parse(PotokiCountBox.Text!)));
            PotokiListBox.ItemsSource = _potoks;
        }
        catch
        {
            await MessageBoxManager.GetMessageBoxStandard("Ошибка", "Неверно заполнены данные!", ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Error, null, WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
        }
        
    }

    private async void StartButton_OnClick(object? sender, RoutedEventArgs e)
    {
        IsEnabled = false;
        foreach (var potok in _potoks)
        {
            potok.Clear();
        }
        await RunAsync();
        IsEnabled = true;
    }

    private async Task RunAsync()
    {
        List<Task> tasks = [];
        
        tasks.AddRange(_potoks.Select(MicroMetod));

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            await MessageBoxManager.GetMessageBoxStandard("Ошибка", e.Message, ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Error, null, WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
        }
    }

    private async Task MicroMetod(Potok potok)
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
            await Task.WhenAll(limited);
        }
    }

    private async Task TryConnect(string host, int port, Potok potok)
    {
        TcpClient client = new TcpClient();

        var connectTask = client.ConnectAsync(host, port);
        var timeoutTask = Task.Delay(500);

        var completedTask = await Task.WhenAny(connectTask, timeoutTask);

        bool connected = completedTask == connectTask && connectTask.IsCompletedSuccessfully;
        
        potok.ProgressBarValue += 1;
        
        if (connected)
        {
            potok.FindedPorts += port + " ";
        }
        else
        {
            Console.WriteLine($"{host}:{port} is not connected");
        }
        
        PotokiListBox.ItemsSource = new ObservableCollection<Potok>(_potoks);
    }
}