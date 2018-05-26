﻿/*
 * Zazzles : A cross platform service framework
 * Copyright (C) 2014-2018 FOG Project
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
using System.Runtime.Serialization;

namespace Zazzles.Core.Device.Power.DataContract
{
    [DataContract(Name="PowerRequest")]
    public class PowerRequest
    {
        [DataMember(Name = "action", IsRequired = true)]
        public PowerAction Action { get; set; }

        [DataMember(Name = "comment", IsRequired = true)]
        public string Comment { get; set; }

        [DataMember(Name = "when", IsRequired = true)]
        public DateTime When { get; set; }

        public PowerRequest()
        {

        }

        public PowerRequest(PowerAction action, DateTime when, string comment = "")
        {
            Action = action;
            Comment = comment;
            When = when;
        }

        public override string ToString()
        {
            return $"{Action}, {When}, {Comment ?? string.Empty}";
        }

    }
}