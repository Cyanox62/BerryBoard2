using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Timers;

namespace BerryBoard2.Model.Libs
{
	internal class Serial
	{
		public delegate void DataReceivedDelegate(string msg);
		public event DataReceivedDelegate? DataReceivedEvent;

		private List<SerialPort> serialPorts = new List<SerialPort>();
		private Timer? portCheckTimer;

		private static SerialPort? senderPort = null;

		public static string GetPortName() => senderPort?.PortName;

		public void SetupPorts()
		{
			ConnectToAllSerialPorts();

			portCheckTimer = new Timer(5000);
			portCheckTimer.Elapsed += new ElapsedEventHandler(PortCheckTimer_Elapsed);
			portCheckTimer.AutoReset = true;
			portCheckTimer.Start();
		}

		private void PortCheckTimer_Elapsed(object? sender, ElapsedEventArgs e)
		{
			ConnectToAllSerialPorts();
		}

		private void ConnectToAllSerialPorts()
		{
			string[]? ports;
			if (senderPort != null) ports = new string[] { senderPort.PortName };
			else ports = SerialPort.GetPortNames();

			foreach (string port in ports)
			{
				if (serialPorts.Any(sp => sp.PortName == port && sp.IsOpen))
				{
					continue;
				}

				var serialPort = new SerialPort(port, 9600);
				serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
				serialPort.Parity = Parity.None;
				serialPort.StopBits = StopBits.One;
				serialPort.DtrEnable = true;
				serialPort.RtsEnable = true;

				serialPorts.Add(serialPort);

				try
				{
					serialPort.Open();
					Debug.WriteLine($"Opened port {port}");
					//serialPort.WriteLine("IDENTIFY");
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Could not open serial port: " + ex.Message);
					continue;
				}
			}
		}

		private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
		{
			SerialPort sp = (SerialPort)sender;
			string indata = sp.ReadExisting().Trim();
			if (indata == string.Empty) return;
			Debug.WriteLine($"Data received ({sp.PortName}): " + indata);

			for (int i = serialPorts.Count - 1; i >= 0; i--)
			{
				SerialPort port = serialPorts[i];
				if (port.PortName == sp.PortName)
				{
					senderPort = port;
					continue;
				}

				KillPort(port);
				serialPorts.Remove(port);
			}

			if (senderPort != null && serialPorts.Count > 1)
			{
				serialPorts.Remove(senderPort);
				serialPorts.Clear();
				serialPorts.Add(senderPort);
			}

			DataReceivedEvent?.Invoke(indata);
		}

		public void KillAllPorts()
		{
			portCheckTimer?.Stop();

			foreach (var port in serialPorts)
			{
				KillPort(port);
			}

			serialPorts.Clear();
		}

		private void KillPort(SerialPort port)
		{
			if (port.IsOpen)
			{
				port.Close();
				port.DataReceived -= DataReceivedHandler;
				port.Dispose();
				Debug.WriteLine("Killed port: " + port.PortName);
			}
		}
	}
}
