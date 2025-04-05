# Hướng dẫn Cập nhật Cơ sở dữ liệu

## Vấn đề
Hiện tại có một số vấn đề với cấu trúc cơ sở dữ liệu:
1. Bảng `BacSis` thiếu các trường:
   - Email
   - DiaChi
   - NgayVaoLam
   - NgayNghiViec
   - TrangThai
   - MaKhoa

2. Bảng `KhachThamBenhs` thiếu các trường:
   - Email
   - DiaChi

## Cách cập nhật

### 1. Sao lưu dữ liệu
Trước khi thực hiện cập nhật, hãy sao lưu cơ sở dữ liệu của bạn:
```sql
BACKUP DATABASE [QuanLyBenhVienNoiTruDB] 
TO DISK = 'C:\Backup\QuanLyBenhVienNoiTruDB.bak'
WITH FORMAT, MEDIANAME = 'SQLServerBackups',
NAME = 'Full Backup of QuanLyBenhVienNoiTruDB';
```

### 2. Chạy script cập nhật
Mở SQL Server Management Studio và chạy file `update_database_structure.sql` để thêm các trường mới vào database.

### 3. Kiểm tra sau khi cập nhật
Sau khi chạy script, hãy kiểm tra cấu trúc các bảng:

```sql
-- Kiểm tra cấu trúc bảng BacSis
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'BacSis'
ORDER BY ORDINAL_POSITION;

-- Kiểm tra cấu trúc bảng KhachThamBenhs
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'KhachThamBenhs'
ORDER BY ORDINAL_POSITION;
```

### 4. Xử lý lỗi (nếu có)
Nếu gặp lỗi khi chạy script cập nhật:

1. **Lỗi khóa ngoại**: Nếu gặp lỗi khi thêm khóa ngoại, hãy đảm bảo:
   - Bảng `Khoas` đã có dữ liệu
   - Không có giá trị `MaKhoa` trong bảng `BacSis` trỏ đến một khoa không tồn tại

2. **Lỗi dữ liệu**: Nếu có dữ liệu cũ trong các bảng, bạn có thể cần cập nhật giá trị mặc định:
```sql
-- Cập nhật giá trị mặc định cho các bác sĩ hiện có
UPDATE [dbo].[BacSis]
SET NgayVaoLam = GETDATE(),
    TrangThai = 1,
    MaKhoa = 1
WHERE NgayVaoLam IS NULL;
```

## Lưu ý
- Đảm bảo sao lưu dữ liệu trước khi thực hiện cập nhật
- Kiểm tra kỹ các ràng buộc và dữ liệu hiện có
- Nếu gặp lỗi, có thể khôi phục từ bản sao lưu

## Hỗ trợ
Nếu cần hỗ trợ thêm, vui lòng liên hệ với team phát triển. 