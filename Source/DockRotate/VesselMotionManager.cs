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
using System.Collections;
using UnityEngine;
using KSP.Localization;

namespace DockRotate
{
	public class VesselMotionManager: MonoBehaviour
	{
		private Vessel vessel;

		private Part rootPart = null;

		private int rotCount = 0;

		public static VesselMotionManager get(Vessel v)
		{
			if (!v)
				return null;
			if (!v.loaded)
				Log.warn(nameof(VesselMotionManager), ".get({0}) called on unloaded vessel", v.desc());
			if (!v.rootPart)
				Log.warn(nameof(VesselMotionManager), ".get({0}) called on rootless vessel", v.desc());

			VesselMotionManager mgr = null;
			VesselMotionManager[] mgrs = v.GetComponents<VesselMotionManager>();
			if (mgrs != null) {
				for (int i = 0; i < mgrs.Length; i++) {
					if (mgrs[i].vessel == v && mgrs[i].rootPart == v.rootPart && !mgr) {
						mgr = mgrs[i];
					} else {
						Log.warn(nameof(VesselMotionManager), ".get({0}) found incoherency with {1}", v.desc() , mgrs[i].desc());
						Destroy(mgrs[i]);
					}
				}
			}

			if (!mgr) {
				mgr = v.gameObject.AddComponent<VesselMotionManager>();
				mgr.vessel = v;
				mgr.rootPart = v.rootPart;
				Log.detail(nameof(VesselMotionManager), ".get({0}) created {1}", v.desc(), mgr.desc());
			}

			return mgr;
		}

		public void resetRotCount()
		{
			if (rotCount != 0)
				Log.trace(desc(), ".resetRotCount(): {0} -> RESET", rotCount);
			rotCount = 0;
		}

		public int changeCount(int delta)
		{
			int ret = rotCount + delta;
			if (ret < 0)
				ret = 0;

			if (rotCount == 0 && delta > 0)
				Log.trace(desc(), "START");

			if (delta != 0)
				Log.warn(desc(), ".changeCount({0}): {1} -> {2}", delta, rotCount, ret);

			if (ret == 0 && rotCount > 0) {
				Log.detail(desc(), ": securing autostruts");
				vessel.CycleAllAutoStrut();
				vessel.KJRNextCycleAllAutoStrut();
			}

			if (ret == 0 && delta < 0)
				Log.trace(desc(), "STOP");

			return rotCount = ret;
		}

		public void Awake()
		{
			if (!vessel) {
				vessel = gameObject.GetComponent<Vessel>();
				if (vessel)
					Log.detail(desc(), ".Awake(): found vessel");
			}
		}

		public void Start()
		{
			if (!vessel) {
				vessel = gameObject.GetComponent<Vessel>();
				if (vessel)
					Log.detail(desc(), ".Start(): found vessel");
			}
			setEvents(true);
			enabled = false;
		}

		public void OnDestroy()
		{
			setEvents(false);
			Log.trace(desc(), ".OnDestroy()");
		}

		private bool eventState = false;

		private void setEvents(bool cmd)
		{
			if (cmd == eventState)
				return;

			if (cmd) {
				GameEvents.onActiveJointNeedUpdate.Add(onActiveJointNeedUpdate);
			} else {
				GameEvents.onActiveJointNeedUpdate.Remove(onActiveJointNeedUpdate);
			}

			eventState = cmd;
		}

		private void onActiveJointNeedUpdate(Vessel v)
		{
			if (v != vessel)
				return;
			Log.trace(desc(), ".onActiveJointNeedUpdate({0})", v.desc());
		}

		public void scheduleDockingStatesCheck(bool verbose)
		{
			StartCoroutine(checkDockingStates(verbose));
		}

		private int dockingCheckCounter = 0;

		private IEnumerator checkDockingStates(bool verbose)
		{
			if (!HighLogic.LoadedSceneIsFlight || vessel != FlightGlobals.ActiveVessel)
				yield break;
			DockingStateChecker checker = DockingStateChecker.load();
			if (checker == null || !checker.enabledCheck)
				yield break;
			int thisCounter = ++dockingCheckCounter;

			int waitFrame = Time.frameCount + checker.checkDelay;
			while (Time.frameCount < waitFrame)
				yield return new WaitForFixedUpdate();

			if (thisCounter < dockingCheckCounter) {
				Log.trace(desc(), "skipping analysis, another pending");
			} else {
				Log.trace(desc(), "{0} analyzing incoherent states in {1}", (verbose ? "verbosely " : ""), vessel.GetName());
				DockingStateChecker.Result result = checker.checkVessel(vessel, verbose);
				if (result.foundError)
					Util.PostScreenMessage(Localizer.Format("#DCKROT_bad_states"), checker);
			}
		}

		private string desc()
		{
			return "VMM:" + GetInstanceID() + ":" + vessel.desc(true);
		}
	}
}

