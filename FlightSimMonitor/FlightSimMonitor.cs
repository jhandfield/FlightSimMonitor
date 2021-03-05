using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CTrue.FsConnect;
using Microsoft.FlightSimulator.SimConnect;

namespace Handfield.FlightSimMonitor
{
    public partial class FlightSimMonitor
    {
        #region Fields
        private FsConnect _fsConn;
        internal DateTime _lastConnectedTime;
        protected DateTime _lastDisconnecedTime;
        private uint _port;
        private List<SimProperty> _dataDefinition;
        private Timer _pollTimer;
        private int _pollInterval;
        private bool _lastGroundState;
        private short _lastParkingBrakeState;
        private bool _firstDataRecvd;
        #endregion

        #region Properties
        /// <summary>
        /// Frequency to request data updates from SimConnect, in milliseconds. Set to 0 to disable automatic polling of data.
        /// </summary>
        public int PollInterval
        {
            get
            {
                return _pollInterval;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Value must be zero or greater");
                else
                    _pollInterval = value;
            }
        }
        
        /// <summary>
        /// Hostname of SimConnect server to connect to
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Port number of SimConnect server to connect to
        /// </summary>
        public uint Port
        {
            get
            {
                return _port;
            }
            set
            {
                if (value < 1 || value > 65535)
                    throw new ArgumentOutOfRangeException("An invalid value was passed - must be between 1 and 65535");
                else
                    _port = value;
            }
        }

        /// <summary>
        /// Name to identify to SimConnect as
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Indicates whether SimConnect is currently connected
        /// </summary>
        public bool IsConnected { get { return _fsConn.Connected; } }
        #endregion

        #region Constructors
        public FlightSimMonitor()
        {
            // Initialize a new FsConnect object
            _fsConn = new FsConnect();
            _fsConn.ConnectionChanged += _fsConn_ConnectionChanged;
            _fsConn.FsDataReceived += _fsConn_FsDataReceived;

            // Initialize the last connected & disconnected times to the minimum DateTime value, since DateTimes are wonky about nulls
            _lastConnectedTime = DateTime.MinValue;
            _lastDisconnecedTime = DateTime.MinValue;

            // Initialize the data definition
            _dataDefinition = InitializeDataDefinition();

            // Note that we have yet to receive any data
            _firstDataRecvd = false;
        }

        /// <summary>
        /// Begin polling SimConnect for data
        /// </summary>
        public void Poll()
        {
            Poll(PollInterval);
        }

        /// <summary>
        /// Begin polling SimConnect for data
        /// </summary>
        /// <param name="pollInterval">Interval (in milliseconds) to poll for new data</param>
        public void Poll(int pollInterval)
        {
            // Ensure we're currently connected
            if (IsConnected)
                // Start the timer
                _pollTimer = new Timer((e) => { _fsConn.RequestData(Requests.PlaneInfo); }, null, 0, pollInterval);
        }

        public void Stop()
        {
            // Destroy the timer
            _pollTimer = null;
        }

        /// <summary>
        /// Returns a hard-coded list of properties to query SimConnect about
        /// </summary>
        private List<SimProperty> InitializeDataDefinition()
        {
            return new List<SimProperty>
            {
                new SimProperty("Title", null, SIMCONNECT_DATATYPE.STRING256),
                new SimProperty(FsSimVar.PlaneLatitude, FsUnit.Degrees, SIMCONNECT_DATATYPE.FLOAT64),
                new SimProperty(FsSimVar.PlaneLongitude, FsUnit.Degree, SIMCONNECT_DATATYPE.FLOAT64),
                new SimProperty(FsSimVar.PlaneAltitude, FsUnit.Feet, SIMCONNECT_DATATYPE.FLOAT64),
                new SimProperty(FsSimVar.PlaneHeadingDegreesTrue, FsUnit.Degrees, SIMCONNECT_DATATYPE.FLOAT64),
                new SimProperty(FsSimVar.PlaneHeadingDegreesMagnetic, FsUnit.Degrees, SIMCONNECT_DATATYPE.FLOAT64),
                new SimProperty(FsSimVar.AirspeedIndicated, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64),
                new SimProperty(FsSimVar.AirspeedTrue, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64),
                new SimProperty(FsSimVar.GpsGroundSpeed, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64),
                new SimProperty(FsSimVar.SimOnGround, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32),
                new SimProperty(FsSimVar.BrakeParkingPosition, FsUnit.Position32k, SIMCONNECT_DATATYPE.INT32)
            };
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct PlaneInfoResponse
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public String Title;
            public double Latitude;
            public double Longitude;
            public double Altitude;
            public double HeadingTrue;
            public double HeadingMagnetic;
            public double Airspeed_Indicated;
            public double Airspeed_True;
            public double GPSGroundSpeed;
            public bool OnGround;
            public short ParkingBrakeSet;
        }

        public enum Requests
        {
            PlaneInfo = 0
        }
        #endregion

        #region Methods
        /// <summary>
        /// Attempt to connect to SimConnect with the currently-configured parameters
        /// </summary>
        public void Connect()
        {
            // Connect to SimConnect at the address configured
            _fsConn.Connect(ApplicationName, Hostname, Port, SimConnectProtocol.Ipv4);
        }

        /// <summary>
        /// Set SimConnect connection parameters, then attempt to connect
        /// </summary>
        /// <param name="applicationName">The name of your application, to identify to SimConnect</param>
        /// <param name="hostName">Hostname to connect to SimConnect at</param>
        /// <param name="port">Port number to connect to SimConnect</param>
        public void Connect(string applicationName, string hostName, uint port)
        {
            // Set properties based on input
            ApplicationName = applicationName;
            Hostname = hostName;
            Port = port;

            // Call Connect()
            Connect();
        }

        /// <summary>
        /// Disconnects from SimConnect
        /// </summary>
        public void Disconnect()
        {
            // Disconnect & dispose
            _fsConn.Disconnect();
        }

        public void Dispose()
        {
            // Disconnect from SimConnect
            _fsConn.Disconnect();

            // Dispose of _fsConn
            _fsConn.Dispose();
        }
        #endregion
    }
}
