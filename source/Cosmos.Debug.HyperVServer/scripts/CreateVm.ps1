Param(
    [string]$vmName,
    [string]$hardDiskPath,
    [string]$isoPath
)

New-VM -Name $vmName -MemoryStartupBytes 268435456 -BootDevice CD -Generation 1

Add-VMHardDiskDrive -VMName $vmName -ControllerNumber 0 -ControllerLocation 0 -Path $hardDiskPath
Set-VMDvdDrive -VMName $vmName -ControllerNumber 1 -ControllerLocation 0 -Path $isoPath

Set-VMComPort -VMName $vmName -Path \\.\pipe\CosmosSerialHyperV -Number 1
