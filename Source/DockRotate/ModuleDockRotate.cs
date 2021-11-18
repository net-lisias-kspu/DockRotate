/*
	This file is part of Dock Rotate /L Unleashed
		© 2021 Lisias T : http://lisias.net <support@lisias.net>
		© 2018-2021 peteletroll

	Dock Rotate /L Unleashed is double licensed, as follows:
		* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt
		* GPL 2.0 : https://www.gnu.org/licenses/gpl-2.0.txt

	And you are allowed to choose the License that better suit your needs.

	Dock Rotate /L Unleashed is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
	along with Dock Rotate /L Unleashed.
	If not, see <https://ksp.lisias.net/SKL-1_0.txt>.

	You should have received a copy of the GNU General Public License 2.0
	along with Dock Rotate /L Unleashed.
	If not, see <https://www.gnu.org/licenses/>.

*/
using System;
using UnityEngine;
using KSP.Localization;
using KSP.UI.Screens.DebugToolbar;

namespace DockRotate
{
	public partial class ModuleDockRotate: ModuleBaseRotate
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

		private ConfigNode getDockingNodeConfig()
		{
			if (part.partInfo == null || part.partInfo.partConfig == null)
				return null;
			return (part.partInfo != null && part.partInfo.partConfig != null) ?
				part.partInfo.partConfig.GetNode("MODULE", "name", nameof(ModuleDockingNode)) :
				null;
		}

		public bool canRotateDefault()
		{
			bool ret = false;
			ConfigNode n = getDockingNodeConfig();
			if (n != null)
				n.TryGetValue("canRotate", ref ret);
			return ret;
		}

		private BaseEvent SwitchToReadyEvent = null;
		[KSPEvent(
			guiName = "#DCKROT_switch_to_ready",
			guiActive = false,
			guiActiveEditor = false,
			requireFullControl = true
		)]
		public void SwitchToReady()
		{
			if (dockingNode && dockingNode.fsm != null && dockingNode.state == "Disengage") {
				Util.setDebug(dockingNode);
				dockingNode.fsm.StartFSM("Ready");
			}
		}

#if false
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
			this.doCheckDockingState();
		}
