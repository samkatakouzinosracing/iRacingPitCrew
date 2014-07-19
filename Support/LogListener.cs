﻿// This file is part of iRacingPitCrew Application.
//
// Copyright 2014 Dean Netherton
// https://github.com/vipoo/iRacingPitCrew.Net
//
// iRacingPitCrew is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// iRacingPitCrew is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with iRacingPitCrew.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace iRacingPitCrew.Support
{
    public class LogListener : TraceListener
    {
        static LogListener logFile;
        ConcurrentQueue<string> items = new ConcurrentQueue<string>();
        bool isCancelled = false;
        Task task;
        string lastMessage = null;
        DateTime lastTime = DateTime.Now;

        public string FileName { get; internal set; }

        LogListener(string filename)
        {
            this.FileName = filename;

            task = Task.Factory.StartNew(Writer);

            Write("\r\n");
            WriteLine("----------------------------");
        }

        void Writer()
        {
            while (!isCancelled)
            {
                string message;
                if (items.TryDequeue(out message))
                    File.AppendAllText(FileName, message);

                Thread.Sleep(1);
            }
        }

        protected override void Dispose(bool disposing)
        {
            isCancelled = true;

            base.Dispose(disposing);
        }

        public override void Write(string message)
        {
            items.Enqueue(message);
        }

        public override void WriteLine(string message)
        {
            var now = DateTime.Now;
            if (message == lastMessage && now - lastTime < TimeSpan.FromSeconds(5))
                return;

            lastMessage = message;
            lastTime = now;

            this.Write(now.ToString("s"));
            this.Write("\t");
            this.Write(message + "\r\n");
        }

        internal static void ToFile(string filename)
        {
            if (logFile != null)
            {
                Trace.WriteLine(string.Format("Moving logging to file {0}", filename), "INFO");
                Trace.Listeners.Remove(logFile);
                logFile.Dispose();
            }

            logFile = new LogListener(filename);
            Trace.Listeners.Add(logFile);
        }
    }
}
