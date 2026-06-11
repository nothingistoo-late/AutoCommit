using System;
using System.Diagnostics;
using System.IO;

class Program
{
    /// <summary>Số commit tối thiểu mỗi lần chạy (>= 1).</summary>
    const int MinCommitsPerRun = 1;
    /// <summary>Số commit tối đa mỗi lần chạy (>= MinCommitsPerRun).</summary>
    const int MaxCommitsPerRun = 10;

    static void Main()
    {
        string repoPath = @"D:\Tool\AutoCommit"; // Thay bằng đường dẫn repo của bạn

        try
        {
            Directory.SetCurrentDirectory(repoPath);

            // Lấy min của hai lần random → tỉ lệ số commit nhỏ cao hơn (ít khi ra nhiều commit).
            int commitCount = RandomCommitCountSkewedLow(MinCommitsPerRun, MaxCommitsPerRun);
            Console.WriteLine($"Sẽ tạo {commitCount} commit (lệch về ít, trong khoảng {MinCommitsPerRun}–{MaxCommitsPerRun}).");

            string logFilePath = Path.Combine(repoPath, "autocommit_log.txt");

            for (int i = 1; i <= commitCount; i++)
            {
                // Định dạng ngày giờ theo mặc định hệ thống (giống bản cũ: $"{DateTime.Now}")
                DateTime now = DateTime.Now;
                File.AppendAllText(logFilePath, $"Commit {i}/{commitCount} at {now}\n");

                RunGitCommand("git add .");
                string message = $"Auto commit ({i}/{commitCount}) - {now}";
                RunGitCommand($"git commit -m \"{message}\"");
                Console.WriteLine($"  Đã commit {i}/{commitCount}.");
            }

            RunGitCommand("git push origin master");

            Console.WriteLine($"✅ Auto commit thành công! Đã đẩy {commitCount} commit lên remote.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Lỗi: {ex.Message}");
        }
    }

    /// <summary>Random trong [min, max] nhưng thiên về giá trị nhỏ (min của hai lần uniform).</summary>
    static int RandomCommitCountSkewedLow(int min, int max)
    {
        int a = Random.Shared.Next(min, max + 1);
        int b = Random.Shared.Next(min, max + 1);
        return Math.Min(a, b);
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
