namespace W12Dash.iRacingSDK
{
    public class Connection
    {
        private readonly MemoryAccess _memoryAccess;

        public Connection()
        {
            _memoryAccess = new MemoryAccess();
        }

        public Data QueryData()
        {
            while (!_memoryAccess.IsConnected())
                System.Threading.Thread.Sleep(100);

            _memoryAccess.WaitForData();
            Data data = null;

            while(data == null)
                data = _memoryAccess.GetData();

            return !data.IsConnected ? QueryData() : data;
        }
    }
}