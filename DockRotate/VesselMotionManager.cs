using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;

namespace DockRotate
{
	public interface IStructureChangeListener
	{
		void OnVesselGoOnRails();
		void OnVesselGoOffRails();
		void RightBeforeStructureChange();
		void RightAfterStructureChange();
		bool wantsVerboseEvents();
		Part getPart();
		int getRevision();
	}

	public static class StructureChangeMapper
	{
		public static void map(this List<IStructureChangeListener> ls, Action<IStructureChangeListener> a)
		{
			int c = ls.Count;
			int i = 0;
			while (i < c) {
				try {
					while (i < c) {
						IStructureChangeListener l = ls[i++];
						if (l == null)
							continue;
						a(l);
					}
				} catch (Exception e) {
					Extensions.log(e.StackTrace);
				}
			}
		}
	}

	public class VesselMotionManager: MonoBehaviour
	{
		private Vessel vessel;

		private int Revision = -1;

		private Part rootPart = null;

		private int rotCount = 0;
		public bool onRails = false;

		private bool verboseEvents = false;
		private bool verboseCamera = false;

		public bool verbose()
		{
			return verboseEvents;
		}

		public static VesselMotionManager get(Vessel v)
		{
			if (!v)
				return null;
			if (!v.loaded)
				log(nameof(VesselMotionManager), ".get(" + v.desc() + ") called on unloaded vessel");
			if (!v.rootPart)
				log(nameof(VesselMotionManager), ".get(" + v.desc() + ") called on rootless vessel");

			VesselMotionManager mgr = null;
			VesselMotionManager[] mgrs = v.GetComponents<VesselMotionManager>();
			if (mgrs != null) {
				for (int i = 0; i < mgrs.Length; i++) {
					if (mgrs[i].vessel == v && mgrs[i].rootPart == v.rootPart && !mgr) {
						mgr = mgrs[i];
					} else {
						log(nameof(VesselMotionManager), ".get(" + v.desc() + ") found incoherency with " + mgrs[i].desc());
						Destroy(mgrs[i]);
					}
				}
			}

			if (!mgr) {
				mgr = v.gameObject.AddComponent<VesselMotionManager>();
				mgr.vessel = v;
				mgr.rootPart = v.rootPart;
				log(nameof(VesselMotionManager), ".get(" + v.desc() + ") created " + mgr.desc()
					+ " [" + mgr.listeners().Count + "]");
			}

			return mgr;
		}

		public void resetRotCount()
		{
			if (rotCount != 0)
				log(desc(), ".resetRotCount(): " + rotCount + " -> RESET");
			rotCount = 0;
		}

		public int changeCount(int delta)
		{
			int ret = rotCount + delta;
			if (ret < 0)
				ret = 0;

			if (rotCount == 0 && delta > 0)
				phase("START");

			if (verboseEvents && delta != 0)
				log(desc(), ".changeCount(" + delta + "): "
					+ rotCount + " -> " + ret);

			if (ret == 0 && rotCount > 0) {
				log(desc(), ": securing autostruts");
				vessel.CycleAllAutoStrut();
				vessel.KJRNextCycleAllAutoStrut();
			}

			if (ret == 0 && delta < 0)
				phase("STOP");

			return rotCount = ret;
		}

		/******** Events ********/

		bool eventState = false;

