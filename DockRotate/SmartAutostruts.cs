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

		private static List<PartJoint> getAllAutostrutJoints(Vessel vessel, bool verbose)
		{
			if (cached_allAutostrutJoints_vessel == vessel
				&& cached_allAutostrutJoints_frame == Time.frameCount)
				return cached_allAutostrutJoints;

			PartJointSet jointsToKeep = new PartJointSet();

			// keep same vessel docking joints
			List<ModuleDockingNode> allDockingNodes = vessel.FindPartModulesImplementing<ModuleDockingNode>();
			for (int i = 0; i < allDockingNodes.Count; i++)
				jointsToKeep.add(allDockingNodes[i].sameVesselDockJoint);

			// keep strut joints
			List<CModuleStrut> allStruts = vessel.FindPartModulesImplementing<CModuleStrut>();
			for (int i = 0; i < allStruts.Count; i++)
				jointsToKeep.add(allStruts[i].strutJoint);

			PartJoint[] allJoints = getAllJoints();
			cached_allAutostrutJoints.Clear();
			for (int ii = 0; ii < allJoints.Length; ii++) {
				PartJoint j = allJoints[ii];
				if (!j)
					continue;

				if (!j.Host || j.Host.vessel != vessel)
					continue;
				if (!j.Target || j.Target.vessel != vessel)
					continue;

				if (j == j.Host.attachJoint || j == j.Target.attachJoint)
					continue;

				if (jointsToKeep.contains(j))
					continue;

				cached_allAutostrutJoints.Add(j);
				if (verbose)
					log("Autostrut [" + cached_allAutostrutJoints.Count + "] " + j.desc());
			}

			cached_allAutostrutJoints_vessel = vessel;
			cached_allAutostrutJoints_frame = Time.frameCount;
			return cached_allAutostrutJoints;
		}

		/******** public interface ********/

		public static void releaseCrossAutoStruts(this Part part, bool verbose)
		{
			PartSet rotParts = PartSet.allPartsFromHere(part);

			List<PartJoint> allAutostrutJoints = getAllAutostrutJoints(part.vessel, verbose);

			int count = 0;
			for (int ii = 0; ii < allAutostrutJoints.Count; ii++) {
				PartJoint j = allAutostrutJoints[ii];
				if (!j || !j.Host || !j.Target)
					continue;
				if (rotParts.contains(j.Host) != rotParts.contains(j.Target)
					|| j.Host == part || j.Target == part) {
					if (verbose)
						log(part.desc() + ": releasing [" + ++count + "] " + j.desc());
					j.Host.ReleaseAutoStruts();
				}
			}
		}

		private static bool log(string msg1, string msg2 = "")
		{
			return Extensions.log(msg1, msg2);
		}
	}
}

