# Installation

for DevKit / linux see [DevKit](DevKit.md)

### Prerequisites
  

*   **Visual Studio 2022** - [Download](https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx)
*   **Visual Studio 2022 Workload: .NET Desktop** - .NET Desktop development
*   **.NET 6.0** - [Download](https://www.microsoft.com/en-us/download/details.aspx?id=53321)
*   **VMware Player or Workstation** VMware Player is free, so that is recommended instead - [Download](https://www.vmware.com/uk/products/workstation-player/workstation-player-evaluation.html)
*   **Microsoft Visual C++ 2010 Redistributable** - [Download](https://www.microsoft.com/en-us/download/details.aspx?id=26999)
  
### Installing Cosmos
 
First, you need to choose between the User Kit and the Dev Kit. It is recommended that new users start with the User Kit but only move later to the Dev Kit if you need the latest features and want to contribute back to the main project. 
The Dev Kit is the live source against which the Cosmos Team develops directly. The Dev Kit has the latest and greatest features, but at various times has known issues, and sometimes may not even build. Thus to use the Dev Kit be sure to join our support channels and inquire about the current status before using the Dev Kit or updating it. 
  
The User Kit is a snapshot stable version of Cosmos including a premade installer. The UserKit however is often quite a bit out of date compared to the DevKit and is only occasionally updated. The User Kit is a great easy way to get familiar with Cosmos, but active developers should transition to the Dev Kit after becoming very familiar with the UserKit, and expect some bugs here and there.
  
### User Kit
  
1.  Download [the latest release of Cosmos](http://github.com/CosmosOS/Cosmos/releases/latest) (download the **exe** file)
2.  Wait for the download to complete then run the installer. Allow it to run as admin. Make sure **VS2022 is NOT running** when you do this.
3.  Click "Next" then "Install"
4.  Wait for the install to progress. **Tip:** At the end the installer may look like it has stalled, but it is still doing something in the background. WAIT for the "Finish" button to become available.
5.  Cosmos should now be installed. Follow other tutorials to find out how to create your first OS.