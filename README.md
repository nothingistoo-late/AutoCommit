# AutoCommit - Polyglot Knowledge Engine

<p align="center">
  <b>A smart, natural, and resilient Git automation tool built with .NET 8</b><br>
  <i>Tự động hóa commit & push Git, sinh code giải thuật đa ngôn ngữ (Python, C#, TypeScript, Go, Rust, Java, C++), phân bổ thời gian xuôi chiều và gửi thông báo Telegram.</i>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/Git-Automation-F05032?style=flat&logo=git&logoColor=white" alt="Git" />
  <img src="https://img.shields.io/badge/Languages-Polyglot-success" alt="Polyglot Languages" />
  <img src="https://img.shields.io/badge/Telegram-Notifications-2CA5E0?style=flat&logo=telegram&logoColor=white" alt="Telegram" />
  <img src="https://img.shields.io/badge/License-MIT-blue" alt="License" />
</p>

---

## Language Selection
- [Tiếng Việt](#tiếng-việt)
- [English](#english)

---

# Tiếng Việt

## Mục lục
1. [Giới thiệu](#giới-thiệu)
2. [Tính năng chính](#tính-năng-chính)
3. [Lịch sử phiên bản](#lịch-sử-phiên-bản)
4. [Ghi chú & Mẹo hữu ích](#ghi-chú--mẹo-hữu-ích)
5. [Cấu trúc thư mục giải thuật](#cấu-trúc-thư-mục-giải-thuật)
6. [Cấu hình config.json](#cấu-hình-configjson)
7. [Thiết lập thông báo Telegram](#thiết-lập-thông-báo-telegram)
8. [Hướng dẫn dòng lệnh CLI](#hướng-dẫn-dòng-lệnh-cli)
9. [Cài đặt với Windows Task Scheduler](#cài-đặt-với-windows-task-scheduler)
10. [Build từ mã nguồn](#build-từ-mã-nguồn)
11. [Thông tin tác giả](#thông-tin-tác-giả)

---

## Giới thiệu
**AutoCommit** là công cụ dòng lệnh (CLI) được phát triển bằng C# (.NET 8) giúp duy trì lịch sử hoạt động Git và biểu đồ đóng góp (GitHub contribution graph) một cách tự nhiên, uy tín và chuẩn chỉnh theo quy trình của lập trình viên thực tế.

---

## Tính năng chính
- **Sinh code giải thuật đa ngôn ngữ (Polyglot Code Generator)**: Tự động tạo các file code giải thuật riêng biệt theo từng ngôn ngữ: Python (`.py`), C# (`.cs`), TypeScript (`.ts`), Go (`.go`), Rust (`.rs`), Java (`.java`), C++ (`.cpp`). Giúp thanh phân bổ ngôn ngữ (Languages Bar) trên GitHub hiển thị đầy đủ và đa dạng.
- **Mục lục tổng hợp tự động ([solutions/INDEX.md](file:///d:/TrungHC/AutoCommit/solutions/INDEX.md))**: Tự động lưu bảng mục lục Markdown chi tiết về ID bài toán, tên, độ khó, ngôn ngữ, đường dẫn file và ngày giải.
- **Cài đặt Task Scheduler 1-Click (`--install-task`)**: Đăng ký lịch chạy ngầm tự động với quyền cao nhất (RunLevel Highest / Admin), chế độ S4U (chạy kể cả khi user không đăng nhập) và tự chạy bù nếu bị tắt máy.
- **Động cơ thời gian xuôi chiều (Monotonic Time Engine)**: Tự động kiểm tra commit mới nhất trong Git history để đảm bảo các commit mới luôn tăng dần theo thứ tự thời gian (`T_mới > T_cũ`), hoàn thành toàn bộ tác vụ chỉ trong 1-2 giây.
- **Cứu chuỗi kép (Dual Streak Protector)**: Tự động quét và bù commit cho các ngày bị quên trong quá khứ gần, hoặc bù thủ công theo ngày chỉ định qua CLI (`--fill`, `--fill-range`).
- **Thông báo tức thì qua Telegram**: Gửi tóm tắt kết quả chi tiết kèm trạng thái push về Telegram cá nhân.

---

## Lịch sử phiên bản

### Phiên bản 3.4 (Hiện tại) - Polyglot Multi-Language & Master Index
- **Sinh code đa ngôn ngữ**: Tự động tạo file giải thuật LeetCode độc lập theo từng ngôn ngữ trong thư mục `solutions/<language>/`.
- **Master Index tự động**: Tự cập nhật bảng mục lục Markdown tổng hợp các bài đã giải vào [solutions/INDEX.md](file:///d:/TrungHC/AutoCommit/solutions/INDEX.md).
- **Cài Task Scheduler bằng CLI (`--install-task`)**: Tự động cấu hình quyền Administrator, LogonType S4U và xử lý điều kiện pin.
- **Động cơ Monotonic Time**: Đảm bảo mốc thời gian commit luôn xuôi chiều tuyệt đối, không bị nhảy cóc về quá khứ.
- **WhatTheCommit & Conventional Commits Offline**: Lấy commit message ngẫu nhiên, tự động chuyển sang bộ sinh chuẩn (> 3.000 câu) nếu mất mạng.
- **File cấu hình `config.json`**: Quản lý toàn bộ thiết lập mà không cần biên dịch lại mã nguồn.

### Phiên bản 3.0 - 3.3 - Smart & Stealth Upgrade
- Tích hợp API WhatTheCommit và LeetCode Daily Challenge.
- Bổ sung cơ chế cứu chuỗi (Auto-Heal và Manual Backdate).
- Tích hợp Telegram Bot gửi báo cáo sau mỗi lượt chạy.
- Khắc phục triệt để lỗi Task Scheduler nhận nhầm tài khoản Windows Domain.

### Phiên bản 2.0 - Multi-Commit & Skewed Distribution
- Hỗ trợ tạo nhiều commit mỗi lần chạy theo dải `[min, max]`.
- Phân phối lệch về số nhỏ (`Math.Min(a, b)`) để phần lớn chỉ tạo 1–3 commit/ngày nhằm giữ tính tự nhiên.
- Tự động dò tìm gốc repository (`.git`) từ thư mục hiện tại hoặc thư mục thực thi.
- Ghi nhận lịch sử chạy vào file `autocommit_log.txt`.

### Phiên bản 1.0 - Khởi tạo ban đầu
- Chạy commit và push đơn lẻ mỗi lần thực thi theo lịch cơ bản.
- Đẩy code trực tiếp lên nhánh `master`.

---

## Ghi chú & Mẹo hữu ích

1. **Quy tắc tính Contribution Graph của GitHub**:
   - GitHub tính chuỗi đóng góp (ô vuông xanh) dựa trên **Ngày của Commit (`AuthorDate`)**, không phụ thuộc vào thời điểm bạn push code lên server.
   - Email cấu hình trong `config.json` (`gitUser.email`) **bắt buộc phải khớp** với email đã xác thực trên tài khoản GitHub của bạn để avatar và chuỗi đóng góp được hiển thị đúng.
2. **Kiểm tra commit chưa push lên Remote**:
   ```bash
   git log origin/master..HEAD
   ```
3. **Kiểm tra lịch sử commit chi tiết kèm mốc giờ**:
   ```bash
   git log -n 10 --pretty=format:"%h | %ad | %an | %s" --date=format:"%Y-%m-%d %H:%M:%S"
   ```
4. **Lưu ý khi tạo Task Scheduler thủ công**:
   - Luôn luôn điền đường dẫn thư mục gốc của repository vào ô **Start in (Thư mục bắt đầu)** để Git tìm thấy `.git` và file `config.json`.
   - *(Khuyên dùng lệnh `AutoCommit.exe --install-task` để tool tự động thiết lập chính xác).*

---

## Cấu trúc thư mục giải thuật

```text
AutoCommit/
├── solutions/
│   ├── python/          # Code giải thuật Python (.py)
│   ├── typescript/      # Code giải thuật TypeScript (.ts)
│   ├── csharp/          # Code giải thuật C# (.cs)
│   ├── golang/          # Code giải thuật Go (.go)
│   ├── rust/            # Code giải thuật Rust (.rs)
│   ├── java/            # Code giải thuật Java (.java)
│   ├── cpp/             # Code giải thuật C++ (.cpp)
│   └── INDEX.md         # Bảng mục lục tổng hợp tự động cập nhật
├── notes/               # Ghi chú học tập & tài liệu
├── config.json          # File cấu hình trung tâm
└── autocommit_log.txt   # Nhật ký thực thi
```

---

## Cấu hình `config.json`

File `config.json` nằm tại thư mục gốc của repository:

```json
{
  "gitUser": {
    "name": "hctrung2k4",
    "email": "hctrung2k4@gmail.com"
  },
  "branch": "master",
  "commitsPerRun": {
    "min": 1,
    "max": 5
  },
  "languages": [
    "csharp",
    "python",
    "typescript",
    "golang",
    "rust",
    "java",
    "cpp"
  ],
  "knowledgeSync": {
    "enabled": true,
    "solutionsDirectory": "solutions"
  },
  "whatTheCommit": {
    "enabled": true,
    "apiUrl": "https://whatthecommit.com/index.txt",
    "timeoutSeconds": 4
  },
  "autoStreakRecovery": {
    "enabled": true,
    "checkPastDays": 3,
    "commitsPerMissedDay": 2
  },
  "telegram": {
    "enabled": false,
    "botToken": "YOUR_BOT_TOKEN_HERE",
    "chatId": "YOUR_CHAT_ID_HERE"
  }
}
```

| Tham số | Kiểu dữ liệu | Mô tả |
| :--- | :--- | :--- |
| `gitUser.name` | `string` | Tên tác giả hiển thị trên Git & GitHub. |
| `gitUser.email` | `string` | Email liên kết với GitHub (quyết định Avatar & Contribution graph). |
| `branch` | `string` | Nhánh Git để đẩy code lên (`master` hoặc `main`). |
| `commitsPerRun.min` | `int` | Số lượng commit tối thiểu mỗi lần chạy. |
| `commitsPerRun.max` | `int` | Số lượng commit tối đa mỗi lần chạy. |
| `languages` | `string[]` | Danh sách các ngôn ngữ lập trình tạo code luân phiên. |
| `knowledgeSync.enabled`| `bool` | Bật/tắt tính năng đồng bộ LeetCode & tạo file code đa ngôn ngữ. |
| `whatTheCommit.enabled` | `bool` | Bật/tắt việc gọi API WhatTheCommit để lấy message phụ. |
| `autoStreakRecovery.enabled`| `bool` | Tự động quét và bù commit cho các ngày bị quên trong quá khứ. |
| `telegram.enabled` | `bool` | Bật/tắt tính năng gửi thông báo về Telegram. |
| `telegram.botToken` | `string` | Mã Token của Telegram Bot do `@BotFather` cấp. |
| `telegram.chatId` | `string` | Chat ID của bạn để nhận tin nhắn từ Bot. |

---

## Thiết lập thông báo Telegram

1. **Tạo Bot**: Mở Telegram, tìm **`@BotFather`**, gõ `/newbot`, đặt tên bot và username kết thúc bằng `bot`. Bạn sẽ nhận được `botToken` (dạng `7123456789:AAFx...`).
2. **Lấy Chat ID**: Tìm bot **`@userinfobot`**, bấm **Start** để lấy số `Id` của bạn. Sau đó gửi cho bot vừa tạo ở bước 1 một tin nhắn bất kỳ (ví dụ: `hi`).
3. **Cập nhật cấu hình**: Điền `botToken` và `chatId` vào `config.json` và đổi `"enabled": true`.

---

## Hướng dẫn dòng lệnh CLI

```powershell
# 1. Chạy tự động bình thường
dotnet run --project AutoCommit\AutoCommit.csproj

# 2. Đăng ký Windows Task Scheduler tự chạy ngầm 09:30 sáng hàng ngày (Run as Admin)
AutoCommit.exe --install-task --time "09:30"

# 3. Gỡ bỏ Windows Task Scheduler
AutoCommit.exe --uninstall-task

# 4. Bù commit cho 1 ngày cụ thể trong quá khứ (ví dụ: 20/08/2026 với 3 commit)
dotnet run --project AutoCommit\AutoCommit.csproj -- --fill 2026-08-20 --count 3

# 5. Bù commit cho cả một dải ngày trong quá khứ
dotnet run --project AutoCommit\AutoCommit.csproj -- --fill-range 2026-08-10:2026-08-20

# 6. Chỉ commit local, không push lên remote
dotnet run --project AutoCommit\AutoCommit.csproj -- --no-push

# 7. Xem toàn bộ các tùy chọn trợ giúp
dotnet run --project AutoCommit\AutoCommit.csproj -- --help
```

---

## Cài đặt với Windows Task Scheduler

### Cách 1: Sử dụng lệnh CLI 1-Click (Khuyên dùng)
Mở Windows Terminal hoặc PowerShell với quyền Administrator:
```powershell
AutoCommit.exe --install-task --time "09:30"
```

### Cách 2: Thiết lập thủ công qua giao diện Windows
1. Nhấn `Win + R`, nhập `taskschd.msc` và nhấn Enter.
2. Nhấn **Create Task...**:
   - **General**: Đặt tên task, chọn *Run whether user is logged on or not*, tích chọn *Run with highest privileges*.
   - **Triggers**: Chọn **Daily** -> Thiết lập giờ chạy (ví dụ `09:30 AM`).
   - **Actions**: Start a program -> Trỏ tới file `AutoCommit.exe`, điền đường dẫn thư mục repo vào ô **Start in**.
3. Nhấn **OK** để hoàn tất.

---

## Build từ mã nguồn

Yêu cầu: **.NET 8 SDK** và **Git**.

```bash
cd AutoCommit
dotnet build -c Release
```

File thực thi nằm tại: `AutoCommit/bin/Release/net8.0/AutoCommit.exe`.

---

## Thông tin tác giả
- **Tác giả**: hctrung2k4
- **Email**: hctrung2k4@gmail.com
- **Repository**: [https://github.com/nothingistoo-late/AutoCommit](https://github.com/nothingistoo-late/AutoCommit)

---

<br><br>

---

# English

## Table of Contents
1. [Overview](#overview)
2. [Key Features](#key-features)
3. [Changelog & Evolution](#changelog--evolution)
4. [Important Notes & Tips](#important-notes--tips)
5. [Polyglot Directory Structure](#polyglot-directory-structure)
6. [config.json Specification](#configjson-specification)
7. [Telegram Bot Setup](#telegram-bot-setup)
8. [CLI Command Reference](#cli-command-reference)
9. [Windows Task Scheduler Automation](#windows-task-scheduler-automation)
10. [Build & Development](#build--development)
11. [Author & Contact](#author--contact)

---

## Overview
**AutoCommit** is a robust CLI automation tool built with C# (.NET 8). It transforms your repository into a living **Polyglot Knowledge Hub**, automatically generating daily algorithm solutions across **Python**, **C#**, **TypeScript**, **Go**, **Rust**, **Java**, and **C++** while keeping your GitHub contribution graph active and natural.

---

## Key Features
- **Polyglot Code Generator**: Generates standalone solution files in `solutions/<language>/` (`.py`, `.cs`, `.ts`, `.go`, `.rs`, `.java`, `.cpp`), enriching your GitHub repository language breakdown bar.
- **Automated Master Index ([solutions/INDEX.md](file:///d:/TrungHC/AutoCommit/solutions/INDEX.md))**: Maintains a comprehensive Markdown catalog linking all solved problems, difficulty ratings, and source files.
- **1-Click Task Scheduler Setup (`--install-task`)**: Installs automated background tasks with Administrator privilege (RunLevel Highest) and LogonType S4U (runs whether user is logged on or not).
- **Monotonic Time Engine**: Queries `git log -1` to ensure newly created commits strictly advance forward in time (`T_new > T_prev`), completing all operations in 1-2 seconds.
- **Dual Streak Protector**: Auto-heals missed days in recent history and supports targeted backdating via CLI (`--fill`, `--fill-range`).
- **Instant Telegram Alerts**: Delivers real-time Markdown summary reports directly to your personal Telegram.

---

## Changelog & Evolution

### Version 3.4 (Current) - Polyglot Multi-Language & Master Index
- **Polyglot Solution Engine**: Generates standalone daily LeetCode solution files across multiple programming languages (Python, C#, TypeScript, Go, Rust, Java, C++).
- **Automated Master Index**: Automatically updates a central Markdown catalog table with problem metadata in [solutions/INDEX.md](file:///d:/TrungHC/AutoCommit/solutions/INDEX.md).
- **1-Click Task Scheduler Setup (`--install-task`)**: Automates task creation with RunLevel Highest and LogonType S4U.
- **Monotonic Time Engine**: Strict chronological progression guaranteeing a linear, forward-moving Git timeline.
- **WhatTheCommit & Offline Conventional Generator**: Humorous commit messages with offline fallback generating over 3,000+ standard commit messages.
- **JSON Configuration (`config.json`)**: Externalized configuration for users, languages, and runtime options.

### Version 3.0 - 3.3 - Smart & Stealth Upgrade
- Integrated WhatTheCommit API and LeetCode Daily Challenge API.
- Added Dual Streak Saver (Auto-Heal and Manual Backdating).
- Telegram Bot notifications for push summaries.
- Complete isolation against Windows Domain account overriding in background tasks.

### Version 2.0 - Multi-Commit & Skewed Distribution
- **Dynamic Commit Counts**: Configurable range of commits per execution (`[min, max]`).
- **Skewed-Low Distribution**: Uses `Math.Min(a, b)` distribution favoring fewer commits (1-3 commits) per execution for maximum realism.
- **Automatic Git Discovery**: Recursively scans upwards from the working directory or binary folder until `.git` is found.
- **Execution Logging**: Records commit history in `autocommit_log.txt`.

### Version 1.0 - Initial Release
- Basic scheduled single commit and push to `master`.

---

## Important Notes & Tips

1. **GitHub Contribution Graph Rules**:
   - GitHub calculates contributions (green squares) based on the **Commit Date (`AuthorDate`)**, not the time when the commit is pushed.
   - The configured email (`gitUser.email`) **must match** a verified email address on your GitHub account to map contributions and avatars properly.
2. **Inspect Unpushed Commits**:
   ```bash
   git log origin/master..HEAD
   ```
3. **View Commit Timeline with Timestamps**:
   ```bash
   git log -n 10 --pretty=format:"%h | %ad | %an | %s" --date=format:"%Y-%m-%d %H:%M:%S"
   ```
4. **Task Scheduler Working Directory**:
   - When setting up manually, always specify the repository root in the **Start in** field so Git can discover `.git` and `config.json`.
   - *(Using `AutoCommit.exe --install-task` configures this automatically).*

---

## Polyglot Directory Structure

```text
AutoCommit/
├── solutions/
│   ├── python/          # Python solution files (.py)
│   ├── typescript/      # TypeScript solution files (.ts)
│   ├── csharp/          # C# solution files (.cs)
│   ├── golang/          # Go solution files (.go)
│   ├── rust/            # Rust solution files (.rs)
│   ├── java/            # Java solution files (.java)
│   ├── cpp/             # C++ solution files (.cpp)
│   └── INDEX.md         # Auto-generated Master Index catalog
├── notes/               # Curated learning notes & cheatsheets
├── config.json          # Core application configuration
└── autocommit_log.txt   # Runtime execution logs
```

---

## `config.json` Specification

```json
{
  "gitUser": {
    "name": "hctrung2k4",
    "email": "hctrung2k4@gmail.com"
  },
  "branch": "master",
  "commitsPerRun": {
    "min": 1,
    "max": 5
  },
  "languages": [
    "csharp",
    "python",
    "typescript",
    "golang",
    "rust",
    "java",
    "cpp"
  ],
  "knowledgeSync": {
    "enabled": true,
    "solutionsDirectory": "solutions"
  },
  "whatTheCommit": {
    "enabled": true,
    "apiUrl": "https://whatthecommit.com/index.txt",
    "timeoutSeconds": 4
  },
  "autoStreakRecovery": {
    "enabled": true,
    "checkPastDays": 3,
    "commitsPerMissedDay": 2
  },
  "telegram": {
    "enabled": false,
    "botToken": "YOUR_BOT_TOKEN_HERE",
    "chatId": "YOUR_CHAT_ID_HERE"
  }
}
```

---

## Telegram Bot Setup

1. **Create Bot**: Open Telegram, search for **`@BotFather`**, send `/newbot`, name your bot, and obtain your `botToken`.
2. **Find Chat ID**: Open **`@userinfobot`**, press **Start** to get your numeric `Id`. Send any message to your newly created bot to initialize chat.
3. **Configure**: Enter `botToken` and `chatId` in `config.json` and set `"enabled": true`.

---

## CLI Command Reference

```powershell
# 1. Normal run (with polyglot code generation & index update)
dotnet run --project AutoCommit\AutoCommit.csproj

# 2. 1-Click Install Windows Task Scheduler (Run as Admin)
AutoCommit.exe --install-task --time "09:30"

# 3. Uninstall Windows Task Scheduler
AutoCommit.exe --uninstall-task

# 4. Backdate commits for a specific date in the past
dotnet run --project AutoCommit\AutoCommit.csproj -- --fill 2026-08-20 --count 3

# 5. Backdate commits across a date range
dotnet run --project AutoCommit\AutoCommit.csproj -- --fill-range 2026-08-10:2026-08-20

# 6. Local commits only (skip git push)
dotnet run --project AutoCommit\AutoCommit.csproj -- --no-push
```

---

## Windows Task Scheduler Automation

1. **Quick CLI (Recommended)**:
   ```powershell
   AutoCommit.exe --install-task --time "09:30"
   ```
2. **Manual GUI Setup**:
   - Open `taskschd.msc`.
   - Create Task -> Set **Start in** to repository root, enable *Run whether user is logged on or not* and *Run with highest privileges*.

---

## Build & Development

Requires **.NET 8 SDK** and **Git**:

```bash
cd AutoCommit
dotnet build -c Release
```

Binary output: `AutoCommit/bin/Release/net8.0/AutoCommit.exe`.

---

## Author & Contact
- **Author**: hctrung2k4
- **Email**: hctrung2k4@gmail.com
- **Repository**: [https://github.com/nothingistoo-late/AutoCommit](https://github.com/nothingistoo-late/AutoCommit)
