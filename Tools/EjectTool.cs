using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace UsbTool.Tools;

class EjectTool
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr SecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, byte[] lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    const uint GENERIC_READ = 0x80000000;
    const uint GENERIC_WRITE = 0x40000000;
    const int FILE_SHARE_READ = 0x1;
    const int FILE_SHARE_WRITE = 0x2;
    const int FSCTL_LOCK_VOLUME = 0x00090018;
    const int FSCTL_DISMOUNT_VOLUME = 0x00090020;
    const int IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;
    const int IOCTL_STORAGE_MEDIA_REMOVAL = 0x002D4804;

    /// <summary>
    /// 指定されたドライブレターが設定されているドライブの接続を解除する
    /// </summary>
    /// <param name="driveLetter">ドライブレター</param>
    /// <returns>接続がが正常に解除できたかの論理値</returns>
    public static bool Eject(string driveLetter)
    {
        string filename = @"\\.\" + driveLetter[0] + ":";
        IntPtr handle = CreateFile(filename, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, 0x3, 0, IntPtr.Zero);
        bool result = false;
        if (LockVolume(handle))
        {
            result = DismountVolume(handle) && PreventRemovalOfVolume(handle, false) && AutoEjectVolume(handle);
        }
        CloseHandle(handle);
        return result;
    }

    /// <summary>
    /// ボリュームが使用中でない場合はボリュームをロックする
    /// </summary>
    /// <param name="handle">デバイスへのハンドル</param>
    /// <returns>操作が正常に完了できたかの論理値</returns>
    private static bool LockVolume(IntPtr handle)
    {
        for (int i = 0; i < 10; i++)
        {
            if (DeviceIoControl(handle, FSCTL_LOCK_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, out uint byteReturned, IntPtr.Zero))
            {
                return true;
            }
            Thread.Sleep(500);
        }
        return false;
    }

    /// <summary>
    /// ボリュームのマウントの強制解除
    /// </summary>
    /// <param name="handle">デバイスへのハンドル</param>
    /// <returns>操作が正常に完了できたかの論理値</returns>
    private static bool DismountVolume(IntPtr handle)
    {
        return DeviceIoControl(handle, FSCTL_DISMOUNT_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, out uint byteReturned, IntPtr.Zero);
    }

    /// <summary>
    /// メディアを取り出すメカニズムを有効または無効にする
    /// </summary>
    /// <param name="handle">デバイスへのハンドル</param>
    /// <param name="prevent">メカニズムを有効または無効</param>
    /// <returns>操作が正常に完了できたかの論理値</returns>
    private static bool PreventRemovalOfVolume(IntPtr handle, bool prevent)
    {
        byte[] buf = new byte[1];
        buf[0] = (prevent) ? (byte)1 : (byte)0;
        return DeviceIoControl(handle, IOCTL_STORAGE_MEDIA_REMOVAL, buf, 1, IntPtr.Zero, 0, out uint retVal, IntPtr.Zero);
    }

    /// <summary>
    /// デバイスからメディアを取り出す
    /// </summary>
    /// <param name="handle">デバイスへのハンドル</param>
    /// <returns>操作が正常に完了できたかの論理値</returns>
    private static bool AutoEjectVolume(IntPtr handle)
    {
        return DeviceIoControl(handle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, out uint byteReturned, IntPtr.Zero);
    }
}
