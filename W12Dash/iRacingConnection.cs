namespace W12Dash
{
    public class iRacingConnection
    {
        MemoryAccess memoryAccess;

        public iRacingConnection()
        {
            this.memoryAccess = new MemoryAccess();
        }

        public Data QueryData()
        {
            while (!memoryAccess.IsConnected())
                System.Threading.Thread.Sleep(100);

            memoryAccess.WaitForData();
            Data data = null;

            while(data == null)
                data = memoryAccess.GetData();

            if (!data.IsConnected)
                return QueryData();

            return data;
        }
    }
}