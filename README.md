# 🚀 Auto Commit & Streak Protector

<p align="center">
  <b>A smart, natural, and resilient Git automation tool built with .NET 8</b><br>
  <i>Tự động hóa commit & push Git thông minh, đồng bộ kiến thức LeetCode & C# thật, rải thời gian xuôi chiều tuyệt đối, chống đứt chuỗi và thông báo Telegram.</i>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/Git-Automation-F05032?style=flat&logo=git&logoColor=white" alt="Git" />
  <img src="https://img.shields.io/badge/Content-LeetCode_%26_C%23_Tips-orange" alt="LeetCode & C# Tips" />
  <img src="https://img.shields.io/badge/Task_Scheduler-1--Click_Install-blue" alt="1-Click Task Scheduler" />
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
**AutoCommit** là công cụ dòng lệnh (CLI) được viết bằng C# (.NET 8) giúp duy trì hoạt động commit và lịch sử đóng góp (GitHub contribution graph) một cách **tự nhiên**, **uy tín**, và **siêu nhanh**.

### 🌟 Điểm đột phá cốt lõi:
* **🧠 Đồng bộ Kiến thức Lập trình thật (Knowledge Sync)**: Tự động kết nối **API LeetCode Problem of the Day** và kho **C# / .NET 8 Best Practices** để cập nhật nội dung học tập thật vào thư mục `notes/` (`leetcode_daily.md`, `csharp_tips.md`). Commit Diff trên GitHub trông y như một lập trình viên đang chăm chỉ giải thuật toán và ghi chú code mỗi ngày!
* **⚡ 1-Click Cài đặt Windows Task Scheduler (`--install-task`)**: Tự động đăng ký lịch chạy ngầm với đầy đủ quyền cao nhất (**RunLevel Highest / Admin**), chế độ **S4U** (chạy dù user có login hay không), và cấu hình pin laptop hoàn hảo chỉ bằng 1 dòng lệnh.
* **⏳ Động cơ thời gian xuôi chiều (Monotonic Time Engine)**: Tự động kiểm tra mốc giờ của commit gần nhất trong repo, đảm bảo các commit mới **luôn luôn tăng dần và nằm sau commit trước** (`09:46` ➡️ `10:00` ➡️ `10:20` ➡️ `10:31`), hoàn thành trong 1-2 giây.
* **🛡️ Cứu chuỗi kép (Dual Streak Protector)**: Tự động phát hiện và bù ngày bị thiếu commit trong quá khứ gần hoặc thủ công qua CLI (`--fill`, `--fill-range`).
* **📱 Thông báo tức thì qua Telegram**: Báo cáo kết quả chi tiết về điện thoại ngay sau khi hoàn tất.

---

## 📜 Lịch sử phiên bản (Changelog)

### 🌟 Phiên bản 3.3 (Hiện tại) - *Real Knowledge Sync & 1-Click Installer*
* **🧩 Tích hợp LeetCode Daily & C# Tips API**: Tự động tải đề bài LeetCode hàng ngày và snippet C# hiện đại ghi vào `notes/`. Commit message khớp 100% với nội dung cập nhật (`docs(leetcode): solve 'Binary Tree Traversal' (Medium)`).
* **🤖 1-Click Task Scheduler Installer**: Thêm lệnh `--install-task` và `--uninstall-task` tự động cấu hình quyền Administrator và LogonType S4U chuẩn xác.
* **⚡ Động cơ Thời gian xuôi chiều (Monotonic Time Engine)**: Truy vấn `git log -1` để đảm bảo thời gian luôn tiến về phía trước và rải đều trong khung giờ làm việc.
* **⚙️ Cấu hình `config.json`**: Quản lý toàn bộ thông số tác giả, LeetCode sync, Telegram, streak recovery linh hoạt.

### 🔹 Phiên bản 2 - *Multi-Commit & Skewed Distribution*
* Cho phép random số lượng commit trong khoảng `[min, max]`, phân phối lệch về số nhỏ (skewed low) và tự tìm gốc `.git`.

