using System.Collections.Generic;

namespace W12Dash
{
    public class Data : Dictionary<string, object>
    {
        public bool IsConnected { get; internal set; }

        public Data(bool isConnected = true)
        {
            IsConnected = isConnected;
        }

        public T Get<T>(string telemetryName)
        {
            if (this.ContainsKey(telemetryName))
                return (T)this[telemetryName];

            return default(T);
        }
    }
}