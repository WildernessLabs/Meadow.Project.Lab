﻿using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Units;
using System;

namespace Meadow.Devices;

internal class ConnectorProviderV3e : IConnectorProvider
{
    private readonly Sc16is752 _uartExpander;

    public ConnectorProviderV3e(ProjectLabHardwareBase projLab)
    {
        _uartExpander = new Sc16is752(projLab.I2cBus, new Frequency(1.8432, Frequency.UnitType.Megahertz), Sc16is7x2.Addresses.Address_0x4D);
    }

    public ModbusRtuClient GetModbusRtuClient(ProjectLabHardwareBase projLab, int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        if (Resolver.Device is not F7CoreComputeV2) throw new NotSupportedException();
    
        try
        {
            // v3.e+ uses an SC16is I2C UART expander for the RS485
            var port = _uartExpander.PortB.CreateRs485SerialPort(baudRate, dataBits, parity, stopBits, false);
            Resolver.Log.Trace($"485 port created");
            return new ModbusRtuClient(port);
        }
        catch (Exception ex)
        {
            Resolver.Log.Warn($"Error creating 485 port: {ex.Message}");
            throw new Exception("Unable to connect to UART expander");
        }
    }

    public MikroBusConnector CreateMikroBus1(IF7CoreComputeMeadowDevice device, Mcp23008 mcp2)
    {
        // todo: verify 3.e and later
        return new MikroBusConnector(
            "MikroBus1",
            new PinMapping
            {
                new PinMapping.PinAlias(MikroBusConnector.PinNames.AN, device.Pins.PA3),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.RST, device.Pins.PH10),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CS, device.Pins.PB12),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCK, device.Pins.SPI5_SCK),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CIPO, device.Pins.SPI5_CIPO),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.COPI, device.Pins.SPI5_COPI),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.PWM, device.Pins.PB8),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.INT, device.Pins.PC2),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.RX, device.Pins.PB15),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.TX, device.Pins.PB14),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCL, device.Pins.I2C3_SCL),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SDA, device.Pins.I2C3_SDA),
            },
            device.PlatformOS.GetSerialPortName("com1")!,
            new I2cBusMapping(device, 3),
            new SpiBusMapping(device, device.Pins.SPI5_SCK, device.Pins.SPI5_COPI, device.Pins.SPI5_CIPO)
            );
    }

    public MikroBusConnector CreateMikroBus2(IF7CoreComputeMeadowDevice device, Mcp23008 mcp2)
    {
        return new MikroBusConnector(
            "MikroBus2",
            new PinMapping
            {
                new PinMapping.PinAlias(MikroBusConnector.PinNames.AN, device.Pins.PB0),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.RST, mcp2.Pins.GP1),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CS, mcp2.Pins.GP2),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCK, device.Pins.SCK),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.CIPO, device.Pins.CIPO),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.COPI, device.Pins.COPI),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.PWM, device.Pins.PB9),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.INT, mcp2.Pins.GP3),
//                new PinMapping.PinAlias(MikroBusConnector.PinNames.RX, uart1rx), // on the I2C uart and not usable for anything else
//                new PinMapping.PinAlias(MikroBusConnector.PinNames.TX, uart1tx), // on the I2C uart and not usable for anything else
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SCL, device.Pins.I2C1_SCL),
                new PinMapping.PinAlias(MikroBusConnector.PinNames.SDA, device.Pins.I2C1_SDA),
            },
            _uartExpander.PortA,
            new I2cBusMapping(device, 1),
            new SpiBusMapping(device, device.Pins.SCK, device.Pins.COPI, device.Pins.CIPO)
            );
    }
}
