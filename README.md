
# datalot-bundle
Data logging and bundling for Datalot.

## Introduction
[**Dark Age of Datalot**](http://darkageofdata.org/), or **Datalot** for short, is a spatiotemporal visualization system for the online game [Dark Age of Camelot (DAoC)](http://www.darkageofcamelot.com). Datalot visualizes data from DAoC over time and space using an online map accessed using your web browser. The data is collected by the players themselves using the Datalot logger and then bundled for upload using the Datalot bundler.

* Download the Datalot logger and bundler from the [releases](https://github.com/nickelm/datalot-bundle/releases) section.
* Access the Datalot system by visiting http://darkageofdata.org/.

## System Requirements
Please ensure that your computer conforms to at least the follow requirements:
* **Dark Age of Camelot** and an active subscription (see the [Dark Age of Camelot](http://www.darkageofcamelot.com) website).
* **Mojo for DAoC** - more details on the [Mojo](http://mojoware.org/) website.
* **Microsoft .NET Framework** - comes with most Windows installations.
* An account on the [Dark Age of Datalot](http://darkageofdata.org/) website.

## Installing Datalot
Follow these simple steps to install Datalot on your system:
1. Download the Datalot tools from the [releases](https://github.com/nickelm/datalot-bundle/releases) section on GitHub.
2. Extract the contents of the Datalot release into where you installed Mojo on your computer (to easily find this location, run Mojo and go to "Folders | Show Mojo Folders | Show Mojo's install folder"). **Note:** Make sure that both the `datalot.dll` and the `DatalotBundler.exe` file end up in the folder!
3. Edit your Mojo hotkey script to contain the following command to toggle logging on and off `mojule(datalot)`. See below for more details on scripting the Datalot mojule.
4. Whenever you run DAoC using Mojo the next time, Datalot will be automatically loaded. You can confirm this by right-clicking Mojo's blue M-icon inside the game window and selecting "Show loaded mojules".

## Privacy and Security
The Datalot logger will automatically capture the following information from your game session when it is **active** (nothing is captured when it is toggled off):
* Your character's position in 3D dimensions (x, y, z) as well as heading;
* Your character's region (Albion mainland, Albion SI, New Frontiers, etc) and zone (Camelot Hills, Dartmoor, Jamtland Mountains, etc);
* Your character's name; and
* Your combat log (minus any communications from other players).

**No data is uploaded automatically** without an explicit action by the user.

Significantly, Datalot does **not** capture the following information:
* No chat messages, including tells, say, region, etc (this is contained in the chat log but filtered for bundling);
* No screen contents (Datalot does not have access to your screen); and
* No username or password information.

## Disclaimer
Please use this utility at your own risk. There is no warranty implied with the usage of this tool. 

## Scripting for Datalot
More to come.

## Source Code
The complete C# source code for the Datalot bundler is provided on this GitHub repository for transparency so that you (or those with programming knowledge) can confirm that there is nothing underhanded with the utility. You are free to use one of the binary files provided with the release, or build the solution yourself.

Note that the Datalot tools release also contains the actual Datalot logger as a DLL file (`datalot.dll`). This is a Mojo plugin, or a so-called *mojule*. It uses Mojo's internal APIs to integrate with the DAoC game client and Mojo itself, logging the pertinent information from the game as well as the chat logs.

## Release History

| Version | Date | Description |
| - | - | - |
| 0.1.0 | March 10, 2021 | Initial public release. |

### Version 0.1.0
Initial public release:
* Basic command-line bundling functionality.
* Integration with the Datalot mojule.

## Creator and Credits
Dark Age of Datalot was created by **Madgrim Laeknir**, a Healer on Midgard/Ywain. The following people have had a significant impact on the development of Datalot: 
* Winter Mouse
* Rob from Mojo
* Zyzix
* Rheddrian
* Kapp
* Kobtalf

## Feedback?
Please send feedback to **BelomarFleetfoot#0319** on Discord.