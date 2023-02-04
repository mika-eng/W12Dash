namespace W12Dash
{
    public abstract class Translator
    {
        private const string NO_DEPLOY = "1 NODLY";
        private const string QUALIFY = "2 QUAL";
        private const string ATTACK = "3 ATTCK";
        private const string BALANCED = "4 BALCD";
        private const string BUILD = "5 BUILD";
        private const string GEAR_R = "R";
        private const string GEAR_N = "N";


        /// <summary>
        /// Show Deploy mode with text
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DeployMode(float value)
        {
            switch (value)
            {
                case 0.0f: return NO_DEPLOY;
                case 1.0f: return QUALIFY;
                case 2.0f: return ATTACK;
                case 3.0f: return BALANCED;
                case 4.0f: return BUILD;
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
                case -1: return GEAR_R;
                case 0: return GEAR_N;
                default: return $"{value}";
            }
        }

        /// <summary>
        /// LapTime (e.g. '1:28.20' or '0:01.00')
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string LapTime(float value)
        {
            //e.g. 1:28.20 or 0:01.00 (if not yet available)
            return value > 0 ? $"{(int)value / 60}:{value % 60:00.00}".Replace(",", ".") : "0:01.00";
        }

        /// <summary>
        /// Delta to LapTime (e.g. '+00:21' or '-00:10')
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Delta(float value)
        {
            if (value > 0) return $"{value % 60:+00.00}".Replace(",", ".");
            
            return value < 0 ? $"{value % 60:00.00}".Replace(",", ".") :
                //show nothing if not yet available
                "";
        }
    }
}