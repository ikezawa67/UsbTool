using System.Runtime.InteropServices;

namespace UsbTool.Tools;

class EjectTool
{
    const uint GENERIC_READ = 0x80000000; // 読み取りアクセス権
    const uint GENERIC_WRITE = 0x40000000; // 書き込みアクセス権

    const int FILE_SHARE_READ = 0x1; // 読み取りアクセス権
    const int FILE_SHARE_WRITE = 0x2; // 書き込みアクセス権

    const int FSCTL_LOCK_VOLUME = 0x00090018; // ボリュームのロックをする制御コード
    const int FSCTL_DISMOUNT_VOLUME = 0x00090020; // マウントの強制的に解除する制御コード
    const int IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808; // SCSI デバイスからメディアを取り出す制御コード
    const int IOCTL_STORAGE_MEDIA_REMOVAL = 0x002D4804; // メディアを取り出すメカニズムを有効または無効にする制御コード

    /// <summary>
    /// ファイルまたはI/Oデバイスを作成または開く
    /// </summary>
    /// <param name="lpFileName">作成または開くファイルまたはデバイスの名前</param>
    /// <param name="dwDesiredAccess">ファイルまたはデバイスへの要求されたアクセス</param>
    /// <param name="dwShareMode">ファイルまたはデバイスの要求された共有モード</param>
    /// <param name="lpSecurityAttributes"> SECURITY_ATTRIBUTES構造体へのポインタ</param>
    /// <param name="dwCreationDisposition">存在または存在しないファイルまたはデバイスに対して実行するアクション</param>
    /// <param name="dwFlagsAndAttributes">ファイルまたはデバイスの属性とフラグ</param>
    /// <param name="hTemplateFile">GENERIC_READアクセス権を持つテンプレート ファイルへの有効なハンドル</param>
    /// <returns>指定されたファイルまたはデバイスへのハンドルのポインタ</returns>
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)] // DllImportを使用して、Win32 CreateFile 関数をインポート
    private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
  
    /// <summary>
    /// 制御コードをデバイスドライバーに送信し操作を実行する
    /// </summary>
    /// <param name="hDevice">操作を実行するデバイスへのハンドルのポインタ</param>
    /// <param name="dwIoControlCode">制御コード</param>
    /// <param name="lpInBuffer">操作の実行に必要なデータを格納している入力バッファーへのポインタ</param>
    /// <param name="nInBufferSize">入力バッファーのサイズ</param>
    /// <param name="lpOutBuffer">操作によって返されるデータを受け取る出力バッファーへのポインタ</param>
    /// <param name="nOutBufferSize">出力バッファーのサイズ</param>
    /// <param name="lpBytesReturned">出力バッファーに格納されているデータのサイズをバイト単位で受け取る変数へのポインタ</param>
    /// <param name="lpOverlapped">OVERLAPPED構造体へのポインタ</param>
    /// <returns>操作が正常に完了したかの真理値</returns>
    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)] // DllImportを使用して、Win32 DeviceIoControl 関数をインポート
    private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);
    

    /// <summary>
    /// ファイルまたはI/Oデバイスを作成または開く
    /// </summary>
    /// <param name="hDevice">操作を実行するデバイスへのハンドルのポインタ</param>
    /// <param name="dwIoControlCode">制御コード</param>
    /// <param name="lpInBuffer">操作の実行に必要なデータを格納している入力バッファー配列</param>
    /// <param name="nInBufferSize">入力バッファーのサイズ</param>
    /// <param name="lpOutBuffer">操作によって返されるデータを受け取る出力バッファーへのポインタ</param>
    /// <param name="nOutBufferSize">出力バッファーのサイズ</param>
    /// <param name="lpBytesReturned">出力バッファーに格納されているデータのサイズをバイト単位で受け取る変数へのポインタ</param>
    /// <param name="lpOverlapped">OVERLAPPED構造体へのポインタ</param>
    /// <returns>操作が正常に完了したかの真理値</returns>
    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)] // DllImportを使用して、Win32 DeviceIoControl 関数をインポート
    private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, byte[] lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

    /// <summary>
    /// 開いているハンドルを閉じ
    /// </summary>
    /// <param name="hObject">閉じるハンドルのポインタ</param>
    /// <returns></returns>
    [DllImport("kernel32.dll", SetLastError = true)] // DllImportを使用して、Win32 CloseHandle 関数をインポート
    private static extern bool CloseHandle(IntPtr hObject);

    /// <summary>
    /// 指定されたドライブレターが設定されているドライブの接続を解除する
    /// </summary>
    /// <param name="driveLetter">ドライブレター</param>
    /// <returns>接続がが正常に解除できたかの論理値</returns>
    public static bool Eject(string driveLetter)
    {
        driveLetter = driveLetter[0] + ":"; // 渡されたdriveLetterの0番目の文字を取得し、確実にドライブレターをなるように処理をする
        IntPtr handle = CreateFile(driveLetter, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, 0x3, 0, IntPtr.Zero);
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
        for (int i = 0; i < 5; i++) // 使用していない判定になるまでにラグがあるため5回繰り返す
        {
            if (DeviceIoControl(handle, FSCTL_LOCK_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, out uint byteReturned, IntPtr.Zero))
            {
                return true;
            }
            Thread.Sleep(500); // 500ミリ秒間スレッドを中断する
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
        byte[] buf = new byte[1]; // サイズ1のバッファーを宣言する
        buf[0] = (prevent) ? (byte)1 : (byte)0; // 宣言したバッファーにpreventの真理値から値を判別して代入する
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
