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

namespace Zazzles.Core.System.Power.DataContract
{
    [DataContract(Name="PowerEvent")]
    public class PowerEvent
    {
        [DataMember(Name = "action", IsRequired = true)]
        public PowerAction Action { get; set; }

        [DataMember(Name = "comment", IsRequired = true)]
        public string Comment { get; set; }

        [DataMember(Name = "options", IsRequired = true)]
        public UserOptions Options { get; set; }

        [DataMember(Name = "atTime", IsRequired = true)]
        public DateTime AtTime { get; set; }


        public PowerEvent()
        {

        }

        public PowerEvent(PowerAction action, UserOptions options, DateTime at, string comment = "")
        {
            Action = action;
            Options = options;
            AtTime = at;
            Comment = comment;
        }

        public override string ToString()
        {
            return $"{Action}, {AtTime}, {Options}, {Comment ?? string.Empty}";
        }
    }
}
