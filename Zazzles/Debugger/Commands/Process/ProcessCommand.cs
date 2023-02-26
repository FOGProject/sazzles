﻿/*
 * Zazzles : A cross platform service framework
 * Copyright (C) 2014-2023 FOG Project
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

using System.Linq;

namespace Zazzles.Debugger.Commands.Process
{
    internal class ProcessCommand : ICommand
    {
        private const string LogName = "Console::ProcessHandler";

        public bool Process(string[] args)
        {
            if (args[0].Equals("?") || args[0].Equals("help"))
            {
                Help();
                return true;
            }

            if (args[0].Equals("run") && args.Length > 1)
            {
                var app = args[1];

                var param = "";

                if (args.Length > 2)
                    param = string.Join(" ", args.Skip(2));

                var code = ProcessHandler.Run(app, param, true);
                Log.Entry(LogName, "--> Return code = " + code);

                return true;
            }

            if (args[0].Equals("runexe") && args.Length > 1)
            {
                var app = args[1];

                var param = "";

                if (args.Length > 2)
                    param = string.Join(" ", args.Skip(2));

                ProcessHandler.RunEXE(app, param, true);
                return true;
            }

            if (args[0].Equals("runexe") && args.Length > 1)
            {
                var app = args[1];

                var param = "";

                if (args.Length > 2)
                    param = string.Join(" ", args.Skip(2));

                ProcessHandler.RunEXE(app, param, true);
                return true;
            }

            if (args[0].Equals("runclient") && args.Length > 1)
            {
                var app = args[1];

                var param = "";

                if (args.Length > 2)
                    param = string.Join(" ", args.Skip(2));

                ProcessHandler.RunClientEXE(app, param, true);
                return true;
            }

            if (args[0].Equals("kill") && args.Length > 1)
            {
                ProcessHandler.Kill(args[1]);
                return true;
            }

            if (args[0].Equals("killexe") && args.Length > 1)
            {
                ProcessHandler.KillEXE(args[1]);
                return true;
            }

            if (args[0].Equals("killall") && args.Length > 1)
            {
                ProcessHandler.KillAll(args[1]);
                return true;
            }

            if (args[0].Equals("killallexe") && args.Length > 1)
            {
                ProcessHandler.KillAllEXE(args[1]);
                return true;
            }

            return false;
        }

        private static void Help()
        {
            Log.WriteLine("Available commands");
            Log.WriteLine("--> run [PATH] [Param ...]");
            Log.WriteLine("--> runexe [PATH] [Param ...]");
            Log.WriteLine("--> runclient [NAME] [Param ...]");
            Log.WriteLine("--> kill [NAME]");
            Log.WriteLine("--> killexe [NAME]");
            Log.WriteLine("--> killall [NAME]");
            Log.WriteLine("--> killallexe [NAME]");
        }
    }
}