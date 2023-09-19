# Devkit changelog

## c5c087c697d106348adbcff86d5d43331500e648 (update then merged in)

### Features

* X# is now packaged as a nuget package
* Plugs are now packaged as a nuget package rather then been hard coded in build scripts

### Breaking changes

Plugs are now included via a nuget package, to update cosmos past this commit you need to add the `Cosmos.Plugs` package to your kernel project. Don't forget to tick the `Include prerelease` checkbox and to set the package origin to `All`!
