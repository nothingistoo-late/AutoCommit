using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main()
    {
        string repoPath = @"C:\Users\YourUsername\YourRepo"; // Thay bằng đường dẫn repo của bạn
        string commitMessage = $"Auto commit - {DateTime.Now}";

        try
        {
            // Thay đổi thư mục làm việc
            Directory.SetCurrentDirectory(repoPath);

            // Ghi nội dung vào file log (để có thay đổi mà commit)
            string logFilePath = Path.Combine(repoPath, "autocommit_log.txt");
            File.AppendAllText(logFilePath, $"Commit at {DateTime.Now}\n");

            // Thực hiện Git commands
            RunGitCommand("git add .");
            RunGitCommand($"git commit -m \"{commitMessage}\"");
            RunGitCommand("git push origin main");

            Console.WriteLine("✅ Auto commit thành công!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Lỗi: {ex.Message}");
        }
    }

    static void RunGitCommand(string command)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.StandardInput.WriteLine(command);
        process.StandardInput.WriteLine("exit");
        process.StandardInput.Flush();
        process.StandardInput.Close();
        process.WaitForExit();

        Console.WriteLine(process.StandardOutput.ReadToEnd());
        Console.WriteLine(process.StandardError.ReadToEnd());
    }
}
