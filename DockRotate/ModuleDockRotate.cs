using System;
using UnityEngine;
using KSP.Localization;
using KSP.UI.Screens.DebugToolbar;

namespace DockRotate
{
	public class ModuleDockRotate: ModuleBaseRotate
	{
		/*

			docking node states:

			* PreAttached
			* Docked (docker/same vessel/dockee) - (docker) and (same vessel) are coupled with (dockee)
			* Ready
			* Disengage
			* Acquire
			* Acquire (dockee)

		*/

		private ModuleDockingNode dockingNode;

		public ModuleDockingNode getDockingNode()
		{
			return dockingNode;
		}

		[KSPEvent(
			guiActive = true,
			// put right label and group according to DEBUG
#if DEBUG
			groupName = DEBUGGROUP,
			groupDisplayName = DEBUGGROUP,
			groupStartCollapsed = true,
			guiActiveUncommand = true
#else
			guiName = "#DCKROT_log_state"
#endif
		)]
		public void CheckDockingState()
		{
			log(part.desc(), ": DOCKING STATE CHECK");

			if (!dockingNode) {
				log(part.desc(), ": no dockingNode");
				return;
			}

			DockingStateChecker checker = DockingStateChecker.load();
			if (checker == null)
				return;
			checker.checkNode(dockingNode, true);
		}

		public void showCheckDockingState(bool active)
		{
			BaseEvent evt = Events["CheckDockingState"];
			if (evt != null) {
				evt.guiActive = active || DEBUGMODE;
				// log(desc(), ".showCheckDockingState(" + active + "): done");
			} else {
				log(desc(), ".showCheckDockingState(" + active + "): can't find event");
			}
		}

		protected override void fillInfo()
		{
			storedModuleDisplayName = Localizer.Format("#DCKROT_port_displayname");
			storedInfo = Localizer.Format("#DCKROT_port_info");
		}

		protected override AttachNode findMovingNodeInEditor(out Part otherPart, bool verbose)
		{
			otherPart = null;
			if (!dockingNode || dockingNode.referenceNode == null)
				return null;
			if (verbose)
				log(desc(), ".findMovingNodeInEditor(): referenceNode = " + dockingNode.referenceNode.desc());
			AttachNode otherNode = dockingNode.referenceNode.getConnectedNode(verboseEvents);
			if (verbose)
				log(desc(), ".findMovingNodeInEditor(): otherNode = " + otherNode.desc());
			if (otherNode == null)
				return null;
			otherPart = otherNode.owner;
			if (verbose)
				log(desc(), ".findMovingNodeInEditor(): otherPart = " + otherPart.desc());
			if (!otherPart)
				return null;
			ModuleDockingNode otherDockingNode = otherPart.FindModuleImplementing<ModuleDockingNode>();
			if (verbose)
				log(desc(), ".findMovingNodeInEditor(): otherDockingNode = "
					+ (otherDockingNode ? otherDockingNode.part.desc() : "null"));
			if (!otherDockingNode)
				return null;
			if (verbose)
				log(desc(), ".findMovingNodeInEditor(): otherDockingNode.referenceNode = "
					+ otherDockingNode.referenceNode.desc());
			if (otherDockingNode.referenceNode == null)
				return null;
			if (!otherDockingNode.matchType(dockingNode)) {
				if (verbose)
					log(desc(), ".findMovingNodeInEditor(): mismatched node types "
						+ dockingNode.nodeType + " != " + otherDockingNode.nodeType);
				return null;
			}
			if (verbose)
				log(desc(), ".findMovingNodeInEditor(): node test is "
					+ (otherDockingNode.referenceNode.FindOpposingNode() == dockingNode.referenceNode));

			return dockingNode.referenceNode;
		}

		protected override bool setupLocalAxis(StartState state)
		{
			dockingNode = part.FindModuleImplementing<ModuleDockingNode>();

			if (!dockingNode) {
				log(desc(), ".setupLocalAxis(" + state + "): no docking node");
				return false;
			}

			partNodePos = Vector3.zero.Tp(dockingNode.T(), part.T());
			partNodeAxis = Vector3.forward.Td(dockingNode.T(), part.T());
			if (verboseEvents)
				log(desc(), ".setupLocalAxis(" + state + ") done: "
					+ partNodeAxis + "@" + partNodePos);
			return true;
		}

