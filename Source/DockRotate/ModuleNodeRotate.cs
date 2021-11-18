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
using System.Collections.Generic;
using KSP.Localization;

namespace DockRotate
{
	public class ModuleNodeRotate: ModuleBaseRotate
	{
		[KSPField(isPersistant = true)]
		public string rotatingNodeName = "";

		[KSPField(isPersistant = true)]
		public bool enableJointMotionProxy = true;

		[KSPField(isPersistant = true)]
		public uint otherPartFlightID = 0;

		public AttachNode rotatingNode;

		private bool isSrfAttach()
		{
			return rotatingNodeName == "srfAttach";
		}

		protected override void fillInfo()
		{
			storedModuleDisplayName = Localizer.Format("#DCKROT_node_displayname");
			storedInfo = Localizer.Format("#DCKROT_node_info", rotatingNodeName);
		}

		protected override void doSetup(bool onLaunch)
		{
			base.doSetup(onLaunch);
			// TODO: change groupDisplayName to "NodeRotate <node name>"
		}

		protected override AttachNode findMovingNodeInEditor(out Part otherPart)
		{
			otherPart = null;
			if (rotatingNode == null)
				return null;
			Log.detail(desc(), ".findMovingNodeInEditor(): rotatingNode = {0}", rotatingNode.desc());
			otherPart = rotatingNode.attachedPart;
			Log.detail(desc(), ".findMovingNodeInEditor(): otherPart = {0}", otherPart.desc());
			if (!otherPart)
				return null;
			Log.detail(desc(), ".findMovingNodeInEditor(): attachedPart = {0}", rotatingNode.attachedPart.desc());
			return rotatingNode;
		}

		protected override bool setupLocalAxis(StartState state)
		{
			rotatingNode = part.FindAttachNode(rotatingNodeName);
			if (rotatingNode == null && isSrfAttach())
				rotatingNode = part.srfAttachNode;

			if (rotatingNode == null) {
				Log.detail(desc(), ".setupLocalAxis({0}): " + "no node \"{1}\"", state, rotatingNodeName);

				List<AttachNode> nodes = part.allAttachNodes();
				for (int i = 0; i < nodes.Count; i++)
					Log.detail(desc(), ": node[{0}] = {1}", i, nodes[i].desc());
				return false;
			}

			partNodePos = rotatingNode.position;
			partNodeAxis = rotatingNode.orientation;
			Log.detail(desc(), ".setupLocalAxis({0}) done: {1}@{2}", state, partNodeAxis, partNodePos);
			return true;
		}

		protected override PartJoint findMovingJoint()
		{
			uint prevOtherPartFlightID = otherPartFlightID;
			otherPartFlightID = 0;

			if (rotatingNode == null || !rotatingNode.owner) {
				Log.trace(desc(), ".findMovingJoint(): no node");
				return null;
			}

			if (part.FindModuleImplementing<ModuleDockRotate>()) {
				Log.trace(desc(), ".findMovingJoint(): has DockRotate, NodeRotate disabled");
				return null;
			}

			Part owner = rotatingNode.owner;
			Part other = rotatingNode.attachedPart;
			if (!other) {
				Log.trace(desc(), ".findMovingJoint({0}): attachedPart is null, try by id = {1}", rotatingNode.id, prevOtherPartFlightID);
				other = findOtherById(prevOtherPartFlightID);
			}
			if (!other) {
				Log.trace(desc(), ".findMovingJoint({0}): no attachedPart", rotatingNode.id);
				return null;
			}
			if (other.flightID != prevOtherPartFlightID)
				Log.trace(desc(), ".findMovingJoint({0}): otherFlightID {1} -> {2}", rotatingNode.id, prevOtherPartFlightID,  other.flightID);
			Log.trace(desc(), ".findMovingJoint({0}): attachedPart is {1}", rotatingNode.id, other.desc());
			other.forcePhysics();
			if (enableJointMotionProxy && HighLogic.LoadedSceneIsFlight)
				JointLockStateProxy.register(other, this);

			if (owner.parent == other) {
				PartJoint ret = owner.attachJoint;
				Log.detail(desc(), ".findMovingJoint({0}): child {1}", rotatingNode.id, ret.desc());
				otherPartFlightID = other.flightID;
				return ret;
			}

			if (other.parent == owner) {
				PartJoint ret = other.attachJoint;
				Log.detail(desc(), ".findMovingJoint({0}): parent {1}", rotatingNode.id, ret.desc());
				otherPartFlightID = other.flightID;
				return ret;
			}

			Log.trace(desc(), ".findMovingJoint({0}): nothing", rotatingNode.id);
			return null;
		}

		private Part findOtherById(uint id)
		{
			if (id == 0)
				return null;
			if (part.parent && part.parent.flightID == id)
				return part.parent;
			if (part.children != null)
				for (int i = 0; i < part.children.Count; i++)
					if (part.children[i] && part.children[i].flightID == id)
						return part.children[i];
			return null;
		}

		public override string descPrefix()
		{
			return "MNR";
		}
	}
}

