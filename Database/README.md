# Database SQL Server cho toàn bộ project RealEstateWeb

Thiết kế dựa trực tiếp trên RealEstateWeb.zip và note-tuan11.txt được cung cấp ngày 31/10/2026. Project thực tế dùng .NET 8, Entity Framework Core SQL Server 8.0.11 và 5 model. Bộ này cung cấp database; không thay đổi mã nguồn hay layout.

## 1. Các file

- `RealEstateWebDb.sql`: tạo database, 5 bảng, khóa chính/ngoại, chỉ mục, ràng buộc và dữ liệu mẫu.
- `KiemTraDatabase.sql`: kiểm tra số lượng, liên kết bảng, trang chủ, tìm kiếm, phân trang và ràng buộc.
- `MoHinhDuLieu.md`: quan hệ, từ điển dữ liệu và ánh xạ chức năng.

## 2. Chạy trên SQL Server Management Studio (SSMS)

1. Dừng ứng dụng trong lúc chuẩn bị database.
2. Kết nối tới đúng SQL Server mà project sẽ sử dụng: `(localdb)\MSSQLLocalDB`, `.\SQLEXPRESS` hoặc tên instance thực tế của bạn.
3. Mở `RealEstateWebDb.sql`, chạy toàn bộ file bằng Execute/F5. Script dùng `GO`, phù hợp SSMS; không gửi nguyên file này vào một lệnh SqlCommand.
4. Refresh Databases, mở `RealEstateWebDb` → Tables. Phải thấy 5 bảng trong schema dbo.
5. Mở và chạy `KiemTraDatabase.sql` trên cùng instance.
6. Sửa `ConnectionStrings:DefaultConnection` trong `appsettings.json` cho trùng instance đã chạy SQL, sau đó chạy project.

Ví dụ giữ đúng cấu hình LocalDB của project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RealEstateWebDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

Nếu dùng SQL Server Express, thay giá trị chuỗi kết nối thành:

```text
Server=.\SQLEXPRESS;Database=RealEstateWebDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

Khi ghi trong JSON, phải viết `Server=.\\SQLEXPRESS`. Giữ nguyên các mục Logging và AllowedHosts đang có.

Khi hosting dùng SQL Authentication, dùng tên server, database và tài khoản do nhà cung cấp cấp; lưu mật khẩu trong biến môi trường `ConnectionStrings__DefaultConnection`. Nếu tài khoản hosting không có quyền CREATE DATABASE, chọn database đã được cấp và chỉ chạy phần sau `USE [RealEstateWebDb];` đầu tiên, đổi tên USE thành tên database được cấp.

## 3. Dữ liệu và tài khoản demo

Database mới có 3 tài khoản, 4 loại bất động sản, 3 khu vực, 10 tin và 10 ảnh. Có 7 tin Approved để thử phân trang 6 tin/trang, 1 Pending, 1 Rejected và 1 Sold. Ảnh sử dụng đúng 3 file hotel_feture_1.jpg, hotel_feture_2.jpg, hotel_feture_3.jpg có sẵn trong wwwroot/user/images; đây là ảnh minh họa từ template.

| Vai trò | Email | Mật khẩu demo |
|---|---|---|
| Admin | admin@realhome.vn | Admin@123 |
| User | user@realhome.vn | User@123 |
| User | ha@realhome.vn | User@123 |

PasswordHash dùng định dạng ASP.NET Core Identity V3: PBKDF2-HMAC-SHA512, 100000 vòng, salt riêng từng tài khoản. Phù hợp cách xác minh PasswordHasher<User> đang dùng trong AccountController. Không lưu mật khẩu thô vào bảng và không thay hash bằng HASHBYTES/SHA256.

Các tài khoản trên dành cho demo. Đổi mật khẩu hoặc bỏ tài khoản demo trước khi đưa lên hosting công khai. Project hiện chưa có trang đổi mật khẩu.

## 4. Chạy lại và database đã tồn tại

- Script không xóa database, không DROP bảng, không ghi đè tài khoản hay tin có sẵn. Chạy lại tuần tự sẽ bỏ qua mẫu đã tồn tại theo email, tên loại, khu vực, hoặc cặp tiêu đề/người đăng.
- Nếu email demo đã có, script giữ nguyên mật khẩu, Role và Status; mật khẩu demo trong bảng hướng dẫn không bảo đảm áp dụng cho tài khoản cũ.
- Trên database có sẵn, số bản ghi có thể khác số lượng demo nêu trên.
- Bảng đã tồn tại được giữ nguyên. Script không tự sửa bảng có cấu trúc khác model và không bổ sung CHECK constraint vào các bảng đã có; các CHECK khai báo trong CREATE TABLE áp dụng khi tạo bảng mới. Cần migration riêng nếu schema cũ không khớp.
- Schema và seed dùng hai transaction riêng. Nếu seed lỗi, phần schema đã tạo vẫn còn, dữ liệu seed trong lượt lỗi được rollback. SSMS hiển thị lỗi; sửa nguyên nhân trước khi chạy lại.
- Không chạy đồng thời nhiều bản script trên một database.

## 5. Khởi động trong bản hoàn thiện tuần 11

Bản tuần 11 đặt `Database:InitializeDemo=false`: chạy bộ SQL này trước khi mở web. Program.cs chỉ gọi EnsureCreated/DbSeeder khi bật tùy chọn đó. Không cần migration để dùng bộ SQL này. Không chạy Update-Database tùy ý vì project chưa có migrations.

## 6. Kiểm tra trên ứng dụng

1. Trang `/`: có tin Approved; chọn loại/khu vực để lọc.
2. `/Property`: có 7 tin Approved trên database mới; kiểm tra trang 2, từ khóa và khoảng giá.
3. `/Account/Login`: thử tài khoản Admin; vào `/Admin` xem thống kê.
4. `/Admin/PropertyTypes`: thêm/sửa/xóa một loại mới chưa có tin. Loại đã có tin được controller chặn xóa.
5. `/Admin/Properties`: thêm/sửa/xóa tin, duyệt Pending thành Approved hoặc từ chối thành Rejected. Tin Approved xuất hiện ngoài trang User.
6. Đăng nhập User: không được vào các controller Admin hiện có.

## 7. Bản hoàn thiện tuần 11

Đã có CRUD Admin cho cả 5 bảng; trang chi tiết hiển thị bộ ảnh. Chức năng chọn nộp cá nhân là Properties. Trang chủ có thông tin sinh viên từ teamsettings.json và liên kết tới chức năng. Xem README.md, DEMO_TUAN11.md và HUONG_DAN_HOSTING.md ở thư mục project.

## 8. Phạm vi kiểm chứng

Đã đối chiếu tên bảng/cột, kiểu dữ liệu, độ dài, nullable và quan hệ với cả 5 model cùng ApplicationDbContext; kiểm tra giá trị trạng thái với form Admin; kiểm tra đường dẫn ảnh tồn tại; giải mã và tính lại PBKDF2 của cả 3 hash demo. Môi trường thực hiện không có SQL Server/sqlcmd hoặc .NET SDK, nên chưa thực thi script trên SQL Server và chưa chạy thử đăng nhập bằng ứng dụng. Chạy file kiểm tra và các bước mục 6 trên máy bạn để xác nhận thực tế.
