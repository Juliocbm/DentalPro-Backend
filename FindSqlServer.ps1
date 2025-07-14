Write-Host "Buscando servidores SQL locales..." -ForegroundColor Green

# Intenta obtener las instancias de SQL Server registradas localmente
try {
    $sqlInstances = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server' -Name InstalledInstances -ErrorAction SilentlyContinue | Select-Object -ExpandProperty InstalledInstances
    
    if ($sqlInstances) {
        Write-Host "Instancias de SQL Server encontradas en el registro:" -ForegroundColor Green
        foreach ($instance in $sqlInstances) {
            if ($instance -eq "MSSQLSERVER") {
                Write-Host "- Instancia predeterminada (sin nombre)" -ForegroundColor Cyan
            } else {
                Write-Host "- $instance" -ForegroundColor Cyan
            }
        }
    } else {
        Write-Host "No se encontraron instancias registradas de SQL Server." -ForegroundColor Yellow
    }
} catch {
    Write-Host "Error al buscar en el registro: $_" -ForegroundColor Red
}

# Buscar servicios de SQL Server en ejecución
Write-Host "`nServicios de SQL Server en ejecución:" -ForegroundColor Green
Get-Service | Where-Object {$_.DisplayName -like "*SQL Server (*" -and $_.Status -eq "Running"} | ForEach-Object {
    $instanceName = $_.DisplayName -replace "SQL Server \((.*)\)", '$1'
    Write-Host "- $instanceName está ejecutándose" -ForegroundColor Cyan
}

# Intenta conectarse a las instancias comunes
Write-Host "`nProbando conexiones a instancias comunes..." -ForegroundColor Green

function Test-SqlConnection {
    param([string]$ServerInstance)
    
    try {
        $connectionString = "Server=$ServerInstance;Database=master;Integrated Security=True;Connection Timeout=3"
        $connection = New-Object System.Data.SqlClient.SqlConnection
        $connection.ConnectionString = $connectionString
        $connection.Open()
        
        # Si llegamos aquí, la conexión fue exitosa
        Write-Host "✅ Conexión exitosa a: $ServerInstance" -ForegroundColor Green
        
        # Obtener el nombre real del servidor y versión
        $command = $connection.CreateCommand()
        $command.CommandText = "SELECT @@SERVERNAME AS ServerName, @@VERSION AS Version"
        $reader = $command.ExecuteReader()
        
        if ($reader.Read()) {
            Write-Host "   - Nombre del servidor: " -NoNewline
            Write-Host $reader["ServerName"] -ForegroundColor Yellow
            Write-Host "   - Cadena de conexión a usar: " -NoNewline
            Write-Host "Server=$($reader["ServerName"]);Database=DentalPro;Integrated Security=True;" -ForegroundColor Yellow
        }
        
        $reader.Close()
        $connection.Close()
        return $true
    }
    catch {
        Write-Host "❌ No se pudo conectar a: $ServerInstance" -ForegroundColor Red
        return $false
    }
}

# Probar diferentes nombres de servidor comunes
$commonNames = @(".", "localhost", "(local)", $env:COMPUTERNAME, "$env:COMPUTERNAME\SQLEXPRESS", "(local)\SQLEXPRESS", ".\SQLEXPRESS")

foreach ($serverName in $commonNames) {
    Test-SqlConnection -ServerInstance $serverName
}

Write-Host "`nPresione Enter para salir..."
$null = Read-Host
