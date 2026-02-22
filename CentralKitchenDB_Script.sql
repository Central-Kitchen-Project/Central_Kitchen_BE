-- =============================================
-- CentralKitchenDB - Database Creation Script
-- =============================================

CREATE DATABASE CentralKitchenDB;
GO

USE CentralKitchenDB;
GO

-- =============================================
-- 1. ROLES
-- =============================================
CREATE TABLE [roles] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [role_name] NVARCHAR(50) NOT NULL,
    [description] NVARCHAR(255) NULL,
    CONSTRAINT PK_roles PRIMARY KEY ([id])
);
GO

-- =============================================
-- 2. USERS
-- =============================================
CREATE TABLE [users] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [username] NVARCHAR(50) NOT NULL,
    [password_hash] NVARCHAR(255) NOT NULL,
    [email] NVARCHAR(100) NULL,
    [role_id] INT NOT NULL,
    [created_at] DATETIME NULL DEFAULT (GETDATE()),
    CONSTRAINT PK_users PRIMARY KEY ([id]),
    CONSTRAINT fk_users_roles FOREIGN KEY ([role_id]) REFERENCES [roles]([id])
);
GO

-- =============================================
-- 3. ITEMS
-- =============================================
CREATE TABLE [items] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [item_name] NVARCHAR(100) NOT NULL,
    [unit] NVARCHAR(20) NULL,
    [item_type] NVARCHAR(30) NULL,
    [created_at] DATETIME NULL DEFAULT (GETDATE()),
    [description] NVARCHAR(255) NULL,
    [price] DECIMAL(18,2) NULL,
    [category] NVARCHAR(50) NULL,
    CONSTRAINT PK_items PRIMARY KEY ([id])
);
GO

-- =============================================
-- 4. LOCATIONS
-- =============================================
CREATE TABLE [locations] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [location_name] NVARCHAR(100) NOT NULL,
    [address] NVARCHAR(255) NULL,
    CONSTRAINT PK_locations PRIMARY KEY ([id])
);
GO

-- =============================================
-- 5. ORDERS
-- =============================================
CREATE TABLE [orders] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [user_id] INT NOT NULL,
    [order_date] DATETIME NULL DEFAULT (GETDATE()),
    [status] NVARCHAR(30) NULL,
    CONSTRAINT PK_orders PRIMARY KEY ([id]),
    CONSTRAINT fk_orders_user FOREIGN KEY ([user_id]) REFERENCES [users]([id])
);
GO

-- =============================================
-- 6. ORDER_LINES
-- =============================================
CREATE TABLE [order_lines] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [order_id] INT NOT NULL,
    [item_id] INT NOT NULL,
    [quantity] DECIMAL(18,3) NULL,
    CONSTRAINT PK_order_lines PRIMARY KEY ([id]),
    CONSTRAINT fk_orderline_order FOREIGN KEY ([order_id]) REFERENCES [orders]([id]),
    CONSTRAINT fk_orderline_item FOREIGN KEY ([item_id]) REFERENCES [items]([id])
);
GO

-- =============================================
-- 7. INVENTORY
-- =============================================
CREATE TABLE [inventory] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [item_id] INT NOT NULL,
    [location_id] INT NOT NULL,
    [quantity] DECIMAL(18,3) NULL DEFAULT ((0)),
    CONSTRAINT PK_inventory PRIMARY KEY ([id]),
    CONSTRAINT fk_inventory_item FOREIGN KEY ([item_id]) REFERENCES [items]([id]),
    CONSTRAINT fk_inventory_location FOREIGN KEY ([location_id]) REFERENCES [locations]([id])
);
GO

-- =============================================
-- 8. INVENTORY_TRANSACTIONS
-- =============================================
CREATE TABLE [inventory_transactions] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [inventory_id] INT NOT NULL,
    [tx_type] NVARCHAR(30) NULL,
    [quantity] DECIMAL(18,3) NULL,
    [reference_type] NVARCHAR(30) NULL,
    [reference_id] INT NULL,
    [created_at] DATETIME NULL DEFAULT (GETDATE()),
    CONSTRAINT PK_inventory_transactions PRIMARY KEY ([id]),
    CONSTRAINT fk_inv_tx_inventory FOREIGN KEY ([inventory_id]) REFERENCES [inventory]([id])
);
GO

-- =============================================
-- 9. RECIPES
-- =============================================
CREATE TABLE [recipes] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [finished_item_id] INT NOT NULL,
    [ingredient_item_id] INT NOT NULL,
    [quantity] DECIMAL(18,3) NOT NULL,
    CONSTRAINT PK_recipes PRIMARY KEY ([id]),
    CONSTRAINT fk_recipe_finished_item FOREIGN KEY ([finished_item_id]) REFERENCES [items]([id]),
    CONSTRAINT fk_recipe_ingredient_item FOREIGN KEY ([ingredient_item_id]) REFERENCES [items]([id])
);
GO

-- =============================================
-- 10. PRODUCTIONS
-- =============================================
CREATE TABLE [productions] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [item_id] INT NOT NULL,
    [location_id] INT NOT NULL,
    [quantity] DECIMAL(18,3) NULL,
    [production_date] DATETIME NULL DEFAULT (GETDATE()),
    CONSTRAINT PK_productions PRIMARY KEY ([id]),
    CONSTRAINT fk_production_item FOREIGN KEY ([item_id]) REFERENCES [items]([id]),
    CONSTRAINT fk_production_location FOREIGN KEY ([location_id]) REFERENCES [locations]([id])
);
GO

