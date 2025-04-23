using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static string repoPath = @"D:\API\AutoCommit"; // Thay bằng repo của đại ca

    static void Main()
    {
        string commitMessage = $"Auto commit - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        try
        {
            // Ghi log để tạo thay đổi
            string logFilePath = Path.Combine(repoPath, "autocommit_log.txt");
            File.AppendAllText(logFilePath, $"Commit at {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

            // Add và commit
            RunGitCommand("add .");
            RunGitCommand($"commit -m \"{commitMessage}\"");

            // Push lần đầu
            var pushResult = RunGitCommandWithOutput("push origin master");

            // Nếu bị từ chối do chưa pull
            if (pushResult.Contains("rejected") || pushResult.Contains("failed to push"))
            {
                Console.WriteLine("⚠️ Push bị từ chối, tiến hành pull --rebase --strategy-option=theirs...");

                var pullResult = RunGitCommandWithOutput("pull --rebase --strategy-option=theirs origin master");

                if (pullResult.Contains("CONFLICT"))
                {
                    LogConflict($"[❌ {DateTime.Now:yyyy-MM-dd HH:mm:ss}] Conflict detected:\n{pullResult}");
                    Console.WriteLine("❌ Vẫn còn conflict. Đã ghi log vào conflict_log.txt.");
                }
                else
                {
                    Console.WriteLine("✅ Pull thành công, đẩy lại lên GitHub...");
                    var finalPushResult = RunGitCommandWithOutput("push origin master");
                    Console.WriteLine(finalPushResult);

                    LogConflict($"[✅ {DateTime.Now:yyyy-MM-dd HH:mm:ss}] Pull & push sau rebase thành công.\n");
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
            LogConflict($"[❌ {DateTime.Now:yyyy-MM-dd HH:mm:ss}] Exception: {ex.Message}");
        }
    }

    static void RunGitCommand(string args)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                WorkingDirectory = repoPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();
    }

    static string RunGitCommandWithOutput(string args)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                WorkingDirectory = repoPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return output + error;
    }

    static void LogConflict(string content)
    {
        string conflictLogPath = Path.Combine(repoPath, "conflict_log.txt");
        File.AppendAllText(conflictLogPath, content + "\n\n");
    }
}
