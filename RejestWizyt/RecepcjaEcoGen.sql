CREATE DATABASE RecepcjaFirma;
GO

USE RecepcjaFirma;
GO

CREATE TABLE Wizyty (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Imie NVARCHAR(100),
    Nazwisko NVARCHAR(100),
    DoKogo NVARCHAR(100),
    GodzinaWejscia DATETIME NOT NULL,
    GodzinaWyjscia DATETIME NULL
);
GO