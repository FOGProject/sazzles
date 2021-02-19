﻿/*
 * Zazzles : A cross platform service framework
 * Copyright (C) 2014-2021 FOG Project
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
using System.Text;
using Zazzles.Data;
using NUnit.Framework;

namespace Zazzles.Tests.Data
{
    [TestFixture]
    public class DPAPITests
    {
        [SetUp]
        public void Init()
        {
            Log.Output = Log.Mode.Console;
        }

        [Test]
        public void NullProtect()
        {
            Assert.Throws<ArgumentNullException>(() => DPAPI.ProtectData(null, true));
        }

        [Test]
        public void NullUnProtect()
        {
            Assert.Throws<ArgumentNullException>(() => DPAPI.UnProtectData(null, true));
        }

        [Test]
        public void RoundTrip_Protect()
        {
            // Roundtrip a message using DPAPI protection
            const string message = @"The dog jumped over the fence #@//\\$";
            var messageBytes = Encoding.ASCII.GetBytes(message);

            var protectedBytes = DPAPI.ProtectData(messageBytes, true);
            var unProtectedBytes = DPAPI.UnProtectData(protectedBytes, true);

            // ensure the message was actually protected
            Assert.AreNotEqual(messageBytes, protectedBytes);

            Assert.AreEqual(messageBytes, unProtectedBytes);
        }
    }
}