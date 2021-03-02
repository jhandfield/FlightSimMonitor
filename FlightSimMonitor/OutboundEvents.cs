using System;

namespace Handfield.FlightSimMonitor
{
    public partial class FlightSimMonitor
    {
        #region Event Definitions
        /// <summary>
        /// Fires when data is received from SimConnect
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> DataReceived;

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
        internal virtual void OnDataReceived(DataReceivedEventArgs e)
        {
            EventHandler<DataReceivedEventArgs> raiseEvent = DataReceived;

            if (raiseEvent != null)
            {
                // Raise the event
                raiseEvent(this, e);
            }
        }

        internal virtual void OnConnected(ConnectedEventArgs e)
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

        internal virtual void OnDisconnected(DisconnectedEventArgs e)
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
        public class DataReceivedEventArgs
        {
            public string Title { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double Altitude { get; set; }
            public double Heading { get; set; }
            public double IndicatedAirspeed { get; set; }
            public double GPSGroundSpeed { get; set; }
            public bool OnGround { get; set; }
        }

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