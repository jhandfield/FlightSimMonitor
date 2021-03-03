using CTrue.FsConnect;
using System;

namespace Handfield.FlightSimMonitor
{
    public partial class FlightSimMonitor
    {
        /// <summary>
        /// Handle ConnectionChanged events from FsConnect
        /// </summary>
        private void _fsConn_ConnectionChanged(object sender, EventArgs e)
        {
            // Fire the appropriate event, based on the new connection state
            if (_fsConn.Connected)
            {
                // Record the time we've connected
                _lastConnectedTime = DateTime.Now;

                // Register the data definition with SimConnect
                _fsConn.RegisterDataDefinition<PlaneInfoResponse>(Requests.PlaneInfo, _dataDefinition);

                // Fire our Connected event
                OnConnected(new ConnectedEventArgs { ConnectedTime = _lastConnectedTime, LastDisconnectedTime = _lastDisconnecedTime });
            }
            else
            {
                // Record the time we've disconnectd
                _lastDisconnecedTime = DateTime.Now;

                // Fire our Disconnected event
                OnDisconnected(new DisconnectedEventArgs { DisconnectedTime = _lastDisconnecedTime, LastConnectedTime = _lastConnectedTime });
            }
        }

        /// <summary>
        /// Handle FsDataReceived events from FsConnect
        /// </summary>
        private void _fsConn_FsDataReceived(object sender, FsDataReceivedEventArgs e)
        {
            if (e.RequestId == (uint)Requests.PlaneInfo)
            {
                // Prepare the data to send out in the DataReceived event
                PlaneInfoResponse r = (PlaneInfoResponse)e.Data;

                DataReceivedEventArgs args = new DataReceivedEventArgs
                {
                    Title = r.Title,
                    Latitude = r.Latitude,
                    Longitude = r.Longitude,
                    Altitude = r.Altitude,
                    TrueHeading = r.TrueHeading,
                    Heading = r.Heading,
                    VerticalSpeedPerSecond = r.VerticalSpeedPerSecond,
                    IndicatedAirspeed = r.IndicatedAirspeed,
                    GPSGroundSpeed = r.GPSGroundSpeed,
                    OnGround = r.OnGround,
                    ParkingBrakeSet = r.ParkingBrakeSet,
                    FlightState = (r.OnGround) ? "Landed" : "Flying",
                    ParkingBrakeState = (r.ParkingBrakeSet > 0) ? "Set" : "Released",
                    Timestamp = DateTime.UtcNow
                };

                // Fire the DataReceived event
                OnDataReceived(args);

                // If this is the first data we've received, set some monitoring flags
                if (!_firstDataRecvd)
                {
                    _lastGroundState = r.OnGround;
                    _lastParkingBrakeState = r.ParkingBrakeSet;

                    // Record that we have received first data
                    _firstDataRecvd = true;
                }
                else
                {
                    // Check if the ground state has changed
                    if (r.OnGround != _lastGroundState)
                        // Detect what happened
                        if (r.OnGround == true)
                            // We landed, fire the Landed event
                            OnLanded();
                        else
                            OnTakeoff();

                    if (r.ParkingBrakeSet != _lastParkingBrakeState)
                        // Detect what happened
                        if (r.ParkingBrakeSet == short.MaxValue)
                            // Parking brake was juset set
                            OnParkingBrakeSet();
                        else
                            OnParkingBrakeReleased();
                }

                // Update record flags
                _lastGroundState = r.OnGround;
                _lastParkingBrakeState = r.ParkingBrakeSet;
            }
        }
    }
}
