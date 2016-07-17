﻿/*
 * Zazzles : A cross platform service framework
 * Copyright (C) 2014-2016 FOG Project
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
using System.Timers;
using Newtonsoft.Json.Linq;
using Zazzles.PowerComponents;

namespace Zazzles
{
    /// <summary>
    ///     Handle all shutdown requests
    ///     The windows shutdown command is used instead of the win32 api because it notifies the user prior
    /// </summary>
    public static class Power
    {
        public enum ShutdownOptions
        {
            None,
            Abort,
            Delay
        }

        private const string LogName = "Power";
        private const int DefaultGracePeriod = 60;
        // Variables needed for aborting a shutdown
        private static Timer _timer;
        private static bool delayed;
        private static dynamic requestData = new JObject();
        private static Func<bool> shouldAbortFunc;
        private static readonly IPower _instance;

        static Power()
        {
            switch (Settings.OS)
            {
                case Settings.OSType.Mac:
                    _instance = new MacPower();
                    break;
                case Settings.OSType.Linux:
                    _instance = new LinuxPower();
                    break;
                default:
                    _instance = new WindowsPower();
                    break;
            }

            Bus.Subscribe(Bus.Channel.Power, ParseBus);
        }

        public static bool ShuttingDown { get; private set; }
        public static bool Requested { get; private set; }
        public static bool Updating { get; set; }

        public static bool IsActionPending()
        {
            return ShuttingDown || Updating || Requested;
        }

        private static void ParseBus(dynamic data)
        {
            if (data.action == null)
                return;

            string action = data.action.ToString();
            action = action.Trim();

            if (action.Equals("abort"))
                AbortShutdown();
            else if (action.Equals("shuttingdown"))
                ShuttingDown = true;
            else if (action.Equals("help"))
                HelpShutdown(data);
            else if (action.Equals("delay"))
                DelayShutdown(data);
            else if (action.Equals("request"))
                Requested = true;
        }

        private static void DelayShutdown(dynamic data)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }

            if (data.delay == null)
                return;

            //DelayTime is in minutes
            int delayTime;

            try
            {
                delayTime = (int) data.delay;
            }
            catch (Exception)
            {
                return;
            }

            if (delayTime < 1)
                return;

            Log.Entry(LogName, "Delayed power action by " + delayTime + " minutes");

            Notification.Emit(
               "Shutdown Delayed",
               "Shutdown has been delayed for " + delayTime + " minutes");

            delayed = true;
            _timer = new Timer(delayTime*1000*60);
            _timer.Elapsed += TimerElapsed;
            _timer.Start();

            if (Settings.OS == Settings.OSType.Windows)
                return;

            ProcessHandler.Run("wall", $"-n <<< \"Shutdown has been delayed by {delayTime} minutes\"", true);
        }

        /// <summary>
        ///     Called when a shutdown is requested via the Bus
        /// </summary>
        /// <param name="data">The shutdown data to use</param>
        private static void HelpShutdown(dynamic data)
        {
            if (data.type == null)
                return;
            if (data.reason == null)
                return;

            string type = data.type.ToString();
            type = type.Trim();

            if (type.Equals("shutdown"))
                Shutdown(data.reason.ToString(), ShutdownOptions.Abort, data.reason.ToString());
            else if (type.Equals("reboot"))
                Restart(data.reason.ToString(), ShutdownOptions.Abort, data.reason.ToString());
        }

        /// <summary>
        ///     Create a shutdown command
        /// </summary>
        /// <param name="parameters">The parameters to use</param>
        public static void CreateTask(string parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            shouldAbortFunc = null;
            requestData = new JObject();

            Log.Entry(LogName, "Creating shutdown request");
            Log.Entry(LogName, "Parameters: " + parameters);

            _instance.CreateTask(parameters);

            dynamic json = new JObject();
            json.action = "shuttingdown";
            Bus.Emit(Bus.Channel.Power, json, true);

            Requested = false;
            ShuttingDown = true;
        }

        public static void QueueShutdown(string parameters, ShutdownOptions options = ShutdownOptions.Abort, string message = null,
            int gracePeriod = -1)
        {
            // If no user is logged in, skip trying to notify users
            if (!User.AnyLoggedIn())
            {
                CreateTask(parameters);
                return;
            }

            // Check if a task is already in progress
            if (_timer != null && _timer.Enabled)
            {
                Log.Entry(LogName, "Power task already in-progress");
                return;
            }

            Requested = true;
            delayed = false;

            // Load the grace period from Settings or use the default one
            try
            {
                if (gracePeriod == -1)
                    gracePeriod = (!string.IsNullOrEmpty(Settings.Get("PromptTime")))
                        ? int.Parse(Settings.Get("PromptTime"))
                        : DefaultGracePeriod;
            }
            catch (Exception)
            {
                gracePeriod = DefaultGracePeriod;
            }

            // Generate the request data
            Log.Entry(LogName, $"Creating shutdown command in {gracePeriod} seconds");

            requestData = new JObject();
            requestData.action = "request";
            requestData.period = gracePeriod;
            requestData.options = options;
            requestData.command = parameters;
            requestData.message = message ?? "This computer needs to perform maintenance.";

            Bus.Emit(Bus.Channel.Power, requestData, true);
            _timer = new Timer(gracePeriod*1000);
            _timer.Elapsed += TimerElapsed;
            _timer.Start();

            // Notify all open consoles about the shutdown (for ssh users)
            if (Settings.OS == Settings.OSType.Windows) return;
            ProcessHandler.Run("wall", $"-n <<< \"Shutdown will occur in {gracePeriod} seconds\"", true);
        }

        private static bool ShouldAbort()
        {
            if (shouldAbortFunc == null || !shouldAbortFunc()) return false;
            Log.Entry(LogName, "Shutdown aborted by calling module");
            AbortShutdown();
            return true;
        }

        private static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (delayed)
                {
                    _timer.Dispose();

                    string message = null;

                    if (requestData.message != null)
                        message = requestData.message.ToString();

                    if (ShouldAbort()) return;

                    QueueShutdown(requestData.command.ToString(), ShutdownOptions.None, message, (int) requestData.period);
                    return;
                }

                if (ShouldAbort()) return;

                CreateTask(requestData.command.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not create shutdown command from request");
                Log.Error(LogName, ex);
            }
        }

        public static void Shutdown(string comment, ShutdownOptions options = ShutdownOptions.Abort, string message = null,
            int seconds = 0)
        {
            _instance.Shutdown(comment, options, message, seconds);
        }

        public static void Restart(string comment, ShutdownOptions options = ShutdownOptions.Abort, string message = null,
            int seconds = 0)
        {
            _instance.Restart(comment, options, message, seconds);
        }

        public static void Shutdown(string comment, Func<bool> abortCheckFunc, ShutdownOptions options = ShutdownOptions.Abort,
            string message = null, int seconds = 0)
        {
            shouldAbortFunc = abortCheckFunc;
            Shutdown(comment, options, message, seconds);
        }

        public static void Restart(string comment, Func<bool> abortCheckFunc, ShutdownOptions options = ShutdownOptions.Abort,
            string message = null, int seconds = 0)
        {
            shouldAbortFunc = abortCheckFunc;
            Restart(comment, options, message, seconds);
        }

        /// <summary>
        ///     Entry off the current user
        /// </summary>
        public static void LogOffUser()
        {
            _instance.LogOffUser();
        }

        /// <summary>
        ///     Hibernate the computer
        /// </summary>
        public static void Hibernate()
        {
            _instance.Hibernate();
        }

        /// <summary>
        ///     Lock the workstation
        /// </summary>
        public static void LockWorkStation()
        {
            _instance.LockWorkStation();
        }

        /// <summary>
        ///     Abort a shutdown if it is not to late
        /// </summary>
        public static void AbortShutdown()
        {
            Log.Entry(LogName, "Aborting shutdown");
            ShuttingDown = false;
            Requested = false;

            if (_timer == null) return;

            _timer.Stop();
            _timer.Close();
            _timer = null;

            dynamic abortJson = new JObject();
            abortJson.action = "abort";
            shouldAbortFunc = null;
            Bus.Emit(Bus.Channel.Power, abortJson, true);

            Notification.Emit(
             "Shutdown Aborted",
             "Shutdown has been aborted");
        }
    }
}