using System.Runtime.InteropServices;

namespace W12Dash
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct VarBuf
	{
        //0..3
        [MarshalAs(UnmanagedType.I4)]
        public int tickCount;
        //4..7
        [MarshalAs(UnmanagedType.I4)]
        public int bufOffset;
        //8..15
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public int[] pad;
	}
}