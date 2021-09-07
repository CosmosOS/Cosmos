Param(
    [string]$vmName
)

Remove-VM -Name $vmName -Force
