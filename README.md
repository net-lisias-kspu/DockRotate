# Dock Rotate /L Unleashed :: Archive

Kerbal Space Program lightweight robotics with docking ports

[Unleashed](https://ksp.lisias.net/add-ons-unleashed/) fork by Lisias.


## In a Hurry

* [Latest Release](https://github.com/net-lisias-kspu/DockRotate/releases)
	+ [Binaries](https://github.com/net-lisias-kspu/DockRotate/tree/Archive)
* [Source](https://github.com/net-lisias-kspu/DockRotate)
* Documentation
	+ [Project's README](https://github.com/net-lisias-kspu/DockRotate/blob/master/README.md)
	+ [Install Instructions](https://github.com/net-lisias-kspu/DockRotate/blob/master/INSTALL.md)
	+ [Change Log](./CHANGE_LOG.md)
	+ [TODO](./TODO.md) list


## Description

Needing **deployable arms** for antennas on your satellites?

Wanting to try VTOL vehicles with **moving engines**?

Tired of **misaligned solar panel **trusses on your space stations?

This plugin can help you.

Docked port pairs ( * ) can rotate via `right-click menu` or `action groups` ( ** ). They can **rotate to snap** for perfect alignment. And if you want to uninstall the module, you won't lose any ship, because there's **no new parts** involved (_this is true for docking ports, but not true if you use NodeRotate parts_). Your station parts will stay aligned, and your satellite arms will stay deployed.

Rotation will malfunction if parts on opposite sides of the rotating joint are connected by struts.

All sorts of decoupling/docking/undocking/klawing/unklawing while moving are fully supported. **Yes, you can build a working Kanadarm**.

`Autostruts` are removed during motion, and restored after.

If advanced tweakables are enabled, you can access to the **experimental smart autostruts**. If the "Smart Autostruts" flag is enabled only the autostruts that cross the rotating joint are removed. This can misbehave with crossing struts, use with caution and quicksave before!

( * ) Should work with any port based on ModuleDockingNode.

( ** ) Since version 1.4.4.15, Mod key reverses action group rotations.

( *** ) DockRotate can be compatible with **Kerbal Joint Reinforcement**: see [@Geonovast](https://forum.kerbalspaceprogram.com/index.php?/profile/179753-geonovast/)'s post [here](https://forum.kerbalspaceprogram.com/index.php?/topic/170484-dockrotate-rotation-control-on-docking-ports/&do=findComment&comment=3305721).

( **** ) DockRotate is reported to work fine with **Konstruction weldable ports**: you can rotate to snap for maximum precision, then weld.

This is the right-click menu:
![](./PR_material/VMFJktu.png)

This is a space station with perfectly aligned solar panels:
![](./PR_material/HjUEwsi.jpg)


### NodeRotate

This module can be used to turn any connection node of any physically significant part into a rotating joint.

NodeRotate is intended for modders who want to create new rotating parts. There's an example NodeRotate.cfg file in the distribution, see there for details.



## Installation

Detailed installation instructions are now on its own file (see the [In a Hurry](#in-a-hurry) section) and on the distribution file.

## License:

* Dock Rotate /L Unleashed is double licensed as follows:
	+ [SKL 1.0](https://ksp.lisias.net/SKL-1_0.txt). See [here](./LICENSE.KSPe.SKL-1_0)
		+ You are free to:
			- Use : unpack and use the material in any computer or device
			- Redistribute : redistribute the original package in any medium
		+ Under the following terms:
			- You agree to use the material only on (or to) KSP
			- You don't alter the package in any form or way (but you can embedded it)
			- You don't change the material in any way, and retain any copyright notices
			- You must explicitly state the author's Copyright, as well an Official Site for downloading the original and new versions (the one you used to download is good enough)
	+ [GPL 2.0](https://www.gnu.org/licenses/gpl-2.0.txt). See [here](./LICENSE.KSPe.GPL-2_0)
		+ You are free to:
			- Use : unpack and use the material in any computer or device
			- Redistribute : redistribute the original package in any medium
			- Adapt : Reuse, modify or incorporate source code into your works (and redistribute it!) 
		+ Under the following terms:
			- You retain any copyright notices
			- You recognise and respect any trademarks
			- You don't impersonate the authors, neither redistribute a derivative that could be misrepresented as theirs.
			- You credit the author and republish the copyright notices on your works where the code is used.
			- You relicense (and fully comply) your works using GPL 2.0
				- Please note that upgrading the license to GPLv3 **IS NOT ALLOWED** for this work, as the author **DID NOT** added the "or (at your option) any later version" on the license.
			- You don't mix your work with GPL incompatible works.
	* If by some reason the GPL would be invalid for you, rest assured that you still retain the right to Use the Work under SKL 1.0. 

Releases previous to 1.13.0.0 are still available under the [MIT](https://opensource.org/licenses/MIT) [here](https://github.com/net-lisias-kspu/DockRotate/tree/Source/MIT) (and on the previous maintainer's repository, see below). Please note this [statement](https://www.gnu.org/licenses/license-list.en.html#Expat) from FSF.

Please note the copyrights and trademarks in [NOTICE](./NOTICE).

### Acknowledgements 

[@Psycho_zs](https://forum.kerbalspaceprogram.com/index.php?/profile/137644-psycho_zs/) contributed a few motor parts. You can find them in VAB/SPH in the Utility section.

This is a variable geometry F-14 Tomcat fighter made by @EvenFlow:
![](./PR_material/dxv2Qvl.png)

These are the rotating parts contributed by @Psycho_zs:
![](./PR_material/qGhdOEQ.png)
![](./PR_material/6Y3kqlo.png)



## UPSTREAM

* [DockRotate](https://forum.kerbalspaceprogram.com/index.php?/profile/144573-peteletroll/) ROOT
	+ [Forum](https://forum.kerbalspaceprogram.com/index.php?/topic/170484-*/)
	+ [Github](https://github.com/peteletroll/DockRotate)
