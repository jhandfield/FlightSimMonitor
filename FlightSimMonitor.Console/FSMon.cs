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
        private static int _lastUpdatePosition;

        private static string StatusLine
        {
            get
            {
                string output = String.Empty; ;

                if (_isConnecting)
                {
                    output = $"CONNECTING to {_host}:{_port}";
                    return output;
                }
                else
                {
                    _lastUpdatePosition = 40 + (!String.IsNullOrEmpty(_fsMon.Hostname) ? _fsMon.Hostname.Length : 0) + _fsMon.Port.ToString().Length - 1;
                    return (_fsMon.IsConnected) ? $"CONNECTED to {_fsMon.Hostname}:{_fsMon.Port} | Last update: {new string(' ', 19)} | Press Q to quit" : "DISCONNECTED";
                }
            }
        }
        static void Main(string[] args)
        {
            // Size the window
            System.Console.WindowHeight = 30;
            System.Console.WindowWidth = 120;

            // Initialize variables
            _run = true;
            _isConnecting = false;
            _fsMon = new FlightSimMonitor();
            _fsMon.PollInterval = 1000;
            _fsMon.Connected += _fsMon_Connected;
            _fsMon.Disconnected += _fsMon_Disconnected;
            _fsMon.DataReceived += _fsMon_DataReceived;
            _fsMon.Landed += _fsMon_Landed;
            _fsMon.ParkingBrakeSet += _fsMon_ParkingBrakeSet;
            _fsMon.ParkingBrakeReleased += _fsMon_ParkingBrakeReleased;
            
            // Drop into the menu loop
            DoMenu();

            // Hide the cursor
            System.Console.CursorVisible = false;

            // Main loop
            while (_run)
            {
                // Wait for a keypress
                ConsoleKeyInfo keyPress = System.Console.ReadKey(true);

                switch (keyPress.KeyChar)
                {
                    case 'Q':
                    case 'q':
                        // Quit the application
                        Shutdown();
                        break;
                    case ' ':
                        System.Diagnostics.Debug.WriteLine("Caught space");
                        BuildDisplay();
                        break;
                }
            }

            // End
            System.Console.Write("Press any key to exit...");
            System.Console.ReadKey();
        }

        private static void Shutdown()
        {
            // Dispose of the FlightSimMonitor instance, then exit
            _fsMon.Dispose();
            Environment.Exit(0);
        }

        private static void _fsMon_ParkingBrakeReleased(object sender, EventArgs e)
        {
        }

        private static void _fsMon_ParkingBrakeSet(object sender, EventArgs e)
        {
        }

        private static void _fsMon_Landed(object sender, EventArgs e)
        {
        }

        private static void _fsMon_DataReceived(object sender, FlightSimMonitor.DataReceivedEventArgs e)
        {
            UpdateDisplay(e);
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
            System.Console.Clear();
            UpdateStatus();
            BuildDisplay();
            //System.Console.WriteLine($"SimConnect connected at {e.ConnectedTime.ToString()}! Last disconnected at {e.LastDisconnectedTime.ToString()}");

            // Begin polling
            _fsMon.Poll();
        }

        private static void BuildDisplay()
        {
            WriteFieldLabel(1, 1, "Position", 9);
            WriteFieldLabel(1, 2, "Altitude", 9);
            WriteFieldLabel(1, 3, "Pitch", 9);
            WriteFieldLabel(1, 4, "Bank", 9);
            WriteFieldLabel(1, 5, "Heading", 9);
            WriteFieldLabel(1, 6, "Flaps", 9);
            WriteFieldLabel(1, 7, "IndicSpd", 9);
            WriteFieldLabel(1, 8, "TAS", 9);
            WriteFieldLabel(1, 9, "GrndSpd", 9);
            WriteFieldLabel(1, 10, "VertSpd", 9);

            WriteFieldLabel(40, 1, "Engine 1", 16);
            WriteFieldLabel(40, 2, "Engine 2", 16);
            WriteFieldLabel(40, 3, "Engine 3", 16);
            WriteFieldLabel(40, 4, "Engine 4", 16);
            WriteFieldLabel(40, 5, "Nav Lights", 16);
            WriteFieldLabel(40, 6, "Beacon", 16);
            WriteFieldLabel(40, 7, "Landing Lights", 16);
            WriteFieldLabel(40, 8, "Taxi Lights", 16);
            WriteFieldLabel(40, 9, "Strobe", 16);
            WriteFieldLabel(40, 10, "Panel Lights", 16);
            WriteFieldLabel(40, 11, "Recog Lights", 16);
            WriteFieldLabel(40, 12, "Wing Lights", 16);
            WriteFieldLabel(40, 13, "Logo Lights", 16);
            WriteFieldLabel(40, 14, "Cabin Lights", 16);

            WriteFieldLabel(80, 1, "State", 14);
            WriteFieldLabel(80, 2, "Parking Brake", 14);
        }

        private static void WriteFieldLabel(int left, int top, string value, int length)
        {
            // Adjust left and top for zero-offset
            left--;
            top--;

            string formatString = "{0," + length + "}";

            System.Console.SetCursorPosition(left, top);
            System.Console.Write(String.Format($"{formatString}:", value));
        }

        private static void UpdateDisplay(FlightSimMonitor.DataReceivedEventArgs args)
        {
            //int heading = Convert.ToInt32(args.Heading * (180 / Math.PI));
            // Column 1
            UpdateFieldValue(12, 1, 15, $"{args.Latitude.ToString("0.000")},{args.Longitude.ToString("0.000")}");   // Position
            UpdateFieldValue(12, 2, 7, $"{args.Altitude.ToString("0")}ft");                                         // Altitude
            UpdateFieldValue(12, 3, 7, $"{args.Pitch.ToString("0.00")}°");
            UpdateFieldValue(12, 4, 7, $"{args.Bank.ToString("0.00")}°");
            UpdateFieldValue(12, 5, 4, $"{args.HeadingMagnetic.ToString("0")}°");                                   // Heading
            UpdateFieldValue(12, 6, 7, $"{args.FlapsHandleIndex.ToString("0")} notch of {args.FlapsNumHandlePositions.ToString("0")}");
            UpdateFieldValue(12, 7, 6, $"{args.Airspeed_Indicated.ToString("0")}kts");                              // Indicated Speed
            UpdateFieldValue(12, 8, 6, $"{args.Airspeed_True.ToString("0")}kts");                                   // True Airspeed
            UpdateFieldValue(12, 9, 6, $"{args.GPSGroundSpeed.ToString("0")}kts");                                  // Groundspeed
            UpdateFieldValue(12, 10, 8, $"{(args.VerticalSpeed * 60).ToString("0")}fpm");                            // Vertical Speed

            // Column 2
            string engine1State = (args.Engine1Combusting) ? "Running" : args.Engine1Starter ? "Armed" : "Off";
            string engine2State = args.NumberOfEngines < 2 ? "N/A" : args.Engine2Combusting ? "Running" : args.Engine2Starter ? "Armed" : "Off";
            string engine3State = args.NumberOfEngines < 3 ? "N/A" : args.Engine3Combusting ? "Running" : args.Engine3Starter ? "Armed" : "Off";
            string engine4State = args.NumberOfEngines < 4 ? "N/A" : args.Engine4Combusting ? "Running" : args.Engine4Starter ? "Armed" : "Off";
            UpdateFieldValue(58, 1, 16, engine1State);
            UpdateFieldValue(58, 2, 16, engine2State);
            UpdateFieldValue(58, 3, 16, engine3State);
            UpdateFieldValue(58, 4, 16, engine4State);
            UpdateFieldValue(58, 5, 16, $"{OnOffState(args.Lights.Nav)}");
            UpdateFieldValue(58, 6, 16, $"{OnOffState(args.Lights.Beacon)}");
            UpdateFieldValue(58, 7, 16, $"{OnOffState(args.Lights.Landing)}");
            UpdateFieldValue(58, 8, 16, $"{OnOffState(args.Lights.Taxi)}");
            UpdateFieldValue(58, 9, 16, $"{OnOffState(args.Lights.Strobe)}");
            UpdateFieldValue(58, 10, 16, $"{OnOffState(args.Lights.Panel)}");
            UpdateFieldValue(58, 11, 16, $"{OnOffState(args.Lights.Recognition)}");
            UpdateFieldValue(58, 12, 16, $"{OnOffState(args.Lights.Wing)}");
            UpdateFieldValue(58, 13, 16, $"{OnOffState(args.Lights.Logo)}");
            UpdateFieldValue(58, 14, 16, $"{OnOffState(args.Lights.Cabin)}");

            // Column 3
            UpdateFieldValue(97, 1, 8, $"{args.FlightState}");
            UpdateFieldValue(97, 2, 8, $"{args.ParkingBrakeState}");

            // Update Last Updated
            UpdateFieldValue(_lastUpdatePosition, 30, 19, $"{args.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")}");
        }

        private static string OnOffState(bool val)
        {
            if (val)
                return "On";
            else
                return "Off";
        }

        /// <summary>
        /// Clears and updates a field's value with new data
        /// </summary>
        /// <param name="left">Column number</param>
        /// <param name="top">Row number</param>
        /// <param name="length">Maximum length of the value</param>
        /// <param name="value">New value to display</param>
        private static void UpdateFieldValue(int left, int top, int length, string value)
        {
            // Adjust left and top for zero-offset
            left--;
            top--;

            // Erase existing data
            System.Console.SetCursorPosition(left, top);
            System.Console.Write(new string(' ', length));

            // Write new data
            System.Console.SetCursorPosition(left, top);
            System.Console.Write(value);
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
