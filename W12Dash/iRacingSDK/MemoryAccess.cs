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

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace W12Dash.iRacingSDK
{
    internal class MemoryAccess
    {
        private MemoryMappedViewAccessor _accessor;
        private IntPtr _dataValidEvent;

        public bool IsConnected()
        {
            if (_accessor != null)
                return true;

            var dataValidEvent = Event.OpenEvent(Event.EVENT_ALL_ACCESS | Event.EVENT_MODIFY_STATE, false, "Local\\IRSDKDataValidEvent");
            if (dataValidEvent == IntPtr.Zero)
                return false;

            MemoryMappedFile irSdkMappedMemory = null;
            try
            {
                irSdkMappedMemory = MemoryMappedFile.OpenExisting("Local\\IRSDKMemMapFileName");
            }
            catch
            {
                // ignored
            }

            if (irSdkMappedMemory == null)
                return false;

            var accessor = irSdkMappedMemory.CreateViewAccessor();

            _dataValidEvent = dataValidEvent;
            _accessor = accessor;
            return true;
        }

        public bool WaitForData()
        {
            return Event.WaitForSingleObject(_dataValidEvent, 1000) == 0;
        }

        public unsafe Data GetData()
        {
            var headers = _accessor.AcquirePointer(ptr =>
            {
                var a = ReadHeader(ptr);
                var b = ReadVariableHeaders(a, ptr);
                return new { Header = a, VarHeaders = b };
            });

            return (headers.Header.status & 1) == 0 ? new Data(false) : ReadVariables(headers.Header, headers.VarHeaders);
        }

        unsafe Header ReadHeader(byte* ptr)
        {
            return (Header)Marshal.PtrToStructure(new IntPtr(ptr), typeof(Header));
        }

        private static readonly int Size = Marshal.SizeOf(typeof(VarHeader));

        private static unsafe VarHeader[] ReadVariableHeaders(Header header, byte* ptr)
        {
            var varHeaders = new VarHeader[header.numVars];
            ptr += header.varHeaderOffset;

            for (var i = 0; i < header.numVars; i++)
            {
                varHeaders[i] = (VarHeader)Marshal.PtrToStructure(new IntPtr(ptr), typeof(VarHeader));
                ptr += Size;
            }

            return varHeaders;
        }

        private unsafe Data ReadVariables(Header header, VarHeader[] varHeaders)
        {
            var buf = header.varBuf;
            var values = ReadAllValues(_accessor, buf.bufOffset, varHeaders);
            _accessor.AcquirePointer(ReadHeader);
            return values;
        }

        private static Data ReadAllValues(UnmanagedMemoryAccessor accessor, int buffOffset, VarHeader[] varHeaders)
        {
            var data = new Data();

            var maps = new Dictionary<VarType, Func<int, object>>() {
                { VarType.Int, (offset) => accessor.ReadInt32(offset) },
                { VarType.BitField, (offset) => accessor.ReadInt32(offset) },
                { VarType.Double, (offset) => accessor.ReadDouble(offset) },
                { VarType.Bool, (offset) => accessor.ReadBoolean(offset) },
                { VarType.Float, (offset) => accessor.ReadSingle(offset) }
            };

            var arrayMaps = new Dictionary<VarType, Func<int, int, object>>() {
                { VarType.Int, (size, offset) => GetArrayData<int>(accessor, size, offset) },
                { VarType.BitField, (size, offset) => GetArrayData<int>(accessor, size, offset) },
                { VarType.Double, (size, offset) => GetArrayData<double>(accessor, size, offset) },
                { VarType.Float, (size, offset) => GetArrayData<float>(accessor, size, offset) },
                { VarType.Bool, (size, offset) => GetArrayData<bool>(accessor, size, offset) }
            };

            foreach (var varHeader in varHeaders)
            {
                var offset = buffOffset + varHeader.offset;

                if (varHeader.type == VarType.Char)
                    throw new NotSupportedException();

                var value = varHeader.count != 1 ? arrayMaps[varHeader.type](varHeader.count, offset) : maps[varHeader.type](offset);

                data.Add(varHeader.name, value);
            }

            return data;
        }

        private static T[] GetArrayData<T>(UnmanagedMemoryAccessor accessor, int size, int offset) where T : struct
        {
            var data = new T[size];
            accessor.ReadArray(offset, data, 0, size);
            return data;
        }
    }
}