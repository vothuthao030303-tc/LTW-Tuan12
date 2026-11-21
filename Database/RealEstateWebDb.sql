/* RealEstateWebDb - khớp project RealEstateWeb .NET 8 / EF Core.
   Chạy toàn bộ file trong SSMS. Không DROP database/bảng, không ghi đè dữ liệu.
   Tạo bảng còn thiếu; không phải script nâng cấp schema cũ đã chỉnh sửa.
   Tài khoản mẫu chỉ dành cho học tập; xem README trước khi triển khai. */
USE [master];
GO
IF DB_ID(N'RealEstateWebDb') IS NULL
BEGIN
    CREATE DATABASE [RealEstateWebDb];
END;
GO
USE [RealEstateWebDb];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Users
        (
            UserId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
            FullName NVARCHAR(100) NOT NULL,
            Email NVARCHAR(150) NOT NULL,
            PasswordHash NVARCHAR(MAX) NOT NULL,
            Phone NVARCHAR(20) NULL,
            Address NVARCHAR(255) NULL,
            Role NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Role DEFAULT N'User',
            Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Status DEFAULT N'Active',
            CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSDATETIME(),
            CONSTRAINT CK_Users_Role CHECK (Role IN (N'Admin', N'User')),
            CONSTRAINT CK_Users_Status CHECK (Status IN (N'Active', N'Inactive', N'Locked'))
        );
    END;

    IF OBJECT_ID(N'dbo.PropertyTypes', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.PropertyTypes
        (
            PropertyTypeId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PropertyTypes PRIMARY KEY,
            TypeName NVARCHAR(100) NOT NULL,
            Description NVARCHAR(500) NULL
        );
    END;

    IF OBJECT_ID(N'dbo.Locations', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Locations
        (
            LocationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Locations PRIMARY KEY,
            City NVARCHAR(100) NOT NULL,
            District NVARCHAR(100) NULL
        );
    END;

    IF OBJECT_ID(N'dbo.Properties', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Properties
        (
            PropertyId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Properties PRIMARY KEY,
            Title NVARCHAR(200) NOT NULL,
            Description NVARCHAR(MAX) NOT NULL,
            Price DECIMAL(18,2) NOT NULL,
            Area DECIMAL(10,2) NOT NULL,
            Address NVARCHAR(255) NOT NULL,
            PropertyTypeId INT NOT NULL,
            LocationId INT NOT NULL,
            UserId INT NOT NULL,
            Bedrooms INT NOT NULL CONSTRAINT DF_Properties_Bedrooms DEFAULT 0,
            Bathrooms INT NOT NULL CONSTRAINT DF_Properties_Bathrooms DEFAULT 0,
            Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Properties_Status DEFAULT N'Pending',
            MainImage NVARCHAR(255) NULL,
            IsFeatured BIT NOT NULL CONSTRAINT DF_Properties_IsFeatured DEFAULT 0,
            CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Properties_CreatedAt DEFAULT SYSDATETIME(),
            UpdatedAt DATETIME2(7) NULL,
            CONSTRAINT FK_Properties_PropertyTypes_PropertyTypeId FOREIGN KEY (PropertyTypeId)
                REFERENCES dbo.PropertyTypes(PropertyTypeId) ON DELETE NO ACTION,
            CONSTRAINT FK_Properties_Locations_LocationId FOREIGN KEY (LocationId)
                REFERENCES dbo.Locations(LocationId) ON DELETE NO ACTION,
            CONSTRAINT FK_Properties_Users_UserId FOREIGN KEY (UserId)
                REFERENCES dbo.Users(UserId) ON DELETE NO ACTION,
            CONSTRAINT CK_Properties_Price CHECK (Price >= 0),
            CONSTRAINT CK_Properties_Area CHECK (Area BETWEEN 1 AND 100000),
            CONSTRAINT CK_Properties_Bedrooms CHECK (Bedrooms BETWEEN 0 AND 50),
            CONSTRAINT CK_Properties_Bathrooms CHECK (Bathrooms BETWEEN 0 AND 50),
            CONSTRAINT CK_Properties_Status CHECK (Status IN (N'Pending', N'Approved', N'Rejected', N'Sold'))
        );
    END;

    IF OBJECT_ID(N'dbo.PropertyImages', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.PropertyImages
        (
            PropertyImageId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PropertyImages PRIMARY KEY,
            PropertyId INT NOT NULL,
            ImageUrl NVARCHAR(255) NOT NULL,
            IsMain BIT NOT NULL CONSTRAINT DF_PropertyImages_IsMain DEFAULT 0,
            CONSTRAINT FK_PropertyImages_Properties_PropertyId FOREIGN KEY (PropertyId)
                REFERENCES dbo.Properties(PropertyId) ON DELETE CASCADE
        );
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'IX_Users_Email')
        CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users(Email);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Properties') AND name = N'IX_Properties_PropertyTypeId')
        CREATE INDEX IX_Properties_PropertyTypeId ON dbo.Properties(PropertyTypeId);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Properties') AND name = N'IX_Properties_LocationId')
        CREATE INDEX IX_Properties_LocationId ON dbo.Properties(LocationId);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Properties') AND name = N'IX_Properties_UserId')
        CREATE INDEX IX_Properties_UserId ON dbo.Properties(UserId);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.PropertyImages') AND name = N'IX_PropertyImages_PropertyId')
        CREATE INDEX IX_PropertyImages_PropertyId ON dbo.PropertyImages(PropertyId);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Properties') AND name = N'IX_Properties_Status_CreatedAt')
        CREATE INDEX IX_Properties_Status_CreatedAt ON dbo.Properties(Status, CreatedAt DESC);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- DỮ LIỆU MẪU: chạy lại không thêm trùng các mẫu đã có.
-- Nếu email đã tồn tại: giữ nguyên mật khẩu, vai trò và trạng thái hiện tại.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'admin@realhome.vn')
        INSERT dbo.Users (FullName, Email, PasswordHash, Phone, Address, Role, Status, CreatedAt)
        VALUES (N'Quản trị viên', N'admin@realhome.vn', N'AQAAAAIAAYagAAAAENfBCngYPuaEYr2N+oPZJg/QEnBq1T5MeSaEMQcTrvBu3x71qi7LVxcLCSDkpYebbA==', N'0900000001', N'TP. Hồ Chí Minh', N'Admin', N'Active', SYSDATETIME());

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'user@realhome.vn')
        INSERT dbo.Users (FullName, Email, PasswordHash, Phone, Address, Role, Status, CreatedAt)
        VALUES (N'Nguyễn Minh Anh', N'user@realhome.vn', N'AQAAAAIAAYagAAAAEPYkelicZUA2b/KZFUtRALeCReDxNh10CGXwSQJWmHaBYfV+5yhLd2w7MvD1g4fOrA==', N'0900000002', N'TP. Hồ Chí Minh', N'User', N'Active', SYSDATETIME());

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'ha@realhome.vn')
        INSERT dbo.Users (FullName, Email, PasswordHash, Phone, Address, Role, Status, CreatedAt)
        VALUES (N'Trần Thu Hà', N'ha@realhome.vn', N'AQAAAAIAAYagAAAAEHmly/3V4fj0YI+Us1aJE72gMEuM3+oVIVP/m5JYnQgw7qaFIu2PCWHNr7t7fU3Mgw==', N'0900000003', N'TP. Hồ Chí Minh', N'User', N'Active', SYSDATETIME());

    IF NOT EXISTS (SELECT 1 FROM dbo.PropertyTypes WHERE TypeName = N'Căn hộ')
        INSERT dbo.PropertyTypes (TypeName, Description) VALUES (N'Căn hộ', N'Căn hộ chung cư hiện đại');
    IF NOT EXISTS (SELECT 1 FROM dbo.PropertyTypes WHERE TypeName = N'Nhà phố')
        INSERT dbo.PropertyTypes (TypeName, Description) VALUES (N'Nhà phố', N'Nhà phố, nhà riêng');
    IF NOT EXISTS (SELECT 1 FROM dbo.PropertyTypes WHERE TypeName = N'Biệt thự')
        INSERT dbo.PropertyTypes (TypeName, Description) VALUES (N'Biệt thự', N'Biệt thự cao cấp');
    IF NOT EXISTS (SELECT 1 FROM dbo.PropertyTypes WHERE TypeName = N'Đất nền')
        INSERT dbo.PropertyTypes (TypeName, Description) VALUES (N'Đất nền', N'Đất nền đầu tư');
    IF NOT EXISTS (SELECT 1 FROM dbo.Locations WHERE City = N'TP. Hồ Chí Minh' AND District = N'Quận 1')
        INSERT dbo.Locations (City, District) VALUES (N'TP. Hồ Chí Minh', N'Quận 1');
    IF NOT EXISTS (SELECT 1 FROM dbo.Locations WHERE City = N'TP. Hồ Chí Minh' AND District = N'TP. Thủ Đức')
        INSERT dbo.Locations (City, District) VALUES (N'TP. Hồ Chí Minh', N'TP. Thủ Đức');
    IF NOT EXISTS (SELECT 1 FROM dbo.Locations WHERE City = N'TP. Hồ Chí Minh' AND District = N'Quận 7')
        INSERT dbo.Locations (City, District) VALUES (N'TP. Hồ Chí Minh', N'Quận 7');

    DECLARE @OwnerId INT = (SELECT UserId FROM dbo.Users WHERE Email = N'user@realhome.vn');

    IF NOT EXISTS (SELECT 1 FROM dbo.Properties WHERE Title = N'Căn hộ cao cấp trung tâm Quận 1' AND UserId = @OwnerId)
        INSERT dbo.Properties
            (Title, Description, Price, Area, Address, PropertyTypeId, LocationId, UserId,
             Bedrooms, Bathrooms, Status, MainImage, IsFeatured, CreatedAt)
        VALUES
            (N'Căn hộ cao cấp trung tâm Quận 1', N'Không gian sống hiện đại, vị trí trung tâm, đầy đủ tiện ích.', 6200000000, 86, N'Quận 1, TP.HCM',
             (SELECT TOP (1) PropertyTypeId FROM dbo.PropertyTypes WHERE TypeName = N'Căn hộ' ORDER BY PropertyTypeId),
             (SELECT TOP (1) LocationId FROM dbo.Locations WHERE City = N'TP. Hồ Chí Minh' AND District = N'Quận 1' ORDER BY LocationId),
             @OwnerId, 2, 2, N'Approved', N'/user/images/hotel_feture_1.jpg', 1, DATEADD(DAY, -0, SYSDATETIME()));

    IF NOT EXISTS (SELECT 1 FROM dbo.Properties WHERE Title = N'Nhà phố hiện đại khu Thảo Điền' AND UserId = @OwnerId)
        INSERT dbo.Properties
            (Title, Description, Price, Area, Address, PropertyTypeId, LocationId, UserId,
             Bedrooms, Bathrooms, Status, MainImage, IsFeatured, CreatedAt)
        VALUES
            (N'Nhà phố hiện đại khu Thảo Điền', N'Nhà phố thiết kế sang trọng, khu dân cư yên tĩnh.', 12800000000, 120, N'Thảo Điền, TP. Thủ Đức',
             (SELECT TOP (1) PropertyTypeId FROM dbo.PropertyTypes WHERE TypeName = N'Nhà phố' ORDER BY PropertyTypeId),
             (SELECT TOP (1) LocationId FROM dbo.Locations WHERE City = N'TP. Hồ Chí Minh' AND District = N'TP. Thủ Đức' ORDER BY LocationId),
             @OwnerId, 4, 4, N'Approved', N'/user/images/hotel_feture_2.jpg', 1, DATEADD(DAY, -1, SYSDATETIME()));

    IF NOT EXISTS (SELECT 1 FROM dbo.Properties WHERE Title = N'Biệt thự ven sông Quận 7' AND UserId = @OwnerId)
        INSERT dbo.Properties
            (Title, Description, Price, Area, Address, PropertyTypeId, LocationId, UserId,
             Bedrooms, Bathrooms, Status, MainImage, IsFeatured, CreatedAt)
        VALUES
            (N'Biệt thự ven sông Quận 7', N'Biệt thự cao cấp, không gian xanh và riêng tư.', 24500000000, 260, N'Quận 7, TP.HCM',
             (SELECT TOP (1) PropertyTypeId FROM dbo.PropertyTypes WHERE TypeName = N'Biệt thự' ORDER BY PropertyTypeId),
             (SELECT TOP (1) LocationId FROM dbo.Locations WHERE City = N'TP. Hồ Chí Minh' AND District = N'Quận 7' ORDER BY LocationId),
             @OwnerId, 5, 5, N'Pending', N'/user/images/hotel_feture_3.jpg', 0, DATEADD(DAY, -2, SYSDATETIME()));

    IF NOT EXISTS (SELECT 1 FROM dbo.Properties WHERE Title = N'Căn hộ hai phòng ngủ Quận 7' AND UserId = @OwnerId)
        INSERT dbo.Properties
            (Title, Description, Price, Area, Address, PropertyTypeId, LocationId, UserId,
             Bedrooms, Bathrooms, Status, MainImage, IsFeatured, CreatedAt)
        VALUES
            (N'Căn hộ hai phòng ngủ Quận 7', N'Căn hộ thoáng mát, gần trường học và siêu thị.', 3500000000, 72, N'Quận 7, TP.HCM',
             (SELECT TOP (1) PropertyTypeId FROM dbo.PropertyTypes WHERE TypeName = N'Căn hộ' ORDER BY PropertyTypeId),
             (SELECT TOP (1) LocationId FROM dbo.Locations WHERE City = N'TP. Hồ Chí Minh' AND District = N'Quận 7' ORDER BY LocationId),
             @OwnerId, 2, 2, N'Approved', N'/user/images/hotel_feture_1.jpg', 1, DATEADD(DAY, -3, SYSDATETIME()));

    IF NOT EXISTS (SELECT 1 FROM dbo.Properties WHERE Title = N'Đất nền khu dân cư Thủ Đức' AND UserId = @OwnerId)
        INSERT dbo.Properties
            (Title, Description, Price, Area, Address, PropertyTypeId, LocationId, UserId,
             Bedrooms, Bathrooms, Status, MainImage, IsFeatured, CreatedAt)
        VALUES
            (N'Đất nền khu dân cư Thủ Đức', N'Đất nền trong khu dân cư, thuận tiện đi lại.', 4200000000, 90, N'TP. Thủ Đức, TP.HCM',
             (SELECT TOP (1) PropertyTypeId FROM dbo.PropertyTypes WHERE TypeName = N'Đất nền' ORDER BY PropertyTypeId),
             (SELECT TOP (1) LocationId FROM dbo.Locations WHERE City = N'TP. Hồ Chí Minh' AND District = N'TP. Thủ Đức' ORDER BY LocationId),
             @OwnerId, 0, 0, N'Approved', N'/user/images/hotel_feture_2.jpg', 0, DATEADD(DAY, -4, SYSDATETIME()));

    IF NOT EXISTS (SELECT 1 FROM dbo.Properties WHERE Title = N'Nhà phố gần công viên Quận 7' AND UserId = @OwnerId)
        INSERT dbo.Properties
            (Title, Description, Price, Area, Address, PropertyTypeId, LocationId, UserId,
             Bedrooms, Bathrooms, Status, MainImage, IsFeatured, CreatedAt)
        VALUES
            (N'Nhà phố gần công viên Quận 7', N'Nhà phố có sân nhỏ, khu vực an ninh.', 8900000000, 100, N'Quận 7, TP.HCM',
             (SELECT TOP (1) PropertyTypeId FROM dbo.PropertyTypes WHERE TypeName = N'Nhà phố' ORDER BY PropertyTypeId),
             (SELECT TOP (1) LocationId FROM dbo.Locations WHERE City = N'TP. Hồ Chí Minh' AND District = N'Quận 7' ORDER BY LocationId),
             @OwnerId, 3, 3, N'Approved', N'/user/images/hotel_feture_3.jpg', 0, DATEADD(DAY, -5, SYSDATETIME()));

    IF NOT EXISTS (SELECT 1 FROM dbo.Properties WHERE Title = N'Căn hộ một phòng ngủ Thủ Đức' AND UserId = @OwnerId)
        INSERT dbo.Properties
            (Title, Description, Price, Area, Address, PropertyTypeId, LocationId, UserId,
             Bedrooms, Bathrooms, Status, MainImage, IsFeatured, CreatedAt)
        VALUES
            (N'Căn hộ một phòng ngủ Thủ Đức', N'Căn hộ phù hợp gia đình nhỏ.', 2100000000, 48, N'TP. Thủ Đức, TP.HCM',
             (SELECT TOP (1) PropertyTypeId FROM dbo.PropertyTypes WHERE TypeName = N'Căn hộ' ORDER BY PropertyTypeId),
             (SELECT TOP (1) LocationId FROM dbo.Locations WHERE City = N'TP. Hồ Chí Minh' AND District = N'TP. Thủ Đức' ORDER BY LocationId),
             @OwnerId, 1, 1, N'Approved', N'/user/images/hotel_feture_1.jpg', 0, DATEADD(DAY, -6, SYSDATETIME()));

    IF NOT EXISTS (SELECT 1 FROM dbo.Properties WHERE Title = N'Biệt thự sân vườn Thảo Điền' AND UserId = @OwnerId)
        INSERT dbo.Properties
            (Title, Description, Price, Area, Address, PropertyTypeId, LocationId, UserId,
             Bedrooms, Bathrooms, Status, MainImage, IsFeatured, CreatedAt)
        VALUES
            (N'Biệt thự sân vườn Thảo Điền', N'Không gian sân vườn rộng, nhiều ánh sáng tự nhiên.', 32000000000, 350, N'Thảo Điền, TP. Thủ Đức',
             (SELECT TOP (1) PropertyTypeId FROM dbo.PropertyTypes WHERE TypeName = N'Biệt thự' ORDER BY PropertyTypeId),
             (SELECT TOP (1) LocationId FROM dbo.Locations WHERE City = N'TP. Hồ Chí Minh' AND District = N'TP. Thủ Đức' ORDER BY LocationId),
             @OwnerId, 5, 6, N'Approved', N'/user/images/hotel_feture_2.jpg', 1, DATEADD(DAY, -7, SYSDATETIME()));

    IF NOT EXISTS (SELECT 1 FROM dbo.Properties WHERE Title = N'Nhà phố cần bổ sung thông tin' AND UserId = @OwnerId)
        INSERT dbo.Properties
            (Title, Description, Price, Area, Address, PropertyTypeId, LocationId, UserId,
             Bedrooms, Bathrooms, Status, MainImage, IsFeatured, CreatedAt)
        VALUES
            (N'Nhà phố cần bổ sung thông tin', N'Tin mẫu để kiểm tra chức năng từ chối duyệt.', 7500000000, 80, N'Quận 1, TP.HCM',
             (SELECT TOP (1) PropertyTypeId FROM dbo.PropertyTypes WHERE TypeName = N'Nhà phố' ORDER BY PropertyTypeId),
             (SELECT TOP (1) LocationId FROM dbo.Locations WHERE City = N'TP. Hồ Chí Minh' AND District = N'Quận 1' ORDER BY LocationId),
             @OwnerId, 3, 2, N'Rejected', N'/user/images/hotel_feture_3.jpg', 0, DATEADD(DAY, -8, SYSDATETIME()));

    IF NOT EXISTS (SELECT 1 FROM dbo.Properties WHERE Title = N'Căn hộ đã bán Quận 7' AND UserId = @OwnerId)
        INSERT dbo.Properties
            (Title, Description, Price, Area, Address, PropertyTypeId, LocationId, UserId,
             Bedrooms, Bathrooms, Status, MainImage, IsFeatured, CreatedAt)
        VALUES
            (N'Căn hộ đã bán Quận 7', N'Tin mẫu kiểm tra trạng thái đã bán.', 2900000000, 65, N'Quận 7, TP.HCM',
             (SELECT TOP (1) PropertyTypeId FROM dbo.PropertyTypes WHERE TypeName = N'Căn hộ' ORDER BY PropertyTypeId),
             (SELECT TOP (1) LocationId FROM dbo.Locations WHERE City = N'TP. Hồ Chí Minh' AND District = N'Quận 7' ORDER BY LocationId),
             @OwnerId, 2, 1, N'Sold', N'/user/images/hotel_feture_1.jpg', 0, DATEADD(DAY, -9, SYSDATETIME()));

    INSERT dbo.PropertyImages (PropertyId, ImageUrl, IsMain)
    SELECT p.PropertyId, p.MainImage, 1
    FROM dbo.Properties AS p
    WHERE p.UserId = @OwnerId
      AND p.Title IN (N'Căn hộ cao cấp trung tâm Quận 1',
                N'Nhà phố hiện đại khu Thảo Điền',
                N'Biệt thự ven sông Quận 7',
                N'Căn hộ hai phòng ngủ Quận 7',
                N'Đất nền khu dân cư Thủ Đức',
                N'Nhà phố gần công viên Quận 7',
                N'Căn hộ một phòng ngủ Thủ Đức',
                N'Biệt thự sân vườn Thảo Điền',
                N'Nhà phố cần bổ sung thông tin',
                N'Căn hộ đã bán Quận 7')
      AND p.MainImage IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM dbo.PropertyImages AS i WHERE i.PropertyId = p.PropertyId);

    COMMIT TRANSACTION;
    PRINT N'Đã tạo cấu trúc và bổ sung dữ liệu mẫu RealEstateWebDb.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
