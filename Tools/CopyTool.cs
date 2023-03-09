using System.IO;

namespace UsbTool.Tools;

class CopyTool
{

    /// <summary>
    /// 比較先のディレクトリー構造を比較元のディレクトリー構造を同じにする
    /// </summary>
    /// <param name="sourceDir">比較元のディレクトリーのパス</param>
    /// <param name="destinationDir">比較先のディレクトリーのパス</param>
    public static void Copy(string sourceDir, string destinationDir)
    {
        DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(sourceDir);
        DirectoryInfo destinationDirectoryInfo = new DirectoryInfo(destinationDir);
        DirectoryDelete(sourceDirectoryInfo, destinationDirectoryInfo);
        DirectoryCopy(sourceDirectoryInfo, destinationDirectoryInfo);
    }

    /// <summary>
    /// 指定された二つのディレクトリー構造を比較し余分なファイルとディレクトリーの削除をする
    /// </summary>
    /// <param name="sourceDirectoryInfo">比較元のディレクトリー情報</param>
    /// <param name="destinationDirectoryInfo">比較先のディレクトリー情報</param>
    private static void DirectoryDelete(DirectoryInfo sourceDirectoryInfo, DirectoryInfo destinationDirectoryInfo)
    {
        foreach (FileInfo destinationFileInfo in destinationDirectoryInfo.GetFiles())
        {
            FileInfo sourceFileInfo = new FileInfo(sourceDirectoryInfo.FullName + @"\" + destinationFileInfo.Name);
            if (!sourceFileInfo.Exists)
            {
                destinationFileInfo.Attributes = FileAttributes.Normal;
                destinationFileInfo.Delete();
            }
        }
        foreach (DirectoryInfo directoryInfo in destinationDirectoryInfo.GetDirectories()) DirectoryDelete(new DirectoryInfo(sourceDirectoryInfo.FullName + @"\" + directoryInfo.Name), directoryInfo);
        if (!sourceDirectoryInfo.Exists)
        {
            destinationDirectoryInfo.Attributes = FileAttributes.Normal;
            destinationDirectoryInfo.Delete();
        }
    }

    /// <summary>
    /// 指定された二つのディレクトリー構造を比較し足りないファイルとディレクトリーのコピーをする
    /// 最後の書き込み時間が古いファイルもコピーする
    /// </summary>
    /// <param name="sourceDirectoryInfo">比較元のディレクトリー情報</param>
    /// <param name="destinationDirectoryInfo">比較先のディレクトリー情報</param>
    private static void DirectoryCopy(DirectoryInfo sourceDirectoryInfo, DirectoryInfo destinationDirectoryInfo)
    {
        if (!destinationDirectoryInfo.Exists)
        {
            destinationDirectoryInfo.Create();
        }
        if (destinationDirectoryInfo.FullName != destinationDirectoryInfo.Root.FullName)
        {
            destinationDirectoryInfo.Attributes = sourceDirectoryInfo.Attributes;
        }
        foreach (FileInfo sourceFileInfo in sourceDirectoryInfo.GetFiles())
        {
            FileInfo destinationFileInfo = new FileInfo(destinationDirectoryInfo.FullName + @"\" + sourceFileInfo.Name);
            if (!destinationFileInfo.Exists)
            {
                File.Copy(sourceFileInfo.FullName, destinationFileInfo.FullName, true);
                destinationFileInfo.Attributes = sourceFileInfo.Attributes;
            }
            else if (destinationFileInfo.Exists && sourceFileInfo.LastWriteTime != destinationFileInfo.LastWriteTime)
            {
                destinationFileInfo.Attributes = FileAttributes.Normal;
                File.Copy(sourceFileInfo.FullName, destinationFileInfo.FullName, true);
                destinationFileInfo.Attributes = sourceFileInfo.Attributes;
            }
            else if (destinationFileInfo.Attributes != sourceFileInfo.Attributes)
            {
                destinationFileInfo.Attributes = sourceFileInfo.Attributes;
            }
        }
        foreach (DirectoryInfo directoryInfo in sourceDirectoryInfo.GetDirectories())
        {
            if (destinationDirectoryInfo.FullName == destinationDirectoryInfo.Root.FullName)
            {
                DirectoryCopy(directoryInfo, new DirectoryInfo(destinationDirectoryInfo.FullName + directoryInfo.Name));
            }
            else
            {
                DirectoryCopy(directoryInfo, new DirectoryInfo(destinationDirectoryInfo.FullName + @"\" + directoryInfo.Name));
            }
        }
    }
}
