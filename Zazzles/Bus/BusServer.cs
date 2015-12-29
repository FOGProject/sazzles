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
using System.Linq;
using SuperSocket.SocketEngine;
using SuperWebSocket;

namespace Zazzles.BusComponents
{
    internal class BusServer
    {
        private const string LogName = "Bus::Server";

        public BusServer()
        {
            // Load the server configuration from app.config
            // This is needed to ensure the server is only
            // broadcasting and accepting connectiosn on 127.0.0.1
            // for security.
            var bootstrap = BootstrapFactory.CreateBootstrap();
            bootstrap.Initialize();
            bootstrap.Start();
            Socket = bootstrap.AppServers.FirstOrDefault() as WebSocketServer;
        }

        public WebSocketServer Socket { get; }

        public bool Start()
        {
            try
            {
                return Socket.Start();
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
                Socket.Stop();
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
        ///     Send a message to all bus clients
        /// </summary>
        /// <param name="message">The message to emit</param>
        public void Send(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            foreach (var session in Socket.GetAllSessions())
                try
                {
                    session.Send(message);
                }
                catch (Exception ex)
                {
                    Log.Error(LogName, "Could not send message");
                    Log.Error(LogName, ex);
                }
        }
    }
}