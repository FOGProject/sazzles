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
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Zazzles.Data;
using RSA = Zazzles.Data.RSA;

// ReSharper disable InconsistentNaming

namespace Zazzles.Middleware
{
    public static class Authentication
    {
        private const string LogName = "Middleware::Authentication";
        private static byte[] Passkey;

        /// <summary>
        ///     Generate a random AES pass key and securely send it to the server
        /// </summary>
        /// <returns>True if successfully authenticated</returns>
        public static bool HandShake()
        {
            try
            {
                // Obtain a public key from the server
                var keyPath = Path.Combine(Settings.Location, "tmp", "public.cer");
                Communication.DownloadFile("/management/other/ssl/srvpublic.crt", keyPath);
                Log.Debug(LogName, "KeyPath = " + keyPath);
                var certificate = new X509Certificate2(keyPath);

                // Ensure the public key came from the pinned server
                if (!RSA.IsFromCA(RSA.ServerCertificate(), certificate))
                    throw new Exception("Certificate is not from FOG CA");
                Log.Entry(LogName, "Cert OK");

                // Generate a random AES key
                Passkey = AES.NewKey();

                // Get the security token from the last handshake
                var token = GetSecurityToken("token.dat");
                // Encrypt the security token and AES key using the public key
                var enKey = Transform.ByteArrayToHexString(RSA.Encrypt(certificate, Passkey));
                var enToken = Transform.ByteArrayToHexString(RSA.Encrypt(certificate, token));
                // Send the encrypted data to the server and get the response
                var response = Communication.Post("/management/index.php?sub=authorize",
                    $"sym_key={enKey}&token={enToken}&mac={Configuration.MACAddresses()}");

                // If the server accepted the token and AES key, save the new token
                //if (!response.Error && response.Encrypted)
               // {
               //     Log.Entry(LogName, "Authenticated");
               //     SetSecurityToken("token.dat", Transform.HexStringToByteArray(response.GetField("#token")));
               //     return true;
               // }

                // If the server does not recognize the host, register it
               // if (response.ReturnCode.Equals("#!ih"))
               //     Communication.Contact($"/service/register.php?hostname={Dns.GetHostName()}", true);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not authenticate");
                Log.Error(LogName, ex);
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="filePath">The path to the file where the security token is stored</param>
        /// <returns>The decrypted security token</returns>
        private static byte[] GetSecurityToken(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path must be provided!", nameof(filePath));

            try
            {
                var token = File.ReadAllBytes(filePath);
                token = DPAPI.UnProtectData(token, true);
                return token;
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not get security token");
                Log.Error(LogName, ex);
            }

            return System.Text.Encoding.ASCII.GetBytes("NoToken");
        }

        /// <summary>
        ///     Encrypt and save a security token
        /// </summary>
        /// <param name="filePath">The path to the file where the security token should be stored</param>
        /// <param name="token">The security token to encrypt and save</param>
        private static void SetSecurityToken(string filePath, byte[] token)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path must be provided!", nameof(filePath));
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            try
            {
                token = DPAPI.ProtectData(token, true);
                File.WriteAllBytes(filePath, token);
            }
            catch (Exception ex)
            {
                Log.Error(LogName, "Could not save security token");
                Log.Error(LogName, ex);
            }
        }

        /// <summary>
        ///     Decrypts a server message using AES
        /// </summary>
        /// <param name="toDecode">The string to decrypt</param>
        /// <param name="payload">The payload length</param>
        /// <returns>True if the server was contacted successfully</returns>
        public static string Decrypt(string toDecode, int payload = 0)
        {
            return AES.Decrypt(toDecode, Passkey, payload);
        }

        /// <summary>
        ///     Encrypts a message for the server using AES
        /// </summary>
        /// <param name="toEncode">The string to encrypt</param>
        /// <param name="payload">The non-secrete payload</param>
        /// <returns>True if the server was contacted successfully</returns>
        public static string Encrypt(string toEncode, byte[] payload = null)
        {
            return AES.Encrypt(toEncode, Passkey, payload);
        }

    }
}