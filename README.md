# 🚀 Auto Commit & Streak Protector

<p align="center">
  <b>A smart, natural, and resilient Git automation tool built with .NET 8</b><br>
  <i>Tự động hóa commit & push Git thông minh, rải thời gian tự nhiên tức thì, chống đứt chuỗi xanh và thông báo Telegram.</i>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/Git-Automation-F05032?style=flat&logo=git&logoColor=white" alt="Git" />
  <img src="https://img.shields.io/badge/Speed-Instant_Time_Scatter-success" alt="Instant Time-Scatter" />
  <img src="https://img.shields.io/badge/Telegram-Notifications-2CA5E0?style=flat&logo=telegram&logoColor=white" alt="Telegram" />
  <img src="https://img.shields.io/badge/License-MIT-blue" alt="License" />
</p>

---

## 🌐 Chọn ngôn ngữ / Choose Language
* 🇻🇳 [Tiếng Việt](#-tiếng-việt)
* 🇬🇧 [English](#-english)

---

# 🇻🇳 Tiếng Việt

## 📖 Giới thiệu
**AutoCommit** là công cụ dòng lệnh (CLI) được viết bằng C# (.NET 8) giúp duy trì hoạt động commit và lịch sử đóng góp (GitHub contribution graph) một cách **tự nhiên**, **an toàn**, và **siêu nhanh**.

### ⚡ Điểm đột phá ở Phiên bản 3.1:
* **Không cần ngồi chờ (Zero Wait)**: Toàn bộ quá trình commit và push chỉ mất **1 – 2 giây**.
* **Rải mốc thời gian tự nhiên (Instant Time-Scattering)**: Các commit tự động mang các mốc thời gian rải rác từ sáng đến chiều (`09:15`, `11:40`, `15:30`), tạo lịch sử đóng góp như người thật làm việc cả ngày!
* **Tránh lộ bot 100%**: Sử dụng WhatTheCommit API + Bộ sinh Conventional Commit chất lượng cao.
* **Không sợ mất chuỗi (Streak Healer)**: Tự động phát hiện và bù ngày bị thiếu commit.
* **Thông báo tức thì qua Telegram**: Gửi kết quả về điện thoại ngay sau khi push.

---

## 📜 Lịch sử phiên bản (Changelog)

### 🌟 Phiên bản 3.1 (Hiện tại) - *Instant Time-Scattering & Stealth Upgrade*
* **⚡ Động cơ Rải mốc thời gian tức thì (Instant Time-Scattering)**: Tự động phân bổ các mốc commit ngẫu nhiên tăng dần trong khung giờ làm việc (`08:30` -> `Hiện tại`) mà không cần `Task.Delay` ngồi đợi, hoàn thành mọi thứ trong 1-2 giây.
* **🌐 Tích hợp API WhatTheCommit**: Lấy commit message hài hước, ngẫu nhiên từ `whatthecommit.com`.
* **🛡️ Bộ sinh Conventional Commits Offline**: Khi mất mạng, tự động chuyển sang bộ ghép từ chuẩn sinh ra hơn **3.000+ commit message** (`feat(api): optimize memory allocation...`), đảm bảo app không bao giờ crash.
* **⚙️ Cấu hình `config.json`**: Tách toàn bộ thiết lập ra ngoài file JSON, không cần compile lại source code.
* **🛡️ Cứu chuỗi kép (Dual Streak Saver)**:
  * **Tự động (Auto-Heal)**: Quét `git log`, tự phát hiện ngày hôm qua/quá khứ gần bị thiếu commit và tự động tạo commit bù.
  * **Thủ công (Manual Backdate)**: Hỗ trợ lệnh CLI bù ngày bất kỳ (`--fill`) hoặc cả khoảng ngày (`--fill-range`).
* **📱 Thông báo Telegram**: Bắn báo cáo tổng kết kèm trạng thái push về điện thoại qua Telegram Bot.
* **🔒 Khắc phục triệt để Task Scheduler**: Gắn cứng thông tin tác giả vào Git environment của process.

### 🔹 Phiên bản 2 - *Multi-Commit & Skewed Distribution*
* **Nhiều commit mỗi lần chạy**: Cho phép random số lượng commit trong khoảng `[min, max]`.
* **Phân phối lệch về số nhỏ (Skewed Low)**: Lấy giá trị nhỏ nhất của 2 lần random để đa số chỉ tạo 1–3 commit/ngày.
* **Tự động dò tìm gốc Git (`.git`)**: Đi ngược từ thư mục hiện tại lên cây thư mục cha cho đến khi tìm thấy repository.

### 🔹 Phiên bản 1 - *Khởi tạo ban đầu*
* Chạy commit và push đơn lẻ mỗi lần thực thi theo lịch cơ bản.

---

## ⚙️ Cấu hình `config.json`

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
| `gitUser.email` | `string` | Email liên kết với GitHub (quyết định Avatar & Contribution graph). |
| `branch` | `string` | Tên nhánh Git để đẩy code lên (`master` hoặc `main`). |
| `commitsPerRun.min` | `int` | Số lượng commit tối thiểu mỗi lần chạy. |
| `commitsPerRun.max` | `int` | Số lượng commit tối đa mỗi lần chạy. |
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
# 1. Chạy tự động (rải mốc thời gian tức thì theo config.json)
dotnet run --project AutoCommit\AutoCommit.csproj

# 2. Bù commit cho 1 ngày cụ thể trong quá khứ (ví dụ: 20/08/2026 với 3 commit rải rác)
dotnet run --project AutoCommit\AutoCommit.csproj -- --fill 2026-08-20 --count 3

# 3. Bù commit cho cả một dải ngày trong quá khứ
dotnet run --project AutoCommit\AutoCommit.csproj -- --fill-range 2026-08-10:2026-08-20

# 4. Chỉ commit local, không push lên GitHub
dotnet run --project AutoCommit\AutoCommit.csproj -- --no-push

# 5. Xem hướng dẫn sử dụng dòng lệnh
dotnet run --project AutoCommit\AutoCommit.csproj -- --help
```

---

## ⏰ Cài đặt chạy tự động bằng Windows Task Scheduler

1. Nhấn tổ hợp phím `Win + R`, nhập `taskschd.msc` và nhấn Enter.
2. Nhấn **Create Task...**:
   * **General**: Đặt tên `AutoCommit Daily`, tích chọn *Run whether user is logged on or not*.
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
**AutoCommit** is an advanced Git automation CLI tool written in C# (.NET 8). It keeps your GitHub contribution graph alive and **realistic** by using an **Instant Time-Scattering Engine** — achieving natural daily activity across working hours in just 1-2 seconds without freezing your computer.

### ⚡ Highlights in Version 3.1:
* **Instant Execution (Zero Wait)**: Commits and pushes in under 2 seconds.
* **Instant Time-Scattering Engine**: Commits carry realistic, ascending timestamps scattered across the working day (`09:15`, `11:40`, `15:30`).
* **WhatTheCommit & Conventional Commits**: Rich, humorous, and natural commit messages.
* **Dual Streak Protector**: Auto-heals missed days and supports manual backdating via CLI.
* **Instant Telegram Alerts**: Real-time push notifications sent straight to your phone.

---

## 📜 Version History (Changelog)

### 🌟 Version 3.1 (Current) - *Instant Time-Scattering & Stealth Upgrade*
* **⚡ Instant Time-Scattering Engine**: Replaces long sleep delays by dynamically calculating sorted, realistic timestamps throughout working hours, finishing everything in seconds.
* **🌐 WhatTheCommit API Integration**: Fetches dynamic commit messages from `whatthecommit.com`.
* **🛡️ Offline Conventional Commits Generator**: Features a zero-dependency generator creating **3,000+** realistic commit messages when offline.
* **⚙️ External Configuration (`config.json`)**: Easily configure authors, branches, and features without rebuilding.
* **🛡️ Dual Streak Saver**:
  * **Auto-Heal Mode**: Scans past history and backdates missed days.
  * **Manual Backdate Mode**: CLI flags (`--fill`, `--fill-range`) for targeted date ranges.
* **📱 Telegram Alerts**: Instant push notifications upon task completion or error.

### 🔹 Version 2 - *Multi-Commit & Skewed Distribution*
* **Dynamic Commit Counts**: Configurable range of commits per execution (`[min, max]`).
* **Skewed-Low Distribution**: Favors fewer commits per run for realism.
* **Auto Git Discovery**: Automatically traverses parent directories to find `.git`.

### 🔹 Version 1 - *Initial Release*
* Basic scheduled single commit and push to remote.

---

## ⚙️ `config.json` Specification

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

## 💻 CLI Commands & Examples

```powershell
# 1. Normal run (with instant time-scattering and auto-heal)
dotnet run --project AutoCommit\AutoCommit.csproj

# 2. Backdate commits for a specific date in the past
dotnet run --project AutoCommit\AutoCommit.csproj -- --fill 2026-08-20 --count 3

# 3. Backdate commits across a date range
dotnet run --project AutoCommit\AutoCommit.csproj -- --fill-range 2026-08-10:2026-08-20

# 4. Local commits only (skip git push)
dotnet run --project AutoCommit\AutoCommit.csproj -- --no-push

# 5. Show CLI Help
dotnet run --project AutoCommit\AutoCommit.csproj -- --help
```

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
