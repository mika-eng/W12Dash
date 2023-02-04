// This file is part of iRacingSDK.
//
// Copyright 2014 Dean Netherton
// https://github.com/vipoo/iRacingSDK.Net
//
// iRacingSDK is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// iRacingSDK is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with iRacingSDK.  If not, see <http://www.gnu.org/licenses/>.

using System.Runtime.InteropServices;

namespace W12Dash.iRacingSDK
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class Header
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