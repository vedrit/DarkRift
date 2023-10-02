using Godot;
using System.Collections.Generic;
using System;

namespace DarkRift.Server.Godot
{
    public sealed class GodotConsoleWriter : LogWriter
    {
        public override Version Version
        {
            get
            {
                return new Version(1, 0, 0);
            }
        }

        public GodotConsoleWriter(LogWriterLoadData pluginLoadData) : base(pluginLoadData)
        {
        }

        public override void WriteEvent(WriteEventArgs args)
        {
            switch (args.LogType)
            {
                case LogType.Trace:
                case LogType.Info:
                    GD.Print(args.FormattedMessage);
                    break;
                case LogType.Warning:
                    GD.PushWarning(args.FormattedMessage);
                    break;
                case LogType.Error:
                case LogType.Fatal:
                    GD.PushError(args.FormattedMessage);
                    break;
            }
        }
    }
}
