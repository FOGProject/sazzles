﻿/*
 * Zazzles : A cross platform service framework
 * Copyright (C) 2014-2015 FOG Project
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using WebSocket4Net;

namespace Zazzles.BusComponents
{
    internal class BusClient
    {
        private const string LogName = "Bus::Client";

        public BusClient(int port)
        {
            Socket = new WebSocket("ws://127.0.0.1:" + port + "/");
        }

        public WebSocket Socket { get; }

        public bool Start()
        {
            try
            {
                Socket.Open();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not start");
                Log.Error(LogName, ex);
            }

            return false;
        }

        public bool Stop()
        {
            try
            {
                Socket.Close();
                Socket.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not stop");
                Log.Error(LogName, ex);
            }

            return false;
        }

        /// <summary>
        ///     Send a message to the bus server
        /// </summary>
        /// <param name="message">The message to emit</param>
        public void Send(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                Socket.Send(message);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not send message");
                Log.Error(LogName, ex);
            }
        }
    }
}