using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media;

namespace NetworkPortScanner;

public class Potok(string ip, int port1, int port2, int count)
{
    public string Ip { get; } = ip;

    public int Port1 { get; } = port1;

    public int Port2 { get; } = port2;

    public int Count {get;} = count;

    public string FindedPorts { get; set; } = string.Empty;
    
    public int ProgressBarValue { get; set; } = 0;

    public int ProgressBarMaxValue { get; private set; } = Math.Abs(port2 - port1);

    public SolidColorBrush ColorBrush { get; set; } = new SolidColorBrush(GetRandomColor());

    private static Color GetRandomColor()
    {
        Random random = new Random();
        return Color.FromRgb(
            (byte)random.Next(256), 
            (byte)random.Next(256), 
            (byte)random.Next(256)
        );
    }
}