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

namespace W12Dash.iRacingSDK
{
    public static class MemoryMappedViewAccessorExt
    {
        public unsafe delegate T MyFn<out T>(byte* ptr);

        public static unsafe T AcquirePointer<T>(this System.IO.MemoryMappedFiles.MemoryMappedViewAccessor self, MyFn<T> fn)
        {
            byte* ptr = null;
            self.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
            try
            {
                return fn(ptr);
            }
            finally
            {
                self.SafeMemoryMappedViewHandle.ReleasePointer();
            }
        }
    }
}