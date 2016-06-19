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
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Zazzles
{
    public static class Settings
    {
        public enum OSType
        {
            All,
            Windows,
            Nix,
            Mac,
            Linux
        }

        private const string LogName = "Settings";
        private static string _file;
        private static JObject _data = new JObject();
        public static OSType OS { get; }
        public static string Location { get; }

        static Settings()
        {
            Location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _file = Path.Combine(Location, "settings.json");
            Reload();

            var pid = Environment.OSVersion.Platform;

            switch (pid)
            {
                case PlatformID.MacOSX:
                    OS = OSType.Mac;
                    break;
                case PlatformID.Unix:
                    string[] stdout;
                    ProcessHandler.Run("uname", "", true, out stdout);

                    if (stdout != null)
                    {
                        var kerInfo = string.Join(" ", stdout).Trim().ToLower();
                        if (kerInfo.Contains("darwin"))
                        {
                            OS = OSType.Mac;
                            break;
                        }
                    }

                    OS = OSType.Linux;
                    break;
                default:
                    OS = OSType.Windows;
                    break;
            }
        }

        /// <summary>
        ///     Check if the current OS is compatible with the given type
        /// </summary>
        /// <param name="type">The type of OS to check for compatibility with</param>
        /// <returns>True if compatible</returns>
        public static bool IsCompatible(OSType type)
        {
            if (type == OSType.All)
                return true;

            if (type == OS)
                return true;

            if (type == OSType.Linux || type == OSType.Mac && OS == OSType.Nix)
                return true;

            return false;
        }

        /// <summary>
        ///     Set the filePath of the settings.json file. Will automatically reload.
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        public static void SetPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path must be provided!", nameof(filePath));

            _file = filePath;
            Reload();
        }

        /// <summary>
        ///     Reparse the settings.json file
        /// </summary>
        public static void Reload()
        {
            try
            {
                _data = JObject.Parse(File.ReadAllText(_file));
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not load settings");
                Log.Error(LogName, ex);
            }
        }

        /// <summary>
        ///     Save the current data to settings.json
        /// </summary>
        /// <returns>True if successful</returns>
        private static bool Save()
        {
            try
            {
                File.WriteAllText(_file, _data.ToString());
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Unable to save settings");
                Log.Error(LogName, ex);
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="key">The setting to retrieve</param>
        /// <returns>The value of a setting. Will return an empty string if the key is not present.</returns>
        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must be provided!", nameof(key));

            if (_data == null) return string.Empty;

            try
            {
                var value = _data.GetValue(key);
                return string.IsNullOrEmpty(value.ToString().Trim()) ? string.Empty : value.ToString().Trim();
            }
            catch (Exception)
            {
                // ignored
            }

            return string.Empty;
        }

        /// <summary>
        ///     Set the value of a setting. Will automatically save.
        /// </summary>
        /// <param name="key">The name of the setting</param>
        /// <param name="value">The new value of the setting</param>
        public static void Set(string key, JToken value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must be provided!", nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (_data == null) _data = new JObject();
            _data[key] = value;
            Save();
        }
    }
}