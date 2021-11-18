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
using UnityEngine;
using CompoundParts;

namespace DockRotate
{
	public class PartSet: Dictionary<uint, Part>
	{
		public void add(Part part)
		{
			if (!part)
				return;
			Add(part.flightID, part);
		}

		public bool contains(Part part)
		{
			return ContainsKey(part.flightID);
		}

		public static PartSet allPartsFromHere(Part p)
		{
			PartSet ret = new PartSet();
			_collect(ret, p);
			return ret;
		}

		private static void _collect(PartSet s, Part p)
		{
			s.add(p);
			int c = p.children.Count;
			for (int i = 0; i < c; i++)
				_collect(s, p.children[i]);
		}
	}

	public class PartJointSet: Dictionary<int, PartJoint>
	{
		public void add(PartJoint j)
		{
			if (j)
				Add(j.GetInstanceID(), j);
		}

		public bool contains(PartJoint j)
		{
			return ContainsKey(j.GetInstanceID());
		}
	}

	public static class SmartAutostruts
	{
		/******** Object.FindObjectsOfType<PartJoint>() cache ********/

		private static PartJoint[] cached_allJoints = null;
		private static int cached_allJoints_frame = 0;

		private static PartJoint[] getAllJoints()
		{
			if (cached_allJoints != null && cached_allJoints_frame == Time.frameCount)
				return cached_allJoints;
			cached_allJoints = UnityEngine.Object.FindObjectsOfType<PartJoint>();
			cached_allJoints_frame = Time.frameCount;
			return cached_allJoints;
		}

		/******** Vessel Autostruts cache ********/

		private static List<PartJoint> cached_allAutostrutJoints = new List<PartJoint>();
		private static Vessel cached_allAutostrutJoints_vessel = null;
		private static int cached_allAutostrutJoints_frame = 0;

		private static List<PartJoint> getAllAutostrutJoints(Vessel vessel)
		{
			if (cached_allAutostrutJoints_vessel == vessel
				&& cached_allAutostrutJoints_frame == Time.frameCount)
				return cached_allAutostrutJoints;

			cached_allAutostrutJoints.Clear();

			List<Part> parts = vessel.parts;
			int l = parts != null ? parts.Count : 0;
			for (int i = 0; i < l; i++) {
				List<PartJoint> partAutoStrutList = parts[i].autoStruts();
				if (partAutoStrutList != null)
					cached_allAutostrutJoints.AddRange(partAutoStrutList);
			}

			cached_allAutostrutJoints_vessel = vessel;
			cached_allAutostrutJoints_frame = Time.frameCount;
			return cached_allAutostrutJoints;
		}

		/******** public interface ********/

		public static void releaseCrossAutoStruts(this Part part)
		{
			if (!part.vessel || part.vessel.parts == null)
				return;
			PartSet rotParts = PartSet.allPartsFromHere(part);
			List<Part> parts = part.vessel.parts;
			int count = 0;
			for (int i = 0; i < parts.Count; i++) {
				if (parts[i].physicalSignificance != Part.PhysicalSignificance.FULL)
					continue;
				List<PartJoint> autoStruts = parts[i].autoStruts();
				if (autoStruts == null)
					continue;
				for (int ii = autoStruts.Count - 1; ii >= 0; ii--) {
					PartJoint j = autoStruts[ii];
					if (!j || !j.Host || !j.Target)
						continue;
					if (rotParts.contains(j.Host) != rotParts.contains(j.Target)
						|| j.Host == part || j.Target == part) {
							Log.detail(part.desc(), ": releasing [{0}] {1}", ++count, j.desc());
						j.DestroyJoint();
						autoStruts.RemoveAt(ii);
					}
				}
			}
		}

		public static void releaseCrossAutoStruts_old(this Part part)
		{
			PartSet rotParts = PartSet.allPartsFromHere(part);

			List<PartJoint> allAutostrutJoints = getAllAutostrutJoints(part.vessel);

			int count = 0;
			for (int ii = 0; ii < allAutostrutJoints.Count; ii++) {
				PartJoint j = allAutostrutJoints[ii];
				if (!j || !j.Host || !j.Target)
					continue;
				if (rotParts.contains(j.Host) != rotParts.contains(j.Target)
					|| j.Host == part || j.Target == part) {
					Log.detail(part.desc(), ": releasing [{0}] {1}", ++count, j.desc());
					j.Host.ReleaseAutoStruts();
				}
			}
		}
	}
}

