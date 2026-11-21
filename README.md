# Tuan11 — Bài cá nhân quản lý bất động sản

Sinh viên: Võ Thu Thảo — MSSV 2001240454.

- Không cần đăng nhập để mở /Admin/Properties và thực hiện CRUD bất động sản.
- Trang chủ có thông tin sinh viên và liên kết Trang quản lý bất động sản.
- Tên hiển thị ở menu, tiêu đề trình duyệt và footer là Tuan11.
- Giữ giao diện đơn giản: bảng dữ liệu và form Thêm/Sửa/Xem/Xóa.
- Tin tạo mới mặc định Đã duyệt, giá hiển thị gọn như 10 tỷ 2.
- Giữ kiểm tra dữ liệu, khóa ngoại, ảnh và chống CSRF trên form POST.

## Chạy

Giữ appsettings.json có kết nối SQL đúng trên máy bạn. Thay teamsettings.json bằng file trong bản này để cập nhật tên/MSSV. Không cần tạo lại database nếu đã chạy bản trước.

```powershell
dotnet restore
dotnet build
dotnet run
```

Mở http://localhost:5188 → Trang quản lý bất động sản. Giữ nguyên tên project/namespace RealEstateWeb và database RealEstateWebDb để không ảnh hưởng kết nối/mã nguồn; chỉ đổi tên hiển thị thành Tuan11.

Các module tài khoản/danh mục của bản nhóm vẫn nằm trong source nhưng không cần dùng để nộp chức năng bất động sản. Chỉ hai controller Properties và Home trong Area Admin được mở không cần đăng nhập; không gỡ phân quyền của các module khác.

Đã kiểm tra tĩnh; môi trường tạo file chưa có .NET SDK và SQL Server nên chưa build hoặc chạy CRUD trực tiếp. Chưa upload hosting.
