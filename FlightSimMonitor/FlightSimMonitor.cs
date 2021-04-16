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
        private List<SimVar> _dataDefinition;
        private Timer _pollTimer;
        private int _pollInterval;
        private bool _lastGroundState;
        private int _lastParkingBrakeState;
        private bool _firstDataRecvd;
        private int _dataDefinitionId;
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
            // Initialize the data definition
            _dataDefinition = InitializeDataDefinition();

            // Initialize a new FsConnect object
            _fsConn = new FsConnect();
            _fsConn.ConnectionChanged += _fsConn_ConnectionChanged;
            _fsConn.FsDataReceived += _fsConn_FsDataReceived;

            // Initialize the last connected & disconnected times to the minimum DateTime value, since DateTimes are wonky about nulls
            _lastConnectedTime = DateTime.MinValue;
            _lastDisconnecedTime = DateTime.MinValue;

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
                _pollTimer = new Timer((e) => { _fsConn.RequestData(Requests.PlaneInfo, _dataDefinitionId); }, null, 0, pollInterval);
        }

        public void Stop()
        {
            // Destroy the timer
            _pollTimer = null;
        }

        /// <summary>
        /// Returns a hard-coded list of properties to query SimConnect about
        /// </summary>
        private List<SimVar> InitializeDataDefinition()
        {
            return new List<SimVar>
            {
                new SimVar("Title", null, SIMCONNECT_DATATYPE.STRING256),
                new SimVar(FsSimVar.PlaneLatitude, FsUnit.Degrees, SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar(FsSimVar.PlaneLongitude, FsUnit.Degree, SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar(FsSimVar.PlaneAltitude, FsUnit.Feet, SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar(FsSimVar.PlanePitchDegrees, FsUnit.Degrees, SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar(FsSimVar.PlaneBankDegrees, FsUnit.Degree, SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar(FsSimVar.PlaneHeadingDegreesTrue, FsUnit.Degrees, SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar(FsSimVar.PlaneHeadingDegreesMagnetic, FsUnit.Degrees, SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar(FsSimVar.FlapsHandleIndex, FsUnit.Number, SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar(FsSimVar.FlapsNumHandlePositions, FsUnit.Number, SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar(FsSimVar.AirspeedIndicated, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar(FsSimVar.AirspeedTrue, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar(FsSimVar.GpsGroundSpeed, FsUnit.Knots, SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar(FsSimVar.VerticalSpeed, FsUnit.FeetPerSecond, SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar(FsSimVar.SimOnGround, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32),
                new SimVar(FsSimVar.BrakeParkingPosition, FsUnit.Position32k, SIMCONNECT_DATATYPE.INT32),
                new SimVar(FsSimVar.NumberOfEngines, FsUnit.Number, SIMCONNECT_DATATYPE.FLOAT64),
                new SimVar("GENERAL ENG STARTER:1", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("GENERAL ENG STARTER:2", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("GENERAL ENG STARTER:3", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("GENERAL ENG STARTER:4", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("GENERAL ENG COMBUSTION:1", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("GENERAL ENG COMBUSTION:2", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("GENERAL ENG COMBUSTION:3", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar("GENERAL ENG COMBUSTION:4", "Bool", SIMCONNECT_DATATYPE.INT32),
                new SimVar(FsSimVar.LightNav, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32),
                new SimVar(FsSimVar.LightBeacon, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32),
                new SimVar(FsSimVar.LightLanding, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32),
                new SimVar(FsSimVar.LightTaxi, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32),
                new SimVar(FsSimVar.LightStrobe, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32),
                new SimVar(FsSimVar.LightPanel, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32),
                new SimVar(FsSimVar.LightRecognition, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32),
                new SimVar(FsSimVar.LightWing, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32),
                new SimVar(FsSimVar.LightLogo, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32),
                new SimVar(FsSimVar.LightCabin, FsUnit.Bool, SIMCONNECT_DATATYPE.INT32)
            };
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct PlaneInfoResponse
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public String Title;
            [SimVar(NameId = FsSimVar.PlaneLatitude, UnitId = FsUnit.Degree)]
            public double Latitude;
            [SimVar(NameId = FsSimVar.PlaneLongitude, UnitId = FsUnit.Degree)]
            public double Longitude;
            [SimVar(NameId = FsSimVar.PlaneAltitude, UnitId = FsUnit.Feet)]
            public double Altitude;
            [SimVar(NameId = FsSimVar.PlanePitchDegrees, UnitId = FsUnit.Degree)]
            public double Pitch;
            [SimVar(NameId = FsSimVar.PlaneBankDegrees, UnitId = FsUnit.Degree)]
            public double Bank;
            [SimVar(NameId = FsSimVar.PlaneHeadingDegreesTrue, UnitId = FsUnit.Degree)]
            public double HeadingTrue;
            [SimVar(NameId = FsSimVar.PlaneHeadingDegreesMagnetic, UnitId = FsUnit.Degree)]
            public double HeadingMagnetic;
            [SimVar(NameId = FsSimVar.FlapsHandleIndex, UnitId = FsUnit.Number)]
            public double FlapsHandleIndex;
            [SimVar(NameId = FsSimVar.FlapsNumHandlePositions, UnitId = FsUnit.Number)]
            public double FlapsNumHandlePositions;
            [SimVar(NameId = FsSimVar.AirspeedIndicated, UnitId = FsUnit.Knots)]
            public double Airspeed_Indicated;
            [SimVar(NameId = FsSimVar.AirspeedTrue, UnitId = FsUnit.Knots)]
            public double Airspeed_True;
            [SimVar(NameId = FsSimVar.GpsGroundSpeed, UnitId = FsUnit.Knots)]
            public double GPSGroundSpeed;
            [SimVar(NameId = FsSimVar.VerticalSpeed, UnitId = FsUnit.FeetPerSecond)]
            public double VerticalSpeed;
            [SimVar(NameId = FsSimVar.SimOnGround, UnitId = FsUnit.Bool)]
            public bool OnGround;
            [SimVar(NameId = FsSimVar.BrakeParkingPosition, UnitId = FsUnit.Position32k)]
            public int ParkingBrakeSet;
            [SimVar(NameId = FsSimVar.NumberOfEngines, UnitId = FsUnit.Number)]
            public double NumberOfEngines;
            [SimVar(Name = "GENERAL ENG STARTER:1", UnitId = FsUnit.Bool)]
            public bool Engine1Starter;
            [SimVar(Name = "GENERAL ENG STARTER:2", UnitId = FsUnit.Bool)]
            public bool Engine2Starter;
            [SimVar(Name = "GENERAL ENG STARTER:3", UnitId = FsUnit.Bool)]
            public bool Engine3Starter;
            [SimVar(Name = "GENERAL ENG STARTER:4", UnitId = FsUnit.Bool)]
            public bool Engine4Starter;
            [SimVar(Name = "GENERAL ENG COMBUSTION:1", UnitId = FsUnit.Bool)]
            public bool Engine1Combusting;
            [SimVar(Name = "GENERAL ENG COMBUSTION:2", UnitId = FsUnit.Bool)]
            public bool Engine2Combusting;
            [SimVar(Name = "GENERAL ENG COMBUSTION:3", UnitId = FsUnit.Bool)]
            public bool Engine3Combusting;
            [SimVar(Name = "GENERAL ENG COMBUSTION:4", UnitId = FsUnit.Bool)]
            public bool Engine4Combusting;
            [SimVar(NameId = FsSimVar.LightNav, UnitId = FsUnit.Bool)]
            public bool LightNav;
            [SimVar(NameId = FsSimVar.LightBeacon, UnitId = FsUnit.Bool)]
            public bool LightBeacon;
            [SimVar(NameId = FsSimVar.LightLanding, UnitId = FsUnit.Bool)]
            public bool LightLanding;
            [SimVar(NameId = FsSimVar.LightTaxi, UnitId = FsUnit.Bool)]
            public bool LightTaxi;
            [SimVar(NameId = FsSimVar.LightStrobe, UnitId = FsUnit.Bool)]
            public bool LightStrobe;
            [SimVar(NameId = FsSimVar.LightPanel, UnitId = FsUnit.Bool)]
            public bool LightPanel;
            [SimVar(NameId = FsSimVar.LightRecognition, UnitId = FsUnit.Bool)]
            public bool LightRecognition;
            [SimVar(NameId = FsSimVar.LightWing, UnitId = FsUnit.Bool)]
            public bool LightWing;
            [SimVar(NameId = FsSimVar.LightLogo, UnitId = FsUnit.Bool)]
            public bool LightLogo;
            [SimVar(NameId = FsSimVar.LightCabin, UnitId = FsUnit.Bool)]
            public bool LightCabin;
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
