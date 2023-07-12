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
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DockRotate
{
	public static class Extensions
	{
		/******** Vessel utilities ********/

		public static string desc(this Vessel v, bool bare = false)
		{
			uint id = (v && v.rootPart) ? v.rootPart.flightID : 0;
			string name = v ? v.name : "no-vessel";
			return (bare ? "" : "V:" + v.GetInstanceID() + ":") + id + ":" + name.Replace(' ', '_');
		}

		private static bool KJRNextInitDone = false;
		private static Type KJRNextManagerType = null;
		private static MethodInfo KJRNextCycleAllAutoStrutMethod = null;

		public static void KJRNextCycleAllAutoStrut(this Vessel v)
		{
			Log.trace(v.desc(), ".KJRNextCycleAllAutoStrut()");

			const string pref = "KJRNext init";

			if (!KJRNextInitDone) {
				KJRNextInitDone = true;
				AssemblyLoader.loadedAssemblies.TypeOperation(t => {
					if (t != null && t.FullName == "KerbalJointReinforcement.KJRManager") {
						KJRNextManagerType = t;
						if (KJRNextManagerType != null) {
							Log.trace(pref, ": found type {0}", KJRNextManagerType);
							KJRNextCycleAllAutoStrutMethod = KJRNextManagerType.GetMethod("CycleAllAutoStrut");
							if (KJRNextCycleAllAutoStrutMethod != null)
								Log.trace(pref, ": found method {0}", KJRNextCycleAllAutoStrutMethod);
						}
					}
				});
			}

			if (KJRNextManagerType != null && KJRNextCycleAllAutoStrutMethod != null) {
				object kjrnm = FlightGlobals.FindObjectOfType(KJRNextManagerType);
				if (kjrnm != null) {
					Log.trace(pref, ": found object {0}", kjrnm);
					KJRNextCycleAllAutoStrutMethod.Invoke(kjrnm, new object[] { v });
				}
			}
		}

		/******** Part utilities ********/

		public static string desc(this Part part, bool bare = false)
		{
			if (!part)
				return "null";
			string id = part.flightID > 0 ? part.flightID.ToString() : "I" + part.GetInstanceID();
			ModuleDockingNode mdn = bare ? null : part.FindModuleImplementing<ModuleDockingNode>();
			ModuleBaseRotate mbr = bare ? null : part.FindModuleImplementing<ModuleBaseRotate>();
			return (bare ? "" : "P:") + part.bareName() + ":" + id
				+ (mdn ? ":\"" + mdn.state + "\"" : "")
				+ (mbr ? ":" + mbr.nodeRole : "");
		}

		public static string bareName(this Part part)
		{
			if (!part)
				return "null";
			int s = part.name.IndexOf(' ');
			return s > 1 ? part.name.Remove(s) : part.name;
		}

		public static List<AttachNode> allAttachNodes(this Part part)
		{
			List<AttachNode> ret = new List<AttachNode>();
			if (part.srfAttachNode != null)
				ret.Add(part.srfAttachNode);
			ret.AddRange(part.attachNodes);
			return ret;
		}

		private static string autoStrutsField = "autoStrutJoints";
		private static FieldInfo autoStrutsInfo = null;
		public static List<PartJoint> autoStruts(this Part part)
		{
			Log.trace(part.desc(), ".autoStruts(): begin");
			if (autoStrutsInfo == null)
				autoStrutsInfo = typeof(Part).GetField(autoStrutsField, BindingFlags.NonPublic | BindingFlags.Instance);
			if (autoStrutsInfo == null) {
				Log.trace(part.desc(), ".autoStruts(): can't access Part.{0}", autoStrutsField);
				return null;
			}
			Log.trace(part.desc(),".autoStruts(): got FieldInfo {0}", autoStrutsInfo);
			List<PartJoint> ret = autoStrutsInfo.GetValue(part) as List<PartJoint>;
			Log.trace(part.desc(), ".autoStruts(): got autoStrutJoints = {0}", (ret == null ? "null" : ret.Count.ToString()));
			if (ret != null && ret.Count <= 0)
				ret = null;
			return ret;
		}

		/******** Physics Activation utilities ********/

		public static bool hasPhysics(this Part part)
		{
			bool ret = (part.physicalSignificance == Part.PhysicalSignificance.FULL);
			if (ret != part.rb) {
				Log.warn(part, ": hasPhysics() Rigidbody incoherency: {0}. {1}", part.physicalSignificance,  (part.rb ? "rb ok" : "rb null"));
				ret = part.rb;
			}
			return ret;
		}

		public static bool forcePhysics(this Part part)
		{
			if (!part || part.hasPhysics())
				return false;

			Log.trace(part.desc(), ": calling PromoteToPhysicalPart(), {0}, {1}", part.physicalSignificance, part.PhysicsSignificance);
			part.PromoteToPhysicalPart();
			Log.trace(part.desc(), ": called PromoteToPhysicalPart(), {0}, {1}", part.physicalSignificance, part.PhysicsSignificance);
			if (part.parent) {
				if (part.attachJoint) {
					Log.trace(part.desc(), ": parent joint exists already: {0}", part.attachJoint.desc());
				} else {
					AttachNode nodeHere = part.FindAttachNodeByPart(part.parent);
					AttachNode nodeParent = part.parent.FindAttachNodeByPart(part);
					AttachModes m = (nodeHere != null && nodeParent != null) ?
						AttachModes.STACK : AttachModes.SRF_ATTACH;
					part.CreateAttachJoint(m);
					Log.trace(part.desc(), ": created joint {0} {1}", m, part.attachJoint.desc());
				}
			}

			return true;
		}

		/******** ModuleDockingNode utilities ********/

		public static ModuleDockingNode getDockedNode(this ModuleDockingNode node)
		{
			ModuleDockingNode other = node.otherNode;
			if (other) {
				Log.detail(node.part, ".getDockedNode(): other is {0}", other.part.desc());
			}
			if (!other && node.dockedPartUId > 0) {
				other = node.FindOtherNode();
				if (other) {
					Log.detail(node.part
						, ".getDockedNode(): other found {0} with dockedPartUId = {1} from id = {2}"
						, other.part.desc(), other.dockedPartUId, node.dockedPartUId
					);
				}
			}
			if (!other)
				Log.detail(node.part, ".getDockedNode(): no other, id = {0}", node.dockedPartUId);
			return other;
		}

		public static PartJoint getDockingJoint(this ModuleDockingNode node)
		{
			PartJoint ret = null;

			ModuleDockingNode other = node.getDockedNode();
			if (!other)
				return null;

			PartJoint tmp = node.sameVesselDockJoint;
			if (tmp && tmp.Target == other.part) {
				Log.detail(node.part, ".getDockingJoint(): to same vessel {0}", (tmp as Log.IClient).who(false));
				ret = tmp;
			}

			tmp = other.sameVesselDockJoint;
			if (!ret && tmp && tmp.Target == node.part) {
				Log.detail(node.part, ".getDockingJoint(): from same vessel {0}", (tmp as Log.IClient).who(false));
				ret = tmp;
			}

			if (ret) {
				tmp = ret.getTreeEquiv();
				if (tmp)
					ret = tmp;
			}

			if (!ret && node.part.parent == other.part) {
				ret = node.part.attachJoint;
				Log.detail(node.part, ".getDockingJoint(): to parent {0}", ret.desc());
			}

			for (int i = 0; !ret && i < node.part.children.Count; i++) {
				Part child = node.part.children[i];
				if (child == other.part) {
					ret = child.attachJoint;
					Log.detail(node.part, ".getDockingJoint(): to child {0}", ret.desc());
				}
			}

			if (ret && other && !node.otherNode) {
				Log.detail(node.part, ": setting otherNode = {0}", other.part.desc());
				node.otherNode = other; // this fixes a ModuleDockingNode bug
				node.dockedPartUId = other.part.flightID;
			}

			if (!ret && node.dockedPartUId > 0) {
				Log.detail(node.part, ": dockedPartUId = {0}, but no joint", node.dockedPartUId);
				Log.detail(node.part, ": zeroing dockedPartUId = {0}", node.dockedPartUId);
				node.dockedPartUId = 0;
			}

			if (!ret)
				Log.detail(node.part, ".getDockingJoint(): nothing");

			return ret;
		}

		public static ModuleDockRotate getDockRotate(this ModuleDockingNode node)
		{
			if (!node.part)
				return null;
			List<ModuleDockRotate> dr = node.part.FindModulesImplementing<ModuleDockRotate>();
			for (int i = 0; i < dr.Count; i++)
				if (dr[i].getDockingNode() == node)
					return dr[i];
			return null;
		}

		public static bool matchType(this ModuleDockingNode node, ModuleDockingNode other)
		{
			fillNodeTypes(node);
			fillNodeTypes(other);
			return node.nodeTypes.Overlaps(other.nodeTypes);
		}

		private static void fillNodeTypes(this ModuleDockingNode node)
		{
			// this fills nodeTypes, sometimes empty in editor
			if (node.nodeTypes != null && node.nodeTypes.Count > 0)
				return;
			Log.detail(node.part, ".fillNodeTypes(): fill with \"{0}\"");
			if (node.nodeTypes == null) {
				Log.detail(node.part, ".fillNodeTypes(): creating HashSet");
				node.nodeTypes = new HashSet<string>();
			}
			string[] types = node.nodeType.Split(',');
			for (int i = 0; i < types.Length; i++) {
				string type = types[i].Trim();
				if (type == "")
					continue;
				Log.detail(node.part, ".fillNodeTypes(): adding \"{0}\" [{1}]", type, i);
				node.nodeTypes.Add(type);
			}
		}

		/******** DockedVesselInfo utilities ********/

		public static string desc(this DockedVesselInfo info)
		{
			if (info == null)
				return "null";
			return "DVI:"
				+ info.vesselType
				+ ":" + info.rootPartUId
				+ ":\"" + info.name + "\"";
		}

		/******** AttachNode utilities ********/

		public static AttachNode getConnectedNode(this AttachNode node)
		{
			Log.trace(node.desc(), ".getConnectedNode()");

			if (node == null || !node.owner)
				return null;

			AttachNode fon = node.FindOpposingNode();
			if (fon != null) {
				Log.trace(node.desc(), ".getConnectedNode(): FindOpposingNode() finds {0}", fon.desc());
				return fon;
			}

			List<Part> neighbours = new List<Part>();
			if (node.attachedPart) {
				neighbours.Add(node.attachedPart);
			} else {
				if (node.owner.parent)
					neighbours.Add(node.owner.parent);
				neighbours.AddRange(node.owner.children);
			}
			Log.trace(node.desc(), ".getConnectedNode(): {0} has {1} neighbours", node.owner.desc(), neighbours.Count);

			AttachNode closest = null;
			float dist = 0f;
			for (int i = 0; i < neighbours.Count; i++) {
				if (neighbours[i] == null)
					continue;
				List<AttachNode> n = neighbours[i].attachNodes;
				Log.trace(node.desc(), ".getConnectedNode(): {0} has {1} nodes", neighbours[i], n.Count);

				for (int j = 0; j < n.Count; j++) {
					float d = node.distFrom(n[j]);
					Log.detail(node, ".getConnectedNode(): {0} at {1}", n[j].desc(), d);
					if (d < dist || closest == null) {
						closest = n[j];
						dist = d;
					}
				}
			}
			Log.detail(node, ".getConnectedNode(): found {0} at {1}", closest.desc(), dist);

			if (closest == null || dist > 1e-2f)
				return null;

			return closest;
		}

		public static float distFrom(this AttachNode node, AttachNode other)
		{
			if (node == null || other == null || !node.owner || !other.owner)
				return 9e9f;
			Vector3 otherPos = other.position.Tp(other.owner.T(), node.owner.T());
			return (otherPos - node.position).magnitude;
		}

		public static string desc(this AttachNode n, bool bare = false)
		{
			if (n == null)
				return "null";
			return (bare ? "" : "AN:") + n.id + ":" + n.size
				+ ":" + n.owner.desc(true)
				+ ":" + (n.attachedPart ? n.attachedPart.desc(true) : "I" + n.attachedPartId);
		}

		/******** PartJoint utilities ********/

		public static bool safetyCheck(this PartJoint j)
		{
			return j
				&& j.Host && j.Target
				&& j.Host.vessel && j.Target.vessel
				&& j.Host.transform && j.Target.transform;
		}

		public static bool sameParts(this PartJoint j1, PartJoint j2)
		{
			if (!j1 || !j2)
				return false;
			return j1.Host == j2.Host && j1.Target == j2.Target
				|| j1.Host == j2.Target && j1.Target == j2.Host;
		}

		public static bool isOffTree(this PartJoint j)
		{
			return !j ? true
				: j.Host && j.Host.attachJoint == j ? false
				: j.Target && j.Target.attachJoint == j ? false
				: true;
		}

		public static PartJoint getTreeEquiv(this PartJoint j)
		{
			if (!j)
				return null;
			PartJoint ret =
				j.sameParts(j.Host.attachJoint) ? j.Host.attachJoint :
				j.sameParts(j.Target.attachJoint) ? j.Target.attachJoint :
				null;

			if (ret)
				Log.detail(j, ".getTreeEquiv(): {0} overruled by {1}", j.desc(), ret.desc());

			return ret;
		}

		public static string desc(this PartJoint j, bool bare = false)
		{
			if (!j)
				return "null";
			string host = j.Host.desc(true) + (j.Child == j.Host ? "" : "/" + j.Child.desc(true));
			string target = j.Target.desc(true) + (j.Parent == j.Target ? "" : "/" + j.Parent.desc(true));
			string ot = j.isOffTree() ? ":OT" : "";
			int n = j.joints.Count;
			return (bare ? "" : "PJ:" + j.GetInstanceID() + ":") + host + new string('>', n) + target + ot;
		}

		public static void dump(this PartJoint j)
		{
			Log.force("PartJoint {0}", j.desc());
			Log.force("jAxes: {0} {1}", j.Axis.desc(), j.SecAxis.desc());
			Log.force("jAxes(rb): {0}, {1}", j.Axis.Td(j.Host.T(), j.Target.rb.T()).desc(), j.SecAxis.Td(j.Host.T(), j.Target.rb.T()).desc());
			Log.force("jAnchors: {0} {1}", j.HostAnchor.desc(), j.TgtAnchor.desc());

			for (int i = 0; i < j.joints.Count; i++) {
				Log.force("ConfigurableJoint[{0}]:", i);
				j.joints[i].dump();
			}
		}

		/******** ConfigurableJoint utilities ********/

		public static void reconfigureForRotation(this ConfigurableJoint joint)
		{
			ConfigurableJointMotion f = ConfigurableJointMotion.Free;
			joint.angularXMotion = f;
			joint.angularYMotion = f;
			joint.angularZMotion = f;
			joint.xMotion = f;
			joint.yMotion = f;
			joint.zMotion = f;
		}

		public static void dump(this ConfigurableJoint j)
		{
			Log.force("  Link: {0} to {1}", j.gameObject, j.connectedBody);
			Log.force("  Axes: {0}, {1}", j.axis.desc(), j.secondaryAxis.desc());
			Log.force("  Axes(rb): {0}, {1}", j.axis.Td(j.T(), j.connectedBody.T()).desc(), j.secondaryAxis.Td(j.T(), j.connectedBody.T()).desc());
			Log.force("  Anchors: {0} -> {1} [{2}]", j.anchor.desc(), j.connectedAnchor.desc(), j.connectedAnchor.Tp(j.connectedBody.T(), j.T()).desc());
			Log.force("  Tgt: {0}, {1}", j.targetPosition.desc(), j.targetRotation.desc());

			/*
			log("  angX: " + desc(j.angularXMotion, j.angularXDrive, j.lowAngularXLimit, j.angularXLimitSpring));
			log("  angY: " + desc(j.angularYMotion, j.angularYZDrive, j.angularYLimit, j.angularYZLimitSpring));
			log("  angZ: " + desc(j.angularZMotion, j.angularYZDrive, j.angularZLimit, j.angularYZLimitSpring));
			log("  linX: " + desc(j.xMotion, j.xDrive, j.linearLimit, j.linearLimitSpring));
			log("  linY: " + desc(j.yMotion, j.yDrive, j.linearLimit, j.linearLimitSpring));
			log("  linZ: " + desc(j.zMotion, j.zDrive, j.linearLimit, j.linearLimitSpring));

			log("  proj: " + j.projectionMode + " ang=" + j.projectionAngle + " dst=" + j.projectionDistance);
			*/
		}

		public static string desc(ConfigurableJointMotion mot, JointDrive drv, SoftJointLimit lim, SoftJointLimitSpring spr)
		{
			return mot.ToString() + " " + drv.desc() + " " + lim.desc() + " " + spr.desc();
		}

		public static string desc(this JointDrive drive)
		{
			return "drv(frc=" + drive.maximumForce
				+ " spr=" + drive.positionSpring
				+ " dmp=" + drive.positionDamper
				+ ")";
		}

		public static string desc(this SoftJointLimit limit)
		{
			return "lim(lim=" + limit.limit
				+ " bnc=" + limit.bounciness
				+ " dst=" + limit.contactDistance
				+ ")";
		}

		public static string desc(this SoftJointLimitSpring spring)
		{
			return "spr(spr=" + spring.spring
				+ " dmp=" + spring.damper
				+ ")";
		}

		/******** action utilities ********/

		public static string desc(this KSPActionParam p)
		{
			return "[" + p.group + ", " + p.type + ", " + p.Cooldown + "]";
		}

		/******** float utilities ********/

		public static bool isZero(this float n)
		{
			return Mathf.Approximately(n, 0f);
		}

		/******** Vector3 utilities ********/

		public static Vector3 findUp(this Vector3 axis)
		{
			Vector3 up1 = Vector3.ProjectOnPlane(Vector3.up, axis);
			Vector3 up2 = Vector3.ProjectOnPlane(Vector3.forward, axis);
			return (up1.magnitude >= up2.magnitude ? up1 : up2).normalized;
		}

		public static Quaternion rotation(this Vector3 axis, float angle)
		{
			return Quaternion.AngleAxis(angle, axis);
		}

		public static float axisSignedAngle(this Vector3 axis, Vector3 v1, Vector3 v2)
		{
			v1 = Vector3.ProjectOnPlane(v1, axis).normalized;
			v2 = Vector3.ProjectOnPlane(v2, axis).normalized;
			float angle = Vector3.Angle(v1, v2);
			float s = Vector3.Dot(axis, Vector3.Cross(v1, v2));
			return (s < 0) ? -angle : angle;
		}

		public static string desc(this Vector3 v)
		{
			return v.ToString(v == Vector3.zero ? "F0" : "F2");
		}

		public static string ddesc(this Vector3 v, Part p)
		{
			string ret = v.desc();
			if (p && p.vessel.rootPart) {
				ret += " VSL" + v.Td(p.T(), p.vessel.rootPart.T()).desc();
			} else {
				ret += " (no vessel)";
			}
			return ret;
		}

		public static string pdesc(this Vector3 v, Part p)
		{
			string ret = v.desc();
			if (p && p.vessel.rootPart) {
				ret += " VSL" + v.Tp(p.T(), p.vessel.rootPart.T()).desc();
			} else {
				ret += " (no vessel)";
			}
			return ret;
		}

		/******** Quaternion utilities ********/

		public static Quaternion inverse(this Quaternion q)
		{
			return Quaternion.Inverse(q);
		}

		public static string desc(this Quaternion q)
		{
			q.ToAngleAxis(out float angle, out Vector3 axis);
			bool isIdentity = angle.isZero();
			return angle.ToString(isIdentity ? "F0" : "F1") + "\u00b0"
				+ (isIdentity ? Vector3.zero : axis).desc();
		}
	}
}

