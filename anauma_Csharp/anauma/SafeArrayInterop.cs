using System.Runtime.InteropServices;

namespace anauma;

internal static class SafeArrayInterop
{
    [DllImport(
        "OleAut32.dll",
        CallingConvention = CallingConvention.StdCall)]
    private static extern uint SafeArrayDestroy(
        IntPtr pSafeArray);

    [DllImport(
        "OleAut32.dll",
        CallingConvention = CallingConvention.StdCall)]
    private static extern uint SafeArrayAccessData(
        IntPtr pSafeArray,
        ref IntPtr ppvData);

    [DllImport(
        "OleAut32.dll",
        CallingConvention = CallingConvention.StdCall)]
    private static extern uint SafeArrayUnaccessData(
        IntPtr pSafeArray);

    public static byte[] CopyToByteArray(
        object objBuff,
        int length)
    {
        IntPtr safeArrayPointer =
            objBuff is IntPtr pointer
                ? pointer
                : new IntPtr(Convert.ToInt64(objBuff));

        byte[] data = new byte[length];
        bool accessed = false;

        try
        {
            IntPtr dataPointer = IntPtr.Zero;

            uint hr = SafeArrayAccessData(
                safeArrayPointer,
                ref dataPointer);

            if (hr != 0)
            {
                throw new InvalidOperationException(
                    $"SafeArrayAccessDataエラー: {hr}");
            }

            accessed = true;

            Marshal.Copy(
                dataPointer,
                data,
                0,
                length);

            return data;
        }
        finally
        {
            if (accessed)
            {
                SafeArrayUnaccessData(
                    safeArrayPointer);
            }

            if (safeArrayPointer != IntPtr.Zero)
            {
                SafeArrayDestroy(
                    safeArrayPointer);
            }
        }
    }
}
