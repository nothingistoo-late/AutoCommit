# AutoCommit - Hướng Dẫn Cài Đặt & Sử Dụng (User Guide)

<p align="center">
  <img src="AutoCommit.png" alt="AutoCommit Banner" width="650" /><br>
  <b>Tài liệu hướng dẫn tải về, cài đặt và cấu hình AutoCommit từ A đến Z.</b>
</p>

---

## 🌐 Chọn ngôn ngữ / Choose Language
* 🇻🇳 [Tiếng Việt](#-tiếng-việt)
* 🇬🇧 [English](#-english)

---

# 🇻🇳 Tiếng Việt

## 📑 Mục lục
1. [Tải ứng dụng từ GitHub Releases](#1-tải-ứng-dụng-từ-github-releases)
2. [Lựa chọn phương thức cài đặt](#2-lựa-chọn-phương-thức-cài-đặt)
   - [Cách A: Cài đặt bằng bộ Setup Wizard (Khuyên dùng)](#cách-a-cài-đặt-bằng-bộ-setup-wizard-khuyên-dùng)
   - [Cách B: Sử dụng bản Portable giải nén dùng ngay](#cách-b-sử-dụng-bản-portable-giải-nén-dùng-ngay)
3. [Cấu hình thông tin lần đầu (config.json)](#3-cấu-hình-thông-tin-lần-đầu-configjson)
4. [Sử dụng các phím tắt 1-Click](#4-sử-dụng-các-phím-tắt-1-click)
5. [Cài đặt chạy tự động hàng ngày (Windows Task Scheduler)](#5-cài-đặt-chạy-tự-động-hàng-ngày-windows-task-scheduler)
6. [Gỡ cài đặt ứng dụng (Uninstall)](#6-gỡ-cài-đặt-ứng-dụng-uninstall)

---

## 1. Tải ứng dụng từ GitHub Releases

1. Truy cập vào trang phát hành chính thức của dự án:
   👉 **[https://github.com/nothingistoo-late/AutoCommit/releases](https://github.com/nothingistoo-late/AutoCommit/releases)**
2. Tại phiên bản mới nhất (ví dụ `v3.4.0`), trong mục **Assets**, bạn sẽ thấy 2 tệp tải về:
   * 📦 **`AutoCommit_Setup.exe`**: Bộ cài đặt tự động từng bước (Next ➡️ Next ➡️ Finish).
   * 📁 **`AutoCommit_Portable.zip`**: Bản nén giải nén dùng ngay không cần cài đặt.

---

## 2. Lựa chọn phương thức cài đặt

### Cách A: Cài đặt bằng bộ Setup Wizard (Khuyên dùng)
1. Tải file **`AutoCommit_Setup.exe`** về máy tính của bạn.
2. Nhấp đúp chuột vào file `AutoCommit_Setup.exe` để mở trình cài đặt.
3. Làm theo hướng dẫn trên màn hình:
   * **Chọn thư mục cài đặt:** Mặc định là `C:\Program Files\AutoCommit` (hoặc bạn có thể chọn thư mục khác).
   * **Tùy chọn Icon:** Tích chọn *Create a desktop shortcut* nếu muốn tạo biểu tượng ngoài màn hình Desktop.
   * Nhấn **Install** để tiến hành cài đặt.
4. Sau khi cài xong, nhấn **Finish**.

### Cách B: Sử dụng bản Portable giải nén dùng ngay
1. Tải file **`AutoCommit_Portable.zip`** về máy.
2. Nhấp chuột phải vào file zip và chọn **Extract All... (Giải nén)** vào thư mục bạn muốn lưu (ví dụ: `D:\Tools\AutoCommit`).
3. Bạn có thể sử dụng ngay mà không cần chạy cài đặt.

---

## 3. Cấu hình thông tin lần đầu (`config.json`)

Khi mở thư mục cài đặt của ứng dụng, bạn sẽ thấy file **`config.example.json`**:

1. Sao chép và đổi tên `config.example.json` thành **`config.json`** (hoặc ứng dụng sẽ tự động tạo giúp bạn trong lần chạy đầu tiên).
2. Mở file `config.json` bằng Notepad hoặc VS Code để chỉnh sửa thông tin của bạn:

```json
{
  "gitUser": {
    "name": "YOUR_GITHUB_USERNAME",
    "email": "YOUR_GITHUB_EMAIL@gmail.com"
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
    "rust"
  ],
  "knowledgeSync": {
    "enabled": true,
    "solutionsDirectory": "solutions"
  },
  "telegram": {
    "enabled": false,
    "botToken": "YOUR_BOT_TOKEN_HERE",
    "chatId": "YOUR_CHAT_ID_HERE"
  }
}
```

> **Lưu ý quan trọng:** `email` bắt buộc phải khớp với email tài khoản GitHub của bạn để chuỗi đóng góp (ô vuông xanh) và avatar được ghi nhận chính xác.

---

## 4. Sử dụng các phím tắt 1-Click

Trong thư mục **`notes/`** (hoặc trong menu Start Menu), có sẵn các file tiện ích để bạn chỉ cần nhấp đúp là thực thi ngay:

| Tên file phím tắt | Chức năng |
| :--- | :--- |
| **`1_Chay_Tu_Dong.bat`** | Chạy ngay quy trình commit giải thuật LeetCode đa ngôn ngữ và đẩy lên GitHub. |
| **`2_Dang_Ky_Task_Scheduler.bat`** | Tự động đăng ký lịch chạy ngầm hàng ngày (09:30 sáng) với quyền Admin cao nhất. |
| **`3_Go_Bo_Task_Scheduler.bat`** | Gỡ bỏ lịch chạy ngầm khỏi Windows Task Scheduler. |
| **`4_Bu_Commit_Dai_Ngay.bat`** | Nhập khoảng ngày để bù commit cho những ngày quên chạy trong quá khứ. |
| **`5_Xem_Tro_Giup.bat`** | Xem toàn bộ các tùy chọn và cờ tham số dòng lệnh CLI. |

---

## 5. Cài đặt chạy tự động hàng ngày (Windows Task Scheduler)

Để AutoCommit tự động chạy ngầm mỗi ngày mà bạn không cần bận tâm mở máy bấm tay:

1. Nhấp đúp vào file **`notes/2_Dang_Ky_Task_Scheduler.bat`** (hoặc chọn trong Start Menu).
2. Khi bảng màu đen hiện lên, nhập giờ bạn muốn chạy (ví dụ `09:30`) rồi nhấn **Enter**.
3. Cửa sổ quyền Administrator (UAC) sẽ hiện lên xác nhận. Bấm **Yes**.
4. Xong! Kể từ bây giờ máy tính sẽ tự động chạy ngầm mỗi ngày.

---

## 6. Gỡ cài đặt ứng dụng (Uninstall)

Nếu bạn đã cài bằng bộ Setup Wizard:
* Vào **Start Menu** ➡️ Tìm mục **AutoCommit** ➡️ Chọn **Uninstall AutoCommit**.
* Hoặc mở **Windows Settings ➡️ Apps & Features ➡️ Tìm AutoCommit ➡️ Nhấn Uninstall**.

---

<br><br>

---

# 🇬🇧 English

## 📑 Table of Contents
1. [Download from GitHub Releases](#1-download-from-github-releases)
2. [Choose Installation Method](#2-choose-installation-method)
   - [Option A: Windows Setup Wizard (Recommended)](#option-a-windows-setup-wizard-recommended)
   - [Option B: Portable ZIP Package](#option-b-portable-zip-package)
3. [First-Time Configuration (config.json)](#3-first-time-configuration-configjson)
4. [Using 1-Click Batch Shortcuts](#4-using-1-click-batch-shortcuts)
5. [Automated Daily Execution (Task Scheduler)](#5-automated-daily-execution-task-scheduler)
6. [Uninstallation](#6-uninstallation)

---

## 1. Download from GitHub Releases

1. Visit the official GitHub Releases page:
   👉 **[https://github.com/nothingistoo-late/AutoCommit/releases](https://github.com/nothingistoo-late/AutoCommit/releases)**
2. Under the latest version (e.g. `v3.4.0`) in the **Assets** section, choose your preferred package:
   * 📦 **`AutoCommit_Setup.exe`**: Full automated installer wizard with Desktop and Start Menu shortcuts.
   * 📁 **`AutoCommit_Portable.zip`**: Standalone portable archive (extract and run).

---

## 2. Choose Installation Method

### Option A: Windows Setup Wizard (Recommended)
1. Download **`AutoCommit_Setup.exe`**.
2. Double-click `AutoCommit_Setup.exe` to launch the Setup Wizard.
3. Follow the on-screen steps:
   * **Destination Folder:** Default is `C:\Program Files\AutoCommit`.
   * **Shortcuts:** Optionally check *Create a desktop shortcut*.
   * Click **Install** to complete installation.
4. Click **Finish**.

### Option B: Portable ZIP Package
1. Download **`AutoCommit_Portable.zip`**.
2. Right-click the `.zip` file and choose **Extract All...** to your desired directory (e.g. `D:\Tools\AutoCommit`).
3. Run `AutoCommit.exe` directly.

---

## 3. First-Time Configuration (`config.json`)

Inside the application folder, locate `config.example.json`:
1. Rename or copy `config.example.json` to **`config.json`**.
2. Edit `config.json` with your GitHub credentials:

```json
{
  "gitUser": {
    "name": "YOUR_GITHUB_USERNAME",
    "email": "YOUR_GITHUB_EMAIL@gmail.com"
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
    "rust"
  ],
  "telegram": {
    "enabled": false,
    "botToken": "YOUR_BOT_TOKEN_HERE",
    "chatId": "YOUR_CHAT_ID_HERE"
  }
}
```

---

## 4. Using 1-Click Batch Shortcuts

Located in the **`notes/`** folder (and accessible via the Start Menu folder):

- **`1_Chay_Tu_Dong.bat`**: Runs AutoCommit immediately.
- **`2_Dang_Ky_Task_Scheduler.bat`**: Registers background Task Scheduler daily automation with Admin rights.
- **`3_Go_Bo_Task_Scheduler.bat`**: Uninstalls background Task Scheduler job.
- **`4_Bu_Commit_Dai_Ngay.bat`**: Prompts for a date range to backdate missed commit streaks.
- **`5_Xem_Tro_Giup.bat`**: Displays full CLI reference.

---

## 5. Automated Daily Execution (Task Scheduler)

1. Double-click **`notes/2_Dang_Ky_Task_Scheduler.bat`**.
2. Enter your desired daily runtime (e.g. `09:30`) and press **Enter**.
3. Accept the Administrator prompt (UAC). Done!

---

## 6. Uninstallation

* Open **Start Menu** ➡️ **AutoCommit** ➡️ **Uninstall AutoCommit**.
* Or navigate to **Windows Settings ➡️ Apps ➡️ Installed Apps ➡️ AutoCommit ➡️ Uninstall**.
