CREATE DATABASE [waterbillproject];
GO

USE [waterbillproject];
GO

---------------------------------------------------------------------
-- CREATE TABLES
---------------------------------------------------------------------

-- Table: CustomerInfo
PRINT 'Creating Table [dbo].[CustomerInfo]...';
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
IF OBJECT_ID('dbo.CustomerInfo', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CustomerInfo](
        [UserName] [nvarchar](50) NOT NULL,
        [passwords] [varchar](72) NOT NULL,
        [NationalID] [bigint] NOT NULL,      -- <<< THÊM LẠI CỘT NÀY
        [SerialID] [int] NOT NULL,
        [Address] [nvarchar](100) NOT NULL,
        [CustomerType] [int] NULL,
        CONSTRAINT [PK_CustomerInfo] PRIMARY KEY CLUSTERED ([SerialID] ASC),
        CONSTRAINT [UQ_CustomerInfo_UserName] UNIQUE NONCLUSTERED ([UserName] ASC)
    );
    PRINT 'Table [dbo].[CustomerInfo] created.';
END
ELSE
BEGIN
    PRINT 'Table [dbo].[CustomerInfo] already exists. Ensure NationalID column is present.';
    -- Nếu bảng đã tồn tại, bạn cần thêm cột NationalID nếu nó chưa có:
    -- IF COL_LENGTH('dbo.CustomerInfo', 'NationalID') IS NULL
    -- BEGIN
    --     ALTER TABLE dbo.CustomerInfo ADD [NationalID] [bigint] NULL; -- Tạm cho phép NULL để thêm cột
    --     PRINT 'Column [NationalID] added to existing [dbo].[CustomerInfo]. You may need to update existing rows and then set it to NOT NULL.';
    --     -- Sau đó bạn cần UPDATE dữ liệu cho NationalID và ALTER COLUMN thành NOT NULL
    -- END
END
GO

-- Table: adminpass
PRINT 'Creating Table [dbo].[adminpass]...';
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
IF OBJECT_ID('dbo.adminpass', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[adminpass](
        [pass] [varchar](72) NOT NULL
    );
    PRINT 'Table [dbo].[adminpass] created.';
END
ELSE
BEGIN
     PRINT 'Table [dbo].[adminpass] already exists.';
END
GO

-- Table: consumption
PRINT 'Creating Table [dbo].[consumption]...';
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
IF OBJECT_ID('dbo.consumption', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[consumption](
        [SerialID] [int] NOT NULL,
        [months] [date] NOT NULL,
        [consumptionamount] [numeric](9, 3) NULL,
        [segmentNumber] [int] NULL,
        [price] [money] NULL,
        CONSTRAINT [PK_consumption] PRIMARY KEY CLUSTERED ([SerialID] ASC, [months] ASC)
    );
    PRINT 'Table [dbo].[consumption] created.';
END
ELSE
BEGIN
    PRINT 'Table [dbo].[consumption] already exists.';
    -- Logic kiểm tra và sửa PK cho consumption nếu cần (giữ nguyên như script bạn gửi)
    IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE type = 'PK' AND OBJECT_NAME(parent_object_id) = 'consumption' AND name = 'PK_consumption')
    BEGIN
        PRINT 'Attempting to fix Primary Key for [dbo].[consumption] to be composite (SerialID, months)...';
        IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE type = 'PK' AND OBJECT_NAME(parent_object_id) = 'consumption' AND OBJECT_DEFINITION(OBJECT_ID) LIKE '%months%ASC%')
        BEGIN
            DECLARE @OldPKNameConsumption NVARCHAR(128);
            SELECT @OldPKNameConsumption = name FROM sys.key_constraints WHERE type = 'PK' AND OBJECT_NAME(parent_object_id) = 'consumption';
            IF @OldPKNameConsumption IS NOT NULL AND @OldPKNameConsumption != 'PK_consumption'
            BEGIN
                EXEC('ALTER TABLE [dbo].[consumption] DROP CONSTRAINT [' + @OldPKNameConsumption + '];');
                PRINT 'Dropped old PK [' + @OldPKNameConsumption + '] from [dbo].[consumption].';
            END
        END
        ALTER TABLE [dbo].[consumption] ALTER COLUMN [SerialID] INT NOT NULL;
        ALTER TABLE [dbo].[consumption] ALTER COLUMN [months] DATE NOT NULL;
        ALTER TABLE [dbo].[consumption] ADD CONSTRAINT [PK_consumption] PRIMARY KEY CLUSTERED ([SerialID] ASC, [months] ASC);
        PRINT 'Composite Primary Key [PK_consumption] created/verified for [dbo].[consumption].';
    END
