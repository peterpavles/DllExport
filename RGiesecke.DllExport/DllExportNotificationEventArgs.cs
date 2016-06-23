﻿// [Decompiled] Assembly: RGiesecke.DllExport, Version=1.2.2.23706, Culture=neutral, PublicKeyToken=ad5f9f4a55b5020b
// Author of original assembly (MIT-License): Robert Giesecke
// Use Readme & LICENSE files for details.

using System;

namespace RGiesecke.DllExport
{
    public class DllExportNotificationEventArgs: EventArgs
    {
        public object Context
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public int Severity
        {
            get;
            set;
        }

        public string Code
        {
            get;
            set;
        }

        public string FileName
        {
            get;
            set;
        }

        public SourceCodePosition? StartPosition
        {
            get;
            set;
        }

        public SourceCodePosition? EndPosition
        {
            get;
            set;
        }
    }
}
