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
using Zazzles.Data;
using NUnit.Framework;

namespace Zazzles.Tests.Data
{
    [TestFixture]
    public class TransformTests
    {
        [SetUp]
        public void Init()
        {
            Log.Output = Log.Mode.Console;
        }

        [Test]
        public void NullEmptyBase64Encode()
        {
            Assert.Throws<ArgumentException>(() => Transform.EncodeBase64(null));
            Assert.Throws<ArgumentException>(() => Transform.EncodeBase64(string.Empty));
        }

        [Test]
        public void NullEmptyBase64Decode()
        {
            Assert.Throws<ArgumentException>(() => Transform.DecodeBase64(null));
            Assert.Throws<ArgumentException>(() => Transform.DecodeBase64(string.Empty));

        }

        [Test]
        public void NullEmptyHexStringToByteArray()
        {
            Assert.Throws<ArgumentException>(() => Transform.HexStringToByteArray(null));
            Assert.Throws<ArgumentException>(() => Transform.HexStringToByteArray(string.Empty));
        }

        [Test]
        public void NullByteArrayToHexString()
        {
            Assert.Throws<ArgumentNullException>(() => Transform.ByteArrayToHexString(null));
        }

        [Test]
        public void Base64Encode()
        {
            const string message = "The dog jumped over the fence #@$";
            const string expected = "VGhlIGRvZyBqdW1wZWQgb3ZlciB0aGUgZmVuY2UgI0Ak";
            var encoded = Transform.EncodeBase64(message);
            Assert.AreEqual(expected, encoded);
        }

        [Test]
        public void Base64Decode()
        {
            const string encoded = "VGhlIGRvZyBqdW1wZWQgb3ZlciB0aGUgZmVuY2UgI0Ak";
            const string expected = "The dog jumped over the fence #@$";
            var decoded = Transform.DecodeBase64(encoded);
            Assert.AreEqual(expected, decoded);
        }

        [Test]
        public void HexStringToByteArray()
        {
            const string data = "bdb2ab3c401ef23602786e9caeb28266c18cbf06de4c634291eb4a0d51e5b7bb";
            var expected = new byte[] {189,178,171,60,64,30,242,54,2,120,110,156,174,178,130,102,193,140,191,6,222,76,99,66,145,235,74,13,81,229,183,187};

            var bytes = Transform.HexStringToByteArray(data);
            Assert.AreEqual(expected, bytes);
        }

        [Test]
        public void ByteArrayToHexString()
        {
            var data = new byte[] {189,178,171,60,64,30,242,54,2,120,110,156,174,178,130,102,193,140,191,6,222,76,99,66,145,235,74,13,81,229,183,187};
            const string expected = "bdb2ab3c401ef23602786e9caeb28266c18cbf06de4c634291eb4a0d51e5b7bb";
            var hexString = Transform.ByteArrayToHexString(data);
            Assert.AreEqual(expected, hexString);
        }
    }
}