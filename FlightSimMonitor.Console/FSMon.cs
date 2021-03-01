using System;
using System.Threading;

namespace Handfield.FlightSimMonitor.Console
{
    class FSMon
    {
        private static FlightSimMonitor _fsMon;
        private static string _host = "127.0.0.1";
        private static uint _port = 500;
        private static bool _run;
        private static bool _isConnecting;

        private static string StatusLine
        {
            get
            {
                if (_isConnecting)
                    return $"CONNECTING to {_host}:{_port}";
                else
                    return (_fsMon.IsConnected) ? $"CONNECTED to {_fsMon.Hostname}:{_fsMon.Port}" : "DISCONNECTED";
            }
        }
        static void Main(string[] args)
        {
            // Initialize
            _run = true;
            _isConnecting = false;
            _fsMon = new FlightSimMonitor();
            _fsMon.Connected += _fsMon_Connected;
            _fsMon.Disconnected += _fsMon_Disconnected;

            // Drop into the menu loop
            DoMenu();

            // Main loop
            while (_run) { }

            // End
            System.Console.Write("Press any key to exit...");
            System.Console.ReadKey();
        }

        private static void DoMenu()
        {
            // Clear the screen
            System.Console.Clear();

            // Update the connection status line at the bottom of the window
            UpdateStatus();

            // Display the menu
            System.Console.Write("1) Connect\n2) Quit\n> ");

            // Collect user input
            ConsoleKeyInfo userSel = System.Console.ReadKey();
            System.Console.Write("\n");

            switch(userSel.KeyChar)
            {
                case '1':
                    // Prompt for and set a value in _host
                    GetHostname();

                    // Prompt for and set a value in _port
                    GetPort();

                    // Tell the user what's happening
                    System.Console.WriteLine($"Connecting to host {_host} on port {_port}...");
                    _isConnecting = true;
                    UpdateStatus();

                    // Connect to SimConnect
                    try
                    {
                        _fsMon.Connect("FlightSimMonitor Console", _host, _port);
                    }
                    catch (Exception)
                    {
                        System.Console.WriteLine($"Failed to connect to host {_host} on port {_port}!");
                        _isConnecting = false;
                        UpdateStatus();
                        Thread.Sleep(4000);
                        DoMenu();
                    }
                    break;
                case '2':
                    Environment.Exit(0);
                    break;
                default:
                    DoMenu();
                    break;
            }
        }

        private static void GetPort()
        {
            System.Console.Write($"Port [{_port}]: ");
            string sPort = System.Console.ReadLine();

            // If we got input, check that it can be parsed as a uint; if no input, we keep the original value
            if (!String.IsNullOrWhiteSpace(sPort) && uint.TryParse(sPort, out uint port))
            {
                // Check that the port value is valid
                if (port < 1 || port > 65535)
                {
                    // Display an error message and re-prompt
                    System.Console.WriteLine("Port values must be between 1 and 65535");
                    GetPort();
                }
                else
                {
                    // User provided input, it's a number, and it's in the right range - reset _port
                    _port = port;
                }
            }
        }
        private static void GetHostname()
        {
            System.Console.Write($"Host [{_host}]: ");
            string hostName = System.Console.ReadLine();

            // If the user entered something, reset _host; otherwise, do nothing.
            if (!String.IsNullOrWhiteSpace(hostName))
                _host = hostName;
        }

        private static void _fsMon_Disconnected(object sender, FlightSimMonitor.DisconnectedEventArgs e)
        {
            UpdateStatus();
            System.Console.WriteLine($"SimConnect disconnected at {e.DisconnectedTime.ToString()}! Last connected at {e.LastConnectedTime.ToString()}");
        }

        private static void _fsMon_Connected(object sender, FlightSimMonitor.ConnectedEventArgs e)
        {
            // We're no longer connecting, we're connected
            _isConnecting = false;
            UpdateStatus();
            System.Console.WriteLine($"SimConnect connected at {e.ConnectedTime.ToString()}! Last disconnected at {e.LastDisconnectedTime.ToString()}");
        }

        private static void UpdateStatus()
        {
            // Get the current cursor position so we can move back when we're done
            int curLeft = System.Console.CursorLeft;
            int curTop = System.Console.CursorTop;

            // Move to the bottom line and update
            System.Console.SetCursorPosition(0, System.Console.WindowHeight - 1);
            System.Console.Write(new string(' ', System.Console.WindowWidth - 1));
            System.Console.SetCursorPosition(0, System.Console.WindowHeight - 1);
            System.Console.Write($"Status: {StatusLine}");

            // Move cursor back to its previous position
            System.Console.SetCursorPosition(curLeft, curTop);
        }
    }
}
