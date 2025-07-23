Step 1: Database Server Setup
Choose your database server location:

Option A: Use an existing company server that has SQL Server
Option B: Set up a dedicated computer as the database server
Option C: Use one of the security desk computers as the "main" one

Install SQL Server on the chosen server:

Download SQL Server Express (free) from Microsoft
During installation, note the server name (usually SERVERNAME\SQLEXPRESS)
Make sure "Mixed Mode Authentication" is enabled



Step 2.
On the server computer, run your SQL script:
-- Run this on your company server (or main computer)
-- Replace 'YOUR-SERVER-NAME' with the actual server name

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


-- Enable network access (run these as administrator)
EXEC sp_configure 'remote access', 1;
RECONFIGURE;
GO

Step 3.
Change the connection string in the app.







1. Dodaj regułę zapory dla portu 1433
Naciśnij Win + R, wpisz wf.msc i kliknij Enter (otworzy się Zapora systemu Windows).

Po lewej stronie kliknij:
Reguły przychodzące → po prawej Nowa reguła…

Wybierz:

Port → Dalej

TCP → określone porty: 1433

Zezwalaj na połączenie → Dalej

Zaznacz wszystkie: Domena, Prywatna, Publiczna → Dalej

Nazwa: np. SQL TCP 1433 → Zakończ

2. Sprawdź nasłuchiwany port SQL Server
Uruchom SQL Server Configuration Manager

Przejdź do:

SQL Server Network Configuration → Protocols for SQLEXPRESS
Kliknij TCP/IP, zakładka IP Addresses → przewiń w dół do IPAll

Sprawdź:

TCP Port — powinien być 1433

Jeśli puste — wpisz 1433, Zastosuj, Zrestartuj SQL Server







Włączenie TCP/IP i ustawienie portu:

Otwórz SQL Server Configuration Manager
Przejdź do SQL Server Network Configuration → Protocols for SQLEXPRESS
Kliknij prawym na TCP/IP → Enable
Kliknij prawym na TCP/IP → Properties → zakładka IP Addresses
Przewiń do IPAll na dole:

TCP Dynamic Ports: pozostaw puste
TCP Port: wpisz 1433


Restart usługi SQL Server (SQLEXPRESS) w Services

Włączenie SQL Server Browser (opcjonalne ale zalecane):

W SQL Server Configuration Manager → SQL Server Services
SQL Server Browser → Properties → Start Mode: Automatic
Start usługi SQL Server Browser

2. Firewall Windows
Dodaj regułę dla portu 1433:

Windows Defender Firewall → Advanced Settings
Inbound Rules → New Rule
Port → TCP → Specific local ports: 1433
Allow the connection → Nazwij regułę np. "SQL Server"






Automatyczne backupy w MSSQL:
https://sqlbak.com/blog/automating-mssql-backup-tasks-how-to-schedule-sql-server-backups/