		private void setEvents(bool cmd)
		{
			if (cmd == eventState) {
				if (verboseEvents)
					log(desc(), ".setEvents(" + cmd + ") repeated");
				return;
			}

			if (verboseEvents)
				log(desc(), ".setEvents(" + cmd + ")");

			if (cmd) {

				GameEvents.onVesselCreate.Add(OnVesselCreate);

				GameEvents.onVesselGoOnRails.Add(OnVesselGoOnRails);
				GameEvents.onVesselGoOffRails.Add(OnVesselGoOffRails);

				GameEvents.OnCameraChange.Add(OnCameraChange_CameraMode);
				GameEvents.OnIVACameraKerbalChange.Add(OnCameraChange_Kerbal);

				GameEvents.onActiveJointNeedUpdate.Add(RightBeforeStructureChange_JointUpdate);

				GameEvents.onPartCouple.Add(RightBeforeStructureChange_Action);
				GameEvents.onPartCoupleComplete.Add(RightAfterStructureChange_Action);
				GameEvents.onPartDeCouple.Add(RightBeforeStructureChange_Part);
				GameEvents.onPartDeCoupleComplete.Add(RightAfterStructureChange_Part);

				GameEvents.onVesselDocking.Add(RightBeforeStructureChange_Ids);
				GameEvents.onDockingComplete.Add(RightAfterStructureChange_Action);
				GameEvents.onPartUndock.Add(RightBeforeStructureChange_Part);
				GameEvents.onPartUndockComplete.Add(RightAfterStructureChange_Part);

				GameEvents.onSameVesselDock.Add(RightAfterSameVesselDock);
				GameEvents.onSameVesselUndock.Add(RightAfterSameVesselUndock);

			} else {

				GameEvents.onVesselCreate.Remove(OnVesselCreate);

				GameEvents.onVesselGoOnRails.Remove(OnVesselGoOnRails);
				GameEvents.onVesselGoOffRails.Remove(OnVesselGoOffRails);

				GameEvents.OnCameraChange.Remove(OnCameraChange_CameraMode);
				GameEvents.OnIVACameraKerbalChange.Add(OnCameraChange_Kerbal);

				GameEvents.onActiveJointNeedUpdate.Remove(RightBeforeStructureChange_JointUpdate);

				GameEvents.onPartCouple.Remove(RightBeforeStructureChange_Action);
				GameEvents.onPartCoupleComplete.Remove(RightAfterStructureChange_Action);
				GameEvents.onPartDeCouple.Remove(RightBeforeStructureChange_Part);
				GameEvents.onPartDeCoupleComplete.Remove(RightAfterStructureChange_Part);

				GameEvents.onVesselDocking.Remove(RightBeforeStructureChange_Ids);
				GameEvents.onDockingComplete.Remove(RightAfterStructureChange_Action);
				GameEvents.onPartUndock.Remove(RightBeforeStructureChange_Part);
				GameEvents.onPartUndockComplete.Remove(RightAfterStructureChange_Part);

				GameEvents.onSameVesselDock.Remove(RightAfterSameVesselDock);
				GameEvents.onSameVesselUndock.Remove(RightAfterSameVesselUndock);

			}

			eventState = cmd;
		}

		struct StructureChangeInfo {
			public Part part;
			public int lastResetFrame;
			public string lastLabel;

			public void reset(string label)
			{
				if (lastLabel == "")
					lastLabel = "Init";
				log("" + GetType(), ".reset() " + label + " after " + lastLabel);
				this = new StructureChangeInfo();
				this.lastResetFrame = Time.frameCount;
				this.lastLabel = "reset " + label;
			}

			public bool isRepeated(string label)
			{
				if (lastLabel == "")
					lastLabel = "Init";
				bool ret = lastResetFrame == Time.frameCount;
				if (ret) {
					log("" + GetType(), ".isRepeated(): repeated " + label
						+ " after " + lastLabel);
				} else {
					log("" + GetType(), ".isRepeated(): set " + label
						+ " after " + lastLabel);
					lastLabel = label;
				}
				return ret;
			}
		}

		StructureChangeInfo structureChangeInfo;

		private bool care(Vessel v)
		{
			bool ret = v && v == vessel;
			if (verboseEvents)
				log(desc(), ".care(" + v.desc() + ") = " + ret + " on " + vessel.desc());
			return ret;
		}

		private bool care(Part p, bool useStructureChangeInfo)
		{
			if (useStructureChangeInfo && p && p == structureChangeInfo.part) {
				if (verboseEvents)
					log(desc(), ".care(" + p.desc() + ") = " + true);
				return true;
			}
			return p && care(p.vessel);
		}

		private bool care(GameEvents.FromToAction<Part, Part> action, bool useStructureChangeInfo)
		{
			return care(action.from, useStructureChangeInfo) || care(action.to, useStructureChangeInfo);
		}

		private bool care(GameEvents.FromToAction<ModuleDockingNode, ModuleDockingNode> action, bool useStructureChangeInfo)
		{
			return care(action.from.part, useStructureChangeInfo) || care(action.to.part, useStructureChangeInfo);
		}

