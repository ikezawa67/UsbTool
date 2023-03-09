namespace UsbTool;

class Program
{
    private static bool taskEnd = false; // タスクの終了判定を行う真理値
    private static List<Task> tasks = new List<Task> { }; // コピー処理を行っているタスクのリスト
    private static List<DriveInfo> usbDriveInfos = new List<DriveInfo> { }; // コピー処理を行っているドライブ情報のリスト

    /// <summary>
    /// キー入力が在った場合にタスクの終了判定用変数にtrueを代入する
    /// </summary>
    private static TimerCallback callback = state =>
    {
        try
        {
            if (Console.KeyAvailable) // キー入力判別
            {
                taskEnd = true;
            }
            return;
        }
        catch (Exception e) when (e is IOException || e is InvalidOperationException)
        {
            return;
        }
    };

    static void Main(string[] args)
    {
        string currentDirectory = Directory.GetCurrentDirectory(); // コピー元のディレクトリーとなるカレントディレクトリーを取得する
        Console.WriteLine("コピー元 : {0}", currentDirectory);
        Timer timer = new Timer(callback, null, 0, 500); // 500ミリ秒間隔でタスクの終了判定を行う
        do
        {
            foreach (DriveInfo driveInfo in DriveInfo.GetDrives()) // 接続されているドライブ情報を全て取得し反復処理を行う
            {                
                DriveInfo? tmp = usbDriveInfos.Where(d => d.Name == driveInfo.Name).FirstOrDefault(); // usbDriveInfos内に存在しないドライブレターを持つドライブか判別する際に使用する一時変数
                if (tmp is null && driveInfo.IsReady && driveInfo.DriveType == DriveType.Removable && driveInfo.VolumeLabel == "USB DISK") // 一時変数がnullで準備可能なUSB DISKというボリュームラベルのリムーバブルディスクか判別する
                {
                    usbDriveInfos.Add(driveInfo); // usbDriveInfosにdriveInfoを追加し、同じドライブへのコピーを行わないようにする
                    Task task = Task.Run(() =>
                    {
                        Console.WriteLine("{0}のコピーを開始しました。", driveInfo.Name);
                        Tools.CopyTool.Copy(currentDirectory, driveInfo.Name);
                        if (Tools.EjectTool.Eject(driveInfo.Name)) // ドライブの接続が正常に解除できたか判別する
                        {
                            Console.WriteLine("{0}のコピーおよび取り外しが正常に終了しました。", driveInfo.Name);
                        }
                        else
                        {
                            Console.WriteLine("{0}の取り外しが正常に終了しませんでした。", driveInfo.Name);
                        }
                        usbDriveInfos.Remove(driveInfo); // usbDriveInfosからdriveInfoを削除する
                    });
                    tasks.Add(task); // tasksにtaskを追加する
                }
            }
            Thread.Sleep(1000); // 1000ミリ秒間スレッドを中断する
        } while (!taskEnd); // taskEndがfalseの間繰り返す
        Console.WriteLine("終了処理中です。");
        timer.Dispose(); // 終了判定用タイマーを解放する
        Task.WaitAll(tasks.ToArray()); // tasksを配列に変換し全てのタスクが終了するまで待つ
        Console.WriteLine("正常に終了しました。");
    }
}