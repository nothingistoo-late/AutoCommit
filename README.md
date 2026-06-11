
# Auto Commit App

## Phiên bản 2

**Version 2** tập trung vào công cụ **C# (.NET 8)** trong thư mục `AutoCommit/`:

- Mỗi lần chạy tạo **nhiều commit** (số lượng ngẫu nhiên trong khoảng cấu hình được), không còn cố định một commit.
- Số commit được chọn theo phân phối **lệch về số nhỏ** (`min` của hai lần random) để thường xuyên chỉ vài commit, ít khi quá nhiều.
- Sau khi commit xong hết trong lần chạy, **một lần** `git push` lên remote (hiện tại `origin master` trong code).
- Mỗi commit ghi thêm dòng vào `autocommit_log.txt` để luôn có thay đổi để commit.
- **Đường dẫn repo** tự nhận: đi lên từ thư mục làm việc (hoặc thư mục chứa `.exe`) cho tới khi gặp `.git`; có thể truyền thêm đối số dòng lệnh nếu cần.

Chỉnh khoảng số commit trong `AutoCommit/Program.cs`: hằng `MinCommitsPerRun` và `MaxCommitsPerRun`.

---

## Giới thiệu

**Auto Commit App** dùng **Git** (và có thể kết hợp **Task Scheduler** trên Windows) để:

- Tự động commit các thay đổi trong repo đã cấu hình
- Tự động push lên remote theo lịch bạn đặt (ví dụ hằng ngày)

Mục đích:

- Giữ lịch sử làm việc đều đặn
- Duy trì **contribution graph** trên GitHub (nếu dùng cho mục đích đó)
- Tự động lưu tiến độ qua file log + commit

---

## Cách hoạt động (v2)

1. **Task Scheduler** (hoặc chạy tay) gọi bản build `AutoCommit.exe` (hoặc `dotnet run` trong thư mục project).
2. Ứng dụng **tự tìm gốc Git** (thư mục có `.git`), đặt đó làm working directory, rồi lặp:
   - Ghi thêm một dòng vào `autocommit_log.txt`
   - `git add .`
   - `git commit -m "Auto commit (i/total) - ..."`
3. Cuối vòng lặp: `git push origin master` (đổi branch/remote trong code nếu repo của bạn khác).

Nếu không có mạng, commit vẫn nằm local; khi có mạng, chạy lại hoặc push sau sẽ đồng bộ remote.

---

## Yêu cầu

- **.NET 8 SDK** (để build project `AutoCommit/AutoCommit.csproj`)
- **Git** trên PATH của tài khoản chạy task / terminal
- Remote đã cấu hình và quyền push (HTTPS/SSH credential)

---

## Hướng dẫn cài đặt

### 1. Clone project

```bash
git clone https://github.com/nothingistoo-late/AutoCommitApp.git
cd AutoCommitApp
```

### 2. Repo cần auto commit (không cần sửa đường dẫn trong code)

- **Task Scheduler:** đặt **Start in** (thư mục bắt đầu) là thư mục gốc repo (hoặc bất kỳ thư mục con nào trong repo). App sẽ đi lên cha cho tới khi thấy `.git`.
- **Chạy tay:** `cd` vào repo rồi chạy `AutoCommit.exe`, hoặc `dotnet run --project AutoCommit` từ trong repo.
- **Tuỳ chọn:** `AutoCommit.exe "D:\path\to\bất kỳ thư mục nào trong repo"` — vẫn tự tìm lên tới gốc `.git`.

Nếu branch mặc định không phải `master`, sửa lệnh `git push` trong `Program.cs`.

### 3. Build

```bash
cd AutoCommit
dotnet build -c Release
```

File chạy thường nằm tại: `AutoCommit/bin/Release/net8.0/AutoCommit.exe` (trên Windows).

### 4. Task Scheduler (Windows, tùy chọn)

- **Create Task** → trigger **Daily** theo giờ bạn chọn
- **Action**: Start a program → đường dẫn tới `AutoCommit.exe`
- **Start in**: đường dẫn tới thư mục gốc repo (để tự nhận repo đúng)
- Đảm bảo user chạy task có `git` và credential push

---

## Kiểm tra trạng thái

### Xem log commit local

```bash
git log
```

### Commit chưa push

```bash
git log origin/master..HEAD
```

(Đổi `master` nếu branch mặc định của bạn khác.)

---

## Ghi chú

- Contribution trên GitHub tính theo **ngày của commit**, không phải ngày push.
- Version 1 (mô tả cũ): một lần chạy ~ một commit đơn; **v2** thêm nhiều commit mỗi lần chạy + phân phối lệch về số nhỏ.

---

## Liên hệ

- **Tác giả:** hctrung2k4
- **Email:** hctrung2k4@gmail.com