#endif
		private void doCheckDockingState()
		{
			Log.trace(part.desc(), ": DOCKING STATE CHECK");

			if (!dockingNode) {
				Log.trace(part.desc(), ": no dockingNode");
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
				Log.trace(desc(), ".showCheckDockingState({0}): can't find event", active);
			}
		}

		protected override void fillInfo()
		{
			storedModuleDisplayName = Localizer.Format("#DCKROT_port_displayname");
			storedInfo = Localizer.Format("#DCKROT_port_info");
		}

		protected override AttachNode findMovingNodeInEditor(out Part otherPart)
		{
			otherPart = null;
			if (!dockingNode || dockingNode.referenceNode == null)
				return null;
			Log.trace(desc(), ".findMovingNodeInEditor(): referenceNode = {0}", dockingNode.referenceNode.desc());
			AttachNode otherNode = dockingNode.referenceNode.getConnectedNode();
			Log.trace(desc(), ".findMovingNodeInEditor(): otherNode = {0}", otherNode.desc());
			if (otherNode == null)
				return null;
			otherPart = otherNode.owner;
			Log.trace(desc(), ".findMovingNodeInEditor(): otherPart = {0}", otherPart.desc());
			if (!otherPart)
				return null;
			ModuleDockingNode otherDockingNode = otherPart.FindModuleImplementing<ModuleDockingNode>();
			Log.trace(desc(), ".findMovingNodeInEditor(): otherDockingNode = {0}", (otherDockingNode ? otherDockingNode.part.desc() : "null"));
			if (!otherDockingNode)
				return null;
			Log.detail(desc(), ".findMovingNodeInEditor(): otherDockingNode.referenceNode = {0}", otherDockingNode.referenceNode.desc());
			if (otherDockingNode.referenceNode == null)
				return null;
			if (!otherDockingNode.matchType(dockingNode)) {
				Log.trace(desc(), ".findMovingNodeInEditor(): mismatched node types {0} != {1}", dockingNode.nodeType, otherDockingNode.nodeType);
				return null;
			}
			Log.trace(desc(), ".findMovingNodeInEditor(): node test is {0}", (otherDockingNode.referenceNode.FindOpposingNode() == dockingNode.referenceNode));

			return dockingNode.referenceNode;
		}

		protected override bool setupLocalAxis(StartState state)
		{
			dockingNode = part.FindModuleImplementing<ModuleDockingNode>();

			if (!dockingNode) {
				Log.trace(desc(), ".setupLocalAxis({0}): no docking node", state);
				return false;
			}

			partNodePos = Vector3.zero.Tp(dockingNode.T(), part.T());
			partNodeAxis = Vector3.forward.Td(dockingNode.T(), part.T());
			Log.trace(desc(), ".setupLocalAxis({0}) done: {1}@{2}", state, partNodeAxis, partNodePos);
			return true;
		}

		protected override PartJoint findMovingJoint()
		{
			if (!dockingNode || !dockingNode.part) {
				Log.trace(desc(), ".findMovingJoint(): no docking node");
				return null;
			}

			ModuleDockingNode other = dockingNode.getDockedNode();
			if (!other || !other.part) {
				Log.trace(desc(), ".findMovingJoint(): no other, id = {0}", dockingNode.dockedPartUId);
				return null;
			}

			if (!dockingNode.matchType(other)) {
				Log.trace(desc(), ".findMovingJoint(): mismatched node types");
				return null;
			}

			ModuleBaseRotate otherModule = other.part.FindModuleImplementing<ModuleBaseRotate>();
			if (otherModule) {
				if (!smartAutoStruts && otherModule.smartAutoStruts) {
					smartAutoStruts = true;
					Log.trace(desc(), ".findMovingJoint(): smartAutoStruts activated by {0}", otherModule.desc());
				}
			}

			return dockingNode.getDockingJoint();
		}

		private static bool consoleSetupDone = false;

		[KSPField(
			guiActive = false,
			guiActiveEditor = false,
			isPersistant = true
		)]
		private bool isDocked = false;

		protected override void doSetup(bool onLaunch)
		{
#if !DEBUG
			showCheckDockingState(false);
#endif
			base.doSetup(onLaunch);

			SwitchToReadyEvent = Events[nameof(SwitchToReady)];

			if (!consoleSetupDone) {
				consoleSetupDone = true;
				DebugScreenConsole.AddConsoleCommand("dr", consoleCommand, "DockRotate commands");
			}

#if DEBUG
			if (dockingNode) {
				Log.detail(nameof(ModuleDockRotate), "Part: {0}-{1} FSM Start State {2} in ModuleDockRotate.doSetup({3})"
						, part.name, part.persistentId, dockingNode.state, part.flightID
					);
				Util.setDebug(dockingNode);
			}
#endif

			if (onLaunch) {
				isDocked = hasJointMotion;
			} else if (isDocked != hasJointMotion) {
				isDocked = hasJointMotion;
				Log.trace(desc(), ": new docked state {0}", isDocked);
			}

			if (hasJointMotion && jointMotion.joint.Host == part && !frozenFlag) {
				float snap = autoSnapStep();
				Log.detail(desc(), ".autoSnapStep() = {0}", snap);
				ModuleDockRotate other = jointMotion.joint.Target.FindModuleImplementing<ModuleDockRotate>();
				if (other) {
					float otherSnap = other.autoSnapStep();
					Log.detail(other.desc(), ".autoSnapStep() = {0}", otherSnap);
					if (otherSnap > 0f && (snap.isZero() || otherSnap < snap))
						snap = otherSnap;
				}
				if (!snap.isZero() && !onLaunch) {
					Log.detail(jointMotion.desc(), ": autosnap at {0}", snap);
					enqueueFrozenRotation(jointMotion.angleToSnap(snap), 5f);
				}
			}
		}

		protected override void updateStatus(JointMotionObj cr)
		{
			base.updateStatus(cr);
			if (SwitchToReadyEvent != null)
				SwitchToReadyEvent.guiActive = dockingNode && dockingNode.state == "Disengage";
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
			Log.trace(desc(), ".autoSnapStep() = {0} from {1}", step, source);
			if (step >= 360f)
				step = 0f;
			return step;
		}

		public override string descPrefix()
		{
			return "MDR";
		}

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
				Log.error(e, typeof(ModuleDockRotate));
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

