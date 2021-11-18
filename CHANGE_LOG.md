# Dock Rotate /L Unleashed :: Change Log

* 2020-1025: 1.10.1.52 (peteletroll) for KSP 1.8
	+ Bug fix
* 2020-1009: 1.10.1.51 (peteletroll) for KSP 1.8
	+ Fixed weird NRE with some part mods
* 2020-0924: 1.10.1.50 (peteletroll) for KSP 1.8
	+ More docking state checks
* 2020-0711: 1.10.0.49 (peteletroll) for KSP 1.8
	+ Configurable docking state checks
* 2020-0629: 1.9.1.48 (peteletroll) for KSP 1.8
	+ No changelog provided
* 2020-0626: 1.9.1.47 (peteletroll) for KSP 1.8
	+ No changelog provided
* 2020-0425: 1.9.1.46 (peteletroll) for KSP 1.8
	+ No changelog provided
* 2020-0329: 1.9.1.45 (peteletroll) for KSP 1.8
	+ No changelog provided
* 2020-0320: 1.9.1.44 (peteletroll) for KSP 1.8
	+ Now PartModules and MonoBehaviours enable Update() and FixedUpdate() only when needed.
* 2020-0307: 1.9.1.43 (peteletroll) for KSP 1.8
	+ No changelog provided
* 2020-0215: 1.9.0.42 (peteletroll) for KSP 1.8
	+ No changelog provided
* 2020-1025: 1.8.1.52 (peteletroll) for KSP 1.8
	+ Backport for KSP 1.8
* 2020-0711: 1.8.1.49 (peteletroll) for KSP 1.8
	+ Backport for RSS
* 2019-1101: 1.8.1.41 (peteletroll) for KSP 1.8
	+ No changelog provided
* 2019-1031: 1.8.1.40 (peteletroll) for KSP 1.8
	+ No changelog provided
* 2019-1022: 1.8.0.39b (peteletroll) for KSP 1.8 PRE-RELEASE
	+ This is a temporary workaround for KSP 1.8. The widget used for setting rotation step and speed is buggy, so I replaced it wit a simpler one, which is uncomfortable. I recommend using the new "#" button on the part menu and typing directly the desired values.
* 2019-1019: 1.7.0.39 (peteletroll) for KSP 1.8
	+ No changelog provided
* 2019-0715: 1.7.0.38c (peteletroll) for KSP 1.4.3. PRE-RELEASE
	+ This is a pre-release for snapOffset align testing
* 2019-0515: 1.7.0.37 (peteletroll) for KSP 1.4.3.
	+ This fixes a NodeRotate related bug. Please upgrade!
* 2019-0512: 1.7.0.36 (peteletroll) for KSP 1.4.3.
	+ Experimental compatibility with KJR Next.
* 2019-0428: 1.7.0.35 (peteletroll) for KSP 1.4.3.
	+ Use "rotatingNodeName = srfAttach" in the ModuleNodeRotate configuration.
* 2019-0419: 1.7.0.34 (peteletroll) for KSP 1.4.3.
	+ anglePosition, angleVelocity and angleIsMoving available to kOS scripts.
* 2019-0414: 1.7.0.33 (peteletroll) for KSP 1.4.3.
	+ No changelog provided
* 2019-0329: 1.6.1.32 (peteletroll) for KSP 1.4.3.
	+ Little UI bug fix! please update.
* 2019-0329: 1.6.1.31 (peteletroll) for KSP 1.4.3.
	+ Bug fix! Please update.
* 2019-0324: 1.6.1.30 (peteletroll) for KSP 1.4.3.
	+ Disabling rotation stops continuous rotation
* 2019-0323: 1.6.1.29 (peteletroll) for KSP 1.4.3.
	+ No changelog provided
* 2019-0319: 1.6.1.28 (peteletroll) for KSP 1.4.3.
	+ No changelog provided
* 2019-0319: 1.6.1.27 (peteletroll) for KSP 1.4.3.
	+ Fixed a bug: in the editor, if you detached a port that was connected to another you'd lose the attachment node, i.e. you couldn't reattach it.
* 2019-0317: 1.6.1.26 (peteletroll) for KSP 1.4.3.
	+ Included DeltaDizzy patch
* 2019-0224: 1.6.1.25 (peteletroll) for KSP 1.4.3.
	+ Now some rotation testing can be done in VAB and SPH
* 2019-0216: 1.6.1.24 (peteletroll) for KSP 1.4.3.
	+ This release solves a Smart Autostruts related problem that may happen when two docked ports have different Smart Autostruts settings. Now if one of the docked ports has Smart Autostruts enabled, the other one will have the flag activated too.