### 🔹 Phiên bản 1 - *Khởi tạo ban đầu*
* Chạy commit và push đơn lẻ mỗi lần thực thi theo lịch cơ bản.

---

## ⚙️ Cấu hình `config.json`

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
  "knowledgeSync": {
    "enabled": true,
    "source": "mixed",
    "notesDirectory": "notes"
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
| `knowledgeSync.enabled`| `bool` | Tự động đồng bộ đề bài LeetCode & C# Tips thật vào thư mục `notes/`. |
| `whatTheCommit.enabled` | `bool` | Bật/tắt việc gọi API WhatTheCommit để lấy message phụ. |
| `autoStreakRecovery.enabled`| `bool` | Tự động quét và bù commit cho các ngày bị quên trong quá khứ. |
| `telegram.enabled` | `bool` | Bật/tắt tính năng gửi thông báo về Telegram. |
| `telegram.botToken` | `string` | Mã Token của Telegram Bot do `@BotFather` cấp. |
| `telegram.chatId` | `string` | Chat ID của bạn để nhận tin nhắn từ Bot. |

---

## 💻 Danh sách lệnh CLI & Ví dụ

```powershell
# 1. Chạy tự động (kèm đồng bộ LeetCode & rải thời gian xuôi chiều)
dotnet run --project AutoCommit\AutoCommit.csproj

# 2. Đăng ký Windows Task Scheduler tự chạy ngầm 09:30 sáng hàng ngày (Run as Admin)
AutoCommit.exe --install-task --time "09:30"

# 3. Gỡ bỏ Windows Task Scheduler
AutoCommit.exe --uninstall-task

# 4. Bù commit cho 1 ngày cụ thể trong quá khứ (kèm kiến thức LeetCode & C#)
dotnet run --project AutoCommit\AutoCommit.csproj -- --fill 2026-08-20 --count 3

# 5. Bù commit cho cả một dải ngày trong quá khứ
dotnet run --project AutoCommit\AutoCommit.csproj -- --fill-range 2026-08-10:2026-08-20

# 6. Chỉ commit local, không push lên GitHub
dotnet run --project AutoCommit\AutoCommit.csproj -- --no-push

# 7. Xem hướng dẫn sử dụng dòng lệnh
dotnet run --project AutoCommit\AutoCommit.csproj -- --help
```

---

<br><br>

---

# 🇬🇧 English

## 📖 Overview
**AutoCommit** is an advanced Git automation CLI tool written in C# (.NET 8). It creates an authentic developer footprint by actively synchronizing **LeetCode Daily Challenge** problems and **C# / .NET 8 best practices** into your repository notes while maintaining natural chronological commit progression.

### 🌟 Key Highlights:
* 🧩 **Real Knowledge Sync**: Automatically fetches and records daily LeetCode problems & C# code snippets into `notes/` (`leetcode_daily.md`, `csharp_tips.md`).
* 🤖 **1-Click Task Scheduler Setup (`--install-task`)**: Installs automated background tasks with Administrator privilege (`RunLevel Highest`) and `LogonType S4U`.
* ⚡ **Monotonic Time Engine**: Ensures newly created commits strictly advance forward in time (`T_new > T_prev`), completing in 1-2 seconds.
* 🛡️ **Dual Streak Protector**: Auto-heals missed days in recent history and supports targeted backdating via CLI.
* 📱 **Instant Telegram Alerts**: Delivers real-time Markdown summary reports directly to your phone.

---

## 💻 CLI Commands & Examples

```powershell
# 1. Normal run (with LeetCode sync & monotonic time-scattering)
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

## 🛠️ Build & Development

```bash
cd AutoCommit
dotnet build -c Release
```

---

## 👤 Author
* **Author:** hctrung2k4
* **Email:** hctrung2k4@gmail.com
* **Repository:** [https://github.com/nothingistoo-late/AutoCommit](https://github.com/nothingistoo-late/AutoCommit)
