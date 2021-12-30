using System.Runtime.InteropServices;

namespace W12Dash
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	internal struct VarHeader
	{
		//0..3
		public VarType type;
		//4..7
		public int offset;
		//8..11
		public int count;
        //12..15
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public int[] pad;
		//16..47
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string name;
		//48..111
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string desc;
		//112..143
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string unit;
	}	
}
