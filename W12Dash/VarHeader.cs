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
