using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CTrue.FsConnect;

namespace Handfield.FlightSimMonitor
{
    /// <summary>
    /// Responsible for communication with the flight simulator
    /// </summary>

    // TODO: Come up with a better name...
    internal class SimCommunicator
    {
        #region Fields
        private FsConnect _fsConn;
        private bool _connected = false;
        #endregion

        #region Properties
        private bool IsConnected { get; }
        #endregion

        #region Constructors
        internal SimCommunicator()
        {
            // Initialize fields
            

            
        }
        #endregion

        #region Methods
        

        /// <summary>
        /// Attempt to connect to SimConnect with the given parameters. Listen for the Connected event to know when the connection has been made.
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="hostName"></param>
        /// <param name="port"></param>
        internal void Connect(string applicationName, string hostName, uint port)
        {
            
        }
        #endregion

       
    }
}