		protected override PartJoint findMovingJoint(bool verbose)
		{
			if (!dockingNode || !dockingNode.part) {
				if (verbose)
					log(desc(), ".findMovingJoint(): no docking node");
				return null;
			}

			ModuleDockingNode other = dockingNode.getDockedNode(verbose);
			if (!other || !other.part) {
				if (verbose)
					log(desc(), ".findMovingJoint(): no other, id = " + dockingNode.dockedPartUId);
				return null;
			}

			if (!dockingNode.matchType(other)) {
				if (verbose)
					log(desc(), ".findMovingJoint(): mismatched node types");
				return null;
			}

			ModuleBaseRotate otherModule = other.part.FindModuleImplementing<ModuleBaseRotate>();
			if (otherModule) {
				if (!smartAutoStruts && otherModule.smartAutoStruts) {
					smartAutoStruts = true;
					log(desc(), ".findMovingJoint(): smartAutoStruts activated by " + otherModule.desc());
				}
			}

			return dockingNode.getDockingJoint(verbose);
		}

		private static bool consoleSetupDone = false;

		protected override void doSetup()
		{
#if !DEBUG
			showCheckDockingState(false);
#endif
			base.doSetup();

			if (!consoleSetupDone) {
				consoleSetupDone = true;
				DebugScreenConsole.AddConsoleCommand("dr", consoleCommand, "DockRotate commands");
			}

#if DEBUG
			if (dockingNode) {
				log("[ModuleDockingNode] Part: " + part.name + "-" + part.persistentId
					+ " FSM Start State " + dockingNode.state
					+ " in ModuleDockRotate.doSetup(" + part.flightID + ")");
				dockingNode.DebugFSMState = true;
			}
#endif

			if (hasJointMotion && jointMotion.joint.Host == part && !frozenFlag) {
				float snap = autoSnapStep();
				if (verboseEvents)
					log(desc(), ".autoSnapStep() = " + snap);
				ModuleDockRotate other = jointMotion.joint.Target.FindModuleImplementing<ModuleDockRotate>();
				if (other) {
					float otherSnap = other.autoSnapStep();
					if (verboseEvents)
						log(other.desc(), ".autoSnapStep() = " + otherSnap);
					if (otherSnap > 0f && (snap.isZero() || otherSnap < snap))
						snap = otherSnap;
				}
				if (!snap.isZero()) {
					if (verboseEvents)
						log(jointMotion.desc(), ": autosnap at " + snap);
					enqueueFrozenRotation(jointMotion.angleToSnap(snap), 5f);
				}
			}
		}

		private float autoSnapStep()
		{
			if (!hasJointMotion || !dockingNode)
				return 0f;

			float step = 0f;
			string source = "no source";
			if (dockingNode.snapRotation && dockingNode.snapOffset > 0.01f) {
				step = dockingNode.snapOffset;
				source = "snapOffset";
			} else if (autoSnap && rotationEnabled && rotationStep > 0.01f) {
				step = rotationStep;
				source = "rotationStep";
			}
			if (verboseEvents)
				log(desc(), ".autoSnapStep() = " + step + " from " + source);
			if (step >= 360f)
				step = 0f;
			return step;
		}

		public override string descPrefix()
		{
			return "MDR";
		}

#if DEBUG
		public override void dumpExtra()
		{
			string d = desc();
			if (dockingNode) {
				log(d, ": attachJoint: " + part.attachJoint.desc());
				log(d, ": dockedPartUId: " + dockingNode.dockedPartUId);
				log(d, ": dockingNode state: \"" + dockingNode.state + "\"");
				log(d, ": sameVesselDockingJoint: " + dockingNode.sameVesselDockJoint.desc());
			} else {
				log(d, ": no dockingNode");
			}
		}
#endif

		private static char[] commandSeparators = { ' ', '\t' };

		public static void consoleCommand(string arg)
		{
			try {
				string[] args = arg.Split(commandSeparators, StringSplitOptions.RemoveEmptyEntries);
				if (args.Length < 1)
					throw new Exception("available /dr commands: check");
				if (!HighLogic.LoadedSceneIsFlight)
					throw new Exception("not in flight mode");
				Vessel v = FlightGlobals.ActiveVessel;
				if (!v)
					throw new Exception("no active vessel");

				if (args[0] == "check") {
					consoleCheck(v, args);
				} else {
					throw new Exception("illegal command");
				}
			} catch (Exception e) {
				log("ERROR: " + e.Message);
			}
			// log("CMD END");
		}

		public static void consoleCheck(Vessel v, string[] args)
		{
			if (args.Length != 1)
				throw new Exception("/dr check wants 0 arguments");
			VesselMotionManager vmm = VesselMotionManager.get(v);
			if (!vmm)
				throw new Exception("can't get VesselMotionManager");
			vmm.scheduleDockingStatesCheck(true);
		}
	}
}

