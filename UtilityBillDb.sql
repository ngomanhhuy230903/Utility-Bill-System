IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'UtilityBillDb')
BEGIN
    CREATE DATABASE UtilityBillDb;
END
GO

-- SỬ DỤNG DATABASE VỪA TẠO
USE UtilityBillDb;

GO
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL DROP TABLE dbo.Roles;
CREATE TABLE dbo.Roles (
    Id NVARCHAR(450) PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL
);

-- Bảng Users (Người dùng/Khách thuê)
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
CREATE TABLE dbo.Users (
    Id NVARCHAR(450) PRIMARY KEY,
    UserName NVARCHAR(256) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL, -- Sẽ được hash bởi ASP.NET Core Identity
    FullName NVARCHAR(256) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Bảng UserRoles (Liên kết Người dùng - Vai trò)
IF OBJECT_ID('dbo.UserRoles', 'U') IS NOT NULL DROP TABLE dbo.UserRoles;
CREATE TABLE dbo.UserRoles (
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);

-- Bảng Rooms (Phòng trọ)
IF OBJECT_ID('dbo.Rooms', 'U') IS NOT NULL DROP TABLE dbo.Rooms;
CREATE TABLE dbo.Rooms (
    Id INT PRIMARY KEY IDENTITY(1,1),
    RoomNumber NVARCHAR(50) NOT NULL UNIQUE,
    Block NVARCHAR(50) NULL,
    Floor INT NULL,
    Area DECIMAL(18, 2) NOT NULL, -- Diện tích m2
    Price DECIMAL(18, 2) NOT NULL, -- Giá thuê/tháng
    QRCodeData NVARCHAR(255) UNIQUE, -- Dữ liệu QR code (ví dụ: Guid, RoomNumber) để quét
    Status NVARCHAR(50) NOT NULL, -- Trạng thái: Vacant, Occupied, Maintenance
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
CREATE INDEX IX_Rooms_Status ON dbo.Rooms(Status);

-- Bảng TenantHistories (Lịch sử thuê phòng của khách)
IF OBJECT_ID('dbo.TenantHistories', 'U') IS NOT NULL DROP TABLE dbo.TenantHistories;
CREATE TABLE dbo.TenantHistories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    RoomId INT NOT NULL,
    TenantId NVARCHAR(450) NOT NULL,
    MoveInDate DATE NOT NULL,
    MoveOutDate DATE NULL, -- NULL nếu đang ở
    FOREIGN KEY (RoomId) REFERENCES Rooms(Id),
    FOREIGN KEY (TenantId) REFERENCES Users(Id)
);
CREATE INDEX IX_TenantHistories_RoomId ON dbo.TenantHistories(RoomId);
CREATE INDEX IX_TenantHistories_TenantId ON dbo.TenantHistories(TenantId);


-- Bảng MeterReadings (Ghi chỉ số Điện/Nước)
IF OBJECT_ID('dbo.MeterReadings', 'U') IS NOT NULL DROP TABLE dbo.MeterReadings;
CREATE TABLE dbo.MeterReadings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    RoomId INT NOT NULL,
    ReadingMonth INT NOT NULL,
    ReadingYear INT NOT NULL,
    ElectricReading DECIMAL(18, 2) NOT NULL, -- Chỉ số điện
    WaterReading DECIMAL(18, 2) NOT NULL,    -- Chỉ số nước
    ReadingDate DATETIME2 NOT NULL,
    RecordedByUserId NVARCHAR(450) NULL, -- ID của người ghi chỉ số
    FOREIGN KEY (RoomId) REFERENCES Rooms(Id),
    FOREIGN KEY (RecordedByUserId) REFERENCES Users(Id),
    UNIQUE (RoomId, ReadingMonth, ReadingYear) -- Đảm bảo mỗi phòng chỉ có 1 bản ghi/tháng
);
CREATE INDEX IX_MeterReadings_RoomMonthYear ON dbo.MeterReadings(RoomId, ReadingYear, ReadingMonth);

-- Bảng Invoices (Hóa đơn)
IF OBJECT_ID('dbo.Invoices', 'U') IS NOT NULL DROP TABLE dbo.Invoices;
CREATE TABLE dbo.Invoices (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RoomId INT NOT NULL,
    InvoicePeriodMonth INT NOT NULL,
    InvoicePeriodYear INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    DueDate DATETIME2 NOT NULL,
    TotalAmount DECIMAL(18, 2) NOT NULL,
    Status NVARCHAR(50) NOT NULL, -- Pending, Paid, Overdue, Cancelled
    FOREIGN KEY (RoomId) REFERENCES Rooms(Id)
);
CREATE INDEX IX_Invoices_Status ON dbo.Invoices(Status);
CREATE INDEX IX_Invoices_RoomPeriod ON dbo.Invoices(RoomId, InvoicePeriodYear, InvoicePeriodMonth);