		private bool care(uint id1, uint id2)
		{
			bool ret = vessel && (vessel.persistentId == id1 || vessel.persistentId == id2);
			if (verboseEvents)
				log(desc(), ".care(" + id1 + ", " + id2 + ") = " + ret);
			return ret;
		}

		public List<IStructureChangeListener> listeners()
		{
			List<IStructureChangeListener> ret = vessel.FindPartModulesImplementing<IStructureChangeListener>();

			bool verboseEventsPrev = verboseEvents;
			verboseEvents = false;

			int l = ret.Count;
			for (int i = 0; i < l; i++) {
				if (ret[i] == null)
					continue;
				if (ret[i].getRevision() > Revision)
					Revision = ret[i].getRevision();
				if (ret[i].wantsVerboseEvents()) {
					log(desc(), ": " + ret[i].getPart().desc() + " wants verboseEvents");
					verboseEvents = true;
					break;
				}
			}
			if (verboseEvents || verboseEventsPrev)
				log(desc(), ".listeners() finds " + ret.Count);

			if (verboseEvents != verboseEventsPrev)
				log(desc(), ".listeners(): verboseEvents changed to " + verboseEvents);

			return ret;
		}

		public List<IStructureChangeListener> listeners(Part p)
		{
			List<IStructureChangeListener> ret = p.FindModulesImplementing<IStructureChangeListener>();
			if (verboseEvents)
				log(desc(), ".listeners(" + p.desc() + ") finds " + ret.Count);
			return ret;
		}

		private bool deadVessel()
		{
			string deadMsg = "";

			if (!vessel) {
				deadMsg = "no vessel";
			} else if (!vessel.rootPart) {
				deadMsg = "no vessel root";
			} else if (vessel.rootPart != rootPart) {
				deadMsg = "root part changed";
			} else if (vessel.rootPart.vessel != vessel) {
				deadMsg = "vessel incoherency";
			}

			if (deadMsg == "")
				return false;

			log(desc(), ".deadVessel(): " + deadMsg);
			Destroy(this);
			return true;
		}

		public void OnVesselCreate(Vessel v)
		{
			if (verboseEvents)
				log(desc(), ".OnVesselCreate(" + v.desc() + ")");
			VesselMotionManager.get(v);
			VesselMotionManager.get(vessel);
		}

		public void OnVesselGoOnRails(Vessel v)
		{
			if (verboseEvents)
				log(desc(), ".OnVesselGoOnRails(" + v.desc() + ")");
			if (deadVessel())
				return;
			if (!care(v))
				return;
			phase("BEGIN ON RAILS");
			structureChangeInfo.reset("OnRails");
			listeners().map(l => l.OnVesselGoOnRails());
			phase("END ON RAILS");
			onRails = true;
		}

		public void OnVesselGoOffRails(Vessel v)
		{
			if (verboseEvents)
				log(desc(), ".OnVesselGoOffRails(" + v.desc() + ")");
			if (deadVessel())
				return;
			if (!care(v))
				return;

			phase("BEGIN OFF RAILS");

			get(v);

			resetRotCount();
			structureChangeInfo.reset("OffRails");
			onRails = false;
			listeners().map(l => l.OnVesselGoOffRails());

			phase("END OFF RAILS");

			scheduleDockingStatesCheck(false);
		}

		private void RightBeforeStructureChange_JointUpdate(Vessel v)
		{
			if (verboseEvents)
				log(desc(), ".RightBeforeStructureChange_JointUpdate()");
			if (!care(v))
				return;
			RightBeforeStructureChange("JointUpdate");
		}

		public void RightBeforeStructureChange_Ids(uint id1, uint id2)
		{
			if (verboseEvents)
				log(desc(), ".RightBeforeStructureChange_Ids("
					+ id1 + ", " + id2 + ")");
			if (!care(id1, id2))
				return;
			RightBeforeStructureChange("Ids");
		}

		public void RightBeforeStructureChange_Action(GameEvents.FromToAction<Part, Part> action)
		{
			if (verboseEvents)
				log(desc(), ".RightBeforeStructureChange_Action("
					+ action.from.desc() + ", " + action.to.desc() + ")");
			if (!care(action, false))
				return;
			RightBeforeStructureChange("Action");
		}