* 2019-0210: 1.6.1.23 (peteletroll) for KSP 1.4.3.
	+ 1) added flip-flop mode for deploy/release with a single action group;
	+ 2) HUGE refactoring, now almost everything is a MonoBehaviour.
* 2019-0118: 1.6.1.22 (peteletroll) for KSP 1.4.3.
	+ Robotic arms with Advanced Grabbing Units work much better now.
* 2018-1225: 1.5.1.21 (peteletroll) for KSP 1.4.3.
	+ Bug fixes, optimizations, random refactoring... the usual stuff.
* 2018-1124: 1.5.1.20 (peteletroll) for KSP 1.4.3.
	+ This is for variable AoA propellers.
* 2018-1116: 1.5.1.19 (peteletroll) for KSP 1.4.3.
	+ NodeRotate now works when attached to physicsless parts too (it actually converts them to full physics parts).
	+ And a new motor part by Psycho_zs.
* 2018-1112: 1.5.1.18 (peteletroll) for KSP 1.4.3.
	+ No changelog provided
* 2018-1102: 1.5.1.17 (peteletroll) for KSP 1.4.3.
	+ Now rotation can be continuous (set rotation step to 0) and fast (new UI for wider speed range up to 3600°/s).
	+ New UI for fine control of rotation step from 1°/s to 360°/s.
	+ You can definitely make action controlled variable-pitch propellers.
	+ Compiled for 1.5.1, but should work with 1.4.* too.
* 2018-1018: 1.4.4.16 (peteletroll) for KSP 1.4.3.
	+ If the rotation step is set to 0, the rotation will be continuous. You can stop it with the "Stop Rotation" right-click menu entry, the "Stop Rotation" action, or Alt-B.
	+ This release is compiled on 1.4.5, but works on 1.5.0 too.
* 2018-0706: 1.4.4.15 (peteletroll) for KSP 1.4.3.
	+ If you press the modifier keys (Alt on Windows, Right Shift on Linux), all rotation actions are reversed: for example, if "1" rotates a port clockwise, "Mod-1" will rotate it counterclockwise.
	+ All docking/undocking/decoupling events while moving are handled properly now.
	+ These improvements make building robotic grappling arms practical.
* 2018-0622: 1.4.3.14 (peteletroll) for KSP 1.4.3.
	+ Recompiled for 1.4.3. Should work with any 1.4.* anyway.
		- Lots of refactoring. I apparently like moving code around a lot.
		- Better handling of going on rails while moving.
* 2018-0427: 1.4.1.13 (peteletroll) for KSP 1.4.1
	+ added Psycho_zs rotating parts based on NodeRotate
	+ French and Spanish localization
* 2018-0421: 1.4.1.12 (peteletroll) for KSP 1.4.1
	+ Bug fix by heavy refactoring
* 2018-0420: 1.4.1.11 (peteletroll) for KSP 1.4.1
	+ Added NodeRotate module
	+ French and Italian localization
	+ Smart autostruts by default
* 2018-0324: 1.4.1.10 (peteletroll) for KSP 1.4.1
	+ Smart Autostruts added, need Advanced Tweakables. Experimental feature!
* 2018-0318: 1.4.1.9 (peteletroll) for KSP 1.4.1
	+ Better precision
	+ Added .version file
	+ Fixed missing resetVessel() after docking
* 2018-0316: 1.4.1.8 (peteletroll) for KSP 1.4.1
	+ Recompiled for KSP 1.4.1
	+ Added sound effect
* 2018-0311: 1.4.0.7 (peteletroll) for KSP 1.4.0
	+ No changelog provided
* 2018-0310: 1.4.0.6 (peteletroll) for KSP 1.4.0
	+ No changelog provided
* 2018-0303: 1.3.1.5 (peteletroll) for KSP 1.3.1
	+ No changelog provided
* 2018-0223: 1.3.1.4 (peteletroll) for KSP 1.3.1
	+ Better handling of undocking while moving and multidocking
* 2018-0222: 1.3.1.3 (peteletroll) for KSP 1.3.1
	+ Now autostruts are automatically disabled during motion.
* 2018-0220: 1.3.1.2 (peteletroll) for KSP 1.3.1
	+ All stock docking ports supported. See README.md for Clamp-O-Tron Sr.
* 2018-0202: 1.3.1.1 (peteletroll) for KSP 1.3.1
	+ Compiled for 1.3.1
