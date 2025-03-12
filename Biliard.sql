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

-- Bảng Categories (Danh mục sản phẩm)
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE
);

-- Chèn danh mục sản phẩm vào ngay sau khi tạo bảng
INSERT INTO Categories (Name) VALUES 
('Cơ Bi-a'), 
('Phụ kiện Bi-a');

-- Bảng Products (Sản phẩm)
CREATE TABLE Products (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18,2) NOT NULL CHECK (Price >= 0),
    Stock INT NOT NULL CHECK (Stock >= 0),
    CategoryID INT NULL,
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID) ON DELETE SET NULL
);

-- Chèn dữ liệu vào bảng Products
INSERT INTO Products (Name, Description, Price, Stock, CategoryID) VALUES 
('Gậy Bi-a Predator', 'Gậy bi-a cao cấp từ Predator với thiết kế chuyên nghiệp.', 5000000, 10, 1),
('Gậy Bi-a Fury', 'Gậy Fury vip cân bằng hoàn hảo, dành cho người chơi chuyên nghiệp.', 3500000, 15, 1),
('Gậy Bi-a Mezz', 'Gậy Mezz từ Nhật Bản, chính xác cao.', 6000000, 8, 1),
('Gậy Bi-a Adam', 'Gậy bi-a cao cấp thương hiệu Adam, sản xuất tại Đức.', 4500000, 12, 1),
('Gậy Bi-a giá rẻ', 'Gậy bi-a phù hợp cho người mới chơi, giá thành hợp lý.', 1500000, 20, 1);

-- Bảng Orders (Đơn hàng)
CREATE TABLE Orders (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalPrice DECIMAL(18,2) NOT NULL CHECK (TotalPrice >= 0),
    Status NVARCHAR(20) CHECK (Status IN ('Pending', 'Shipping', 'Completed', 'Cancelled')) NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);

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
    Weight DECIMAL(5,2) NOT NULL CHECK (Weight > 0),
    Length DECIMAL(5,2) NOT NULL CHECK (Length > 0),
    Origin NVARCHAR(255) NOT NULL,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID) ON DELETE CASCADE
);

-- Bảng Chats (Tin nhắn hỗ trợ)
CREATE TABLE Chats (
    ChatID INT IDENTITY(1,1) PRIMARY KEY,
    SenderID INT NOT NULL,
    ReceiverID INT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Timestamp DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (SenderID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (ReceiverID) REFERENCES Users(UserID) ON DELETE NO ACTION
);

-- Bảng UserInformation (Thông tin người dùng)
CREATE TABLE UserInformation (
    UserInformationID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    ShippingAddress NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(15) NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);

-- Chèn dữ liệu vào Users
INSERT INTO Users (Username, Email, PasswordHash, Role)
VALUES ('admin', 'admin@gmail.com', HASHBYTES('SHA2_256', 'admin123'), 'Admin');

-- Kiểm tra dữ liệu sau khi tạo
SELECT * FROM Products;
SELECT * FROM Users;
SELECT * FROM Orders;
SELECT * FROM Categories;
SELECT * FROM ProductDetails;