END
GO

-- Table: Debts
PRINT 'Creating Table [dbo].[Debts]...';
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
IF OBJECT_ID('dbo.Debts', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Debts](
        [SerialID] [int] NOT NULL,
        [months] [date] NOT NULL
    );
     PRINT 'Table [dbo].[Debts] created.';
END
ELSE
BEGIN
    PRINT 'Table [dbo].[Debts] already exists.';
    ALTER TABLE [dbo].[Debts] ALTER COLUMN [SerialID] INT NOT NULL;
    ALTER TABLE [dbo].[Debts] ALTER COLUMN [months] DATE NOT NULL;
END
GO

-- Table: techsupport
PRINT 'Creating Table [dbo].[techsupport]...';
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
IF OBJECT_ID('dbo.techsupport', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[techsupport](
        [description] [nvarchar](900) NULL
    );
    PRINT 'Table [dbo].[techsupport] created.';
END
ELSE
BEGIN
     PRINT 'Table [dbo].[techsupport] already exists.';
     IF COL_LENGTH('dbo.techsupport', 'discreption') IS NOT NULL AND COL_LENGTH('dbo.techsupport', 'description') IS NULL
     BEGIN
        EXEC sp_rename 'dbo.techsupport.discreption', 'description', 'COLUMN';
        PRINT 'Renamed column discreption to description in dbo.techsupport.';
     END
     ALTER TABLE [dbo].[techsupport] ALTER COLUMN [description] NVARCHAR(900) NULL;
END
GO

---------------------------------------------------------------------
-- INSERT INITIAL DATA
---------------------------------------------------------------------
PRINT 'Inserting initial data...';

-- Insert Admin Password Hash
IF NOT EXISTS (SELECT 1 FROM dbo.adminpass WHERE pass = N'$2a$11$Hb88VcHvfhrnNuo6CF6XdeeeNuSb6bIX22.CFJ6ln.Hx4Iol4fPfS')
BEGIN
    INSERT INTO [dbo].[adminpass] ([pass]) VALUES (N'$2a$11$Hb88VcHvfhrnNuo6CF6XdeeeNuSb6bIX22.CFJ6ln.Hx4Iol4fPfS');
    PRINT 'Admin password hash inserted.';
END
ELSE BEGIN PRINT 'Admin password hash already exists.'; END
GO

-- Insert CustomerInfo Data
PRINT 'Inserting CustomerInfo data...';
-- (Lệnh INSERT CustomerInfo sẽ chạy đúng sau khi cột NationalID được thêm vào định nghĩa bảng)
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerInfo] WHERE [SerialID] = 12345)
    INSERT [dbo].[CustomerInfo] ([UserName], [passwords], [NationalID], [SerialID], [Address], [CustomerType]) VALUES (N'mog', N'$2a$11$M74rE9jg7vzXwaWmGNzGkeZmKrJDI/0dYVubxoyEgMd7PnvzGMPqa', 12345, 12345, N'Duy Tan', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerInfo] WHERE [SerialID] = 123123)
    INSERT [dbo].[CustomerInfo] ([UserName], [passwords], [NationalID], [SerialID], [Address], [CustomerType]) VALUES (N'lmn', N'$2a$11$C2Wy7QPNNaHkCvUawVP4zO2D4BfT5qR69YnaE4ydXHldd7aCTs7D6', 123123, 123123, N'Phu My Hung', 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerInfo] WHERE [SerialID] = 123456)
    INSERT [dbo].[CustomerInfo] ([UserName], [passwords], [NationalID], [SerialID], [Address], [CustomerType]) VALUES (N'Nhat', N'$2a$11$wGWpjmt.xKavlDIXuNpuAupWklOhJEILvzvxkIBjf/RjjmgZQRAUC', 123456, 123456, N'HCM', 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerInfo] WHERE [SerialID] = 1000)
    INSERT [dbo].[CustomerInfo] ([UserName], [passwords], [NationalID], [SerialID], [Address], [CustomerType]) VALUES (N'user1', N'$2a$11$u0PmWL2W8JCSNlrjhkdEPuUa8LzAkrwdF7.I9I5ryf37Ku28KC.xa', 1122334455, 1000, N'Phu Nhuan', 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerInfo] WHERE [SerialID] = 1001)
    INSERT [dbo].[CustomerInfo] ([UserName], [passwords], [NationalID], [SerialID], [Address], [CustomerType]) VALUES (N'user2', N'$2a$11$TG2JBpXOWLsouQWYDSxUCuUTrahTynUzWBt/jCixckPHCEfikXbk.', 1212312312, 1001, N'123 Duy Tan', 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerInfo] WHERE [SerialID] = 1003)
    INSERT [dbo].[CustomerInfo] ([UserName], [passwords], [NationalID], [SerialID], [Address], [CustomerType]) VALUES (N'user3', N'$2a$11$vwxESKJcUb3fYzpb9cK9sOPa3IuUUae99Tjfvr6EF/z0kF6UEFxO.', 1235329856, 1003, N'Go Vap', 0);
