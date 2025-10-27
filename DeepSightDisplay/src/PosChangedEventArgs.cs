using OpenCvSharp;
using System;

namespace DeepSightDisplay
{
    public class PosChangedEventArgs : EventArgs
    {
        public Point Pos { get; protected set; }

        public object[] ChannelValues { get; protected set; }

        public PosChangedEventArgs(Point pos, object[] channelsValue)
        {
            Pos = pos;
            ChannelValues = channelsValue;
        }
    }
}