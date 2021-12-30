using System.Runtime.InteropServices;

namespace W12Dash
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class iRacingHeader
    {
		//0..3
		public int ver;
        //4..7
		public int status;
        //8..11
		public int tickRate;
		//12..15
		public int sessionInfoUpdate;
        //16..19
		public int sessionInfoLen;
        //20..23
		public int sessionInfoOffset;
		//24..27
		public int numVars;
        //28..31
		public int varHeaderOffset;
		//32..35
		public int numBuf;
        //36..39
		public int bufLen;
        //40..47
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public int[] pad;
		//48..63
		public VarBuf varBuf;
	}
}