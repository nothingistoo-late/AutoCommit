using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static readonly HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

    static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("=================================================");
        Console.WriteLine("        🚀 AUTO COMMIT & STREAK PROTECTOR       ");
        Console.WriteLine("=================================================");

        // Parse CLI arguments
        var cli = CliOptions.Parse(args);
        if (cli.ShowHelp)
        {
            PrintHelp();
            return 0;
        }

        try
        {
            string repoPath = ResolveRepositoryRoot(cli.CustomRepoPath);
            Console.WriteLine($"📂 Repository: {repoPath}");
            Directory.SetCurrentDirectory(repoPath);

            // Load configuration
            var config = AppConfig.Load(repoPath, cli.CustomConfigPath);
            Console.WriteLine($"👤 Git User  : {config.GitUser.Name} <{config.GitUser.Email}>");
            Console.WriteLine($"🌿 Branch    : {config.Branch}");

            // Ensure local git config has user info set (fixes Task Scheduler environment quirks)
            EnsureLocalGitConfig(repoPath, config);

            var summaryLogs = new List<string>();
            var createdCommitMessages = new List<string>();

            // Handle Manual Backdate / Fill Mode
            if (cli.IsFillMode)
            {
                Console.WriteLine("\n⏳ Đang chạy chế độ LẤP Ô TRỐNG THỦ CÔNG (Manual Backdate)...");
                await ExecuteManualFillAsync(repoPath, config, cli, summaryLogs, createdCommitMessages);
            }
            else
            {
                // Auto Streak Healer check (Check missing dates in recent days)
                if (config.AutoStreakRecovery.Enabled)
                {
                    await AutoHealStreakAsync(repoPath, config, summaryLogs, createdCommitMessages);
                }

                // Query the latest commit in repo to guarantee strictly increasing chronological order
                DateTime? lastCommitTime = GetLatestCommitTime(repoPath, config);
                if (lastCommitTime.HasValue)
                {
                    Console.WriteLine($"🕒 Commit mới nhất trước đó: {lastCommitTime.Value:yyyy-MM-dd HH:mm:ss}");
                }

                // Normal commit execution for today with Monotonic Time-Scattering
                int commitCount = cli.CustomCommitCount ?? RandomSkewedLow(config.CommitsPerRun.Min, config.CommitsPerRun.Max);
                Console.WriteLine($"\n🌱 Sẽ tạo {commitCount} commit rải rác tăng dần cho hôm nay ({DateTime.Now:yyyy-MM-dd})...");

                string logFilePath = Path.Combine(repoPath, "autocommit_log.txt");
                var timestamps = GenerateMonotonicTimestamps(lastCommitTime, DateTime.Today, commitCount, isToday: true);

                for (int i = 0; i < commitCount; i++)
                {
                    DateTime commitTime = timestamps[i];
                    string commitMsg = await GetCommitMessageAsync(config);
                    
                    File.AppendAllText(logFilePath, $"[{commitTime:yyyy-MM-dd HH:mm:ss}] Commit {i + 1}/{commitCount}: {commitMsg}\n");

                    RunGit(repoPath, "add .", config);
                    RunGit(repoPath, $"commit -m \"{EscapeQuote(commitMsg)}\"", config, customDate: commitTime);
                    
                    createdCommitMessages.Add(commitMsg);
                    Console.WriteLine($"  [{i + 1}/{commitCount}] ✅ Đã commit ({commitTime:HH:mm:ss}): \"{commitMsg}\"");
                }

                summaryLogs.Add($"Hôm nay ({DateTime.Now:yyyy-MM-dd}): {commitCount} commits");
            }

            // Push to remote if not disabled
            if (!cli.NoPush)
            {
                Console.WriteLine($"\n🚀 Đang đẩy code lên remote origin/{config.Branch}...");
                bool pushSuccess = PushWithRetry(repoPath, config.Branch, config, maxRetries: 3);

                if (pushSuccess)
                {
                    Console.WriteLine($"✅ Đã đẩy thành công tất cả commit lên origin/{config.Branch}!");
                }
                else
                {
                    Console.WriteLine($"⚠️ Không thể push lên remote origin/{config.Branch}. Vui lòng kiểm tra kết nối mạng.");
                }

                // Send Telegram Notification
                if (config.Telegram.Enabled)
                {
                    await SendTelegramNotificationAsync(config, pushSuccess, createdCommitMessages, summaryLogs);
                }
            }
            else
            {
                Console.WriteLine("\nℹ️ Cờ --no-push được bật: Bỏ qua bước đẩy lên remote.");
            }

            Console.WriteLine("\n🎉 Hoàn thành xuất sắc toàn bộ quy trình AutoCommit (Thời gian luôn xuôi chiều tuyệt đối)!");
            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ LỖI: {ex.Message}");
            Console.ResetColor();

            // Try sending telegram error notification if possible
            try
            {
                var repoPath = Environment.CurrentDirectory;
                var config = AppConfig.Load(repoPath, cli.CustomConfigPath);
                if (config.Telegram.Enabled)
                {
                    _ = SendTelegramNotificationAsync(config, false, new List<string>(), new List<string> { $"❌ Lỗi: {ex.Message}" });
                }
            }
            catch { }

            return 1;
        }
    }

    #region Monotonic Time-Scattering Engine
    /// <summary>
    /// Lấy mốc thời gian của commit gần nhất trong repo Git.
    /// </summary>
    static DateTime? GetLatestCommitTime(string repoPath, AppConfig config)
    {
        var res = RunGit(repoPath, "log -1 --date=iso-strict --pretty=format:\"%cd\"", config);
        if (res.ExitCode == 0 && !string.IsNullOrWhiteSpace(res.StdOut))
        {
            if (DateTimeOffset.TryParse(res.StdOut.Trim(), out var dto))
            {
                return dto.LocalDateTime;
            }
        }
        return null;
    }

    /// <summary>
    /// Sinh ra danh sách mốc thời gian LUÔN TĂNG DẦN và NẰM SAU commit mới nhất.
    /// Đảm bảo cây lịch sử Git luôn xuôi chiều 100%, không bao giờ bị nhảy cóc về quá khứ!
    /// </summary>
    static List<DateTime> GenerateMonotonicTimestamps(DateTime? lastCommitTime, DateTime date, int count, bool isToday)
    {
        count = Math.Max(1, count);
        var timestamps = new List<DateTime>();

        // Giờ bắt đầu mặc định trong ngày: 08:30
        DateTime startWindow = new DateTime(date.Year, date.Month, date.Day, 8, 30, 0);

        // NẾU commit gần nhất diễn ra trong CÙNG NGÀY ĐANG XÉT:
        // Cửa sổ thời gian bắt buộc phải bắt đầu SAU commit gần nhất (cách 1 - 5 phút).
        if (lastCommitTime.HasValue && lastCommitTime.Value.Date == date.Date)
        {
            if (lastCommitTime.Value >= startWindow)
            {
                startWindow = lastCommitTime.Value.AddMinutes(Random.Shared.Next(1, 6));
            }
        }

        DateTime endWindow;
        if (isToday)
        {
            DateTime now = DateTime.Now;
            if (now <= startWindow)
            {
                endWindow = startWindow.AddMinutes(Math.Max(5, count * 3));
            }
            else
            {
                endWindow = now;
            }
        }
        else
        {
            // Quá khứ: rải đều tới cuối giờ chiều (18:30)
            endWindow = new DateTime(date.Year, date.Month, date.Day, 18, 30, 0);
            if (endWindow <= startWindow)
            {
                endWindow = startWindow.AddMinutes(Math.Max(15, count * 5));
            }
        }

        double totalSeconds = (endWindow - startWindow).TotalSeconds;

        // Nếu khoảng cách quá hẹp (ví dụ chạy liên tiếp nhiều lần sát giờ hiện tại):
        if (totalSeconds < count * 60)
        {
            DateTime current = startWindow;
            for (int i = 0; i < count; i++)
            {
                current = current.AddSeconds(Random.Shared.Next(60, 180));
                timestamps.Add(current);
            }
            return timestamps;
        }

        // Bốc ngẫu nhiên 'count' điểm mốc giây rồi sắp xếp tăng dần
        var offsets = new List<int>();
        for (int i = 0; i < count; i++)
        {
            offsets.Add(Random.Shared.Next(0, (int)totalSeconds));
        }
        offsets.Sort();

        // Đảm bảo mỗi commit cách nhau ít nhất 60-180 giây
        for (int i = 1; i < offsets.Count; i++)
        {
            if (offsets[i] <= offsets[i - 1])
            {
                offsets[i] = offsets[i - 1] + Random.Shared.Next(60, 180);
            }
        }

        foreach (var offset in offsets)
        {
            DateTime dt = startWindow.AddSeconds(offset);
            timestamps.Add(dt);
        }

        return timestamps;
    }
    #endregion

    #region Manual Fill / Backdate Logic
    static async Task ExecuteManualFillAsync(string repoPath, AppConfig config, CliOptions cli, List<string> summaryLogs, List<string> commitMessages)
    {
        string logFilePath = Path.Combine(repoPath, "autocommit_log.txt");
        int commitsPerDay = cli.CustomCommitCount ?? config.AutoStreakRecovery.CommitsPerMissedDay;
        commitsPerDay = Math.Max(1, commitsPerDay);

        foreach (var targetDate in cli.FillDates)
        {
            Console.WriteLine($"\n📅 Đang tạo {commitsPerDay} commit bù rải rác cho ngày: {targetDate:yyyy-MM-dd}...");
            DateTime? lastCommit = GetLatestCommitTime(repoPath, config);
            var timestamps = GenerateMonotonicTimestamps(lastCommit, targetDate, commitsPerDay, isToday: false);

            for (int i = 0; i < commitsPerDay; i++)
            {
                DateTime commitTime = timestamps[i];
                string commitMsg = await GetCommitMessageAsync(config);
                File.AppendAllText(logFilePath, $"[{commitTime:yyyy-MM-dd HH:mm:ss}] [Backdate] Commit {i + 1}/{commitsPerDay}: {commitMsg}\n");

                RunGit(repoPath, "add .", config);
                RunGit(repoPath, $"commit -m \"{EscapeQuote(commitMsg)}\"", config, customDate: commitTime);

                commitMessages.Add(commitMsg);
                Console.WriteLine($"  [{i + 1}/{commitsPerDay}] ✅ Đã bù ({commitTime:HH:mm:ss}): \"{commitMsg}\"");
            }

            summaryLogs.Add($"Bù ngày {targetDate:yyyy-MM-dd}: {commitsPerDay} commits");
        }
    }
    #endregion

    #region Auto Streak Recovery
    static async Task AutoHealStreakAsync(string repoPath, AppConfig config, List<string> summaryLogs, List<string> commitMessages)
    {
        int pastDays = Math.Max(1, config.AutoStreakRecovery.CheckPastDays);
        DateTime today = DateTime.Today;

        // Query git log for dates with commits in the last N days
        var gitLogResult = RunGit(repoPath, $"log --since=\"{pastDays + 2} days ago\" --date=short --pretty=format:\"%cd\"", config);
        var existingDates = new HashSet<string>(
            gitLogResult.StdOut
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim())
        );

        var missedDates = new List<DateTime>();
        for (int d = 1; d <= pastDays; d++)
        {
            DateTime checkDate = today.AddDays(-d);
            string dateStr = checkDate.ToString("yyyy-MM-dd");
            if (!existingDates.Contains(dateStr))
            {
                missedDates.Add(checkDate);
            }
        }

        if (missedDates.Count > 0)
        {
            Console.WriteLine($"\n🛡️ [Auto Streak Healer] Phát hiện {missedDates.Count} ngày bị lỡ commit: {string.Join(", ", missedDates.Select(d => d.ToString("yyyy-MM-dd")))}");
            Console.WriteLine("   Tiến hành tự động tạo commit cứu chuỗi (rải rác tăng dần)...");

            string logFilePath = Path.Combine(repoPath, "autocommit_log.txt");
            int commitsPerDay = Math.Max(1, config.AutoStreakRecovery.CommitsPerMissedDay);

            foreach (var missedDate in missedDates.OrderBy(d => d))
            {
                DateTime? lastCommit = GetLatestCommitTime(repoPath, config);
                var timestamps = GenerateMonotonicTimestamps(lastCommit, missedDate, commitsPerDay, isToday: false);

                for (int i = 0; i < commitsPerDay; i++)
                {
                    DateTime commitTime = timestamps[i];
                    string commitMsg = await GetCommitMessageAsync(config);
                    File.AppendAllText(logFilePath, $"[{commitTime:yyyy-MM-dd HH:mm:ss}] [AutoHeal] Commit {i + 1}/{commitsPerDay}: {commitMsg}\n");

                    RunGit(repoPath, "add .", config);
                    RunGit(repoPath, $"commit -m \"{EscapeQuote(commitMsg)}\"", config, customDate: commitTime);

                    commitMessages.Add(commitMsg);
                    Console.WriteLine($"  [AutoHeal] ✅ Đã cứu ngày {missedDate:yyyy-MM-dd} ({commitTime:HH:mm:ss}): \"{commitMsg}\"");
                }

                summaryLogs.Add($"Tự động bù ngày {missedDate:yyyy-MM-dd}: {commitsPerDay} commits");
            }
        }
    }
    #endregion

    #region Commit Message Service
    static async Task<string> GetCommitMessageAsync(AppConfig config)
    {
        if (config.WhatTheCommit.Enabled && !string.IsNullOrWhiteSpace(config.WhatTheCommit.ApiUrl))
        {
            try
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(config.WhatTheCommit.TimeoutSeconds));
                var response = await httpClient.GetStringAsync(config.WhatTheCommit.ApiUrl, cts.Token);
                string cleanMsg = response.Trim().Replace("\r", "").Replace("\n", " ");
                if (!string.IsNullOrWhiteSpace(cleanMsg) && cleanMsg.Length < 150)
                {
                    return cleanMsg;
                }
            }
            catch
            {
                // Fallback to offline generator smoothly
            }
        }

        return GenerateDynamicConventionalCommit();
    }

    static string GenerateDynamicConventionalCommit()
    {
        string[] types = { "feat", "fix", "docs", "refactor", "perf", "chore", "test", "style", "build", "ci" };
        string[] scopes = { "core", "api", "auth", "utils", "config", "cache", "logger", "parser", "worker", "db", "network", "storage", "scheduler" };
        string[] actions =
        {
            "optimize memory allocation in batch worker",
            "improve error handling for timeout requests",
            "clean up redundant variables and imports",
            "update documentation and code examples",
            "enhance response parsing and validation",
            "refactor helper methods for clarity",
            "add edge-case unit test coverage",
            "fine-tune caching layer invalidation",
            "update project configuration and dependencies",
            "resolve minor race condition in worker loop",
            "improve logging format and verbosity",
            "streamline data processing pipeline",
            "standardize exception handling across modules",
            "optimize string formatting and allocations",
            "adjust retry policy and exponential backoff"
        };

        var type = types[Random.Shared.Next(types.Length)];
        var scope = scopes[Random.Shared.Next(scopes.Length)];
        var action = actions[Random.Shared.Next(actions.Length)];

        return $"{type}({scope}): {action}";
    }
    #endregion

    #region Telegram Notification
    static async Task SendTelegramNotificationAsync(AppConfig config, bool pushSuccess, List<string> commitMessages, List<string> summaryLogs)
    {
        if (!config.Telegram.Enabled || 
            string.IsNullOrWhiteSpace(config.Telegram.BotToken) || 
            config.Telegram.BotToken.Contains("YOUR_BOT_TOKEN") ||
            string.IsNullOrWhiteSpace(config.Telegram.ChatId) ||
            config.Telegram.ChatId.Contains("YOUR_CHAT_ID"))
        {
            return;
        }

        try
        {
            string statusIcon = pushSuccess ? "🌱" : "⚠️";
            string title = pushSuccess 
                ? "<b>[AutoCommit] Thành công!</b>" 
                : "<b>[AutoCommit] Cảnh báo lỗi Push!</b>";

            string lastCommitText = commitMessages.Count > 0 
                ? $"<i>\"{commitMessages.Last()}\"</i>" 
                : "N/A";

            string details = string.Join("\n", summaryLogs.Select(s => $"• {s}"));

            string message = $"{statusIcon} {title}\n\n" +
                             $"👤 <b>User:</b> {config.GitUser.Name}\n" +
                             $"🌿 <b>Branch:</b> <code>{config.Branch}</code>\n" +
                             $"⏰ <b>Thời gian:</b> {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n" +
                             $"📊 <b>Tổng kết:</b>\n{details}\n" +
                             $"💬 <b>Commit gần nhất:</b> {lastCommitText}\n" +
                             $"🚀 <b>Push remote:</b> {(pushSuccess ? "✅ Thành công" : "❌ Thất bại")}";

            var postData = new Dictionary<string, string>
            {
                { "chat_id", config.Telegram.ChatId },
                { "text", message },
                { "parse_mode", "HTML" }
            };

            var url = $"https://api.telegram.org/bot{config.Telegram.BotToken}/sendMessage";
            using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(postData), cts.Token);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("📱 Đã gửi thông báo kết quả qua Telegram!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Không thể gửi thông báo Telegram: {ex.Message}");
        }
    }
    #endregion

    #region Git Helpers
    static void EnsureLocalGitConfig(string repoPath, AppConfig config)
    {
        RunGit(repoPath, $"config user.name \"{EscapeQuote(config.GitUser.Name)}\"", config);
        RunGit(repoPath, $"config user.email \"{EscapeQuote(config.GitUser.Email)}\"", config);
    }

    static bool PushWithRetry(string repoPath, string branch, AppConfig config, int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            var res = RunGit(repoPath, $"push origin {branch}", config);
            if (res.ExitCode == 0)
            {
                return true;
            }

            Console.WriteLine($"  ⚠️ Lần thử {attempt}/{maxRetries} thất bại. Đang đợi thử lại...");
            if (attempt < maxRetries)
            {
                System.Threading.Thread.Sleep(3000 * attempt);
            }
        }
        return false;
    }

    static (int ExitCode, string StdOut, string StdErr) RunGit(string repoPath, string arguments, AppConfig config, DateTime? customDate = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = repoPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Guarantee user author information
        psi.Environment["GIT_AUTHOR_NAME"] = config.GitUser.Name;
        psi.Environment["GIT_AUTHOR_EMAIL"] = config.GitUser.Email;
        psi.Environment["GIT_COMMITTER_NAME"] = config.GitUser.Name;
        psi.Environment["GIT_COMMITTER_EMAIL"] = config.GitUser.Email;

        if (customDate.HasValue)
        {
            string dateIso = customDate.Value.ToString("yyyy-MM-ddTHH:mm:ss");
            psi.Environment["GIT_AUTHOR_DATE"] = dateIso;
            psi.Environment["GIT_COMMITTER_DATE"] = dateIso;
        }

        using var process = new Process { StartInfo = psi };
        process.Start();

        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return (process.ExitCode, stdout, stderr);
    }

    static string EscapeQuote(string text) => text.Replace("\"", "\\\"");

    static int RandomSkewedLow(int min, int max)
    {
        min = Math.Max(1, min);
        max = Math.Max(min, max);
        int a = Random.Shared.Next(min, max + 1);
        int b = Random.Shared.Next(min, max + 1);
        return Math.Min(a, b);
    }

    static string ResolveRepositoryRoot(string? customPath)
    {
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            string path = Path.GetFullPath(customPath.Trim());
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Không tìm thấy thư mục: {path}");

            string? root = FindGitRoot(path);
            if (root != null) return root;
            throw new InvalidOperationException($"Không phát hiện repo Git tại {path}");
        }

        foreach (string start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            if (string.IsNullOrWhiteSpace(start) || !Directory.Exists(start)) continue;
            string? root = FindGitRoot(Path.GetFullPath(start));
            if (root != null) return root;
        }

        throw new InvalidOperationException("Không tìm thấy repo Git (.git). Vui lòng chạy ứng dụng từ thư mục repo hoặc truyền đường dẫn thư mục.");
    }

    static string? FindGitRoot(string startPath)
    {
        for (var dir = new DirectoryInfo(startPath); dir != null; dir = dir.Parent)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")) || File.Exists(Path.Combine(dir.FullName, ".git")))
                return dir.FullName;
        }
        return null;
    }
    #endregion

    #region Help Info
    static void PrintHelp()
    {
        Console.WriteLine(@"
Sử dụng: AutoCommit.exe [tùy chọn]

Tùy chọn:
  --fill <YYYY-MM-DD>               Bù commit cho một ngày trong quá khứ (ví dụ: --fill 2026-08-20)
  --fill-range <FROM>:<TO>          Bù commit cho một khoảng ngày (ví dụ: --fill-range 2026-08-15:2026-08-20)
  --count <số lượng>                Số lượng commit tạo ra mỗi ngày (ghi đè cấu hình)
  --no-push                         Chỉ tạo commit local, không đẩy lên remote
  --config <đường dẫn file JSON>    Chỉ định file cấu hình config.json khác
  --repo <đường dẫn thư mục repo>   Chỉ định thư mục repo Git
  -h, --help                        Hiển thị trợ giúp này

Ví dụ:
  AutoCommit.exe                               Chạy tự động (thời gian tăng dần sau commit mới nhất)
  AutoCommit.exe --fill 2026-08-27 --count 3   Bù 3 commit rải rác cho ngày 27/08/2026
  AutoCommit.exe --fill-range 2026-08-20:2026-08-25
");
    }
    #endregion
}

