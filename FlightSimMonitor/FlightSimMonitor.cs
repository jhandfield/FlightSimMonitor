using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTrue.FsConnect;

namespace Handfield.FlightSimMonitor
{
    public class FlightSimMonitor
    {
        #region Fields
        private FsConnect _fsConn;
        private DateTime _lastConnectedTime;
        private DateTime _lastDisconnecedTime;
        private uint _port;
        #endregion

        #region Properties
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
            _fsConn = new FsConnect();
            _fsConn.ConnectionChanged += _fsConn_ConnectionChanged;

            _lastConnectedTime = DateTime.MinValue;
            _lastDisconnecedTime = DateTime.MinValue;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Attempt to connect to SimConnect with the currently-configured parameters
        /// </summary>
        public void Connect()
        {
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
        /// Handle ConnectionChanged events from FsConnect
        /// </summary>
        private void _fsConn_ConnectionChanged(object sender, EventArgs e)
        {
            // Fire the appropriate event, based on the new connection state
            if (_fsConn.Connected)
            {
                _lastConnectedTime = DateTime.Now;
                OnConnected(new ConnectedEventArgs { ConnectedTime = _lastConnectedTime, LastDisconnectedTime = _lastDisconnecedTime });
            }
            else
            {
                _lastDisconnecedTime = DateTime.Now;
                OnDisconnected(new DisconnectedEventArgs { DisconnectedTime = _lastDisconnecedTime, LastConnectedTime = _lastConnectedTime });
            }

        }
        #endregion

        #region Event Definitions
        /// <summary>
        /// Fires when SimConnect reports it has connected to the server
        /// </summary>
        public event EventHandler<ConnectedEventArgs> Connected;

        /// <summary>
        /// Fires when SimConnect reports it has been disconnected from the server
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected;
        #endregion

        #region Event Handlers
        protected virtual void OnConnected(ConnectedEventArgs e)
        {
            EventHandler<ConnectedEventArgs> raiseEvent = Connected;

            if (raiseEvent != null)
            {
                // Set properties
                e.ConnectedTime = _lastConnectedTime;
                e.LastDisconnectedTime = _lastDisconnecedTime;

                // Raise the event
                raiseEvent(this, e);
            }
        }

        protected virtual void OnDisconnected(DisconnectedEventArgs e)
        {
            EventHandler<DisconnectedEventArgs> raiseEvent = Disconnected;

            if (raiseEvent != null)
            {
                // Set properties
                e.DisconnectedTime = _lastDisconnecedTime;
                e.LastConnectedTime = _lastConnectedTime;

                // Raise the event
                raiseEvent(this, e);
            }
        }
        #endregion

        #region Event Arguments
        public class ConnectedEventArgs
        {
            public DateTime ConnectedTime { get; set; }
            public DateTime LastDisconnectedTime { get; set; }

            public ConnectedEventArgs() { }
            public ConnectedEventArgs(DateTime connTime, DateTime disconTime)
            {
                ConnectedTime = connTime;
                LastDisconnectedTime = disconTime;
            }
        }

        public class DisconnectedEventArgs
        {
            public DateTime DisconnectedTime { get; set; }
            public DateTime LastConnectedTime { get; set; }

            public DisconnectedEventArgs() { }
            public DisconnectedEventArgs(DateTime disconnTime, DateTime connTime)
            {
                DisconnectedTime = disconnTime;
                LastConnectedTime = connTime;
            }
        }
        #endregion
    }
}
