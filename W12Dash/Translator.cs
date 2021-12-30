namespace W12Dash
{
    public class Translator
    {
        /// <summary>
        /// Show Deploy mode with text
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string MGUKDeployMode(float value)
        {
            switch (value)
            {
                case 0.0f: return "1 NODLY";
                case 1.0f: return "2 QUAL";
                case 2.0f: return "3 ATTCK";
                case 3.0f: return "4 BALCD";
                case 4.0f: return "5 BUILD";
                default: return "";
            }
        }

        /// <summary>
        /// 'R' 'N' or '1'-'8'
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Gear(int value)
        {
            switch (value)
            {
                case -1: return "R";
                case 0: return "N";
                default: return $"{value}";
            }
        }

        /// <summary>
        /// Laptime (e.g. '1:28.20' or '0:01.00')
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string LapTime(float value)
        {
            //e.g. 1:28.20 or 0:01.00 (if not yet available)
            return value > 0 ?
                $"{(int)value / 60}:{value % 60:00.00}".Replace(",", ".") : "0:01.00";
        }

        /// <summary>
        /// Delta to Laptime (e.g. '+00:21' or '-00:10')
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Delta(float value)
        {
            if (value > 0)
                return $"{value % 60:+00.00}".Replace(",", ".");
            else if (value < 0)
                return $"{value % 60:00.00}".Replace(",", ".");

            //show nothing if not yet available
            return "";
        }
    }
}