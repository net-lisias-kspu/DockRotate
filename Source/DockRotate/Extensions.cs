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
		/******** logging ********/

		private static int framelast = 0;
		private static string msg1last = "";

		public static bool log(string msg1, string msg2 = "")
		{
			int now = Time.frameCount;
			if (msg2 == "") {
				msg1last = "";
			} else {
				if (msg1 == msg1last && framelast == now) {
					msg1 = "    ... ";
				} else {
					framelast = now;
					msg1last = msg1;
				}
			}
			Debug.Log("[DR:"
#if DEBUG
				+ "d:"
#endif
				+ now + "] " + msg1 + msg2);
			return true;
		}

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
			log(v.desc(), ".KJRNextCycleAllAutoStrut()");

			const string pref = "KJRNext init";

			if (!KJRNextInitDone) {
				KJRNextInitDone = true;
				AssemblyLoader.loadedAssemblies.TypeOperation(t => {
					if (t != null && t.FullName == "KerbalJointReinforcement.KJRManager") {
						KJRNextManagerType = t;
						if (KJRNextManagerType != null) {
							log(pref, ": found type " + KJRNextManagerType);
							KJRNextCycleAllAutoStrutMethod = KJRNextManagerType.GetMethod("CycleAllAutoStrut");
							if (KJRNextCycleAllAutoStrutMethod != null)
								log(pref, ": found method " + KJRNextCycleAllAutoStrutMethod);
						}
					}
				});
			}

			if (KJRNextManagerType != null && KJRNextCycleAllAutoStrutMethod != null) {
				object kjrnm = FlightGlobals.FindObjectOfType(KJRNextManagerType);
				if (kjrnm != null) {
					log(pref, ": found object " + kjrnm);
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
		public static List<PartJoint> autoStruts(this Part part, bool verbose = false)
		{
			if (verbose)
				log(part.desc() + ".autoStruts(): begin");
			if (autoStrutsInfo == null)
				autoStrutsInfo = typeof(Part).GetField(autoStrutsField, BindingFlags.NonPublic | BindingFlags.Instance);
			if (autoStrutsInfo == null) {
				log(part.desc() + ".autoStruts(): can't access Part." + autoStrutsField);
				return null;
			}
			if (verbose)
				log(part.desc() + ".autoStruts(): got FieldInfo " + autoStrutsInfo);
			List<PartJoint> ret = autoStrutsInfo.GetValue(part) as List<PartJoint>;
			if (verbose)
				log(part.desc() + ".autoStruts(): got autoStrutJoints = "
					+ (ret == null ? "null" : "[" + ret.Count + "]"));
			if (ret != null && ret.Count <= 0)
				ret = null;
			return ret;
		}

		/******** Physics Activation utilities ********/

		public static bool hasPhysics(this Part part)
		{
			bool ret = (part.physicalSignificance == Part.PhysicalSignificance.FULL);
			if (ret != part.rb) {
				log(part.desc(), ": hasPhysics() Rigidbody incoherency: "
					+ part.physicalSignificance + ", " + (part.rb ? "rb ok" : "rb null"));
				ret = part.rb;
			}
			return ret;
		}

		public static bool forcePhysics(this Part part)
		{
			if (!part || part.hasPhysics())
				return false;

			log(part.desc(), ": calling PromoteToPhysicalPart(), "
				+ part.physicalSignificance + ", " + part.PhysicsSignificance);
			part.PromoteToPhysicalPart();
			log(part.desc(), ": called PromoteToPhysicalPart(), "
				+ part.physicalSignificance + ", " + part.PhysicsSignificance);
			if (part.parent) {
				if (part.attachJoint) {
					log(part.desc(), ": parent joint exists already: " + part.attachJoint.desc());
				} else {
					AttachNode nodeHere = part.FindAttachNodeByPart(part.parent);
					AttachNode nodeParent = part.parent.FindAttachNodeByPart(part);
					AttachModes m = (nodeHere != null && nodeParent != null) ?
						AttachModes.STACK : AttachModes.SRF_ATTACH;
					part.CreateAttachJoint(m);
					log(part.desc(), ": created joint " + m + " " + part.attachJoint.desc());
				}
			}

			return true;
		}

		/******** ModuleDockingNode utilities ********/

		public static ModuleDockingNode getDockedNode(this ModuleDockingNode node, bool verbose)
		{
			string label = node.part.desc();
			ModuleDockingNode other = node.otherNode;
			if (other) {
				if (verbose)
					log(label, ".getDockedNode(): other is " + other.part.desc());
			}
			if (!other && node.dockedPartUId > 0) {
				other = node.FindOtherNode();
				if (other) {
					log(label, ".getDockedNode(): other found " + other.part.desc()
						+ " with dockedPartUId = " + other.dockedPartUId
						+ " from id = " + node.dockedPartUId);
				}
			}
			if (!other && verbose)
				log(label, ".getDockedNode(): no other, id = " + node.dockedPartUId);
			return other;
		}

		public static PartJoint getDockingJoint(this ModuleDockingNode node, bool verbose)
		{
			PartJoint ret = null;

			ModuleDockingNode other = node.getDockedNode(verbose);
			if (!other)
				return null;

			PartJoint tmp = node.sameVesselDockJoint;
			if (tmp && tmp.Target == other.part) {
				if (verbose)
					log(node.part.desc(), ".getDockingJoint(): to same vessel " + tmp.desc());
				ret = tmp;
			}

			tmp = other.sameVesselDockJoint;
			if (!ret && tmp && tmp.Target == node.part) {
				if (verbose)
					log(node.part.desc(), ".getDockingJoint(): from same vessel " + tmp.desc());
				ret = tmp;
			}

			if (ret) {
				tmp = ret.getTreeEquiv(verbose);
				if (tmp)
					ret = tmp;
			}

			if (!ret && node.part.parent == other.part) {
				ret = node.part.attachJoint;
				if (verbose)
					log(node.part.desc(), ".getDockingJoint(): to parent " + ret.desc());
			}

			for (int i = 0; !ret && i < node.part.children.Count; i++) {
				Part child = node.part.children[i];
				if (child == other.part) {
					ret = child.attachJoint;
					if (verbose)
						log(node.part.desc(), ".getDockingJoint(): to child " + ret.desc());
				}
			}

			if (ret && other && !node.otherNode) {
				log(node.part.desc(), ": setting otherNode = " + other.part.desc());
				node.otherNode = other; // this fixes a ModuleDockingNode bug
				node.dockedPartUId = other.part.flightID;
			}

			if (!ret && node.dockedPartUId > 0) {
				log(node.part.desc(), ": dockedPartUId = " + node.dockedPartUId + ", but no joint");
				log(node.part.desc(), ": zeroing dockedPartUId = " + node.dockedPartUId);
				node.dockedPartUId = 0;
			}

			if (!ret && verbose)
				log(node.part.desc(), ".getDockingJoint(): nothing");

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
			log(node.part.desc(), ".fillNodeTypes(): fill with \"" + node.nodeType + "\"");
			if (node.nodeTypes == null) {
				log(node.part.desc(), ".fillNodeTypes(): creating HashSet");
				node.nodeTypes = new HashSet<string>();
			}
			string[] types = node.nodeType.Split(',');
			for (int i = 0; i < types.Length; i++) {
				string type = types[i].Trim();
				if (type == "")
					continue;
				log(node.part.desc(), ".fillNodeTypes(): adding \"" + type + "\" [" + i + "]");
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

		public static AttachNode getConnectedNode(this AttachNode node, bool verbose)
		{
			if (verbose)
				log(node.desc(), ".getConnectedNode()");

			if (node == null || !node.owner)
				return null;

			AttachNode fon = node.FindOpposingNode();
			if (fon != null) {
				if (verbose)
					log(node.desc(), ".getConnectedNode(): FindOpposingNode() finds " + fon.desc());
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
			if (verbose)
				log(node.desc(), ".getConnectedNode(): " + node.owner.desc()
					+ " has " + neighbours.Count + " neighbours");

			AttachNode closest = null;
			float dist = 0f;
			for (int i = 0; i < neighbours.Count; i++) {
				if (neighbours[i] == null)
					continue;
				List<AttachNode> n = neighbours[i].attachNodes;
				if (verbose)
					log(node.desc(), ".getConnectedNode(): " + neighbours[i] + " has " + n.Count + " nodes");

				for (int j = 0; j < n.Count; j++) {
					float d = node.distFrom(n[j]);
					if (verbose)
						log(node.desc(), ".getConnectedNode(): " + n[j].desc() + " at " + d);
					if (d < dist || closest == null) {
						closest = n[j];
						dist = d;
					}
				}
			}
			if (verbose)
				log(node.desc(), ".getConnectedNode(): found " + closest.desc() + " at " + dist);

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

		public static PartJoint getTreeEquiv(this PartJoint j, bool verbose)
		{
			if (!j)
				return null;
			PartJoint ret =
				j.sameParts(j.Host.attachJoint) ? j.Host.attachJoint :
				j.sameParts(j.Target.attachJoint) ? j.Target.attachJoint :
				null;

			if (ret && verbose)
				log(j.desc(), ".getTreeEquiv(): " + j.desc()
					+ " overruled by " + ret.desc());

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
			log("PartJoint " + j.desc());
			log("jAxes: " + j.Axis.desc() + " " + j.SecAxis.desc());
			log("jAxes(rb): " + j.Axis.Td(j.Host.T(), j.Target.rb.T()).desc()
				+ ", " + j.SecAxis.Td(j.Host.T(), j.Target.rb.T()).desc());
			log("jAnchors: " + j.HostAnchor.desc() + " " + j.TgtAnchor.desc());

			for (int i = 0; i < j.joints.Count; i++) {
				log("ConfigurableJoint[" + i + "]:");
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
			log("  Link: " + j.gameObject + " to " + j.connectedBody);
			log("  Axes: " + j.axis.desc() + ", " + j.secondaryAxis.desc());
			log("  Axes(rb): " + j.axis.Td(j.T(), j.connectedBody.T()).desc()
				+ ", " + j.secondaryAxis.Td(j.T(), j.connectedBody.T()).desc());

			log("  Anchors: " + j.anchor.desc()
				+ " -> " + j.connectedAnchor.desc()
				+ " [" + j.connectedAnchor.Tp(j.connectedBody.T(), j.T()).desc() + "]");

			log("  Tgt: " + j.targetPosition.desc() + ", " + j.targetRotation.desc());

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

