using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Modbus.Device;

public class ModbusService
{
    private TcpClient? _tcpClient;
    private ModbusIpMaster? _modbusMaster;
    private readonly string _ipAddress = "127.0.0.1";
    private readonly int _port = 5020;

    public bool IsConnected => _tcpClient != null && _tcpClient.Connected;

    public async Task<bool> ConnectAsync()
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(_ipAddress, _port);
            _modbusMaster = ModbusIpMaster.CreateIp(_tcpClient);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao conectar no servidor Modbus: {ex.Message}");
            return false;
        }
    }

    public ushort[]? ReadHoldingRegisters(ushort startAddress, ushort numberOfPoints)
    {
        try
        {
            if (_modbusMaster == null || !IsConnected) return null;
            return _modbusMaster.ReadHoldingRegisters(1, startAddress, numberOfPoints);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro na leitura Modbus: {ex.Message}");
            return null;
        }
    }

    public void WriteRegister(ushort address, ushort value)
    {
        try
        {
            _modbusMaster?.WriteSingleRegister(1, address, value);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro na escrita Modbus: {ex.Message}");
        }
    }

    public void Disconnect()
    {
        _tcpClient?.Close();
        _tcpClient = null;
        _modbusMaster = null;
    }
}