		public void RightBeforeStructureChange_Part(Part p)
		{
			if (verboseEvents)
				log(desc(), ".RightBeforeStructureChange_Part("
					+ p.vessel.desc() + ")");
			if (!care(p, false))
				return;
			structureChangeInfo.part = p;
			RightBeforeStructureChange("Part");
		}

		private void RightBeforeStructureChange(string label)
		{
			phase("BEGIN BEFORE CHANGE");
			if (!deadVessel() && !structureChangeInfo.isRepeated(label)) {
				structureChangeInfo.reset("BeforeChange");
				listeners().map(l => l.RightBeforeStructureChange());
			}
			phase("END BEFORE CHANGE");
		}

		public void RightAfterStructureChange_Action(GameEvents.FromToAction<Part, Part> action)
		{
			if (verboseEvents)
				log(desc(), ".RightAfterStructureChange_Action("
					+ action.from.vessel.desc() + ", " + action.to.vessel.desc() + ")");
			if (!care(action, true))
				return;
			RightAfterStructureChange();
		}

		public void RightAfterStructureChange_Part(Part p)
		{
			if (verboseEvents)
				log(desc(), ".RightAfterStructureChange_Part("
					+ p.vessel.desc() + ")");
			if (!care(p, true))
				return;
			RightAfterStructureChange();
		}

		private void RightAfterStructureChange()
		{
			phase("BEGIN AFTER CHANGE");
			if (!deadVessel())
				listeners().map(l => l.RightAfterStructureChange());
			phase("END AFTER CHANGE");
			scheduleDockingStatesCheck(false);
		}

		public void RightAfterSameVesselDock(GameEvents.FromToAction<ModuleDockingNode, ModuleDockingNode> action)
		{
			if (verboseEvents)
				log(desc(), ".RightAfterSameVesselDock("
					+ action.from.part.desc() + "@" + action.from.vessel.desc()
					+ ", " + action.to.part.desc() + "@" + action.to.vessel.desc() + ")");
			if (deadVessel())
				return;
			if (!care(action, false))
				return;
			log(vessel.desc(), ": same vessel dock " + action.from.state + " -> " + action.to.state);
			phase("BEGIN AFTER SV DOCK");
			if (rotCount != 0) {
				log(desc(), ": same vessel dock, rotCount = " + rotCount);
				log(desc(), ": from: " + action.from.getDockingJoint(true));
				log(desc(), ": to: " + action.to.getDockingJoint(true));
				listeners(action.from.part).map(l => l.RightAfterStructureChange());
				listeners(action.to.part).map(l => l.RightAfterStructureChange());
			} else {
				listeners(action.from.part).map(l => l.RightAfterStructureChange());
				listeners(action.to.part).map(l => l.RightAfterStructureChange());
			}
			phase("END AFTER SV DOCK");
			scheduleDockingStatesCheck(false);
		}

		public void RightAfterSameVesselUndock(GameEvents.FromToAction<ModuleDockingNode, ModuleDockingNode> action)
		{
			if (verboseEvents)
				log(desc(), ".RightAfterSameVesselUndock("
					+ action.from.vessel.desc() + ", " + action.to.vessel.desc()
					+ ")");
			if (deadVessel())
				return;
			if (!care(action, false))
				return;
			log(vessel.desc(), ": same vessel undock " + action.from.state + " -> " + action.to.state);
			phase("BEGIN AFTER SV UNDOCK");
			listeners(action.from.part).map(l => l.RightAfterStructureChange());
			listeners(action.to.part).map(l => l.RightAfterStructureChange());
			phase("END AFTER SV UNDOCK");
			scheduleDockingStatesCheck(false);
		}

		public void OnCameraChange_Kerbal(Kerbal k)
		{
			OnCameraChange();
		}

		public void OnCameraChange_CameraMode(CameraManager.CameraMode mode)
		{
			OnCameraChange();
		}

