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

namespace Zazzles.Data
{
    public static class Time
    {
        public static string FormatMinutes(double totalMinutes)
        {
            return FormatSeconds(totalMinutes*60);
        }

        public static string FormatSeconds(double totalSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(totalSeconds);

            var hours = (int)timeSpan.TotalHours;
            var minutes = timeSpan.Minutes;
            var seconds = timeSpan.Seconds;

            return BuildFormatSection(hours, "hour", "hours") + BuildFormatSection(minutes, "minute", "minutes") + BuildFormatSection(seconds, "second", "seconds");
        }

        private static string BuildFormatSection(int value, string singular, string plural)
        {
            return value <= 0 ? string.Empty : $"{value} {(value == 1 ? singular : plural)} ";
        }
    }
}