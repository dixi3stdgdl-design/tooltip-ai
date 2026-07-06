<#
.SYNOPSIS
    Deploy TooltipAI via Group Policy (GPO)

.DESCRIPTION
    This script deploys TooltipAI using Group Policy Software Installation.
    It can be used as a startup script or login script.

.PARAMETER Action
    The action to perform: Install, Uninstall, or Status

.EXAMPLE
    .\deploy-gpo.ps1 -Action Install
    .\deploy-gpo.ps1 -Action Status
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Install", "Uninstall", "Status")]
    [string]$Action
)

$AppName = "TooltipAI"
$ServiceName = "TooltipAIService"
$InstallPath = "$env:ProgramFiles\TooltipAI"
$MsiPath = "\\domain.local\netlogon\TooltipAI\TooltipAI.msi"
$LogPath = "$env:Temp\TooltipAI-GPO-Install.log"

function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] $Message"
    Add-Content -Path $LogPath -Value $logEntry
    Write-Host $logEntry
}

function Install-TooltipAI {
    Write-Log "Starting $AppName installation via GPO..."
    
    # Check if already installed
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($service) {
        Write-Log "$AppName is already installed."
        return $true
    }
    
    # Verify MSI exists
    if (-not (Test-Path $MsiPath)) {
        Write-Log "MSI not found at: $MsiPath"
        return $false
    }
    
    # Install MSI silently
    $arguments = "/i `"$MsiPath`" /quiet /norestart ALLUSERS=1 /L*v `"$LogPath`""
    Write-Log "Running: msiexec.exe $arguments"
    
    $process = Start-Process -FilePath "msiexec.exe" -ArgumentList $arguments -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Log "$AppName installed successfully."
        
        # Verify installation
        $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
        if ($service) {
            Write-Log "Service verified running."
            return $true
        } else {
            Write-Log "Service not found after installation."
            return $false
        }
    } else {
        Write-Log "Installation failed with exit code: $($process.ExitCode)"
        return $false
    }
}

function Uninstall-TooltipAI {
    Write-Log "Starting $AppName uninstallation via GPO..."
    
    # Stop service first
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    
    # Uninstall MSI
    $arguments = "/x `"$MsiPath`" /quiet /norestart"
    $process = Start-Process -FilePath "msiexec.exe" -ArgumentList $arguments -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Log "$AppName uninstalled successfully."
        return $true
    } else {
        Write-Log "Uninstall failed with exit code: $($process.ExitCode)"
        return $false
    }
}

function Get-TooltipAIStatus {
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    $installed = Test-Path "$InstallPath\TooltipAI.Service.exe"
    $running = $service -and $service.Status -eq "Running"
    
    $status = [PSCustomObject]@{
        AppName = $AppName
        Installed = $installed
        ServiceExists = $null -ne $service
        ServiceStatus = if ($service) { $service.Status.ToString() } else { "Not Found" }
        InstallPath = if ($installed) { $InstallPath } else { "Not Installed" }
    }
    
    return $status
}

# Main execution
Write-Log "=== $AppName GPO Deployment Script ==="

switch ($Action) {
    "Install" {
        $result = Install-TooltipAI
        if ($result) {
            Write-Log "Installation completed successfully."
            exit 0
        } else {
            Write-Log "Installation failed."
            exit 1
        }
    }
    "Uninstall" {
        $result = Uninstall-TooltipAI
        if ($result) {
            Write-Log "Uninstallation completed successfully."
            exit 0
        } else {
            Write-Log "Uninstallation failed."
            exit 1
        }
    }
    "Status" {
        $status = Get-TooltipAIStatus
        Write-Log "Status: $($status | ConvertTo-Json)"
        if ($status.Installed) { exit 0 } else { exit 1 }
    }
}