-- Bảng InvoiceDetails (Chi tiết hóa đơn)
IF OBJECT_ID('dbo.InvoiceDetails', 'U') IS NOT NULL DROP TABLE dbo.InvoiceDetails;
CREATE TABLE dbo.InvoiceDetails (
    Id INT PRIMARY KEY IDENTITY(1,1),
    InvoiceId UNIQUEIDENTIFIER NOT NULL,
    Description NVARCHAR(500) NOT NULL, -- e.g., "Tiền điện tháng 5/2025 (từ 1200 kWh đến 1250 kWh)"
    Quantity DECIMAL(18, 2) NOT NULL,
    UnitPrice DECIMAL(18, 2) NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE
);
CREATE INDEX IX_InvoiceDetails_InvoiceId ON dbo.InvoiceDetails(InvoiceId);

-- Bảng Payments (Lịch sử thanh toán)
IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL DROP TABLE dbo.Payments;
CREATE TABLE dbo.Payments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    InvoiceId UNIQUEIDENTIFIER NOT NULL,
    PaymentDate DATETIME2 NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL, -- VNPAY, MOMO, STRIPE, CASH
    TransactionCode NVARCHAR(255) NULL, -- Mã giao dịch từ cổng thanh toán
    Status NVARCHAR(50) NOT NULL, -- Success, Failed, Pending
    Notes NVARCHAR(500) NULL,
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id)
);
CREATE INDEX IX_Payments_InvoiceId ON dbo.Payments(InvoiceId);

