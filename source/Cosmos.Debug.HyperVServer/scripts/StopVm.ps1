Param(
    [string]$vmName
)

Stop-VM -Name $vmName -TurnOff
