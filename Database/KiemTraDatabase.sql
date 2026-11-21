USE [RealEstateWebDb];
GO
SET NOCOUNT ON;
-- 1. Số lượng bản ghi. Database mới: 3 / 4 / 3 / 10 / 10.
SELECT N'Users' AS TenBang, COUNT(*) AS SoLuong FROM dbo.Users
UNION ALL SELECT N'PropertyTypes', COUNT(*) FROM dbo.PropertyTypes
UNION ALL SELECT N'Locations', COUNT(*) FROM dbo.Locations
UNION ALL SELECT N'Properties', COUNT(*) FROM dbo.Properties
UNION ALL SELECT N'PropertyImages', COUNT(*) FROM dbo.PropertyImages;

-- 2. Dữ liệu kết hợp phục vụ danh sách tin Admin.
SELECT p.PropertyId, p.Title, t.TypeName, l.City, l.District,
       u.FullName AS NguoiDang, p.Price, p.Area, p.Status, p.IsFeatured,
       CONVERT(VARCHAR(10), p.CreatedAt, 103) AS NgayDang
FROM dbo.Properties AS p
INNER JOIN dbo.PropertyTypes AS t ON t.PropertyTypeId = p.PropertyTypeId
INNER JOIN dbo.Locations AS l ON l.LocationId = p.LocationId
INNER JOIN dbo.Users AS u ON u.UserId = p.UserId
ORDER BY p.CreatedAt DESC;

-- 3. Trang chủ chỉ lấy tối đa 6 tin đã duyệt.
SELECT TOP (6) PropertyId, Title, Price, Area, MainImage
FROM dbo.Properties
WHERE Status = N'Approved'
ORDER BY IsFeatured DESC, CreatedAt DESC;

-- 4. Mô phỏng tìm kiếm, lọc giá và phân trang như PropertyController.
DECLARE @Keyword NVARCHAR(200) = N'Căn hộ';
DECLARE @MinPrice DECIMAL(18,2) = 0, @MaxPrice DECIMAL(18,2) = 7000000000;
DECLARE @TypeId INT = NULL, @LocationId INT = NULL, @Page INT = 1, @PageSize INT = 6;
SELECT PropertyId, Title, Address, Price, Area
FROM dbo.Properties
WHERE Status = N'Approved'
  AND (@Keyword IS NULL OR Title LIKE N'%' + @Keyword + N'%' OR Address LIKE N'%' + @Keyword + N'%')
  AND (@MinPrice IS NULL OR Price >= @MinPrice)
  AND (@MaxPrice IS NULL OR Price <= @MaxPrice)
  AND (@TypeId IS NULL OR PropertyTypeId = @TypeId)
  AND (@LocationId IS NULL OR LocationId = @LocationId)
ORDER BY CreatedAt DESC, PropertyId DESC
OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;

-- 5. Thống kê trạng thái: Approved / Pending / Rejected / Sold.
SELECT Status, COUNT(*) AS SoTin FROM dbo.Properties GROUP BY Status;

-- 6. Kiểm tra khóa ngoại và CHECK constraint. Không lỗi: không trả về vi phạm.
DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS;

-- 7. Xem tên, kiểu và khả năng nhận NULL của toàn bộ cột.
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH,
       NUMERIC_PRECISION, NUMERIC_SCALE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = N'dbo'
ORDER BY TABLE_NAME, ORDINAL_POSITION;
