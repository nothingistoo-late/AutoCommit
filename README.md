# 🚀 Auto Commit & Streak Protector

<p align="center">
  <b>A smart, natural, and resilient Git automation tool built with .NET 8</b><br>
  <i>Tự động hóa commit & push Git thông minh, tự nhiên, chống đứt chuỗi xanh và thông báo tức thì.</i>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/Git-Automation-F05032?style=flat&logo=git&logoColor=white" alt="Git" />
  <img src="https://img.shields.io/badge/Telegram-Notifications-2CA5E0?style=flat&logo=telegram&logoColor=white" alt="Telegram" />
  <img src="https://img.shields.io/badge/Status-Active-brightgreen" alt="Status" />
  <img src="https://img.shields.io/badge/License-MIT-blue" alt="License" />
</p>

---

## 🌐 Chọn ngôn ngữ / Choose Language
* 🇻🇳 [Tiếng Việt](#-tiếng-việt)
* 🇬🇧 [English](#-english)

---

# 🇻🇳 Tiếng Việt

## 📖 Giới thiệu
**AutoCommit** là công cụ dòng lệnh (CLI) được viết bằng C# (.NET 8) giúp duy trì hoạt động commit và lịch sử đóng góp (GitHub contribution graph) một cách **tự nhiên**, **an toàn**, và **linh hoạt nhất**. 

Công cụ giải quyết triệt để các vấn đề thường gặp của các bot auto commit truyền thống:
* ❌ Tránh các commit vô nghĩa hoặc giống hệt nhau gây lộ bot.
* ❌ Tránh tạo dồn dập hàng loạt commit trong cùng 1 giây.
* ❌ Không lo mất chuỗi xanh (streak) khi quên bật máy hoặc máy gặp sự cố.
* ❌ Khắc phục lỗi Task Scheduler tự nhận sai thông tin tài khoản Windows Domain.

---

## 📜 Lịch sử phiên bản (Changelog)

### 🌟 Phiên bản 3 (Hiện tại) - *Smart & Stealth Upgrade*
* **🌐 Tích hợp API WhatTheCommit**: Lấy commit message hài hước, ngẫu nhiên từ `whatthecommit.com`.
* **🛡️ Bộ sinh Conventional Commits Offline**: Khi mất mạng, tự động kích hoạt thuật toán ghép từ thông minh sinh ra hơn **3.000+ commit message** chuẩn chuyên nghiệp (`feat(api): optimize memory allocation...`), đảm bảo app không bao giờ bị crash.
* **⚙️ Cấu hình `config.json`**: Tách toàn bộ thiết lập ra ngoài file JSON, không cần compile lại source code.
* **🛡️ Cứu chuỗi kép (Dual Streak Saver)**:
  * **Tự động (Auto-Heal)**: Quét `git log`, tự phát hiện ngày hôm qua/quá khứ gần bị thiếu commit và tự động tạo commit bù.
  * **Thủ công (Manual Backdate)**: Hỗ trợ lệnh CLI bù ngày bất kỳ (`--fill`) hoặc cả khoảng ngày (`--fill-range`).
* **⏳ Độ trễ tự nhiên (Smart Jitter Delay)**: Tự động nghỉ ngẫu nhiên từ 15s – 90s giữa các commit.
* **📱 Thông báo tức thì qua Telegram**: Gửi kết quả (số commit, thời gian, commit mới nhất, trạng thái push) về điện thoại.
* **🔒 Khắc phục lỗi Task Scheduler**: Gắn cứng thông tin tác giả vào Git environment của process.

### 🔹 Phiên bản 2 - *Multi-Commit & Skewed Distribution*
* **Nhiều commit mỗi lần chạy**: Cho phép random số lượng commit trong khoảng `[min, max]`.
* **Phân phối lệch về số nhỏ (Skewed Low)**: Lấy giá trị nhỏ nhất của 2 lần random để đa số chỉ tạo 1–3 commit/ngày, hiếm khi tạo quá nhiều.
* **Tự động dò tìm gốc Git (`.git`)**: Đi ngược từ thư mục hiện tại lên cây thư mục cha cho đến khi tìm thấy repository.
* **Ghi log**: Ghi nhận lịch sử commit vào file `autocommit_log.txt`.

### 🔹 Phiên bản 1 - *Khởi tạo ban đầu*
* Chạy commit và push đơn lẻ mỗi lần thực thi theo lịch cơ bản.

---

## ⚙️ Giải thích chi tiết file `config.json`

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
  "delaySeconds": {
    "min": 15,
    "max": 90
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

| Tham số | Kiểu dữ liệu | Ý nghĩa |
| :--- | :--- | :--- |
| `gitUser.name` | `string` | Tên tác giả hiển thị trên Git & GitHub. |
| `gitUser.email` | `string` | Email liên kết với tài khoản GitHub (quyết định Avatar & Contribution graph). |
| `branch` | `string` | Tên nhánh Git để đẩy code lên (ví dụ: `master` hoặc `main`). |
| `commitsPerRun.min` | `int` | Số lượng commit tối thiểu mỗi lần chạy. |
| `commitsPerRun.max` | `int` | Số lượng commit tối đa mỗi lần chạy. |
| `delaySeconds.min` | `int` | Số giây nghỉ tối thiểu giữa 2 lần commit liên tiếp. |
| `delaySeconds.max` | `int` | Số giây nghỉ tối đa giữa 2 lần commit liên tiếp. |
| `whatTheCommit.enabled` | `bool` | Bật/tắt việc gọi API WhatTheCommit để lấy message. |
| `whatTheCommit.apiUrl` | `string` | Địa chỉ URL của API WhatTheCommit. |
| `whatTheCommit.timeoutSeconds` | `int` | Thời gian chờ tối đa khi gọi API trước khi chuyển sang chế độ Offline. |
| `autoStreakRecovery.enabled` | `bool` | Tự động quét và bù commit cho các ngày bị quên trong quá khứ gần. |
| `autoStreakRecovery.checkPastDays`| `int` | Số ngày gần nhất trong quá khứ cần quét kiểm tra. |
| `autoStreakRecovery.commitsPerMissedDay`| `int` | Số commit sẽ tạo bù cho mỗi ngày bị thiếu. |
| `telegram.enabled` | `bool` | Bật/tắt tính năng gửi thông báo về Telegram. |
| `telegram.botToken` | `string` | Mã Token của Telegram Bot do `@BotFather` cấp. |
| `telegram.chatId` | `string` | Chat ID của bạn để nhận tin nhắn từ Bot. |

---

## 📱 Hướng dẫn cài đặt Telegram Bot (3 bước)

1. **Tạo Bot**: Mở Telegram, tìm **`@BotFather`**, gõ `/newbot`, đặt tên bot và username kết thúc bằng `bot`. Bạn sẽ nhận được `botToken` (dạng `7123456789:AAFx...`).
2. **Lấy Chat ID**: Tìm bot **`@userinfobot`**, bấm **Start** để lấy số `Id` của bạn. Sau đó gửi cho bot vừa tạo ở bước 1 một tin nhắn bất kỳ (ví dụ: `hi`).
3. **Cấu hình**: Điền `botToken` và `chatId` vào `config.json` và đổi `"enabled": true`.

---

## 💻 Danh sách lệnh CLI & Ví dụ

```powershell
# 1. Chạy bình thường theo config.json (đầy đủ delay và auto-heal)
dotnet run --project AutoCommit\AutoCommit.csproj

# 2. Chạy nhanh (bỏ qua độ trễ giữa các commit - dùng khi test)
dotnet run --project AutoCommit\AutoCommit.csproj -- --no-delay

# 3. Bù commit cho 1 ngày cụ thể trong quá khứ (ví dụ: 20/08/2026 với 3 commit)
dotnet run --project AutoCommit\AutoCommit.csproj -- --fill 2026-08-20 --count 3

# 4. Bù commit cho cả một dải ngày trong quá khứ
dotnet run --project AutoCommit\AutoCommit.csproj -- --fill-range 2026-08-10:2026-08-20 --no-delay

# 5. Chỉ commit local, không push lên GitHub
dotnet run --project AutoCommit\AutoCommit.csproj -- --no-push

# 6. Xem hướng dẫn sử dụng dòng lệnh
dotnet run --project AutoCommit\AutoCommit.csproj -- --help
```

---

## ⏰ Cài đặt chạy tự động bằng Windows Task Scheduler

1. Nhấn tổ hợp phím `Win + R`, nhập `taskschd.msc` và nhấn Enter.
2. Nhấn **Create Task...** ở cột bên phải:
   * **General**: Đặt tên `AutoCommit Daily`, tích chọn *Run whether user is logged on or not* (hoặc *Run only when user is logged on*).
   * **Triggers**: Chọn **New...** -> **Daily** -> Thiết lập giờ chạy (ví dụ `09:15:00 AM`).
   * **Actions**: Chọn **New...** -> **Start a program**:
     * **Program/script**: `D:\TrungHC\AutoCommit\AutoCommit\bin\Release\net8.0\AutoCommit.exe`
     * **Start in (BẮT BUỘC)**: `D:\TrungHC\AutoCommit`
3. Nhấn **OK** để hoàn tất.

---

<br><br>

---

# 🇬🇧 English

## 📖 Overview
**AutoCommit** is a robust and intelligent Git automation CLI tool developed in C# (.NET 8). It is designed to maintain a consistent, healthy, and **natural-looking** GitHub contribution graph while eliminating the common pitfalls of naive auto-commit scripts.

### Why AutoCommit v3?
* 🚀 **Human-like Commits**: Integrates with WhatTheCommit API and smart Conventional Commit generators.
* ⏳ **Smart Jitter Delay**: Emulates realistic developer workflow by pausing randomly between commits instead of bursting commits within the same second.
* 🛡️ **Streak Recovery**: Automatically detects and backdates missed days to protect your streaks.
* 📱 **Instant Telegram Alerts**: Get real-time status updates right on your smartphone.
* 🔒 **Task Scheduler Safe**: Eliminates Windows Domain / system account author overriding issues.

---

## 📜 Version History (Changelog)

### 🌟 Version 3 (Current) - *Smart & Stealth Upgrade*
* **🌐 WhatTheCommit API Integration**: Fetches humorous, realistic commit messages dynamically from `whatthecommit.com`.
* **🛡️ Offline Conventional Commits Generator**: Features a zero-dependency dynamic builder generating over **3,000+** standardized messages (e.g. `feat(api): optimize memory allocation...`) when offline.
* **⚙️ JSON Configuration (`config.json`)**: Full externalized configuration without needing to rebuild.
* **🛡️ Dual Streak Saver**:
  * **Auto-Heal Mode**: Scans recent `git log` history to automatically backdate missed days.
  * **Manual Backdate Mode**: CLI flags (`--fill`, `--fill-range`) to backdate specific dates or ranges in the past.
* **⏳ Smart Jitter Delay**: Configurable random delay (default: 15s–90s) between commits.
* **📱 Telegram Notifications**: Real-time push notifications with Markdown summary reports.
* **🔒 Task Scheduler Isolation**: Explicitly injects Git author/committer credentials per execution.

### 🔹 Version 2 - *Multi-Commit & Skewed Distribution*
* **Dynamic Commit Counts**: Configurable range of commits per execution (`[min, max]`).
* **Skewed-Low Distribution**: Random distribution favored towards smaller commit numbers (1-3 commits) to look realistic.
* **Auto Git Discovery**: Recursively traverses upwards to locate the nearest `.git` directory.
* **File Logging**: Appends commit timestamp and info into `autocommit_log.txt`.

### 🔹 Version 1 - *Initial Release*
* Basic scheduled single commit and push to remote.

---

## ⚙️ `config.json` Specification

Located at the repository root:

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
  "delaySeconds": {
    "min": 15,
    "max": 90
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

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `gitUser.name` | `string` | Author name displayed on Git commits & GitHub profile. |
| `gitUser.email` | `string` | Email associated with GitHub account (maps avatar and contributions). |
| `branch` | `string` | Target Git branch to push (e.g. `master` or `main`). |
| `commitsPerRun.min` | `int` | Minimum number of commits per execution. |
| `commitsPerRun.max` | `int` | Maximum number of commits per execution. |
| `delaySeconds.min` | `int` | Minimum delay in seconds between consecutive commits. |
| `delaySeconds.max` | `int` | Maximum delay in seconds between consecutive commits. |
| `whatTheCommit.enabled` | `bool` | Enables/disables fetching from WhatTheCommit API. |
| `whatTheCommit.apiUrl` | `string` | API endpoint for WhatTheCommit. |
| `whatTheCommit.timeoutSeconds` | `int` | Network timeout before falling back to offline generator. |
| `autoStreakRecovery.enabled` | `bool` | Automatically checks and fills missed days in recent history. |
| `autoStreakRecovery.checkPastDays`| `int` | Number of past days to check for missing activity. |
| `autoStreakRecovery.commitsPerMissedDay`| `int` | Number of commits created for each missed day. |
| `telegram.enabled` | `bool` | Enables/disables Telegram notification delivery. |
| `telegram.botToken` | `string` | Bot token provided by `@BotFather`. |
| `telegram.chatId` | `string` | Target user/group chat ID. |

---

## 📱 Telegram Bot Setup (3 Steps)

1. **Create Bot**: Open Telegram, search for **`@BotFather`**, send `/newbot`, name your bot, and obtain your `botToken`.
2. **Find Chat ID**: Open **`@userinfobot`**, press **Start** to get your numeric `Id`. Send any text message to your newly created bot to initialize the conversation.
3. **Configure**: Enter `botToken` and `chatId` in `config.json` and set `"enabled": true`.

---

## 💻 CLI Commands & Examples

```powershell
# 1. Normal execution (with natural delay and auto streak healer)
dotnet run --project AutoCommit\AutoCommit.csproj

# 2. Fast execution (bypasses delays, useful for testing)
dotnet run --project AutoCommit\AutoCommit.csproj -- --no-delay

# 3. Backdate commits for a specific date in the past
dotnet run --project AutoCommit\AutoCommit.csproj -- --fill 2026-08-20 --count 3

# 4. Backdate commits across a date range
dotnet run --project AutoCommit\AutoCommit.csproj -- --fill-range 2026-08-10:2026-08-20 --no-delay

# 5. Local commits only (skip git push)
dotnet run --project AutoCommit\AutoCommit.csproj -- --no-push

# 6. Show CLI Help
dotnet run --project AutoCommit\AutoCommit.csproj -- --help
```

---

## ⏰ Windows Task Scheduler Automation

1. Press `Win + R`, type `taskschd.msc`, and press Enter.
2. Click **Create Task...**:
   * **General**: Name the task (e.g. `AutoCommit GitHub Daily`).
   * **Triggers**: **New...** -> **Daily** -> Set execution time (e.g. `09:15 AM`).
   * **Actions**: **New...** -> **Start a program**:
     * **Program/script**: `D:\TrungHC\AutoCommit\AutoCommit\bin\Release\net8.0\AutoCommit.exe`
     * **Start in (CRITICAL)**: `D:\TrungHC\AutoCommit`
3. Click **OK** to save.

---

## 🛠️ Build & Development

Requires **.NET 8 SDK** and **Git**:

```bash
cd AutoCommit
dotnet build -c Release
```

---

## 👤 Author
* **Author:** hctrung2k4
* **Email:** hctrung2k4@gmail.com
* **Repository:** [https://github.com/nothingistoo-late/AutoCommit](https://github.com/nothingistoo-late/AutoCommit)