#region Data Models
public class AppConfig
{
    public GitUserConfig GitUser { get; set; } = new GitUserConfig();
    public string Branch { get; set; } = "master";
    public MinMaxConfig CommitsPerRun { get; set; } = new MinMaxConfig { Min = 1, Max = 5 };
    public WhatTheCommitConfig WhatTheCommit { get; set; } = new WhatTheCommitConfig();
    public AutoStreakConfig AutoStreakRecovery { get; set; } = new AutoStreakConfig();
    public TelegramConfig Telegram { get; set; } = new TelegramConfig();

    public static AppConfig Load(string repoPath, string? customConfigPath)
    {
        string[] searchPaths = {
            customConfigPath ?? "",
            Path.Combine(repoPath, "config.json"),
            Path.Combine(AppContext.BaseDirectory, "config.json"),
            Path.Combine(Environment.CurrentDirectory, "config.json")
        };

        foreach (var path in searchPaths)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var loaded = JsonSerializer.Deserialize<AppConfig>(json, opts);
                    if (loaded != null) return loaded;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Không đọc được config từ {path}: {ex.Message}");
                }
            }
        }

        Console.WriteLine("ℹ️ Sử dụng cấu hình mặc định trong ứng dụng.");
        return new AppConfig();
    }
}

public class GitUserConfig
{
    public string Name { get; set; } = "hctrung2k4";
    public string Email { get; set; } = "hctrung2k4@gmail.com";
}

