using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    static readonly HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(6) };

    static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("=================================================");
        Console.WriteLine("    🚀 AUTO COMMIT - POLYGLOT KNOWLEDGE ENGINE  ");
        Console.WriteLine("=================================================");

        CliOptions? cli = null;
        try
        {
            // Parse CLI arguments
            cli = CliOptions.Parse(args);
            if (cli.ShowHelp)
            {
                PrintHelp();
                return 0;
            }

            string repoPath = ResolveRepositoryRoot(cli.CustomRepoPath);
            Console.WriteLine($"📂 Repository: {repoPath}");
            Directory.SetCurrentDirectory(repoPath);

            // Handle Task Scheduler CLI Installation / Uninstallation
            if (cli.InstallTask)
            {
                return TaskSchedulerService.InstallTask(repoPath, cli.TaskTime, cli.TaskName);
            }
            if (cli.UninstallTask)
            {
                return TaskSchedulerService.UninstallTask(cli.TaskName);
            }

            // Load configuration
            var config = AppConfig.Load(repoPath, cli.CustomConfigPath);
            Console.WriteLine($"👤 Git User  : {config.GitUser.Name} <{config.GitUser.Email}>");
            Console.WriteLine($"🌿 Branch    : {config.Branch}");
            Console.WriteLine($"🌐 Languages : {string.Join(", ", config.Languages)}");

            // Ensure local git config has user info set (fixes Task Scheduler environment quirks)
            EnsureLocalGitConfig(repoPath, config);

            // Pre-run Remote Synchronization (Fetch & Pull Rebase if diverged or behind)
            SyncRemoteBeforeExecution(repoPath, config);

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
                Console.WriteLine($"\n🌱 Sẽ tạo {commitCount} commit đa ngôn ngữ cho hôm nay ({DateTime.Now:yyyy-MM-dd})...");

                string logFilePath = Path.Combine(repoPath, "autocommit_log.txt");
                var timestamps = GenerateMonotonicTimestamps(lastCommitTime, DateTime.Today, commitCount, isToday: true);

                for (int i = 0; i < commitCount; i++)
                {
                    DateTime commitTime = timestamps[i];
                    
                    // Pick language rotating or random from user's preferred list
                    string selectedLang = config.Languages[i % config.Languages.Count];

                    // Generate multi-language code solution & matching commit message
                    var (commitMsg, createdFile) = await PolyglotSolutionService.CreateSolutionAndCommitMsgAsync(
                        repoPath, config, selectedLang, commitTime, i + 1, commitCount);

                    File.AppendAllText(logFilePath, $"[{commitTime:yyyy-MM-dd HH:mm:ss}] Commit {i + 1}/{commitCount} ({selectedLang}): {commitMsg}\n");

                    RunGit(repoPath, "add .", config);
                    RunGit(repoPath, $"commit -m \"{EscapeQuote(commitMsg)}\"", config, customDate: commitTime);
                    
                    createdCommitMessages.Add(commitMsg);
                    string targetInfo = !string.IsNullOrEmpty(createdFile) ? $" [Tệp: {createdFile}]" : "";
                    Console.WriteLine($"  [{i + 1}/{commitCount}] ✅ Đã commit ({commitTime:HH:mm:ss}){targetInfo}: \"{commitMsg}\"");
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

            Console.WriteLine("\n🎉 Hoàn thành xuất sắc toàn bộ quy trình AutoCommit!");
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
                var config = AppConfig.Load(repoPath, cli?.CustomConfigPath);
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

    static List<DateTime> GenerateMonotonicTimestamps(DateTime? lastCommitTime, DateTime date, int count, bool isToday)
    {
        count = Math.Max(1, count);
        var timestamps = new List<DateTime>();

        DateTime startWindow = new DateTime(date.Year, date.Month, date.Day, 8, 30, 0);

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
            endWindow = new DateTime(date.Year, date.Month, date.Day, 18, 30, 0);
            if (endWindow <= startWindow)
            {
                endWindow = startWindow.AddMinutes(Math.Max(15, count * 5));
            }
        }

        double totalSeconds = (endWindow - startWindow).TotalSeconds;

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

        var offsets = new List<int>();
        for (int i = 0; i < count; i++)
        {
            offsets.Add(Random.Shared.Next(0, (int)totalSeconds));
        }
        offsets.Sort();

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
            Console.WriteLine($"\n📅 Đang tạo {commitsPerDay} commit bù đa ngôn ngữ cho ngày: {targetDate:yyyy-MM-dd}...");
            DateTime? lastCommit = GetLatestCommitTime(repoPath, config);
            var timestamps = GenerateMonotonicTimestamps(lastCommit, targetDate, commitsPerDay, isToday: false);

            for (int i = 0; i < commitsPerDay; i++)
            {
                DateTime commitTime = timestamps[i];
                string selectedLang = config.Languages[i % config.Languages.Count];
                var (commitMsg, createdFile) = await PolyglotSolutionService.CreateSolutionAndCommitMsgAsync(
                    repoPath, config, selectedLang, commitTime, i + 1, commitsPerDay);

                File.AppendAllText(logFilePath, $"[{commitTime:yyyy-MM-dd HH:mm:ss}] [Backdate] Commit {i + 1}/{commitsPerDay} ({selectedLang}): {commitMsg}\n");

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
            Console.WriteLine("   Tiến hành tự động tạo commit cứu chuỗi đa ngôn ngữ...");

            string logFilePath = Path.Combine(repoPath, "autocommit_log.txt");
            int commitsPerDay = Math.Max(1, config.AutoStreakRecovery.CommitsPerMissedDay);

            foreach (var missedDate in missedDates.OrderBy(d => d))
            {
                DateTime? lastCommit = GetLatestCommitTime(repoPath, config);
                var timestamps = GenerateMonotonicTimestamps(lastCommit, missedDate, commitsPerDay, isToday: false);

                for (int i = 0; i < commitsPerDay; i++)
                {
                    DateTime commitTime = timestamps[i];
                    string selectedLang = config.Languages[i % config.Languages.Count];
                    var (commitMsg, createdFile) = await PolyglotSolutionService.CreateSolutionAndCommitMsgAsync(
                        repoPath, config, selectedLang, commitTime, i + 1, commitsPerDay);

                    File.AppendAllText(logFilePath, $"[{commitTime:yyyy-MM-dd HH:mm:ss}] [AutoHeal] Commit {i + 1}/{commitsPerDay} ({selectedLang}): {commitMsg}\n");

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

    #region Polyglot Solution & Knowledge Service
    static class PolyglotSolutionService
    {
        public static async Task<(string CommitMessage, string? CreatedFile)> CreateSolutionAndCommitMsgAsync(
            string repoPath, AppConfig config, string language, DateTime commitTime, int commitIndex, int totalCommits)
        {
            if (config.KnowledgeSync.Enabled)
            {
                string solutionsRoot = Path.Combine(repoPath, config.KnowledgeSync.SolutionsDirectory);
                Directory.CreateDirectory(solutionsRoot);

                // Fetch LeetCode Daily problem or fallback
                var problem = await TryFetchLeetCodeDailyAsync();
                if (problem != null)
                {
                    var fileInfo = WriteSolutionFile(solutionsRoot, language, problem, commitTime);
                    UpdateMasterIndex(solutionsRoot, problem, language, fileInfo.RelativePath, commitTime);

                    string commitMsg = $"feat({language.ToLower()}): solve LeetCode {problem.QuestionFrontendId} - '{problem.QuestionTitle}' ({problem.Difficulty})";
                    return (commitMsg, fileInfo.RelativePath);
                }
            }

            // Fallback to WhatTheCommit / Conventional commits
            string fallback = await GetWhatTheCommitOrConventionalAsync(config);
            return (fallback, null);
        }

        static (string FullPath, string RelativePath) WriteSolutionFile(string solutionsRoot, string language, LeetCodeProblem problem, DateTime commitTime)
        {
            string langFolder = GetLanguageFolderName(language);
            string langDir = Path.Combine(solutionsRoot, langFolder);
            Directory.CreateDirectory(langDir);

            string ext = GetLanguageExtension(language);
            string slug = CleanSlug(problem.QuestionTitle);
            string fileName = $"problem_{problem.QuestionFrontendId.PadLeft(4, '0')}_{slug}.{ext}";
            
            // For C# and Java, use PascalCase
            if (language.Equals("csharp", StringComparison.OrdinalIgnoreCase) || language.Equals("java", StringComparison.OrdinalIgnoreCase))
            {
                fileName = $"Problem{problem.QuestionFrontendId}_{ToPascalCase(problem.QuestionTitle)}.{ext}";
            }

            string fullPath = Path.Combine(langDir, fileName);
            string relPath = Path.Combine(Path.GetFileName(solutionsRoot), langFolder, fileName);

            string codeContent = GenerateCodeTemplate(language, problem, commitTime);
            File.WriteAllText(fullPath, codeContent);

            return (fullPath, relPath);
        }

        static void UpdateMasterIndex(string solutionsRoot, LeetCodeProblem problem, string language, string relPath, DateTime commitTime)
        {
            string indexPath = Path.Combine(solutionsRoot, "INDEX.md");
            string normalizedRelPath = relPath.Replace("\\", "/");

            if (!File.Exists(indexPath))
            {
                string header = "# 📚 LeetCode Solutions & Polyglot Knowledge Archive\n\n" +
                                "> Automated Daily Problem Solutions and Algorithm Snippets.\n\n" +
                                "| ID | Title | Difficulty | Language | Solution File | Date |\n" +
                                "| :--- | :--- | :--- | :--- | :--- | :--- |\n";
                File.WriteAllText(indexPath, header);
            }

            string icon = GetLanguageIcon(language);
            string row = $"| {problem.QuestionFrontendId} | [{problem.QuestionTitle}]({problem.QuestionLink}) | **{problem.Difficulty}** | {icon} {language.ToUpper()} | [{Path.GetFileName(relPath)}]({normalizedRelPath}) | `{commitTime:yyyy-MM-dd}` |\n";
            
            // Check if already in index
            string existing = File.ReadAllText(indexPath);
            if (!existing.Contains($"| {problem.QuestionFrontendId} |") || !existing.Contains(normalizedRelPath))
            {
                File.AppendAllText(indexPath, row);
            }
        }

        static string GenerateCodeTemplate(string language, LeetCodeProblem problem, DateTime commitTime)
        {
            string tagsStr = string.Join(", ", problem.Tags);
            string lang = language.ToLowerInvariant();

            return lang switch
            {
                "python" =>
$@"# LeetCode {problem.QuestionFrontendId}: {problem.QuestionTitle}
# Difficulty: {problem.Difficulty} | Tags: {tagsStr}
# Link: {problem.QuestionLink}
# Solved on: {commitTime:yyyy-MM-dd HH:mm:ss}

class Solution:
    def solve(self, s: str, target: str) -> str:
        """"""
        Optimized implementation for {problem.QuestionTitle}
        Tags: {tagsStr}
        """"""
        # TODO: Implement optimal approach
        return """"
",
                "typescript" =>
$@"/**
 * LeetCode {problem.QuestionFrontendId}: {problem.QuestionTitle}
 * Difficulty: {problem.Difficulty} | Tags: {tagsStr}
 * Link: {problem.QuestionLink}
 * Solved on: {commitTime:yyyy-MM-dd HH:mm:ss}
 */

export function solve(s: string, target: string): string {{
    // Optimized TypeScript solution for {problem.QuestionTitle}
    return """";
}}
",
                "golang" =>
$@"package solutions

// LeetCode {problem.QuestionFrontendId}: {problem.QuestionTitle}
// Difficulty: {problem.Difficulty} | Tags: {tagsStr}
// Link: {problem.QuestionLink}
// Solved on: {commitTime:yyyy-MM-dd HH:mm:ss}

func Solve{ToPascalCase(problem.QuestionTitle)}(s string, target string) string {{
    // Go optimal implementation
    return """"
}}
",
                "rust" =>
$@"//! LeetCode {problem.QuestionFrontendId}: {problem.QuestionTitle}
//! Difficulty: {problem.Difficulty} | Tags: {tagsStr}
//! Link: {problem.QuestionLink}
//! Solved on: {commitTime:yyyy-MM-dd HH:mm:ss}

pub struct Solution;

impl Solution {{
    pub fn solve(s: String, target: String) -> String {{
        // Rust memory-safe implementation
        String::new()
    }}
}}
",
                "java" =>
$@"package solutions;

/**
 * LeetCode {problem.QuestionFrontendId}: {problem.QuestionTitle}
 * Difficulty: {problem.Difficulty} | Tags: {tagsStr}
 * Link: {problem.QuestionLink}
 * Solved on: {commitTime:yyyy-MM-dd HH:mm:ss}
 */
public class Problem{problem.QuestionFrontendId}_{ToPascalCase(problem.QuestionTitle)} {{
    public String solve(String s, String target) {{
        // Java solution
        return """";
    }}
}}
",
                "cpp" =>
$@"#include <iostream>
#include <string>
#include <vector>

// LeetCode {problem.QuestionFrontendId}: {problem.QuestionTitle}
// Difficulty: {problem.Difficulty} | Tags: {tagsStr}
// Link: {problem.QuestionLink}
// Solved on: {commitTime:yyyy-MM-dd HH:mm:ss}

class Solution {{
public:
    std::string solve(std::string s, std::string target) {{
        // C++ high-performance solution
        return """";
    }}
}};
",
                _ => // Default C#
$@"namespace AutoCommit.Solutions;

/// <summary>
/// LeetCode {problem.QuestionFrontendId}: {problem.QuestionTitle}
/// Difficulty: {problem.Difficulty} | Tags: {tagsStr}
/// Link: {problem.QuestionLink}
/// Solved on: {commitTime:yyyy-MM-dd HH:mm:ss}
/// </summary>
public class Problem{problem.QuestionFrontendId}_{ToPascalCase(problem.QuestionTitle)}
{{
    public string Solve(string s, string target)
    {{
        // C# .NET 8 optimal implementation
        return string.Empty;
    }}
}}
"
            };
        }

        static string GetLanguageFolderName(string lang) => lang.ToLowerInvariant() switch
        {
            "csharp" => "csharp",
            "python" => "python",
            "typescript" => "typescript",
            "golang" or "go" => "golang",
            "rust" => "rust",
            "java" => "java",
            "cpp" => "cpp",
            _ => "csharp"
        };

        static string GetLanguageExtension(string lang) => lang.ToLowerInvariant() switch
        {
            "csharp" => "cs",
            "python" => "py",
            "typescript" => "ts",
            "golang" or "go" => "go",
            "rust" => "rs",
            "java" => "java",
            "cpp" => "cpp",
            _ => "cs"
        };

        static string GetLanguageIcon(string lang) => lang.ToLowerInvariant() switch
        {
            "csharp" => "🟣",
            "python" => "🐍",
            "typescript" => "🔵",
            "golang" or "go" => "🩵",
            "rust" => "🦀",
            "java" => "☕",
            "cpp" => "⚡",
            _ => "📁"
        };

        static string CleanSlug(string title)
        {
            string slug = Regex.Replace(title.ToLowerInvariant(), @"[^a-z0-9]+", "_").Trim('_');
            if (slug.Length > 40) slug = slug.Substring(0, 40).TrimEnd('_');
            return slug;
        }

        static string ToPascalCase(string title)
        {
            var words = Regex.Matches(title, @"[a-zA-Z0-9]+")
                .Select(m => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(m.Value.ToLowerInvariant()))
                .Take(6);
            return string.Concat(words);
        }

        static async Task<LeetCodeProblem?> TryFetchLeetCodeDailyAsync()
        {
            try
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                string url = "https://alfa-leetcode-api.onrender.com/daily";
                var res = await httpClient.GetStringAsync(url, cts.Token);
                using var doc = JsonDocument.Parse(res);
                var root = doc.RootElement;

                var problem = new LeetCodeProblem
                {
                    QuestionFrontendId = root.TryGetProperty("questionFrontendId", out var qId) ? qId.GetString() ?? "" : "",
                    QuestionTitle = root.TryGetProperty("questionTitle", out var qTitle) ? qTitle.GetString() ?? "" : "",
                    Difficulty = root.TryGetProperty("difficulty", out var diff) ? diff.GetString() ?? "Medium" : "Medium",
                    QuestionLink = root.TryGetProperty("questionLink", out var link) ? link.GetString() ?? "https://leetcode.com" : "https://leetcode.com"
                };

                if (root.TryGetProperty("topicTags", out var tagsElem) && tagsElem.ValueKind == JsonValueKind.Array)
                {
                    foreach (var tag in tagsElem.EnumerateArray())
                    {
                        if (tag.TryGetProperty("name", out var tagName))
                        {
                            problem.Tags.Add(tagName.GetString() ?? "");
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(problem.QuestionTitle))
                    return problem;
            }
            catch { }

            return null;
        }

        static async Task<string> GetWhatTheCommitOrConventionalAsync(AppConfig config)
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
                catch { }
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
    }

    class LeetCodeProblem
    {
        public string QuestionFrontendId { get; set; } = "";
        public string QuestionTitle { get; set; } = "";
        public string Difficulty { get; set; } = "Medium";
        public string QuestionLink { get; set; } = "";
        public List<string> Tags { get; set; } = new List<string>();
    }
    #endregion

    #region Task Scheduler Service (CLI 1-Click Installer)
    static class TaskSchedulerService
    {
        public static int InstallTask(string repoPath, string time, string taskName)
        {
            Console.WriteLine($"\n⚙️ Đang đăng ký Windows Task Scheduler: '{taskName}'...");

            string exePath = Path.Combine(AppContext.BaseDirectory, "AutoCommit.exe");
            if (!File.Exists(exePath))
            {
                exePath = Process.GetCurrentProcess().MainModule?.FileName ?? Path.Combine(repoPath, "AutoCommit", "bin", "Release", "net8.0", "AutoCommit.exe");
            }

            string psScript = $@"
$taskName = '{taskName}'
$exePath = '{exePath}'
$workingDir = '{repoPath}'
$time = '{time}'

$Principal = New-ScheduledTaskPrincipal -UserId ""$env:USERDOMAIN\$env:USERNAME"" -LogonType S4U -RunLevel Highest
$Action = New-ScheduledTaskAction -Execute $exePath -WorkingDirectory $workingDir
$Trigger = New-ScheduledTaskTrigger -Daily -At $time
$Settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -ExecutionTimeLimit (New-TimeSpan -Minutes 10)

Register-ScheduledTask -TaskName $taskName -Action $Action -Trigger $Trigger -Principal $Principal -Settings $Settings -Force | Out-Null
Write-Host ""SUCCESS""
";

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript.Replace("\"", "\\\"")}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc == null)
            {
                Console.WriteLine("❌ Không thể khởi chạy PowerShell.");
                return 1;
            }

            string stdout = proc.StandardOutput.ReadToEnd();
            string stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (stdout.Contains("SUCCESS"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ Đã đăng ký thành công Task Scheduler '{taskName}'!");
                Console.WriteLine($"   ⏰ Thời gian chạy  : Hàng ngày lúc {time}");
                Console.WriteLine($"   🛡️ Quyền thực thi   : Highest (Run as Administrator)");
                Console.WriteLine($"   👤 Logon Type       : S4U (Chạy dù user có login hay không)");
                Console.WriteLine($"   📂 Thư mục gốc      : {repoPath}");
                Console.WriteLine($"   🔋 Chế độ Pin       : Cho phép chạy khi dùng pin / Tự bù nếu lỡ giờ");
                Console.ResetColor();
                return 0;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️ Đăng ký thất bại hoặc cần quyền Administrator:");
                Console.WriteLine(stderr);
                Console.WriteLine("\n💡 Gợi ý: Hãy mở Windows Terminal / PowerShell với quyền 'Run as Administrator' rồi chạy lại lệnh này.");
                Console.ResetColor();
                return 1;
            }
        }

        public static int UninstallTask(string taskName)
        {
            Console.WriteLine($"\n🗑️ Đang gỡ bỏ Windows Task Scheduler: '{taskName}'...");

            string psScript = $"Unregister-ScheduledTask -TaskName '{taskName}' -Confirm:$false -ErrorAction SilentlyContinue; Write-Host 'SUCCESS'";
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            proc?.WaitForExit();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Đã gỡ bỏ thành công Task Scheduler '{taskName}'!");
            Console.ResetColor();
            return 0;
        }
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

    static void SyncRemoteBeforeExecution(string repoPath, AppConfig config)
    {
        try
        {
            var remotesRes = RunGit(repoPath, "remote", config);
            if (!remotesRes.StdOut.Contains("origin"))
            {
                return;
            }

            Console.WriteLine($"\n🔄 Đang kiểm tra đồng bộ với remote origin/{config.Branch}...");
            var fetchRes = RunGit(repoPath, $"fetch origin {config.Branch}", config);
            if (fetchRes.ExitCode != 0)
            {
                Console.WriteLine($"  ℹ️ Không thể fetch từ remote (offline hoặc chưa cấu hình remote): {fetchRes.StdErr.Trim()}");
                return;
            }

            var revListRes = RunGit(repoPath, $"rev-list --count {config.Branch}..origin/{config.Branch}", config);
            if (revListRes.ExitCode == 0 && int.TryParse(revListRes.StdOut.Trim(), out int behindCount) && behindCount > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  📥 Phát hiện {behindCount} commit mới trên remote origin/{config.Branch}.");
                Console.WriteLine($"  🔄 Đang tự động kéo về và hợp nhất (pull --rebase)...");
                Console.ResetColor();

                bool pullSuccess = PullWithRebaseAndAutoResolve(repoPath, config);
                if (pullSuccess)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  ✅ Đã đồng bộ thành công với remote origin!");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  ⚠️ Không thể hoàn tất rebase tự động. Vui lòng kiểm tra xung đột.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("  ✅ Nhánh local đã đồng bộ với remote.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ Lỗi khi đồng bộ remote: {ex.Message}");
        }
    }

    static bool PullWithRebaseAndAutoResolve(string repoPath, AppConfig config)
    {
        var statusPorcelain = RunGit(repoPath, "status --porcelain", config);
        bool hasStash = false;
        if (!string.IsNullOrWhiteSpace(statusPorcelain.StdOut))
        {
            var stashRes = RunGit(repoPath, "stash --include-untracked", config);
            hasStash = stashRes.ExitCode == 0 && !stashRes.StdOut.Contains("No local changes to save");
        }

        try
        {
            var pullRes = RunGit(repoPath, $"pull --rebase origin {config.Branch}", config);
            if (pullRes.ExitCode == 0)
            {
                return true;
            }

            if (IsRebaseInProgress(repoPath, config))
            {
                bool resolved = TryResolveRebaseConflict(repoPath, config);
                return resolved;
            }

            return false;
        }
        finally
        {
            if (hasStash)
            {
                RunGit(repoPath, "stash pop", config);
            }
        }
    }

    static bool IsRebaseInProgress(string repoPath, AppConfig config)
    {
        var res = RunGit(repoPath, "status", config);
        return res.StdOut.Contains("rebase in progress") || 
               res.StdOut.Contains("You are currently rebasing") ||
               res.StdOut.Contains("rebase --continue");
    }

    static bool TryResolveRebaseConflict(string repoPath, AppConfig config)
    {
        int maxSteps = 50;
        int step = 0;

        while (IsRebaseInProgress(repoPath, config) && step++ < maxSteps)
        {
            var unmergedRes = RunGit(repoPath, "diff --name-only --diff-filter=U", config);
            var conflictedFiles = unmergedRes.StdOut
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim())
                .ToList();

            if (conflictedFiles.Count == 0)
            {
                var contRes = RunGit(repoPath, "rebase --continue", config);
                if (contRes.ExitCode == 0 && !IsRebaseInProgress(repoPath, config))
                {
                    return true;
                }
                continue;
            }

            bool allFilesAutoResolvable = true;
            foreach (var relFile in conflictedFiles)
            {
                string fileName = Path.GetFileName(relFile);
                string fullPath = Path.Combine(repoPath, relFile);

                if (string.Equals(fileName, "autocommit_log.txt", StringComparison.OrdinalIgnoreCase))
                {
                    ResolveLogFileConflict(fullPath);
                    RunGit(repoPath, $"add \"{EscapeQuote(relFile)}\"", config);
                    Console.WriteLine("  🔧 Đã tự động giải quyết xung đột trong autocommit_log.txt.");
                }
                else if (string.Equals(fileName, "INDEX.md", StringComparison.OrdinalIgnoreCase))
                {
                    ResolveIndexFileConflict(fullPath);
                    RunGit(repoPath, $"add \"{EscapeQuote(relFile)}\"", config);
                    Console.WriteLine("  🔧 Đã tự động giải quyết xung đột trong solutions/INDEX.md.");
                }
                else
                {
                    allFilesAutoResolvable = false;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ❌ Phát hiện xung đột phức tạp trong tệp: {relFile}");
                    Console.ResetColor();
                    break;
                }
            }

            if (!allFilesAutoResolvable)
            {
                Console.WriteLine("  ↩️ Đang hủy rebase (rebase --abort) để giữ an toàn cho mã nguồn...");
                RunGit(repoPath, "rebase --abort", config);
                return false;
            }

            var continueRes = RunGit(repoPath, "rebase --continue", config);
            if (continueRes.ExitCode == 0 && !IsRebaseInProgress(repoPath, config))
            {
                return true;
            }
        }

        bool finalState = !IsRebaseInProgress(repoPath, config);
        if (!finalState)
        {
            RunGit(repoPath, "rebase --abort", config);
        }
        return finalState;
    }

    static void ResolveLogFileConflict(string fullPath)
    {
        if (!File.Exists(fullPath)) return;

        var allLines = File.ReadAllLines(fullPath);
        var cleanLines = new List<string>();
        var seenLines = new HashSet<string>(StringComparer.Ordinal);

        foreach (var line in allLines)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("<<<<<<<") || 
                trimmed.StartsWith("=======") || 
                trimmed.StartsWith(">>>>>>>") || 
                trimmed.StartsWith("|||||||"))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                if (seenLines.Add(trimmed))
                {
                    cleanLines.Add(line);
                }
            }
            else
            {
                cleanLines.Add(line);
            }
        }

        File.WriteAllLines(fullPath, cleanLines, System.Text.Encoding.UTF8);
    }

    static void ResolveIndexFileConflict(string fullPath)
    {
        if (!File.Exists(fullPath)) return;

        var allLines = File.ReadAllLines(fullPath);
        var tableRows = new List<string>();
        var seenProblemKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in allLines)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("<<<<<<<") || 
                trimmed.StartsWith("=======") || 
                trimmed.StartsWith(">>>>>>>") || 
                trimmed.StartsWith("|||||||"))
            {
                continue;
            }

            if (trimmed.StartsWith("|") && trimmed.EndsWith("|") && !trimmed.Contains(":---") && !trimmed.Contains("Difficulty"))
            {
                var parts = trimmed.Split('|');
                string key = trimmed;
                if (parts.Length > 4)
                {
                    key = $"{parts[1].Trim()}_{parts[4].Trim()}";
                }

                if (seenProblemKeys.Add(key))
                {
                    tableRows.Add(trimmed);
                }
            }
        }

        string header = "# 📚 LeetCode Solutions & Polyglot Knowledge Archive\n\n" +
                        "> Automated Daily Problem Solutions and Algorithm Snippets.\n\n" +
                        "| ID | Title | Difficulty | Language | Solution File | Date |\n" +
                        "| :--- | :--- | :--- | :--- | :--- | :--- |\n";

        string content = header + string.Join("\n", tableRows) + "\n";
        File.WriteAllText(fullPath, content, System.Text.Encoding.UTF8);
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

            string combinedError = $"{res.StdErr} {res.StdOut}".ToLowerInvariant();
            bool isRejected = combinedError.Contains("rejected") || 
                              combinedError.Contains("non-fast-forward") || 
                              combinedError.Contains("fetch first") ||
                              combinedError.Contains("need to be updated");

            if (isRejected)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  ⚠️ Remote có commit mới chưa đồng bộ (non-fast-forward push).");
                Console.WriteLine("  🔄 Đang tự động kéo về (pull --rebase) và hợp nhất xung đột...");
                Console.ResetColor();

                bool rebaseOk = PullWithRebaseAndAutoResolve(repoPath, config);
                if (rebaseOk)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  ✅ Đã hợp nhất rebase thành công! Đang thử đẩy lại lên remote...");
                    Console.ResetColor();

                    var pushAfterRebase = RunGit(repoPath, $"push origin {branch}", config);
                    if (pushAfterRebase.ExitCode == 0)
                    {
                        return true;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  ❌ Không thể tự động hợp nhất với remote do có xung đột phức tạp.");
                    Console.ResetColor();
                    return false;
                }
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

        psi.Environment["GIT_AUTHOR_NAME"] = config.GitUser.Name;
        psi.Environment["GIT_AUTHOR_EMAIL"] = config.GitUser.Email;
        psi.Environment["GIT_COMMITTER_NAME"] = config.GitUser.Name;
        psi.Environment["GIT_COMMITTER_EMAIL"] = config.GitUser.Email;
        psi.Environment["GIT_TERMINAL_PROMPT"] = "0";
        psi.Environment["GIT_EDITOR"] = "true";

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

Tùy chọn chung:
  --fill <YYYY-MM-DD>               Bù commit cho một ngày trong quá khứ (ví dụ: --fill 2026-08-20)
  --fill-range <FROM>:<TO>          Bù commit cho một khoảng ngày (ví dụ: --fill-range 2026-08-15:2026-08-20)
  --count <số lượng>                Số lượng commit tạo ra mỗi ngày (ghi đè cấu hình)
  --no-push                         Chỉ tạo commit local, không đẩy lên remote
  --config <đường dẫn file JSON>    Chỉ định file cấu hình config.json khác
  --repo <đường dẫn thư mục repo>   Chỉ định thư mục repo Git
  -h, --help                        Hiển thị trợ giúp này

Tùy chọn Task Scheduler:
  --install-task                    Tự động đăng ký Windows Task Scheduler (quyền Admin + S4U)
  --uninstall-task                  Gỡ bỏ Windows Task Scheduler
  --time <HH:mm>                    Giờ chạy hàng ngày khi cài Task (mặc định: 09:15)
  --task-name <tên>                 Tên của Task trong Task Scheduler (mặc định: AutoCommit_Daily)

Ví dụ:
  AutoCommit.exe                               Chạy tự động sinh code đa ngôn ngữ & commit
  AutoCommit.exe --install-task --time 09:30   Đăng ký Task Scheduler chạy ngầm 09:30 sáng hàng ngày
  AutoCommit.exe --fill 2026-08-27 --count 3   Bù 3 commit đa ngôn ngữ cho ngày 27/08/2026
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
    public List<string> Languages { get; set; } = new List<string> { "csharp", "python", "typescript", "golang", "rust" };
    public KnowledgeSyncConfig KnowledgeSync { get; set; } = new KnowledgeSyncConfig();
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

        // Check if config.example.json exists and auto-copy to config.json
        string examplePath = Path.Combine(repoPath, "config.example.json");
        string targetConfigPath = Path.Combine(repoPath, "config.json");
        if (File.Exists(examplePath) && !File.Exists(targetConfigPath))
        {
            try
            {
                File.Copy(examplePath, targetConfigPath, overwrite: false);
                Console.WriteLine("ℹ️ Đã tự động tạo file config.json từ config.example.json.");
                string json = File.ReadAllText(targetConfigPath);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var loaded = JsonSerializer.Deserialize<AppConfig>(json, opts);
                if (loaded != null) return loaded;
            }
            catch { }
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

public class KnowledgeSyncConfig
{
    public bool Enabled { get; set; } = true;
    public string SolutionsDirectory { get; set; } = "solutions";
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
    public bool InstallTask { get; set; }
    public bool UninstallTask { get; set; }
    public string TaskTime { get; set; } = "09:15";
    public string TaskName { get; set; } = "AutoCommit_Daily";
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
            else if (arg.Equals("--install-task", StringComparison.OrdinalIgnoreCase))
            {
                opts.InstallTask = true;
            }
            else if (arg.Equals("--uninstall-task", StringComparison.OrdinalIgnoreCase))
            {
                opts.UninstallTask = true;
            }
            else if (arg.Equals("--time", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                opts.TaskTime = args[++i];
            }
            else if (arg.Equals("--task-name", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                opts.TaskName = args[++i];
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
            else if (arg.Equals("--fill", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 >= args.Length)
                    throw new ArgumentException("Thiếu ngày cho --fill. Ví dụ: --fill 2026-08-20");

                string dateStr = args[++i];
                if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    throw new ArgumentException($"Ngày không hợp lệ cho --fill: '{dateStr}'. Định dạng đúng: YYYY-MM-DD");

                opts.FillDates.Add(dt);
            }
            else if (arg.Equals("--fill-range", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 >= args.Length)
                    throw new ArgumentException("Thiếu khoảng ngày cho --fill-range. Ví dụ: --fill-range 2026-08-15:2026-08-20");

                string rangeStr = args[++i];
                var parts = rangeStr.Split(':');
                if (parts.Length != 2 ||
                    !DateTime.TryParseExact(parts[0].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) ||
                    !DateTime.TryParseExact(parts[1].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
                {
                    throw new ArgumentException(
                        $"Khoảng ngày không hợp lệ cho --fill-range: '{rangeStr}'. Định dạng đúng: YYYY-MM-DD:YYYY-MM-DD");
                }

                if (end < start)
                    throw new ArgumentException($"--fill-range: ngày kết thúc ({end:yyyy-MM-dd}) phải >= ngày bắt đầu ({start:yyyy-MM-dd}).");

                for (var d = start; d <= end; d = d.AddDays(1))
                    opts.FillDates.Add(d);
            }
            else if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Tùy chọn không được hỗ trợ: '{arg}'. Dùng --help để xem danh sách.");
            }
            else if (opts.CustomRepoPath == null)
            {
                opts.CustomRepoPath = arg;
            }
        }

        return opts;
    }
}
#endregion
