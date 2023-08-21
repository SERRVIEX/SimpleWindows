using System.IO;

public static class DirectoryInfoExtensions
{
    public static void Copy(string sourceDir, string destinationDir)
    {
        DirectoryInfo directory = new DirectoryInfo(sourceDir);

        foreach (string dir in Directory.GetDirectories(directory.FullName, "*", SearchOption.AllDirectories))
        {
            string dirToCreate = dir.Replace(directory.FullName, destinationDir);
            Directory.CreateDirectory(dirToCreate);
        }

        foreach (string newPath in Directory.GetFiles(directory.FullName, "*.*", SearchOption.AllDirectories))
            File.Copy(newPath, newPath.Replace(directory.FullName, destinationDir), true);
    }

    public static async void CopyAsync(string sourceDir, string destinationDir)
    {
        DirectoryInfo directory = new DirectoryInfo(sourceDir);

        foreach (string dir in Directory.GetDirectories(directory.FullName, "*", SearchOption.AllDirectories))
        {
            string dirToCreate = dir.Replace(directory.FullName, destinationDir);
            Directory.CreateDirectory(dirToCreate);
        }

        foreach (string newPath in Directory.GetFiles(directory.FullName, "*.*", SearchOption.AllDirectories))
            await FileAsync.Copy(newPath, newPath.Replace(directory.FullName, destinationDir));
    }
}