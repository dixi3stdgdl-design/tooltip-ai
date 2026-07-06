<#
.SYNOPSIS
    Deploy TooltipAI via Microsoft Intune

.DESCRIPTION
    This script deploys TooltipAI to managed Windows devices using Microsoft Intune.
    It can be used as a detection script, install script, or uninstall script.

.PARAMETER Action
    The action to perform: Install, Uninstall, or Detect

.EXAMPLE
    .\deploy-intune.ps1 -Action Install
    .\deploy-intune.ps1 -Action Uninstall
    .\deploy-intune.ps1 -Action Detect
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Install", "Uninstall", "Detect")]
    [string]$Action
)

$AppName = "TooltipAI"
$ServiceName = "TooltipAIService"
$InstallPath = "$env:ProgramFiles\TooltipAI"
$MsiPath = "\\server\share\TooltipAI.msi"  # Update with actual path

function Install-TooltipAI {
    Write-Host "Installing $AppName..."
    
    # Check if already installed
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($service) {
        Write-Host "$AppName is already installed."
        return $true
    }
    
    # Install MSI silently
    $arguments = "/i `"$MsiPath`" /quiet /norestart ALLUSERS=1"
    $process = Start-Process -FilePath "msiexec.exe" -ArgumentList $arguments -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Host "$AppName installed successfully."
        return $true
    } else {
        Write-Host "Installation failed with exit code: $($process.ExitCode)"
        return $false
    }
}

function Uninstall-TooltipAI {
    Write-Host "Uninstalling $AppName..."
    
    # Stop service first
    Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    
    # Uninstall MSI
    $arguments = "/x `"$MsiPath`" /quiet /norestart"
    $process = Start-Process -FilePath "msiexec.exe" -ArgumentList $arguments -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Host "$AppName uninstalled successfully."
        return $true
    } else {
        Write-Host "Uninstall failed with exit code: $($process.ExitCode)"
        return $false
    }
}

function Test-TooltipAIInstalled {
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    $installed = Test-Path "$InstallPath\TooltipAI.Service.exe"
    
    if ($service -and $installed) {
        Write-Host "$AppName is installed."
        return $true
    } else {
        Write-Host "$AppName is not installed."
        return $false
    }
}

# Main execution
switch ($Action) {
    "Install" {
        $result = Install-TooltipAI
        if ($result) { exit 0 } else { exit 1 }
    }
    "Uninstall" {
        $result = Uninstall-TooltipAI
        if ($result) { exit 0 } else { exit 1 }
    }
    "Detect" {
        $result = Test-TooltipAIInstalled
        if ($result) { exit 0 } else { exit 1 }
    }
}