public class MinMaxConfig
{
    public int Min { get; set; } = 1;
    public int Max { get; set; } = 5;
}

public class WhatTheCommitConfig
{
    public bool Enabled { get; set; } = true;
    public string ApiUrl { get; set; } = "https://whatthecommit.com/index.txt";
    public int TimeoutSeconds { get; set; } = 4;
}

public class AutoStreakConfig
{
    public bool Enabled { get; set; } = true;
    public int CheckPastDays { get; set; } = 3;
    public int CommitsPerMissedDay { get; set; } = 2;
}

public class TelegramConfig
{
    public bool Enabled { get; set; } = false;
    public string BotToken { get; set; } = "";
    public string ChatId { get; set; } = "";
}

public class CliOptions
{
    public bool ShowHelp { get; set; }
    public bool NoPush { get; set; }
    public int? CustomCommitCount { get; set; }
    public string? CustomRepoPath { get; set; }
    public string? CustomConfigPath { get; set; }
    public List<DateTime> FillDates { get; set; } = new List<DateTime>();
    public bool IsFillMode => FillDates.Count > 0;

    public static CliOptions Parse(string[] args)
    {
        var opts = new CliOptions();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            if (arg.Equals("--help", StringComparison.OrdinalIgnoreCase) || arg.Equals("-h", StringComparison.OrdinalIgnoreCase))
            {
                opts.ShowHelp = true;
                return opts;
            }
            else if (arg.Equals("--no-push", StringComparison.OrdinalIgnoreCase))
            {
                opts.NoPush = true;
            }
            else if (arg.Equals("--count", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                if (int.TryParse(args[++i], out int c)) opts.CustomCommitCount = c;
            }
            else if (arg.Equals("--repo", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                opts.CustomRepoPath = args[++i];
            }
            else if (arg.Equals("--config", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                opts.CustomConfigPath = args[++i];
            }
            else if (arg.Equals("--fill", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                if (DateTime.TryParseExact(args[++i], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    opts.FillDates.Add(dt);
                }
            }
            else if (arg.Equals("--fill-range", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                string rangeStr = args[++i];
                var parts = rangeStr.Split(':');
                if (parts.Length == 2 &&
                    DateTime.TryParseExact(parts[0].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) &&
                    DateTime.TryParseExact(parts[1].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
                {
                    for (var d = start; d <= end; d = d.AddDays(1))
                    {
                        opts.FillDates.Add(d);
                    }
                }
            }
            else if (!arg.StartsWith("-") && opts.CustomRepoPath == null)
            {
                opts.CustomRepoPath = arg;
            }
        }

        return opts;
    }
}
#endregion
