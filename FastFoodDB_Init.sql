SET NOCOUNT ON;

IF DB_ID(N'FastFoodDB') IS NULL
BEGIN
    CREATE DATABASE FastFoodDB;
END
GO

USE FastFoodDB;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
GO

IF OBJECT_ID(N'dbo.TaiKhoan', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaiKhoan
    (
        MaTK INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TaiKhoan PRIMARY KEY,
        TenTK NVARCHAR(100) NOT NULL,
        TenDangNhap NVARCHAR(50) NOT NULL CONSTRAINT UQ_TaiKhoan_TenDangNhap UNIQUE,
        MatKhau NVARCHAR(100) NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.TaiKhoan WHERE TenDangNhap = N'admin')
BEGIN
    INSERT INTO dbo.TaiKhoan (TenTK, TenDangNhap, MatKhau)
    VALUES (N'Quản trị', N'admin', N'admin123');
END
GO

IF OBJECT_ID(N'dbo.DanhMuc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DanhMuc
    (
        MaDM INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DanhMuc PRIMARY KEY,
        TenDM NVARCHAR(100) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.MonAn', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MonAn
    (
        MaMon INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MonAn PRIMARY KEY,
        TenMon NVARCHAR(200) NOT NULL,
        Gia DECIMAL(18,2) NOT NULL CONSTRAINT DF_MonAn_Gia DEFAULT(0),
        MaDM INT NOT NULL,
        TrangThai NVARCHAR(50) NULL,
        HinhAnh NVARCHAR(500) NULL,
        CONSTRAINT FK_MonAn_DanhMuc FOREIGN KEY (MaDM) REFERENCES dbo.DanhMuc(MaDM)
    );
END
GO

IF OBJECT_ID(N'dbo.KhachHang', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.KhachHang
    (
        MaKH INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KhachHang PRIMARY KEY,
        TenKH NVARCHAR(150) NOT NULL,
        SDT NVARCHAR(20) NOT NULL CONSTRAINT UQ_KhachHang_SDT UNIQUE,
        DiaChi NVARCHAR(255) NULL,
        Email NVARCHAR(150) NULL,
        NgaySinh DATE NULL
    );
END
GO

IF OBJECT_ID(N'dbo.DonHang', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DonHang
    (
        MaDH INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DonHang PRIMARY KEY,
        NgayLap DATETIME NOT NULL CONSTRAINT DF_DonHang_NgayLap DEFAULT(GETDATE()),
        TongTien DECIMAL(18,2) NOT NULL CONSTRAINT DF_DonHang_TongTien DEFAULT(0),
        TrangThaiThanhToan NVARCHAR(50) NULL,
        MaKH INT NULL,
        CONSTRAINT FK_DonHang_KhachHang FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH)
    );
END
GO

IF OBJECT_ID(N'dbo.ChiTietDonHang', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChiTietDonHang
    (
        MaDH INT NOT NULL,
        MaMon INT NOT NULL,
        SoLuong INT NOT NULL CONSTRAINT DF_ChiTietDonHang_SoLuong DEFAULT(1),
        DonGia DECIMAL(18,2) NOT NULL CONSTRAINT DF_ChiTietDonHang_DonGia DEFAULT(0),
        CONSTRAINT PK_ChiTietDonHang PRIMARY KEY (MaDH, MaMon),
        CONSTRAINT FK_ChiTietDonHang_DonHang FOREIGN KEY (MaDH) REFERENCES dbo.DonHang(MaDH) ON DELETE CASCADE,
        CONSTRAINT FK_ChiTietDonHang_MonAn FOREIGN KEY (MaMon) REFERENCES dbo.MonAn(MaMon)
    );
END
GO

IF OBJECT_ID(N'dbo.TonKhoMonAn', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TonKhoMonAn
    (
        MaMon INT NOT NULL CONSTRAINT PK_TonKhoMonAn PRIMARY KEY,
        SoLuongTon INT NOT NULL CONSTRAINT DF_TonKhoMonAn_SoLuongTon DEFAULT(0),
        NgayCapNhat DATETIME NOT NULL CONSTRAINT DF_TonKhoMonAn_NgayCapNhat DEFAULT(GETDATE()),
        CONSTRAINT FK_TonKhoMonAn_MonAn FOREIGN KEY (MaMon) REFERENCES dbo.MonAn(MaMon)
    );
END
GO

IF OBJECT_ID(N'dbo.PhieuNhapKho', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PhieuNhapKho
    (
        MaPN INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PhieuNhapKho PRIMARY KEY,
        NgayNhap DATETIME NOT NULL CONSTRAINT DF_PhieuNhapKho_NgayNhap DEFAULT(GETDATE()),
        MaTK INT NULL,
        GhiChu NVARCHAR(255) NULL,
        TongTien DECIMAL(18,2) NOT NULL CONSTRAINT DF_PhieuNhapKho_TongTien DEFAULT(0),
        CONSTRAINT FK_PhieuNhapKho_TaiKhoan FOREIGN KEY (MaTK) REFERENCES dbo.TaiKhoan(MaTK)
    );
END
GO

IF OBJECT_ID(N'dbo.ChiTietPhieuNhapKho', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChiTietPhieuNhapKho
    (
        MaPN INT NOT NULL,
        MaMon INT NOT NULL,
        SoLuong INT NOT NULL,
        DonGia DECIMAL(18,2) NOT NULL,
        ThanhTien AS (CONVERT(DECIMAL(18,2), SoLuong * DonGia)) PERSISTED,
        CONSTRAINT PK_ChiTietPhieuNhapKho PRIMARY KEY (MaPN, MaMon),
        CONSTRAINT FK_ChiTietPhieuNhapKho_PhieuNhapKho FOREIGN KEY (MaPN) REFERENCES dbo.PhieuNhapKho(MaPN) ON DELETE CASCADE,
        CONSTRAINT FK_ChiTietPhieuNhapKho_MonAn FOREIGN KEY (MaMon) REFERENCES dbo.MonAn(MaMon)
    );
END
GO

IF OBJECT_ID(N'dbo.PhieuXuatKho', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PhieuXuatKho
    (
        MaPX INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PhieuXuatKho PRIMARY KEY,
        NgayXuat DATETIME NOT NULL CONSTRAINT DF_PhieuXuatKho_NgayXuat DEFAULT(GETDATE()),
        MaTK INT NULL,
        LyDo NVARCHAR(255) NULL,
        GhiChu NVARCHAR(255) NULL,
        CONSTRAINT FK_PhieuXuatKho_TaiKhoan FOREIGN KEY (MaTK) REFERENCES dbo.TaiKhoan(MaTK)
    );
END
GO

IF OBJECT_ID(N'dbo.ChiTietPhieuXuatKho', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChiTietPhieuXuatKho
    (
        MaPX INT NOT NULL,
        MaMon INT NOT NULL,
        SoLuong INT NOT NULL,
        DonGia DECIMAL(18,2) NULL,
        ThanhTien AS (CASE WHEN DonGia IS NULL THEN NULL ELSE CONVERT(DECIMAL(18,2), SoLuong * DonGia) END) PERSISTED,
        CONSTRAINT PK_ChiTietPhieuXuatKho PRIMARY KEY (MaPX, MaMon),
        CONSTRAINT FK_ChiTietPhieuXuatKho_PhieuXuatKho FOREIGN KEY (MaPX) REFERENCES dbo.PhieuXuatKho(MaPX) ON DELETE CASCADE,
        CONSTRAINT FK_ChiTietPhieuXuatKho_MonAn FOREIGN KEY (MaMon) REFERENCES dbo.MonAn(MaMon)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PhieuNhapKho_NgayNhap' AND object_id = OBJECT_ID(N'dbo.PhieuNhapKho'))
BEGIN
    CREATE INDEX IX_PhieuNhapKho_NgayNhap ON dbo.PhieuNhapKho(NgayNhap DESC);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PhieuXuatKho_NgayXuat' AND object_id = OBJECT_ID(N'dbo.PhieuXuatKho'))
BEGIN
    CREATE INDEX IX_PhieuXuatKho_NgayXuat ON dbo.PhieuXuatKho(NgayXuat DESC);
END
GO