-- Bảng MaintenanceSchedules (Lịch bảo trì)
IF OBJECT_ID('dbo.MaintenanceSchedules', 'U') IS NOT NULL DROP TABLE dbo.MaintenanceSchedules;
CREATE TABLE dbo.MaintenanceSchedules (
    Id INT PRIMARY KEY IDENTITY(1,1),
    RoomId INT NULL, -- Có thể NULL nếu bảo trì cả block
    Block NVARCHAR(50) NULL,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ScheduledStart DATETIME2 NOT NULL,
    ScheduledEnd DATETIME2 NOT NULL,
    Status NVARCHAR(50) NOT NULL, -- Scheduled, InProgress, Completed, Cancelled
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedByUserId NVARCHAR(450) NOT NULL,
    FOREIGN KEY (RoomId) REFERENCES Rooms(Id),
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

-- Bảng Notifications (Thông báo)
IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL DROP TABLE dbo.Notifications;
CREATE TABLE dbo.Notifications (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(450) NOT NULL,
    Type NVARCHAR(50) NOT NULL, -- EMAIL, SMS, PUSH
    Content NVARCHAR(MAX) NOT NULL,
    SentAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsRead BIT NOT NULL DEFAULT 0,
    RelatedEntityId NVARCHAR(255) NULL, -- ID của Invoice, Maintenance... liên quan
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

BEGIN TRANSACTION;
BEGIN TRY

-- 1. Dữ liệu vai trò (Roles)
DELETE FROM UserRoles;
DELETE FROM Roles;
INSERT INTO Roles (Id, Name) VALUES
('role-admin-guid', 'Admin'),
('role-tenant-guid', 'Tenant');

-- 2. Dữ liệu người dùng (Users)
-- LƯU Ý: PasswordHash được tạo bởi Identity, ở đây chỉ là placeholder.
-- Khi tạo user trong app, hãy dùng `UserManager.CreateAsync(user, password)`
DELETE FROM Users;
INSERT INTO Users (Id, UserName, Email, PasswordHash, FullName, PhoneNumber, IsActive) VALUES
('admin-user-guid', 'admin', 'admin@email.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Quản Trị Viên', '0987654321', 1),
('tenant1-user-guid', 'nguyenvana', 'nguyenvana@email.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Nguyễn Văn A', '0123456789', 1),
('tenant2-user-guid', 'tranvanb', 'tranvanb@email.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Trần Văn B', '0123456788', 1),
('tenant3-user-guid', 'lethic', 'lethic@email.com', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Lê Thị C', '0123456787', 1);

-- 3. Phân quyền cho người dùng (UserRoles)
INSERT INTO UserRoles (UserId, RoleId) VALUES
('admin-user-guid', 'role-admin-guid'),
('tenant1-user-guid', 'role-tenant-guid'),
('tenant2-user-guid', 'role-tenant-guid'),
('tenant3-user-guid', 'role-tenant-guid');


-- 4. Dữ liệu phòng trọ (Rooms)
DELETE FROM TenantHistories;
DELETE FROM Rooms;
SET IDENTITY_INSERT Rooms ON;
INSERT INTO Rooms (Id, RoomNumber, Block, Floor, Area, Price, QRCodeData, Status)VALUES
(101, 'P101', 'A', 1, 25.5, 3000000, 'QR-ROOM-101', 'Occupied'),
(102, 'P102', 'A', 1, 22.0, 2800000, 'QR-ROOM-102', 'Occupied'),
(201, 'P201', 'A', 2, 30.0, 3500000, 'QR-ROOM-201', 'Occupied'),
(202, 'P202', 'A', 2, 25.5, 3000000, 'QR-ROOM-202', 'Vacant'),
(301, 'P301', 'B', 3, 28.0, 3200000, 'QR-ROOM-301', 'Maintenance');
SET IDENTITY_INSERT Rooms OFF;


-- 5. Lịch sử thuê phòng (TenantHistories) -> Gán khách vào phòng
-- Nguyễn Văn A (tenant1) đang ở phòng 101
INSERT INTO TenantHistories (RoomId, TenantId, MoveInDate, MoveOutDate) VALUES
(101, 'tenant1-user-guid', '2024-01-15', NULL);

-- Trần Văn B (tenant2) đang ở phòng 102
INSERT INTO TenantHistories (RoomId, TenantId, MoveInDate, MoveOutDate) VALUES
(102, 'tenant2-user-guid', '2023-11-20', NULL);

-- Lê Thị C (tenant3) ĐÃ TỪNG ở phòng 201 và bây giờ đã chuyển đi (test lịch sử)
INSERT INTO TenantHistories (RoomId, TenantId, MoveInDate, MoveOutDate) VALUES
(201, 'tenant3-user-guid', '2023-05-01', '2025-04-30');
-- Và giờ phòng 201 được người mới thuê (test gán người mới vào phòng cũ)
INSERT INTO TenantHistories (RoomId, TenantId, MoveInDate, MoveOutDate) VALUES
(201, 'tenant3-user-guid', '2025-05-10', NULL);


-- 6. Dữ liệu chỉ số điện nước (MeterReadings)
-- Current time is June 2025, so we need data for April, May, June to calculate May's bill
DELETE FROM MeterReadings;
-- Phòng 101 (Nguyễn Văn A)
INSERT INTO MeterReadings (RoomId, ReadingMonth, ReadingYear, ElectricReading, WaterReading, ReadingDate, RecordedByUserId) VALUES
(101, 4, 2025, 1200, 500, '2025-05-01', 'admin-user-guid'),
(101, 5, 2025, 1285, 525, '2025-06-01', 'admin-user-guid');
-- Phòng 102 (Trần Văn B)
INSERT INTO MeterReadings (RoomId, ReadingMonth, ReadingYear, ElectricReading, WaterReading, ReadingDate, RecordedByUserId) VALUES
(102, 4, 2025, 2500, 1100, '2025-05-01', 'admin-user-guid'),
(102, 5, 2025, 2590, 1130, '2025-06-01', 'admin-user-guid');
-- Phòng 201 (Lê Thị C)
INSERT INTO MeterReadings (RoomId, ReadingMonth, ReadingYear, ElectricReading, WaterReading, ReadingDate, RecordedByUserId) VALUES
(201, 4, 2025, 800, 300, '2025-05-01', 'admin-user-guid'),
(201, 5, 2025, 875, 318, '2025-06-01', 'admin-user-guid');

-- 7. Dữ liệu hóa đơn (Invoices & InvoiceDetails) cho kỳ T5/2025
DELETE FROM InvoiceDetails;
DELETE FROM Payments;
DELETE FROM Invoices;

-- Hóa đơn phòng 101 - Trạng thái: Chưa thanh toán (Pending)
DECLARE @Invoice101Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO Invoices (Id, RoomId, InvoicePeriodMonth, InvoicePeriodYear, CreatedAt, DueDate, TotalAmount, Status) VALUES
(@Invoice101Id, 101, 5, 2025, '2025-06-05', '2025-06-20', 3357500, 'Pending');
-- Chi tiết hóa đơn phòng 101
INSERT INTO InvoiceDetails (InvoiceId, Description, Quantity, UnitPrice, Amount) VALUES
(@Invoice101Id, 'Tiền phòng tháng 5/2025', 1, 3000000, 3000000),
(@Invoice101Id, 'Tiền điện (85 kWh)', 85, 3500, 297500), -- 1285 - 1200 = 85
(@Invoice101Id, 'Tiền nước (25 m³)', 25, 2400, 60000);  -- 525 - 500 = 25

-- Hóa đơn phòng 102 - Trạng thái: Đã thanh toán (Paid)
DECLARE @Invoice102Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO Invoices (Id, RoomId, InvoicePeriodMonth, InvoicePeriodYear, CreatedAt, DueDate, TotalAmount, Status) VALUES
(@Invoice102Id, 102, 5, 2025, '2025-06-05', '2025-06-20', 3187000, 'Paid');
-- Chi tiết hóa đơn phòng 102
INSERT INTO InvoiceDetails (InvoiceId, Description, Quantity, UnitPrice, Amount) VALUES
(@Invoice102Id, 'Tiền phòng tháng 5/2025', 1, 2800000, 2800000),
(@Invoice102Id, 'Tiền điện (90 kWh)', 90, 3500, 315000), -- 2590 - 2500 = 90
(@Invoice102Id, 'Tiền nước (30 m³)', 30, 2400, 72000);   -- 1130 - 1100 = 30

-- Hóa đơn phòng 201 - Trạng thái: Quá hạn (Overdue)
DECLARE @Invoice201Id UNIQUEIDENTIFIER = NEWID();
INSERT INTO Invoices (Id, RoomId, InvoicePeriodMonth, InvoicePeriodYear, CreatedAt, DueDate, TotalAmount, Status) VALUES
(@Invoice201Id, 201, 4, 2025, '2025-05-05', '2025-05-20', 3800000, 'Overdue');
-- Chi tiết hóa đơn phòng 201 (kỳ T4/2025)
INSERT INTO InvoiceDetails (InvoiceId, Description, Quantity, UnitPrice, Amount) VALUES
(@Invoice201Id, 'Tiền phòng tháng 4/2025', 1, 3500000, 3500000),
(@Invoice201Id, 'Tiền điện (70 kWh)', 70, 3500, 245000),
(@Invoice201Id, 'Tiền nước (23 m³)', 23, 2400, 55000);


-- 8. Dữ liệu thanh toán (Payments)
-- Thanh toán cho hóa đơn phòng 102
INSERT INTO Payments (InvoiceId, PaymentDate, Amount, PaymentMethod, TransactionCode, Status, Notes) VALUES
(@Invoice102Id, '2025-06-10', 3187000, 'VNPAY', 'VNPAY_TRN_1337', 'Success', 'Thanh toan qua cong VNPAY');


-- 9. Dữ liệu lịch bảo trì (MaintenanceSchedules)
DELETE FROM MaintenanceSchedules;
-- Lịch bảo trì ĐÃ HOÀN THÀNH cho cả block A
INSERT INTO MaintenanceSchedules (RoomId, Block, Title, Description, ScheduledStart, ScheduledEnd, Status, CreatedByUserId) VALUES
(NULL, 'A', 'Bảo trì hệ thống PCCC Block A', 'Kiểm tra toàn bộ hệ thống báo cháy và bình chữa cháy.', '2025-03-15 09:00:00', '2025-03-15 17:00:00', 'Completed', 'admin-user-guid');
-- Lịch bảo trì SẮP TỚI cho phòng 301 đang ở trạng thái bảo trì
INSERT INTO MaintenanceSchedules (RoomId, Block, Title, Description, ScheduledStart, ScheduledEnd, Status, CreatedByUserId) VALUES
(301, 'B', 'Sửa chữa điều hòa phòng 301', 'Điều hòa không lạnh, cần kiểm tra và nạp gas.', '2025-06-25 14:00:00', '2025-06-25 16:00:00', 'Scheduled', 'admin-user-guid');


-- 10. Dữ liệu thông báo (Notifications)
DELETE FROM Notifications;
-- Thông báo hóa đơn mới cho người thuê phòng 101 và 102
INSERT INTO Notifications (UserId, Type, Content, IsRead, RelatedEntityId) VALUES
('tenant1-user-guid', 'EMAIL', 'Hóa đơn tiền nhà tháng 5/2025 đã được phát hành. Vui lòng thanh toán trước ngày 20/06/2025.', 0, CAST(@Invoice101Id AS NVARCHAR(255))),
('tenant2-user-guid', 'EMAIL', 'Hóa đơn tiền nhà tháng 5/2025 đã được phát hành. Vui lòng thanh toán trước ngày 20/06/2025.', 1, CAST(@Invoice102Id AS NVARCHAR(255)));
-- Thông báo thanh toán thành công
INSERT INTO Notifications (UserId, Type, Content, IsRead, RelatedEntityId) VALUES
('tenant2-user-guid', 'SMS', 'Thanh toan hoa don so ' + CAST(@Invoice102Id AS NVARCHAR(255)) + ' tri gia 3,187,000 VND da thanh cong. Cam on ban.', 1, CAST(@Invoice102Id AS NVARCHAR(255)));
-- Thông báo nhắc lịch bảo trì
INSERT INTO Notifications (UserId, Type, Content, IsRead, RelatedEntityId) VALUES
('admin-user-guid', 'PUSH', 'Nhắc nhở: Lịch bảo trì phòng P301 sẽ diễn ra vào lúc 14:00 ngày 25/06/2025.', 0, '301');

    COMMIT TRANSACTION;
    PRINT 'Database and seed data created successfully!';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'An error occurred. Transaction rolled back.';
    THROW;
END CATCH;
GO