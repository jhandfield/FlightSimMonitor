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

        /// <summary>
        /// Fires when the plane has landed
        /// </summary>
        public event EventHandler Landed;

        /// <summary>
        /// Fires when the plane has taken off
        /// </summary>
        public event EventHandler Takeoff;

        /// <summary>
        /// Fires when the parking brake has been set
        /// </summary>
        public event EventHandler ParkingBrakeSet;

        /// <summary>
        /// Fires when the parking brake is released
        /// </summary>
        public event EventHandler ParkingBrakeReleased;

        #endregion

        #region Event Handlers
        internal virtual void OnParkingBrakeSet()
        {
            EventHandler raiseEvent = ParkingBrakeSet;

            if (raiseEvent != null)
                // Raise the event
                raiseEvent(this, EventArgs.Empty);
        }

        internal virtual void OnParkingBrakeReleased()
        {
            EventHandler raiseEvent = ParkingBrakeReleased;

            if (raiseEvent != null)
                // Raise the event
                raiseEvent(this, EventArgs.Empty);
        }

        internal virtual void OnLanded()
        {
            EventHandler raiseEvent = Landed;

            if (raiseEvent != null)
                // Raise the event
                raiseEvent(this, EventArgs.Empty);
        }

        internal virtual void OnTakeoff()
        {
            EventHandler raiseEvent = Takeoff;

            if (raiseEvent != null)
                raiseEvent(this, EventArgs.Empty);
        }

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
            public double HeadingTrue { get; set; }
            public double HeadingMagnetic { get; set; }
            public double Airspeed_Indicated { get; set; }
            public double Airspeed_True { get; set; }
            public double GPSGroundSpeed { get; set; }
            public double VerticalSpeed { get; set; }
            public bool OnGround { get; set; }
            public bool Engine1Combusting { get; set; }
            public bool Engine2Combusting { get; set; }
            public bool Engine3Combusting { get; set; }
            public bool Engine4Combusting { get; set; }
            public double ParkingBrakeSet { get; set; }
            public string FlightState { get; set; }
            public string ParkingBrakeState { get; set; }
            public DateTime Timestamp { get; set; }
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