-- =============================================
-- 11. SHIPMENTS
-- =============================================
CREATE TABLE [shipments] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [from_location_id] INT NOT NULL,
    [shipment_date] DATETIME NULL DEFAULT (GETDATE()),
    [status] NVARCHAR(30) NULL,
    CONSTRAINT PK_shipments PRIMARY KEY ([id]),
    CONSTRAINT fk_shipment_location FOREIGN KEY ([from_location_id]) REFERENCES [locations]([id])
);
GO

-- =============================================
-- 12. SHIPMENT_LINES
-- =============================================
CREATE TABLE [shipment_lines] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [shipment_id] INT NOT NULL,
    [item_id] INT NOT NULL,
    [quantity] DECIMAL(18,3) NULL,
    CONSTRAINT PK_shipment_lines PRIMARY KEY ([id]),
    CONSTRAINT fk_shipmentline_shipment FOREIGN KEY ([shipment_id]) REFERENCES [shipments]([id]),
    CONSTRAINT fk_shipmentline_item FOREIGN KEY ([item_id]) REFERENCES [items]([id])
);
GO

-- =============================================
-- 13. QUALITY_FEEDBACKS
-- =============================================
CREATE TABLE [quality_feedbacks] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [item_id] INT NULL,
    [description] NVARCHAR(255) NULL,
    [feedback_date] DATETIME NULL DEFAULT (GETDATE()),
    CONSTRAINT PK_quality_feedbacks PRIMARY KEY ([id]),
    CONSTRAINT fk_feedback_item FOREIGN KEY ([item_id]) REFERENCES [items]([id])
);
GO

-- =============================================
-- 14. STOCK_CHECKS
-- =============================================
CREATE TABLE [stock_checks] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [location_id] INT NOT NULL,
    [check_date] DATETIME NULL DEFAULT (GETDATE()),
    [note] NVARCHAR(255) NULL,
    CONSTRAINT PK_stock_checks PRIMARY KEY ([id]),
    CONSTRAINT fk_stockcheck_location FOREIGN KEY ([location_id]) REFERENCES [locations]([id])
);
GO

-- =============================================
-- 15. SUPPLY_REQUESTS
-- =============================================
CREATE TABLE [supply_requests] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [item_id] INT NOT NULL,
    [requested_quantity] DECIMAL(18,3) NOT NULL,
    [available_quantity] DECIMAL(18,3) NOT NULL DEFAULT ((0)),
    [shortage_quantity] DECIMAL(18,3) NOT NULL,
    [status] NVARCHAR(30) NOT NULL DEFAULT ('Pending'),
    [note] NVARCHAR(255) NULL,
    [requested_by] INT NOT NULL,
    [created_at] DATETIME NOT NULL DEFAULT (GETDATE()),
    [updated_at] DATETIME NULL,
    CONSTRAINT PK_supply_requests PRIMARY KEY ([id]),
    CONSTRAINT fk_supply_requests_item FOREIGN KEY ([item_id]) REFERENCES [items]([id]),
    CONSTRAINT fk_supply_requests_user FOREIGN KEY ([requested_by]) REFERENCES [users]([id])
);
GO

-- =============================================
-- 16. SYSTEM_PARAMETERS
-- =============================================
CREATE TABLE [system_parameters] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [param_key] NVARCHAR(50) NULL,
    [param_value] NVARCHAR(255) NULL,
    CONSTRAINT PK_system_parameters PRIMARY KEY ([id]),
    CONSTRAINT UQ_system_parameters_key UNIQUE ([param_key])
);
GO

-- =============================================
-- SEED DATA
-- =============================================

-- Roles
INSERT INTO [roles] ([role_name], [description]) VALUES 
(N'Admin', N'Quan tri vien'),
(N'Staff', N'Nhan vien'),
(N'Manager', N'Quan ly');
GO

-- Items
INSERT INTO [items] ([item_name], [unit], [item_type], [description], [price], [category]) VALUES 
(N'Bot mi', N'kg', N'ingredient', N'Bot mi da dung', 15000, N'ingredient'),
(N'Duong', N'kg', N'ingredient', N'Duong trang', 12000, N'ingredient'),
(N'Trung ga', N'unit', N'ingredient', N'Trung ga tuoi', 3500, N'ingredient'),
(N'Banh mi', N'unit', N'finished', N'Banh mi truyen thong', 25000, N'finished'),
(N'Banh ngot', N'unit', N'finished', N'Banh ngot kem bo', 35000, N'finished');
GO

-- Locations
INSERT INTO [locations] ([location_name], [address]) VALUES 
(N'Kho chinh', N'123 Nguyen Van Linh, HCM');
GO

-- Inventory
INSERT INTO [inventory] ([item_id], [location_id], [quantity]) VALUES 
(1, 1, 100.000),
(2, 1, 50.000),
(3, 1, 200.000),
(4, 1, 80.000),
(5, 1, 60.000);
GO

-- Recipes (Cong thuc)
INSERT INTO [recipes] ([finished_item_id], [ingredient_item_id], [quantity]) VALUES 
(4, 1, 0.500),  -- Banh mi can 0.5kg Bot mi
(4, 3, 2.000),  -- Banh mi can 2 Trung ga
(5, 1, 0.300),  -- Banh ngot can 0.3kg Bot mi
(5, 2, 0.200),  -- Banh ngot can 0.2kg Duong
(5, 3, 3.000);  -- Banh ngot can 3 Trung ga
GO
