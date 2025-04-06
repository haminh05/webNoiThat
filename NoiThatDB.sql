-- Xóa database nếu đã tồn tại (nếu cần)
DROP DATABASE IF EXISTS bia;
GO


-- Tạo database mới
CREATE DATABASE bia;
GO
USE bia;
GO

-- Bảng Users (Tài khoản người dùng)
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) CHECK (Role IN ('Admin', 'Employee', 'Customer')) NOT NULL
);
TRUNCATE TABLE Products;


-- Bảng Products (Sản phẩm)
CREATE TABLE Products (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18,2) NOT NULL CHECK (Price >= 0),
    Stock INT NOT NULL CHECK (Stock >= 0),
    CategoryID INT NULL,
    
);
--	RESET ID
DBCC CHECKIDENT ('Products', RESEED, 0);
-- Chèn dữ liệu vào bảng Products
INSERT INTO Products (Name, Description, Price, Stock, CategoryID) VALUES 
('Gậy Bi-a Predator', 'Gậy bi-a cao cấp từ Predator với thiết kế chuyên nghiệp.', 5000000, 10, 1),
('Gậy Bi-a Fury', 'Gậy Fury vip cân bằng hoàn hảo, dành cho người chơi chuyên nghiệp.', 3500000, 15, 1),
('Gậy Bi-a Mezz', 'Gậy Mezz từ Nhật Bản, chính xác cao.', 6000000, 8, 1),
('Gậy Bi-a Adam', 'Gậy bi-a cao cấp thương hiệu Adam, sản xuất tại Đức.', 4500000, 12, 1),
('Gậy Bi-a giá rẻ', 'Gậy bi-a phù hợp cho người mới chơi, giá thành hợp lý.', 1500000, 20, 1);


DBCC CHECKIDENT ('Orders', RESEED, 0);
-- Bảng Orders (Đơn hàng)
CREATE TABLE Orders (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    ShippingAddress NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(15) NOT NULL,
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalPrice DECIMAL(18,2) NOT NULL CHECK (TotalPrice >= 0),
    Status NVARCHAR(20) CHECK (Status IN ('Pending', 'Shipping', 'Completed', 'Cancelled')) NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);
ALTER TABLE Orders ADD 
    FullName NVARCHAR(100) NOT NULL ,
    ShippingAddress NVARCHAR(255) NOT NULL ,
    PhoneNumber NVARCHAR(15) NOT NULL ;

DBCC CHECKIDENT ('OrderDetails', RESEED, 0);
-- Bảng OrderDetails (Chi tiết đơn hàng)
CREATE TABLE OrderDetails (
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(18,2) NOT NULL CHECK (UnitPrice >= 0),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID) ON DELETE CASCADE
);

-- Bảng Reviews (Đánh giá & nhận xét)
CREATE TABLE Reviews (
    ReviewID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    ProductID INT NOT NULL,
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment NVARCHAR(MAX),
    ReviewDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID) ON DELETE CASCADE
);
ALTER TABLE Reviews
ADD CONSTRAINT UQ_User_Product UNIQUE (UserID, ProductID);
-- Bảng Reports (Báo cáo & thống kê)
CREATE TABLE Reports (
    ReportID INT IDENTITY(1,1) PRIMARY KEY,
    ReportType NVARCHAR(50) NOT NULL,
    UserID INT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    Content NVARCHAR(MAX),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE SET NULL
);

-- Bảng ProductDetails (Chi tiết sản phẩm)
CREATE TABLE ProductDetails (
    ProductDetailID INT IDENTITY(1,1) PRIMARY KEY,
    ProductID INT NOT NULL UNIQUE,
    Material NVARCHAR(255) NOT NULL,
    Height DECIMAL(5,2) NOT NULL CHECK (Height > 0),
    Length DECIMAL(5,2) NOT NULL CHECK (Length > 0),
    Origin NVARCHAR(255) NOT NULL,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID) ON DELETE CASCADE
);



