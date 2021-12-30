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
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace W12Dash
{
    class MemoryAccess
    {
        MemoryMappedViewAccessor accessor;
        IntPtr dataValidEvent;
        MemoryMappedFile irsdkMappedMemory;

        public bool IsConnected()
        {
            if (this.accessor != null)
                return true;

            var dataValidEvent = Event.OpenEvent(Event.EVENT_ALL_ACCESS | Event.EVENT_MODIFY_STATE, false, "Local\\IRSDKDataValidEvent");
            if (dataValidEvent == IntPtr.Zero)
                return false;

            MemoryMappedFile irsdkMappedMemory = null;
            try
            {
                irsdkMappedMemory = MemoryMappedFile.OpenExisting("Local\\IRSDKMemMapFileName");
            }
            catch
            {
            }

            if (irsdkMappedMemory == null)
                return false;

            var accessor = irsdkMappedMemory.CreateViewAccessor();
            if (accessor == null)
            {
                irsdkMappedMemory.Dispose();
                return false;
            }

            this.irsdkMappedMemory = irsdkMappedMemory;
            this.dataValidEvent = dataValidEvent;
            this.accessor = accessor;
            return true;
        }

        public bool WaitForData()
        {
            return Event.WaitForSingleObject(dataValidEvent, 1000) == 0;
        }

        public unsafe Data GetData()
        {
            var headers = accessor.AcquirePointer(ptr =>
            {
                var a = ReadHeader(ptr);
                var b = ReadVariableHeaders(a, ptr);
                return new { Header = a, VarHeaders = b };
            });

            if ((headers.Header.status & 1) == 0)
                return new Data(false);

            return ReadVariables(headers.Header, headers.VarHeaders);
        }

        unsafe iRacingHeader ReadHeader(byte* ptr)
        {
            return (iRacingHeader)Marshal.PtrToStructure(new IntPtr(ptr), typeof(iRacingHeader));
        }

        static readonly int size = Marshal.SizeOf(typeof(VarHeader));

        unsafe VarHeader[] ReadVariableHeaders(iRacingHeader header, byte* ptr)
        {
            var varHeaders = new VarHeader[header.numVars];
            ptr += header.varHeaderOffset;

            for (var i = 0; i < header.numVars; i++)
            {
                varHeaders[i] = (VarHeader)Marshal.PtrToStructure(new IntPtr(ptr), typeof(VarHeader));
                ptr += size;
            }

            return varHeaders;
        }

        unsafe Data ReadVariables(iRacingHeader header, VarHeader[] varHeaders)
        {
            var buf = header.varBuf;
            var values = ReadAllValues(accessor, buf.bufOffset, varHeaders);
            var latestHeader = accessor.AcquirePointer(ptr => ReadHeader(ptr));
            return values;
        }

        static Data ReadAllValues(MemoryMappedViewAccessor accessor, int buffOffset, VarHeader[] varHeaders)
        {
            var data = new Data();

            var maps = new Dictionary<VarType, Func<int, object>>() {
                { VarType.Int, (offset) => accessor.ReadInt32(offset) },
                { VarType.BitField, (offset) => accessor.ReadInt32(offset) },
                { VarType.Double, (offset) => accessor.ReadDouble(offset) },
                { VarType.Bool, (offset) => accessor.ReadBoolean(offset) },
                { VarType.Float, (offset) => accessor.ReadSingle(offset) }
            };

            var arryMaps = new Dictionary<VarType, Func<int, int, object>>() {
                { VarType.Int, (size, offset) => GetArrayData<int>(accessor, size, offset) },
                { VarType.BitField, (size, offset) => GetArrayData<int>(accessor, size, offset) },
                { VarType.Double, (size, offset) => GetArrayData<double>(accessor, size, offset) },
                { VarType.Float, (size, offset) => GetArrayData<float>(accessor, size, offset) },
                { VarType.Bool, (size, offset) => GetArrayData<bool>(accessor, size, offset) }
            };

            for (var i = 0; i < varHeaders.Length; i++)
            {
                var varHeader = varHeaders[i];
                var offset = buffOffset + varHeader.offset;

                if (varHeader.type == VarType.Char)
                    throw new NotSupportedException();

                object value;
                if (varHeader.count != 1)
                    value = arryMaps[varHeader.type](varHeader.count, offset);
                else
                    value = maps[varHeader.type](offset);

                data.Add(varHeader.name, value);
            }

            return data;
        }

        static T[] GetArrayData<T>(MemoryMappedViewAccessor accessor, int size, int offset) where T : struct
        {
            var data = new T[size];
            accessor.ReadArray<T>(offset, data, 0, size);
            return data;
        }
    }
}