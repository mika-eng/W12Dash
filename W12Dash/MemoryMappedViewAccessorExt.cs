namespace W12Dash
{
    public static class MemoryMappedViewAccessorExt
    {
        public unsafe delegate T MyFn<T>(byte* ptr);

        public unsafe static T AcquirePointer<T>(this System.IO.MemoryMappedFiles.MemoryMappedViewAccessor self, MyFn<T> fn)
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