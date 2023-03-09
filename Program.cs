using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UsbTool;

class Program
{
    private static bool taskEnd = false;
    private static List<Task> tasks = new List<Task> { };
    private static List<DriveInfo> usbDriveInfos = new List<DriveInfo> { };

    private static TimerCallback callback = state =>
    {
        try
        {
            if (!taskEnd && Console.KeyAvailable) taskEnd = true;
            return;
        }
        catch
        {
            return;
        }
    };

    static void Main(string[] args)
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string sourceDirName = String.Format(@"\\?\{0}", currentDirectory);
        Console.WriteLine("コピー元 : {0}", currentDirectory);
        Timer timer = new Timer(callback, null, 0, 500);
        do
        {
            foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
            {
                DriveInfo? tmp = usbDriveInfos.Where(d => d.Name == driveInfo.Name).FirstOrDefault();
                if (tmp is null && driveInfo.IsReady && driveInfo.DriveType == DriveType.Removable && driveInfo.VolumeLabel == "USB DISK")
                {
                    usbDriveInfos.Add(driveInfo);
                    Task task = Task.Run(() =>
                    {
                        Console.WriteLine("{0}のコピーを開始しました。", driveInfo.Name);
                        Tools.CopyTool.Copy(sourceDirName, String.Format(@"\\?\{0}", driveInfo.Name));
                        if (Tools.EjectTool.Eject(driveInfo.Name))
                        {

                            Console.WriteLine("{0}のコピーおよび取り外しが正常に終了しました。", driveInfo.Name);
                        }
                        else
                        {
                            Console.WriteLine("{0}の取り外しが正常に終了しませんでした。", driveInfo.Name);
                        }
                        usbDriveInfos.Remove(driveInfo);
                    });
                    tasks.Add(task);
                }
            }
            Thread.Sleep(1000);
        } while (!taskEnd);
        Console.WriteLine("終了処理中です。");
        timer.Dispose();
        Task.WaitAll(tasks.ToArray());
        Console.WriteLine("正常に終了しました。");
        try
        {
            Console.ReadKey(true);
            return;
        }
        catch
        {
            return;
        }
    }
}