PRINT 'CustomerInfo data insertion attempt finished.';
GO

-- Insert Consumption Data
PRINT 'Inserting consumption data...';
IF NOT EXISTS (SELECT 1 FROM [dbo].[consumption] WHERE [SerialID] = 1000 AND [months] = CAST(N'2025-05-09' AS Date))
    INSERT [dbo].[consumption] ([SerialID], [months], [consumptionamount], [segmentNumber], [price]) VALUES (1000, CAST(N'2025-05-09' AS Date), CAST(22.000 AS Numeric(9, 3)), 3, 184000.0000);
IF NOT EXISTS (SELECT 1 FROM [dbo].[consumption] WHERE [SerialID] = 1000 AND [months] = CAST(N'2025-05-10' AS Date))
    INSERT [dbo].[consumption] ([SerialID], [months], [consumptionamount], [segmentNumber], [price]) VALUES (1000, CAST(N'2025-05-10' AS Date), CAST(11.000 AS Numeric(9, 3)), 2, 94900.0000);
IF NOT EXISTS (SELECT 1 FROM [dbo].[consumption] WHERE [SerialID] = 12345 AND [months] = CAST(N'2025-05-09' AS Date))
    INSERT [dbo].[consumption] ([SerialID], [months], [consumptionamount], [segmentNumber], [price]) VALUES (12345, CAST(N'2025-05-09' AS Date), CAST(10.000 AS Numeric(9, 3)), 1, 59730.0000);
IF NOT EXISTS (SELECT 1 FROM [dbo].[consumption] WHERE [SerialID] = 123123 AND [months] = CAST(N'2025-12-01' AS Date))
    INSERT [dbo].[consumption] ([SerialID], [months], [consumptionamount], [segmentNumber], [price]) VALUES (123123, CAST(N'2025-12-01' AS Date), CAST(12.000 AS Numeric(9, 3)), 2, 104800.0000);
IF NOT EXISTS (SELECT 1 FROM [dbo].[consumption] WHERE [SerialID] = 123456 AND [months] = CAST(N'2025-05-09' AS Date))
    INSERT [dbo].[consumption] ([SerialID], [months], [consumptionamount], [segmentNumber], [price]) VALUES (123456, CAST(N'2025-05-09' AS Date), CAST(26.000 AS Numeric(9, 3)), 3, 184000.0000);
PRINT 'Consumption data insertion attempt finished.';
GO

-- Insert Debts Data
PRINT 'Inserting Debts data...';
IF NOT EXISTS (SELECT 1 FROM [dbo].[Debts] WHERE [SerialID] = 1000 AND [months] = CAST(N'2025-05-10' AS Date))
    INSERT [dbo].[Debts] ([SerialID], [months]) VALUES (1000, CAST(N'2025-05-10' AS Date));
IF NOT EXISTS (SELECT 1 FROM [dbo].[Debts] WHERE [SerialID] = 1000 AND [months] = CAST(N'2025-05-09' AS Date))
    INSERT [dbo].[Debts] ([SerialID], [months]) VALUES (1000, CAST(N'2025-05-09' AS Date));
PRINT 'Debts data insertion attempt finished.';
GO

-- Insert TechSupport Data
PRINT 'Inserting techsupport data...';
IF NOT EXISTS (SELECT 1 FROM [dbo].[techsupport] WHERE [description] = N'112233')
    INSERT [dbo].[techsupport] ([description]) VALUES (N'112233');
