using Avalonia.Controls;
using Avalonia.Threading;
using System;

namespace CellSupervisor;

public partial class MainWindow : Window
{
    private readonly ModbusService _modbusService = new();
    private DispatcherTimer? _timer;

    public MainWindow()
    {
        InitializeComponent();
        InitializeModbusConnection();
    }

    private async void InitializeModbusConnection()
    {
        bool connected = await _modbusService.ConnectAsync();
        if (connected)
        {
            // Configurar um temporizador para ler os dados a cada 1 segundo
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => ReadData();
            _timer.Start();
        }
    }

    private void ReadData()
    {
        var registers = _modbusService.ReadHoldingRegisters(0, 2);
        if (registers != null && registers.Length >= 2)
        {
            ushort cicloCount = registers[0];
            ushort statusCell = registers[1];

            Dispatcher.UIThread.Post(() =>
            {
                var statusText = this.FindControl<TextBlock>("StatusText");
                if (statusText != null)
                {
                    statusText.Text = $"Ligado | Ciclos: {cicloCount} | Estado: {statusCell}";
                }
            });
        }
    }

    public void OnStartButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
{
    _modbusService.WriteRegister(0, 1);
}
}