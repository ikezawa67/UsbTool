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
        DirectoryInfo source = new DirectoryInfo(sourceDir);
        DirectoryInfo destination = new DirectoryInfo(destinationDir);
        DirectoryDelete(source, destination);
        DirectoryCopy(source, destination);
    }

    /// <summary>
    /// 指定された二つのディレクトリー構造を比較し余分なファイルとディレクトリーを強制的に削除をする
    /// </summary>
    /// <param name="source">比較元のディレクトリー情報</param>
    /// <param name="destination">比較先のディレクトリー情報</param>
    private static void DirectoryDelete(DirectoryInfo source, DirectoryInfo destination)
    {
        foreach (FileInfo fileInfo in destination.GetFiles()) // destinationの下部にあるファイル情報を全て取得し反復処理を行う
        {
            string sourceFilePath = Path.Combine(source.FullName, fileInfo.Name); // 比較元のディレクトリの絶対パスに比較先のファイル名を結合する
            FileInfo sourceFile = new FileInfo(sourceFilePath); // 結合したパスからFileInfoのインスタンスを作成する
            if (!sourceFile.Exists)
            {
                fileInfo.Attributes = FileAttributes.Normal; // ファイル属性を標準に変更する
                fileInfo.Delete();
            }
        }
        foreach (DirectoryInfo directoryInfo in destination.GetDirectories()) // destinationの下部にあるディレクトリ情報を全て取得し反復処理を行う
        {
            string sourceDirectoryPath = Path.Combine(source.FullName, directoryInfo.Name); // 比較元のディレクトリの絶対パスに比較先のディレクトリ名を結合する
            DirectoryDelete(new DirectoryInfo(sourceDirectoryPath), directoryInfo); // 結合したパスから作成したDirectoryInfoのインスタンスとdirectoryInfoを渡し再帰的に実行する
        }
        if (!source.Exists)
        {
            destination.Attributes = FileAttributes.Normal; // ディレクトリ属性を標準に変更する
            destination.Delete();
        }
    }

    /// <summary>
    /// 指定された二つのディレクトリー構造を比較し同じ構造になる様にファイルとディレクトリーのコピーをする
    /// 比較先にファイルが在った場合最終書き込み時刻を比較し異なる場合のみコピーする
    /// </summary>
    /// <param name="source">比較元のディレクトリー情報</param>
    /// <param name="destination">比較先のディレクトリー情報</param>
    private static void DirectoryCopy(DirectoryInfo source, DirectoryInfo destination)
    {
        if (!destination.Exists) // ディレクトリの存在確認
        {
            destination.Create();
        }
        if (destination.FullName != destination.Root.FullName) // 比較先のディレクトリがルートディレクトリの場合属性の変更が不可能なため判別する
        {
            destination.Attributes = source.Attributes; // 比較先のディレクトリ属性を比較元のディレクトリ属性に変更する
        }
        foreach (FileInfo fileInfo in source.GetFiles()) // sourceの下部にあるファイル情報を全て取得し反復処理を行う
        {
            string destinationFilePath = Path.Combine(destination.FullName, fileInfo.Name); // 比較先のディレクトリの絶対パスに比較元のファイル名を結合する
            FileInfo destinationFile = new FileInfo(destinationFilePath);
            if (!destinationFile.Exists) // ファイルの存在確認
            {
                File.Copy(fileInfo.FullName, destinationFile.FullName);
            }
            else if (fileInfo.LastWriteTime != destinationFile.LastWriteTime) // ファイルの最終書き込み時刻の比較
            {
                destinationFile.Attributes = FileAttributes.Normal; // ファイル属性を標準に変更する
                File.Copy(fileInfo.FullName, destinationFile.FullName, true); // 上書きコピーをする
            }
            destinationFile.Attributes = fileInfo.Attributes; // 比較先のファイル属性を比較元のファイル属性に変更する
        }
        foreach (DirectoryInfo directoryInfo in source.GetDirectories()) // sourceの下部にあるディレクトリー情報を全て取得し反復処理を行う
        {
            string destinationDirectoryPath = Path.Combine(destination.FullName, directoryInfo.Name); // 比較先のディレクトリの絶対パスに比較元のディレクトリ名を結合する
            DirectoryCopy(directoryInfo, new DirectoryInfo(destinationDirectoryPath)); // directoryInfoと結合したパスから作成したDirectoryInfoのインスタンスを渡し再帰的に実行する
        }
    }
}