IF NOT EXISTS (SELECT 1 FROM [dbo].[techsupport] WHERE [description] = N'asdkbakhsdbadbiasdbib asd bab aweh sadu ad oaasdhdaodaeo do ia')
    INSERT [dbo].[techsupport] ([description]) VALUES (N'asdkbakhsdbadbiasdbib asd bab aweh sadu ad oaasdhdaodaeo do ia');
IF NOT EXISTS (SELECT 1 FROM [dbo].[techsupport] WHERE [description] = N'1234 alo alo asikdjaodoadjidoadasd a a sdjis ajpiodsa s asda')
    INSERT [dbo].[techsupport] ([description]) VALUES (N'1234 alo alo asikdjaodoadjidoadasd a a sdjis ajpiodsa s asda');
PRINT 'Techsupport data insertion attempt finished.';
GO

---------------------------------------------------------------------
-- ADD FOREIGN KEYS
---------------------------------------------------------------------
PRINT 'Adding Foreign Keys...';

-- FK từ consumption -> CustomerInfo
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_consumption_CustomerInfo]') AND parent_object_id = OBJECT_ID(N'[dbo].[consumption]'))
BEGIN
    ALTER TABLE [dbo].[consumption] WITH CHECK ADD CONSTRAINT [FK_consumption_CustomerInfo] FOREIGN KEY([SerialID])
    REFERENCES [dbo].[CustomerInfo] ([SerialID]);

    ALTER TABLE [dbo].[consumption] CHECK CONSTRAINT [FK_consumption_CustomerInfo];
    PRINT 'Foreign Key [FK_consumption_CustomerInfo] created.';
END
ELSE BEGIN PRINT 'Foreign Key [FK_consumption_CustomerInfo] already exists.'; END
GO

-- FK từ Debts -> CustomerInfo
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Debts_CustomerInfo]') AND parent_object_id = OBJECT_ID(N'[dbo].[Debts]'))
BEGIN
    ALTER TABLE [dbo].[Debts] WITH CHECK ADD CONSTRAINT [FK_Debts_CustomerInfo] FOREIGN KEY([SerialID])
    REFERENCES [dbo].[CustomerInfo] ([SerialID]);

    ALTER TABLE [dbo].[Debts] CHECK CONSTRAINT [FK_Debts_CustomerInfo];
     PRINT 'Foreign Key [FK_Debts_CustomerInfo] created.';
END
ELSE BEGIN PRINT 'Foreign Key [FK_Debts_CustomerInfo] already exists.'; END
GO

-- FK từ Debts -> consumption (Composite Key Reference)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Debts_consumption]') AND parent_object_id = OBJECT_ID(N'[dbo].[Debts]'))
BEGIN
    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE type = 'PK' AND OBJECT_NAME(parent_object_id) = 'consumption' AND name = 'PK_consumption')
    BEGIN
        ALTER TABLE [dbo].[Debts] WITH CHECK ADD CONSTRAINT [FK_Debts_consumption] FOREIGN KEY([SerialID], [months])
        REFERENCES [dbo].[consumption] ([SerialID], [months]);

        ALTER TABLE [dbo].[Debts] CHECK CONSTRAINT [FK_Debts_consumption];
        PRINT 'Foreign Key [FK_Debts_consumption] created (Composite).';
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Cannot create FK_Debts_consumption because composite PK on consumption table not found/correct. Please ensure consumption table PK is (SerialID, months).';
    END
END
ELSE BEGIN PRINT 'Foreign Key [FK_Debts_consumption] already exists.'; END
GO

---------------------------------------------------------------------
-- CREATE STORED PROCEDURES
---------------------------------------------------------------------
PRINT 'Creating Stored Procedure [dbo].[usp_insert]...';
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
IF OBJECT_ID('dbo.usp_insert', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_insert;
GO

CREATE PROC [dbo].[usp_insert]
    @id int,
    @months date,
    @consumptionamount numeric(9,3),
    @segmentNumber int
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.consumption(SerialID, months, consumptionamount, segmentNumber)
    VALUES (@id, @months, @consumptionamount, @segmentNumber);
END
GO
PRINT 'Stored Procedure [dbo].[usp_insert] created/updated.';
GO

PRINT 'Database script execution finished.';
GO