-- Bảng Chats (Tin nhắn hỗ trợ)
CREATE TABLE Chats (
    ChatID INT IDENTITY(1,1) PRIMARY KEY,
    SenderID INT NOT NULL,
    ReceiverID INT  NULL,
	SenderName NVARCHAR(100) NULL, 
    ReceiverName NVARCHAR(100) NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Timestamp DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (SenderID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (ReceiverID) REFERENCES Users(UserID) ON DELETE NO ACTION
);
DROP TABLE Chats;
-- Bảng UserInformation (Thông tin người dùng)
CREATE TABLE UserInformation (
    UserInformationID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    ShippingAddress NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(15) NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);
CREATE TABLE Employee (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    FullName NVARCHAR(255) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(10) NOT NULL CHECK (Gender IN ('Male', 'Female', 'Other')),
    Address NVARCHAR(500) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL UNIQUE,
    Position NVARCHAR(100) NOT NULL,
    IdentityCard NVARCHAR(20) NOT NULL UNIQUE, -- National ID
    StartDate DATE NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1, -- 1: Active (Working), 0: Inactive (Left Job)
    CONSTRAINT FK_Employee_User FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

-- Chèn dữ  liệu vào Users
INSERT INTO Users (Username, Email, PasswordHash, Role)
VALUES ('admin', 'admin11@gmail.com', ('admin123'), 'Admin');
UPDATE Users 
SET PasswordHash = ('admin123') 
WHERE Username = 'admin';


-- Kiểm tra dữ liệu sau khi tạo
SELECT * FROM Products
SELECT * FROM Users;
SELECT * FROM UserInformation;
SELECT * FROM Orders;
SELECT * FROM ProductDetails;
SELECT * FROM UserInformation;
SELECT * FROM OrderDetails;
SELECT * FROM Chats;
SELECT * FROM Reviews;
SELECT * FROM Employee;

DELETE FROM Orders
DELETE FROM Chats
DELETE FROM OrderDetails

SELECT DISTINCT Status FROM Orders;

DROP TABLE Orders
SELECT name, definition 
FROM sys.check_constraints 
WHERE parent_object_id = OBJECT_ID('Orders');

UPDATE Orders
SET Status = N'Shipping'
WHERE OrderID = 2;

INSERT INTO Products (Name, Description, Price, Stock, CategoryID, ImagePath) VALUES 
-- Bàn
(N'Bàn Làm Việc Gỗ Sồi', N'Bàn làm việc làm từ gỗ sồi tự nhiên, thiết kế hiện đại.', 2500000, 10, 1, NULL),
(N'Bàn Học Gấp Gọn', N'Bàn học tiện lợi có thể gấp gọn, phù hợp với không gian nhỏ.', 1500000, 20, 1, NULL),
(N'Bàn Ăn 6 Ghế', N'Bàn ăn cao cấp đi kèm với 6 ghế, phù hợp cho gia đình.', 5500000, 8, 1, NULL),
(N'Bàn Trà Kính Cường Lực', N'Bàn trà với mặt kính cường lực chắc chắn, thiết kế sang trọng.', 3200000, 12, 1, NULL),
(N'Bàn Gỗ Nguyên Khối', N'Bàn gỗ tự nhiên nguyên khối, phù hợp cho không gian cao cấp.', 10000000, 5, 1, NULL),

-- Ghế
(N'Ghế Văn Phòng Ergonomic', N'Ghế văn phòng thiết kế công thái học, hỗ trợ cột sống.', 3000000, 15, 2, NULL),
(N'Ghế Gaming RGB', N'Ghế chơi game với đèn LED RGB, đệm êm ái.', 4500000, 10, 2, NULL),
(N'Ghế Sofa Đơn', N'Ghế sofa đơn phong cách hiện đại, phù hợp cho phòng khách nhỏ.', 2800000, 20, 2, NULL),
(N'Ghế Ăn Gỗ Tự Nhiên', N'Ghế ăn gỗ sồi bền đẹp, thiết kế tối giản.', 1200000, 25, 2, NULL),
(N'Ghế Bập Bênh Thư Giãn', N'Ghế bập bênh bằng gỗ, giúp thư giãn thoải mái.', 3500000, 12, 2, NULL),

-- Sofa
(N'Sofa Góc Chữ L', N'Sofa góc chữ L bọc vải cao cấp, phù hợp phòng khách.', 9000000, 8, 3, NULL),
(N'Sofa Da Bò', N'Sofa bọc da bò thật, thiết kế sang trọng.', 15000000, 5, 3, NULL),
(N'Sofa Giường Thông Minh', N'Sofa có thể chuyển đổi thành giường, tiết kiệm không gian.', 7000000, 10, 3, NULL),
(N'Sofa Văng 3 Chỗ', N'Sofa văng dài 3 chỗ ngồi, phong cách hiện đại.', 6500000, 7, 3, NULL),
(N'Sofa Bọc Nhung', N'Sofa bọc nhung mềm mại, tạo cảm giác ấm cúng.', 8500000, 6, 3, NULL),

-- Giường
(N'Giường Ngủ Gỗ Sồi', N'Giường ngủ làm từ gỗ sồi chắc chắn, bền đẹp.', 7500000, 10, 4, NULL),
(N'Giường Tầng Tiện Lợi', N'Giường tầng tiết kiệm không gian, phù hợp cho trẻ em.', 5500000, 12, 4, NULL),
(N'Giường Gấp Gọn', N'Giường có thể gấp gọn, phù hợp không gian nhỏ.', 4200000, 15, 4, NULL),
(N'Giường Da Cao Cấp', N'Giường bọc da sang trọng, mang lại sự đẳng cấp.', 12000000, 6, 4, NULL),
(N'Giường Pallet Phong Cách', N'Giường phong cách pallet mộc mạc, phù hợp decor vintage.', 3800000, 18, 4, NULL);



INSERT INTO ProductDetails (ProductID, Material, Height,Length, Origin) 
VALUES 
-- Bàn
(1, N'Gỗ Sồi', 75.00, 120.00, N'Việt Nam'),
(2, N'Gỗ Công Nghiệp', 75.00, 100.00, N'Việt Nam'),
(3, N'Gỗ Tự Nhiên', 75.00, 150.00, N'Việt Nam'),
(4, N'Kính Cường Lực & Gỗ', 50.00, 100.00, N'Việt Nam'),
(5, N'Gỗ Nguyên Khối', 75.00, 200.00, N'Việt Nam'),

-- Ghế
(6, N'Nhựa & Lưới', 45.00, 60.00, N'Trung Quốc'),
(7, N'Da PU & Kim Loại', 50.00, 70.00, N'Trung Quốc'),
(8, N'Vải & Gỗ', 45.00, 65.00, N'Trung Quốc'),
(9, N'Gỗ Tự Nhiên', 45.00, 50.00, N'Trung Quốc'),
(10, N'Gỗ & Vải', 45.00, 55.00, N'Trung Quốc'),

-- Sofa
(11, N'Vải Cao Cấp', 85.00, 250.00, N'Italia'),
(12, N'Da Bò', 85.00, 220.00, N'Italia'),
(13, N'Vải & Gỗ', 85.00, 180.00, N'Italia'),
(14, N'Vải', 85.00, 200.00, N'Italia'),
(15, N'Nhung', 85.00, 230.00, N'Italia'),

-- Giường
(16, N'Gỗ Sồi', 50.00, 200.00, N'Nhật Bản'),
(17, N'Gỗ Công Nghiệp', 50.00, 190.00, N'Nhật Bản'),
(18, N'Kim Loại & Gỗ', 50.00, 180.00, N'Nhật Bản'),
(19, N'Da PU & Gỗ', 50.00, 210.00, N'Nhật Bản'),
(20, N'Gỗ Pallet', 50.00, 200.00, N'Nhật Bản');

UPDATE Products SET ImagePath ='\Image\BanLamGo_1.jpg'  WHERE ProductID=1
UPDATE Products SET ImagePath ='\Image\BanHocGapGon_2.webp'  WHERE ProductID=2
UPDATE Products SET ImagePath ='\Image\Bo-Ban-an-6-ghe_3.jpg'  WHERE ProductID=3



UPDATE Products SET ImagePath ='\Image\BanTraCuongLuc_3.jpg'  WHERE ProductID=4
UPDATE Products SET ImagePath ='\Image\ban-an-go-nguyen-khoi.jpg'  WHERE ProductID=5
UPDATE Products SET ImagePath ='\Image\Ghe-ergonomic-cong-thai-hoc.jpg'  WHERE ProductID=6
UPDATE Products SET ImagePath ='\Image\asus-rog-chariot-rgb.jpg'  WHERE ProductID=7
UPDATE Products SET ImagePath ='\Image\ghe-sofa-don.jpg'  WHERE ProductID=8
UPDATE Products SET ImagePath ='\Image\ghe-an-go-tu-nhien-gr-caro-1-1.jpg'  WHERE ProductID=9
UPDATE Products SET ImagePath ='\Image\sofa-thu-gian-bap-benh.jpg'  WHERE ProductID=10
UPDATE Products SET ImagePath ='\Image\SofaGocL.jpg'  WHERE ProductID=11
UPDATE Products SET ImagePath ='\Image\sofaDaBo.webp'  WHERE ProductID=12
UPDATE Products SET ImagePath ='\Image\sofa-giuong.jpg'  WHERE ProductID=13
UPDATE Products SET ImagePath ='\Image\sofa-vang-da-.webp'  WHERE ProductID=14
UPDATE Products SET ImagePath ='\Image\giuongGoSoi.jpg'  WHERE ProductID=16
UPDATE Products SET ImagePath ='\Image\giuong-tang-da-nag.jpg'  WHERE ProductID=17
UPDATE Products SET ImagePath ='\Image\giuong-gap.jpg'  WHERE ProductID=18
UPDATE Products SET ImagePath ='\Image\giuongDa.jpg'  WHERE ProductID=19
UPDATE Products SET ImagePath ='\Image\giuong-ngu-go-pallet-1.png'  WHERE ProductID=20


UPDATE Products SET ImagePath ='\Image\sofa-giuong.jpg'  WHERE ProductID=13
UPDATE Products SET ImagePath ='\Image\sofa-giuong.jpg'  WHERE ProductID=13
UPDATE Products SET ImagePath ='\Image\sofa-giuong.jpg'  WHERE ProductID=13