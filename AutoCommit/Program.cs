using System;
using System.Diagnostics;
using System.IO;

class Program
{
    /// <summary>Số commit tối thiểu mỗi lần chạy (>= 1).</summary>
    const int MinCommitsPerRun = 1;
    /// <summary>Số commit tối đa mỗi lần chạy (>= MinCommitsPerRun).</summary>
    const int MaxCommitsPerRun = 10;

    static void Main(string[] args)
    {
        try
        {
            string repoPath = ResolveRepositoryRoot(args);
            Console.WriteLine($"Repo: {repoPath}");
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

    /// <summary>
    /// 1) Có tham số: dùng đường dẫn đó (bất kỳ thư mục con nào trong repo cũng được), đi lên tìm .git.
    /// 2) Không tham số: thử <see cref="Environment.CurrentDirectory"/> rồi thư mục chạy app (BaseDirectory), mỗi nơi đều đi lên tìm .git.
    /// </summary>
    static string ResolveRepositoryRoot(string[] args)
    {
        if (args.Length > 0)
        {
            string path = Path.GetFullPath(args[0].Trim());
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(
                    $"Không tìm thấy thư mục: {path}. Chương trình không chạy tiếp.");

            string? fromArg = FindGitRepositoryRoot(path);
            if (fromArg != null)
                return fromArg;

            throw new InvalidOperationException(
                "Không phát hiện repo Git (không có .git trên đường dẫn đã cho). Chương trình không chạy — không commit, không push.");
        }

        foreach (string start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            if (string.IsNullOrWhiteSpace(start) || !Directory.Exists(start))
                continue;

            string? root = FindGitRepositoryRoot(Path.GetFullPath(start));
            if (root != null)
                return root;
        }

        throw new InvalidOperationException(
            "Không phát hiện repo Git (không có .git). Chương trình không chạy — không commit, không push. " +
            "Với Task Scheduler: đặt \"Start in\" vào thư mục repo. Hoặc chạy: AutoCommit.exe \"<đường dẫn trong repo>\"");
    }

    static string? FindGitRepositoryRoot(string startPath)
    {
        var dir = new DirectoryInfo(startPath);
        if (!dir.Exists)
            return null;

        for (DirectoryInfo? d = dir; d != null; d = d.Parent)
        {
            string gitPath = Path.Combine(d.FullName, ".git");
            if (Directory.Exists(gitPath) || File.Exists(gitPath))
                return d.FullName;
        }

        return null;
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
