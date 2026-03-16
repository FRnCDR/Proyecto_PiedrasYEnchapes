
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 03/13/2026 20:25:08
-- Generated from EDMX file: C:\Users\frany\source\repos\Proyecto_PiedrasYEnchapes\PiedrasYEnchapes\WebApplication1\EF\Model1.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [DATABASE_PYE];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Categoria]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tbProductos] DROP CONSTRAINT [FK_Categoria];
GO
IF OBJECT_ID(N'[dbo].[FK_tbCotizaciones_tbClientes]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tbCotizaciones] DROP CONSTRAINT [FK_tbCotizaciones_tbClientes];
GO
IF OBJECT_ID(N'[dbo].[FK_tbDetalleCotizacion_tbCotizaciones]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tbDetalleCotizacion] DROP CONSTRAINT [FK_tbDetalleCotizacion_tbCotizaciones];
GO
IF OBJECT_ID(N'[dbo].[FK_tbDetalleCotizacion_tbProductos]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tbDetalleCotizacion] DROP CONSTRAINT [FK_tbDetalleCotizacion_tbProductos];
GO
IF OBJECT_ID(N'[dbo].[FK_tbProductos_tbProveedores]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tbProductos] DROP CONSTRAINT [FK_tbProductos_tbProveedores];
GO
IF OBJECT_ID(N'[dbo].[FK_Usuario_Perfil]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tbUsuario] DROP CONSTRAINT [FK_Usuario_Perfil];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[tbCategorias]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tbCategorias];
GO
IF OBJECT_ID(N'[dbo].[tbClientes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tbClientes];
GO
IF OBJECT_ID(N'[dbo].[tbCotizaciones]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tbCotizaciones];
GO
IF OBJECT_ID(N'[dbo].[tbDetalleCotizacion]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tbDetalleCotizacion];
GO
IF OBJECT_ID(N'[dbo].[tbEmpleados]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tbEmpleados];
GO
IF OBJECT_ID(N'[dbo].[tbPerfil]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tbPerfil];
GO
IF OBJECT_ID(N'[dbo].[tbProductos]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tbProductos];
GO
IF OBJECT_ID(N'[dbo].[tbProveedores]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tbProveedores];
GO
IF OBJECT_ID(N'[dbo].[tbUsuario]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tbUsuario];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'tbCategorias'
CREATE TABLE [dbo].[tbCategorias] (
    [CategoriaID] int IDENTITY(1,1) NOT NULL,
    [Nombre] nvarchar(100)  NOT NULL,
    [Descripcion] nvarchar(255)  NULL
);
GO

-- Creating table 'tbClientes'
CREATE TABLE [dbo].[tbClientes] (
    [IdCliente] int IDENTITY(1,1) NOT NULL,
    [Identificacion] varchar(20)  NOT NULL,
    [Nombre] nvarchar(100)  NOT NULL,
    [Apellidos] nvarchar(100)  NOT NULL,
    [CorreoElectronico] varchar(100)  NOT NULL,
    [Telefono] varchar(20)  NULL,
    [Direccion] nvarchar(255)  NULL,
    [FechaRegistro] datetime  NOT NULL,
    [Estado] bit  NOT NULL
);
GO

-- Creating table 'tbEmpleados'
CREATE TABLE [dbo].[tbEmpleados] (
    [IdEmpleado] int IDENTITY(1,1) NOT NULL,
    [Nombre] nvarchar(100)  NOT NULL,
    [Apellidos] nvarchar(100)  NOT NULL,
    [Correo] nvarchar(255)  NOT NULL,
    [Telefono] nvarchar(15)  NULL,
    [Estado] bit  NOT NULL,
    [FechaRegistro] datetime  NOT NULL,
    [IdPerfil] int  NULL
);
GO

-- Creating table 'tbPerfil'
CREATE TABLE [dbo].[tbPerfil] (
    [IdPerfil] int IDENTITY(1,1) NOT NULL,
    [Nombre] varchar(100)  NOT NULL
);
GO

-- Creating table 'tbProductos'
CREATE TABLE [dbo].[tbProductos] (
    [ProductoID] int IDENTITY(1,1) NOT NULL,
    [Nombre] nvarchar(100)  NOT NULL,
    [Descripcion] nvarchar(255)  NOT NULL,
    [Stock] int  NOT NULL,
    [Precio] decimal(18,2)  NOT NULL,
    [Imagen] nvarchar(255)  NULL,
    [CategoriaID] int  NULL,
    [ProveedorID] int  NOT NULL
);
GO

-- Creating table 'tbProveedores'
CREATE TABLE [dbo].[tbProveedores] (
    [ProveedorID] int IDENTITY(1,1) NOT NULL,
    [NombreEmpresa] nvarchar(150)  NOT NULL,
    [Contacto] nvarchar(100)  NOT NULL,
    [CorreoElectronico] varchar(100)  NOT NULL,
    [Telefono] varchar(20)  NULL,
    [Direccion] nvarchar(255)  NULL,
    [FechaRegistro] datetime  NOT NULL,
    [Estado] bit  NOT NULL,
    [Tipo] varchar(20)  NOT NULL
);
GO

-- Creating table 'tbUsuario'
CREATE TABLE [dbo].[tbUsuario] (
    [IdUsuario] int IDENTITY(1,1) NOT NULL,
    [Identificacion] varchar(15)  NOT NULL,
    [Nombre] varchar(255)  NOT NULL,
    [CorreoElectronico] varchar(100)  NOT NULL,
    [Contrasenna] varchar(255)  NOT NULL,
    [Estado] bit  NOT NULL,
    [IdPerfil] int  NOT NULL
);
GO

-- Creating table 'tbCotizaciones'
CREATE TABLE [dbo].[tbCotizaciones] (
    [CotizacionID] int IDENTITY(1,1) NOT NULL,
    [IdCliente] int  NULL,
    [FechaCotizacion] datetime  NOT NULL,
    [Total] decimal(18,2)  NOT NULL,
    [Estado] varchar(20)  NOT NULL,
    [Observaciones] nvarchar(255)  NULL
);
GO

-- Creating table 'tbDetalleCotizacion'
CREATE TABLE [dbo].[tbDetalleCotizacion] (
    [DetalleCotizacionID] int IDENTITY(1,1) NOT NULL,
    [CotizacionID] int  NOT NULL,
    [ProductoID] int  NOT NULL,
    [Cantidad] int  NOT NULL,
    [PrecioUnitario] decimal(18,2)  NOT NULL,
    [Subtotal] decimal(18,2)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [CategoriaID] in table 'tbCategorias'
ALTER TABLE [dbo].[tbCategorias]
ADD CONSTRAINT [PK_tbCategorias]
    PRIMARY KEY CLUSTERED ([CategoriaID] ASC);
GO

-- Creating primary key on [IdCliente] in table 'tbClientes'
ALTER TABLE [dbo].[tbClientes]
ADD CONSTRAINT [PK_tbClientes]
    PRIMARY KEY CLUSTERED ([IdCliente] ASC);
GO

-- Creating primary key on [IdEmpleado] in table 'tbEmpleados'
ALTER TABLE [dbo].[tbEmpleados]
ADD CONSTRAINT [PK_tbEmpleados]
    PRIMARY KEY CLUSTERED ([IdEmpleado] ASC);
GO

-- Creating primary key on [IdPerfil] in table 'tbPerfil'
ALTER TABLE [dbo].[tbPerfil]
ADD CONSTRAINT [PK_tbPerfil]
    PRIMARY KEY CLUSTERED ([IdPerfil] ASC);
GO

-- Creating primary key on [ProductoID] in table 'tbProductos'
ALTER TABLE [dbo].[tbProductos]
ADD CONSTRAINT [PK_tbProductos]
    PRIMARY KEY CLUSTERED ([ProductoID] ASC);
GO

-- Creating primary key on [ProveedorID] in table 'tbProveedores'
ALTER TABLE [dbo].[tbProveedores]
ADD CONSTRAINT [PK_tbProveedores]
    PRIMARY KEY CLUSTERED ([ProveedorID] ASC);
GO

-- Creating primary key on [IdUsuario] in table 'tbUsuario'
ALTER TABLE [dbo].[tbUsuario]
ADD CONSTRAINT [PK_tbUsuario]
    PRIMARY KEY CLUSTERED ([IdUsuario] ASC);
GO

-- Creating primary key on [CotizacionID] in table 'tbCotizaciones'
ALTER TABLE [dbo].[tbCotizaciones]
ADD CONSTRAINT [PK_tbCotizaciones]
    PRIMARY KEY CLUSTERED ([CotizacionID] ASC);
GO

-- Creating primary key on [DetalleCotizacionID] in table 'tbDetalleCotizacion'
ALTER TABLE [dbo].[tbDetalleCotizacion]
ADD CONSTRAINT [PK_tbDetalleCotizacion]
    PRIMARY KEY CLUSTERED ([DetalleCotizacionID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [CategoriaID] in table 'tbProductos'
ALTER TABLE [dbo].[tbProductos]
ADD CONSTRAINT [FK_Categoria]
    FOREIGN KEY ([CategoriaID])
    REFERENCES [dbo].[tbCategorias]
        ([CategoriaID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Categoria'
CREATE INDEX [IX_FK_Categoria]
ON [dbo].[tbProductos]
    ([CategoriaID]);
GO

-- Creating foreign key on [IdPerfil] in table 'tbUsuario'
ALTER TABLE [dbo].[tbUsuario]
ADD CONSTRAINT [FK_Usuario_Perfil]
    FOREIGN KEY ([IdPerfil])
    REFERENCES [dbo].[tbPerfil]
        ([IdPerfil])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Usuario_Perfil'
CREATE INDEX [IX_FK_Usuario_Perfil]
ON [dbo].[tbUsuario]
    ([IdPerfil]);
GO

-- Creating foreign key on [ProveedorID] in table 'tbProductos'
ALTER TABLE [dbo].[tbProductos]
ADD CONSTRAINT [FK_tbProductos_tbProveedores]
    FOREIGN KEY ([ProveedorID])
    REFERENCES [dbo].[tbProveedores]
        ([ProveedorID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tbProductos_tbProveedores'
CREATE INDEX [IX_FK_tbProductos_tbProveedores]
ON [dbo].[tbProductos]
    ([ProveedorID]);
GO

-- Creating foreign key on [IdCliente] in table 'tbCotizaciones'
ALTER TABLE [dbo].[tbCotizaciones]
ADD CONSTRAINT [FK_tbCotizaciones_tbClientes]
    FOREIGN KEY ([IdCliente])
    REFERENCES [dbo].[tbClientes]
        ([IdCliente])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tbCotizaciones_tbClientes'
CREATE INDEX [IX_FK_tbCotizaciones_tbClientes]
ON [dbo].[tbCotizaciones]
    ([IdCliente]);
GO

-- Creating foreign key on [CotizacionID] in table 'tbDetalleCotizacion'
ALTER TABLE [dbo].[tbDetalleCotizacion]
ADD CONSTRAINT [FK_tbDetalleCotizacion_tbCotizaciones]
    FOREIGN KEY ([CotizacionID])
    REFERENCES [dbo].[tbCotizaciones]
        ([CotizacionID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tbDetalleCotizacion_tbCotizaciones'
CREATE INDEX [IX_FK_tbDetalleCotizacion_tbCotizaciones]
ON [dbo].[tbDetalleCotizacion]
    ([CotizacionID]);
GO

-- Creating foreign key on [ProductoID] in table 'tbDetalleCotizacion'
ALTER TABLE [dbo].[tbDetalleCotizacion]
ADD CONSTRAINT [FK_tbDetalleCotizacion_tbProductos]
    FOREIGN KEY ([ProductoID])
    REFERENCES [dbo].[tbProductos]
        ([ProductoID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tbDetalleCotizacion_tbProductos'
CREATE INDEX [IX_FK_tbDetalleCotizacion_tbProductos]
ON [dbo].[tbDetalleCotizacion]
    ([ProductoID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------