		public void OnCameraChange()
		{
			if (!verboseCamera)
				return;

			if (!HighLogic.LoadedSceneIsFlight || vessel != FlightGlobals.ActiveVessel)
				return;

			Camera camera = CameraManager.GetCurrentCamera();
			CameraManager manager = CameraManager.Instance;
			if (!camera || !manager)
				return;

			CameraManager.CameraMode mode = manager.currentCameraMode;
			if (mode != CameraManager.CameraMode.IVA && mode != CameraManager.CameraMode.Internal)
				return;

			phase("BEGIN CAMERA CHANGE " + mode, verboseCamera);
			log(desc(), ".OnCameraChange(" + mode + ")");

			/*
			Camera[] cameras = Camera.allCameras;
			for (int i = 0; i < cameras.Length; i++) {
				log("camera[" + i + "] = " + cameras[i].desc());
				log(cameras[i].transform.desc(10));
			}
			*/

			if (verboseCamera)
				reparentInternalModel();

			phase("END CAMERA CHANGE " +  mode, verboseCamera);
		}

		private void reparentInternalModel()
		{
			if (!CameraManager.Instance)
				return;
			CameraManager.CameraMode mode = CameraManager.Instance.currentCameraMode;
			if (mode != CameraManager.CameraMode.IVA && mode != CameraManager.CameraMode.Internal)
				return;

			InternalModel im = null;

			Camera[] c = Camera.allCameras;
			int nc = c.Length;
			for (int ic = 0; !im && ic < nc; ic++)
				for (Transform t = c[ic].transform; !im && t; t = t.parent)
					im = t.gameObject.GetComponent<InternalModel>();

			if (!im || !im.part)
				return;
			log(desc(), ".reparentInternalModel(): found in " + im.part.desc());

			for (int ic = 0; ic < nc; ic++) {
				for (Transform t = c[ic].transform; t && t.parent; t = t.parent) {
					Part pp = t.parent.gameObject.GetComponent<Part>();
					if (pp && pp != im.part) {
						phase("BEFORE REPARENTING", true);
						log(c[ic].transform.desc(10));

						t.SetParent(im.part.transform, true);
						phase("AFTER REPARENTING", true);
						log(c[ic].transform.desc(10));

						phase("REPARENTED", true);
						break;
					}
				}
			}
		}

		public void Awake()
		{
			if (!vessel) {
				vessel = gameObject.GetComponent<Vessel>();
				if (verboseEvents && vessel)
					log(desc(), ".Awake(): found vessel");
			}
			setEvents(true);
		}

		public void Start()
		{
			listeners(); // just to set verboseEvents
			enabled = false;
		}

		public void OnDestroy()
		{
			log(desc(), ".OnDestroy()");
			setEvents(false);
		}

		public void scheduleDockingStatesCheck(bool verbose)
		{
			StartCoroutine(checkDockingStates(verbose));
		}

		private int dockingCheckCounter = 0;

		public IEnumerator checkDockingStates(bool verbose)
		{
			if (!HighLogic.LoadedSceneIsFlight || vessel != FlightGlobals.ActiveVessel)
				yield break;
			DockingStateChecker checker = DockingStateChecker.load();
			if (checker == null)
				yield break;
			int thisCounter = ++dockingCheckCounter;

			int waitFrame = Time.frameCount + checker.checkDelay;
			while (Time.frameCount < waitFrame)
				yield return new WaitForFixedUpdate();

			if (thisCounter < dockingCheckCounter) {
				log("skipping analysis, another pending");
			} else {
				if (checker != null) {
					log((verbose ? "verbosely " : "")
						+ "analyzing incoherent states in " + vessel.GetName());
					DockingStateChecker.Result result = checker.checkVessel(vessel, verbose);
					if (result.foundError)
						ScreenMessages.PostScreenMessage(Localizer.Format("#DCKROT_bad_states"),
							checker.messageTimeout, checker.messageStyle, checker.colorBad);
				}
			}
		}

		private string desc()
		{
			return "VMM:" + GetInstanceID() + ":" + vessel.desc(true);
		}

		private void phase(string msg, bool force = false)
		{
			if (verboseEvents || force)
				log(new string('-', 10) + " " + msg + " " + new string('-', 60 - msg.Length));
		}

		protected static bool log(string msg1, string msg2 = "")
		{
			return Extensions.log(msg1, msg2);
		}
	}
}

