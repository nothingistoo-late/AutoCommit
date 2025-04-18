
# 🕒 Auto Commit App

## 📌 Giới thiệu
**Auto Commit App** là một ứng dụng nhỏ sử dụng **Git** và **Task Scheduler** để:
- Tự động commit các thay đổi trong thư mục dự án
- Tự động push lên GitHub vào khung giờ cố định hàng ngày (hoặc theo lịch trình tùy chỉnh)

Mục đích:
- Giữ lịch sử làm việc đều đặn mỗi ngày
- Duy trì **contribution streak** (chuỗi chấm xanh) trên GitHub
- Tự động lưu trữ tiến độ công việc

---

## ⚙️ Cách hoạt động

1. **Task Scheduler** sẽ tự động gọi file script vào đúng giờ, ví dụ `7:30 AM` mỗi ngày.
2. Script sẽ:
   - Thêm tất cả thay đổi vào Git:
     ```bash
     git add .
     ```
   - Tạo commit mới với nội dung tự động ghi ngày/giờ:
     ```bash
     git commit -m "Auto commit - ngày giờ"
     ```
   - Thử push lên GitHub:
     ```bash
     git push
     ```
3. Nếu không có mạng:
   - Commit vẫn được lưu **ở local**
   - Contribution vẫn được tính vào **ngày commit**
   - Ngày mai có mạng → push lên đầy đủ

---

## 📝 Yêu cầu
- Máy đã cài **Git**
- Cấu hình **Git remote** sẵn với GitHub
- Có sẵn **Task Scheduler** (hoặc cronjob) để chạy script định kỳ

---

## 🚀 Hướng dẫn cài đặt

### 1️⃣ Tải về project
```bash
git clone https://github.com/nothingistoo-late/AutoCommitApp.git
```

### 2️⃣ Chỉnh sửa thông tin remote (nếu cần)
```bash
git remote -v
git remote set-url origin <your-github-repo-url>
```

### 3️⃣ Tạo Task Scheduler (Windows)
- Mở **Task Scheduler**
- Chọn **Create Basic Task**
- Đặt lịch `Daily` lúc `7:30 AM`
- Ở phần **Action** chọn:
  - **Start a program**
  - Trỏ tới file `.bat` hoặc script của dự án

---

## 📈 Kiểm tra trạng thái

### Xem commit local:
```bash
git log
```

### Kiểm tra commit chưa push:
```bash
git log origin/master..HEAD
```

---

## 💡 Ghi chú
- Contribution sẽ tính theo **ngày commit**, không phải **ngày push**
- Khi mất mạng:
  - Commit vẫn được lưu **local**
  - Khi push sau, GitHub vẫn tính đúng ngày

---

## 📮 Liên hệ
- **Tác giả:** hctrung2k4  
- **Email:** hctrung2k4@gmail.com
