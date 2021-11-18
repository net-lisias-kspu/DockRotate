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
using UnityEngine;

namespace DockRotate
{
	public struct ConfigurableJointManager
	{
		// local space:
		// defined by joint.transform.

		// joint space:
		// origin is joint.anchor;
		// right is joint.axis;
		// up is joint.secondaryAxis;
		// anchor, axis and secondaryAxis are defined in local space.

		private ConfigurableJoint joint;
		private Quaternion localToJoint, jointToLocal;
		public Quaternion tgtRot0;
		public Vector3 tgtPos0;

		public void setup(ConfigurableJoint joint)
		{
			this.joint = joint;
			setup();
		}

		public void setup()
		{
			// the jointToLocal rotation turns Vector3.right (1, 0, 0) to axis
			// and Vector3.up (0, 1, 0) to secondaryAxis

			// jointToLocal * v means:
			// vector v expressed in joint space
			// result is same vector in local space

			// source: https://answers.unity.com/questions/278147/how-to-use-target-rotation-on-a-configurable-joint.html

			Vector3 right = joint.axis.normalized;
			Vector3 forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
			Vector3 up = Vector3.Cross(forward, right).normalized;
			jointToLocal = Quaternion.LookRotation(forward, up);

			localToJoint = jointToLocal.inverse();

			tgtPos0 = joint.targetPosition;
			if (tgtPos0 != Vector3.zero)
				Extensions.log("JointManager: tgtPos0 = " + tgtPos0.desc());

			tgtRot0 = joint.targetRotation;
			if (tgtRot0 != Quaternion.identity)
				Extensions.log("JointManager: tgtRot0 = " + tgtRot0.desc());
		}

		public void setRotation(float angle, Vector3 axis, Vector3 node)
		// axis and node are in local space
		{
			Quaternion jointRotation = L2Jr(axis.rotation(angle));
			Vector3 jointNode = L2Jp(node);
			joint.targetRotation = tgtRot0 * jointRotation;
			joint.targetPosition = jointRotation * (tgtPos0 - jointNode) + jointNode;
		}

		public Vector3 L2Jd(Vector3 v)
		{
			return localToJoint * v;
		}

		public Vector3 J2Ld(Vector3 v)
		{
			return jointToLocal * v;
		}

		public Vector3 L2Jp(Vector3 v)
		{
			return localToJoint * (v - joint.anchor);
		}

		public Vector3 J2Lp(Vector3 v)
		{
			return jointToLocal * v + joint.anchor;
		}

		public Quaternion L2Jr(Quaternion r)
		{
			return localToJoint * r * jointToLocal;
		}

		public Quaternion J2Lr(Quaternion r)
		{
			return jointToLocal * r * localToJoint;
		}
	}
}

