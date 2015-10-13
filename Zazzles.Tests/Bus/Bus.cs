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

using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Zazzles.Tests.Bus
{
    [TestFixture]
    public class BusTests
    {
        [SetUp]
        public void Init()
        {
            Zazzles.Bus.Subscribe(Zazzles.Bus.Channel.Debug, RecieveMessage);
        }

        private string message = "";

        private void RecieveMessage(dynamic data)
        {
            message = data.message;
        }

        [Test]
        public void LocalEmit()
        {
            var expected = "HelloWorld@123$";

            var data = new JObject
            {
                {"message", expected}
            };

            Zazzles.Bus.Emit(Zazzles.Bus.Channel.Debug, data);
			Assert.AreEqual(expected, message);
        }

        [Test]
        public void Unsubscribe()
        {
            var expected = "HelloWorld@123555$";

            var data = new JObject
            {
                {"message", expected}
            };

            Zazzles.Bus.Unsubscribe(Zazzles.Bus.Channel.Debug, RecieveMessage);
            Zazzles.Bus.Emit(Zazzles.Bus.Channel.Debug, data);
            Assert.AreNotEqual(expected, message);
        }
    }
}