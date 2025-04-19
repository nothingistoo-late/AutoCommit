using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main()
    {
        string repoPath = @"D:\API\AutoCommit"; // Thay bằng đường dẫn repo của đại ca
        string commitMessage = $"Auto commit - {DateTime.Now}";

        try
        {
            Directory.SetCurrentDirectory(repoPath);

            // Ghi nội dung vào file log (để có thay đổi mà commit)
            string logFilePath = Path.Combine(repoPath, "autocommit_log.txt");
            File.AppendAllText(logFilePath, $"Commit at {DateTime.Now}\n");

            // Thêm và commit
            RunGitCommand("git add .");
            RunGitCommand($"git commit -m \"{commitMessage}\"");

            // Thử push lên
            var pushResult = RunGitCommandWithOutput("git push origin master");

            // Nếu push lỗi do cần fetch trước
            if (pushResult.Contains("rejected") || pushResult.Contains("failed to push"))
            {
                Console.WriteLine("⚠️ Push bị từ chối, tiến hành pull --rebase...");

                var pullResult = RunGitCommandWithOutput("git pull --rebase origin master");

                // Nếu có conflict
                if (pullResult.Contains("CONFLICT"))
                {
                    Console.WriteLine("❌ Lỗi conflict khi pull. Vui lòng xử lý conflict thủ công.");
                    Console.WriteLine(pullResult);
                }
                else
                {
                    // Nếu pull ok thì push lại
                    Console.WriteLine("✅ Pull thành công, đẩy lại lên GitHub...");
                    var finalPushResult = RunGitCommandWithOutput("git push origin master");
                    Console.WriteLine(finalPushResult);
                }
            }
            else
            {
                Console.WriteLine("✅ Auto commit thành công!");
            }
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
    }

    static string RunGitCommandWithOutput(string command)
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

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        return output + error;
    }
}
