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
using UnityEngine;

namespace DockRotate
{
	public class JointMotion: MonoBehaviour
	{
		private PartJoint _joint;

		public PartJoint joint { get => _joint; }

		public Vector3 hostAxis, hostNode;
		private Vector3 hostUp, targetUp;

		private float orgRot = 0f;

		private JointMotionObj _rotCur;
		public JointMotionObj rotCur {
			get { return _rotCur; }
			set {
				bool sas = (_rotCur && _rotCur.smartAutoStruts)
					|| (value && value.smartAutoStruts);

				int delta = (value && !_rotCur) ? +1
					: (!value && _rotCur) ? -1
					: 0;

				_rotCur = value;
				enabled = _rotCur;

				if (delta != 0 && joint && joint.Host && joint.Host.vessel) {
					VesselMotionManager.get(joint.Host.vessel).changeCount(delta);
					joint.Host.vessel.KJRNextCycleAllAutoStrut();
					if (!sas) {
						Log.trace(desc(), ": triggered CycleAllAutoStruts()");
						joint.Host.vessel.CycleAllAutoStrut();
					}
				}
			}
		}

		private ModuleBaseRotate _controller;
		public ModuleBaseRotate controller {
			get {
				if (!_controller)
					Log.warn(desc(), ": null controller");
				return _controller;
			}
			set {
				if (!value) {
					Log.warn(desc(), ": refusing to set null controller");
					return;
				}
				if (value != _controller) {
					if (_controller) {
						Log.trace(desc(), ": change controller {0} -> {1}", _controller.part.desc(), value.part.desc());
					} else {
						Log.trace(desc(), ": set controller {0}", value.part.desc());
					}
					if (_rotCur) {
						Log.trace(desc(), ": refusing to set controller while moving");
						return;
					}
				}

				_controller = value;
				if (_controller)
					_controller.putAxis(this);
			}
		}

		public static JointMotion get(PartJoint j)
		{
			if (!j)
				return null;

			if (j.gameObject != j.Host.gameObject)
				Log.warn(nameof(JointMotion), ".get(): gameObject incoherency");

			JointMotion jm = null;
			JointMotion[] jms = j.gameObject.GetComponents<JointMotion>();
			for (int i = 0; i < jms.Length; i++) {
				if (jms[i].joint == j) {
					if (!jm) {
						jm = jms[i];
					} else {
						Log.detail(nameof(JointMotion), ".get(): duplicated {0}", jms[i].desc());
						Destroy(jms[i]);
					}
				}
			}

			if (!jm) {
				jm = j.gameObject.AddComponent<JointMotion>();
				jm._joint = j;
				Log.detail(nameof(JointMotion), ".get(): created {0}", jm.desc());
			}

			return jm;
		}

		public bool hasController()
		{
			return _controller != null;
		}

		public void setAxis(Part part, Vector3 axis, Vector3 node)
		{
			if (rotCur) {
				Log.trace(desc(), ".setAxis(): rotating, skipped");
				return;
			}

			string state = "none";
			if (part == joint.Host) {
				state = "direct";
				// no conversion needed
			} else if (part == joint.Target) {
				state = "inverse";
				axis = -axis;
			} else {
				Log.trace(desc(), ".setAxis(): part {0} not in {1}", part.desc(), joint.desc());
			}
			if (!_controller)
				Log.trace(desc(), ".setAxis(): {0}@{1}, {2}", axis.desc(), node.desc(), state);
			hostAxis = axis.STd(part, joint.Host);
			hostNode = node.STp(part, joint.Host);
			hostUp = hostAxis.findUp();
			targetUp = hostAxis.STd(joint.Host, joint.Target).findUp();
		}

		public virtual bool enqueueRotation(ModuleBaseRotate source, float angle, float speed, float startSpeed = 0f)
		{
			if (!joint) {
				Log.detail(desc(), ".enqueueRotation(): canceled, no joint");
				return false;
			}

			if (speed < 0.1f) {
				Log.detail(desc(), ".enqueueRotation(): canceled, no speed");
				return false;
			}

			string action = "";
			if (rotCur) {
				if (rotCur.isBraking()) {
					Log.detail(desc(), ".enqueueRotation(): canceled, braking");
					return false;
				}
				rotCur.maxvel = speed;
				if (SmoothMotion.isContinuous(ref angle)) {
					if (!Mathf.Approximately(rotCur.tgt, angle)) {
						rotCur.tgt = angle;
						controller.updateFrozenRotation("MERGECONT");
						action = "updated to cont";
					}
				} else {
					float refAngle = rotCur.isContinuous() ? rotCur.pos + rotCur.curBrakingSpace() : rotCur.tgt;
					rotCur.tgt = refAngle + angle;
					controller.updateFrozenRotation("MERGELIM");
					action = "updated to lim";
				}
			} else {
				JointMotionObj r = new JointMotionObj(this, 0, angle, speed);
				r.vel = startSpeed;
				controller = source;
				r.electricityRate = source.electricityRate;
				r.smartAutoStruts = source.smartAutoStruts;
				rotCur = r;
				action = "added";
			}
			if (action != "")
				Log.detail(desc(), ": enqueueRotation({0}, {1:F4}\u00b0, {2}\u00b0/s, {3}\u00b0/s), {4}",
					hostAxis.desc(), rotCur.tgt, rotCur.maxvel, rotCur.vel, action);
			return true;
		}

		public void updateOrgRot()
		{
			Vector3 a = hostAxis;
			Vector3 v1 = hostUp;
			Vector3 v2 = targetUp.STd(joint.Target, joint.Host);
			float orgRotPrev = orgRot;
			orgRot = a.axisSignedAngle(v1, v2);
			if (!Mathf.Approximately(orgRot, orgRotPrev))
				Log.detail(desc(), ".updateOrgRot(): {0}\u00b0", orgRot);
		}

		public float rotationAngle()
		{
			return Mathf.DeltaAngle(0f, orgRot + (_rotCur ? _rotCur.pos : 0f));
		}

		public float rotationTarget()
		{
			if (!_rotCur)
				return orgRot;

			float tgt = _rotCur.tgt;
			if (SmoothMotion.isContinuous(ref tgt))
				return tgt > 0 ? float.PositiveInfinity : float.NegativeInfinity;

			tgt = Mathf.DeltaAngle(0f, orgRot + tgt);
			if (Mathf.Abs(tgt) < 1e-4f)
				tgt = 0f;
			return tgt;
		}

		private float dynamicDeltaAngleRestart = 0f;

		public float dynamicDeltaAngle()
		// = dynamic - static
		{
			float ret = float.NaN;
			if (Time.fixedTime > dynamicDeltaAngleRestart) {
				try {
					Vector3 a = hostAxis;
					Vector3 vd = targetUp.Td(joint.Target.T(), joint.Host.T());
					Vector3 vs = targetUp.STd(joint.Target, joint.Host);
					ret = a.axisSignedAngle(vs, vd);
				} catch (Exception e) {
					float delayException = 20f;
					dynamicDeltaAngleRestart = Time.fixedTime + delayException;
					Log.error(e, "{0}.dynamicDeltaAngle(): disabling for {1} seconds", this.desc(), delayException);
					Log.error("{0} : safetyCheck() is {1}", this.desc(), joint.safetyCheck());
				}
			}
			return ret;
		}

		public float angleToSnap(float snap)
		{
			snap = Mathf.Abs(snap);
			if (snap < 0.1f)
				return 0f;
			float refAngle = !rotCur ? rotationAngle() :
				rotCur.isContinuous() ? rotationAngle() + rotCur.curBrakingSpace() + snap * Mathf.Sign(rotCur.vel) / 2f :
				orgRot + rotCur.tgt;
			if (float.IsNaN(refAngle))
				return 0f;
			float snapAngle = snap * Mathf.Floor(refAngle / snap + 0.5f);
			return snapAngle - refAngle;
		}

		protected bool brakeRotationKey()
		{
			return GameSettings.MODIFIER_KEY.GetKey()
				&& GameSettings.BRAKES.GetKeyDown()
				&& joint && joint.Host && joint.Host.vessel == FlightGlobals.ActiveVessel;
		}

		public void FixedUpdate()
		{
			if (!rotCur || !HighLogic.LoadedSceneIsFlight) {
				// log(joint.desc(), ": disabling useless MonoBehaviour updates");
				enabled = false;
				return;
			}

			if (rotCur.done()) {
				Log.trace(desc(), ": removing rotation (done)");
				rotCur = null;
				return;
			}

			rotCur.clampAngle();
			if (brakeRotationKey())
				rotCur.brake();
			rotCur.advance(Time.fixedDeltaTime);
			controller.updateFrozenRotation("FIXED");
		}

		public void OnDestroy()
		{
			Log.trace(desc(), ".OnDestroy()");
			rotCur = null;
			stopSound();
		}

		/******** sound stuff ********/

		public const float pitchAlterationRateMax = 0.1f;

		public AudioSource sound;

		public float soundVolume = 1f;
		public float soundPitch = 1f;
		public float pitchAlteration = 1f;

		public void startSound()
		{
			if (sound)
				return;

			if (!rotCur || !controller) {
				Log.detail(desc(), "sound: no {0}", (rotCur ? "controller" : "rotation"));
				return;
			}

			try {
				soundVolume = controller.soundVolume;
				soundPitch = controller.soundPitch;
				AudioClip clip = GameDatabase.Instance.GetAudioClip(controller.soundClip);
				if (!clip) {
					Log.warn(desc(), "sound: clip \"{0}\" not found", controller.soundClip);
					return;
				}

				sound = joint.Host.gameObject.AddComponent<AudioSource>();
				sound.clip = clip;
				sound.volume = 0f;
				sound.pitch = 0f;
				sound.loop = true;
				sound.rolloffMode = AudioRolloffMode.Logarithmic;
				sound.spatialBlend = 1f;
				sound.minDistance = 1f;
				sound.maxDistance = 1000f;
				sound.playOnAwake = false;

				uint pa = (33u * (joint.Host.flightID ^ joint.Target.flightID)) % 10000u;
				pitchAlteration = 2f * pitchAlterationRateMax * (pa / 10000f)
					+ (1f - pitchAlterationRateMax);

				sound.Play();
			} catch (Exception e) {
				Log.error(e, "sound");
				stopSound();
			}
		}

		public void stepSound()
		{
			if (rotCur && sound != null) {
				float p = Mathf.Sqrt(Mathf.Abs(rotCur.vel / rotCur.maxvel));
				sound.volume = soundVolume * p * GameSettings.SHIP_VOLUME;
				sound.pitch = p * soundPitch * pitchAlteration;
			}
		}

		public void stopSound()
		{
			AudioSource s = sound;
			sound = null;
			if (s != null) {
				s.Stop();
				Destroy(s);
			}
		}

		public string desc(bool bare = false)
		{
			return (bare ? "" : "JM:") + GetInstanceID() + ":" + joint.desc(true);
		}
	}

	public class JointMotionObj: SmoothMotion
	{
		private JointMotion jm;

		public bool smartAutoStruts = false;

		public double electricity = 0d;
		public float electricityRate = 1f;

		private Part hostPart { get { return jm.joint.Host; } }
		private Part targetPart { get { return jm.joint.Target; } }

		public ModuleBaseRotate controller { get => jm ? jm.controller : null; }

		private Vector3 axis { get { return jm.hostAxis; } }
		private Vector3 node { get { return jm.hostNode; } }

		private struct RotJointInfo
		{
			public ConfigurableJointManager cjm;
			public Vector3 localAxis, localNode;
			public Vector3 connectedBodyAxis, connectedBodyNode;
		}
		private RotJointInfo[] rji;

		public JointMotionObj(JointMotion jm, float pos, float tgt, float maxvel)
		{
			this.jm = jm;

			this.pos = pos;
			this.tgt = tgt;
			this.maxvel = maxvel;

			this.vel = 0;
		}

		protected override void onStart()
		{
			hostPart.vessel.KJRNextCycleAllAutoStrut();
			if (smartAutoStruts) {
				hostPart.releaseCrossAutoStruts();
			} else {
				List<Part> parts = hostPart.vessel.parts;
				for (int i = 0; i < parts.Count; i++)
					parts[i].ReleaseAutoStruts();
			}
			int c = jm.joint.joints.Count;
			rji = new RotJointInfo[c];
			for (int i = 0; i < c; i++) {
				ConfigurableJoint j = jm.joint.joints[i];

				ref RotJointInfo ji = ref rji[i];

				ji.cjm = new ConfigurableJointManager();
				ji.cjm.setup(j);

				ji.localAxis = axis.Td(hostPart.T(), j.T());
				ji.localNode = node.Tp(hostPart.T(), j.T());

				ji.connectedBodyAxis = axis.STd(hostPart, targetPart)
					.Td(targetPart.T(), targetPart.rb.T());
				ji.connectedBodyNode = node.STp(hostPart, targetPart)
					.Tp(targetPart.T(), targetPart.rb.T());

				j.reconfigureForRotation();
			}

			jm.startSound();
		}

		protected override void onStep(float deltat)
		{
			int c = jm.joint.joints.Count;
			for (int i = 0; i < c; i++) {
				ConfigurableJoint j = jm.joint.joints[i];
				if (!j)
					continue;
				ref RotJointInfo ji = ref rji[i];
				ji.cjm.setRotation(pos, ji.localAxis, ji.localNode);
				/*
				if (jm.verboseEvents && Time.frameCount % 10 == 0)
					log(jm.desc(), ": currentTorque[" + i + "] = " + j.currentTorque.desc()
						+ " |" + j.currentTorque.magnitude.ToString("E10") + "|");
				*/
			}

			jm.stepSound();

			if (jm.controller) {
				float s = jm.controller.speed();
				if (!Mathf.Approximately(s, maxvel)) {
					Log.detail(jm.controller.part.desc(), ": speed change {0} -> {1}", maxvel, s);
					maxvel = s;
				}
				if (!jm.controller.rotationEnabled && isContinuous() && !isBraking()) {
					Log.detail(jm.desc(), ": disabled rotation, braking");
					brake();
				}
			}

			if (deltat > 0f && electricityRate > 0f) {
				double el = hostPart.RequestResource("ElectricCharge", (double) electricityRate * deltat);
				electricity += el;
				if (el <= 0d && !isBraking()) {
					Log.detail(jm.desc(), ": no electricity, braking rotation");
					brake();
				}
			}
		}

		protected override void onStop()
		{
			jm.stopSound();

			onStep(0);

			staticize();

			int c = VesselMotionManager.get(hostPart.vessel).changeCount(0);
			Log.detail(hostPart.desc(), ": rotation stopped [{0}], {0:F2} electricity", c, electricity);
			electricity = 0d;
		}

		public void staticize()
		{
			staticizeJoints();
			staticizeOrgInfo();
			jm.updateOrgRot();
		}

		private void staticizeJoints()
		{
			int c = jm.joint.joints.Count;
			for (int i = 0; i < c; i++) {
				ConfigurableJoint j = jm.joint.joints[i];
				if (!j)
					continue;

				ref RotJointInfo ji = ref rji[i];

				// staticize joint rotation

				Quaternion localRotation = ji.localAxis.rotation(pos);
				j.axis = localRotation * j.axis;
				j.secondaryAxis = localRotation * j.secondaryAxis;
				j.targetRotation = ji.cjm.tgtRot0;

				// staticize joint position

				Quaternion connectedBodyRot = ji.connectedBodyAxis.rotation(-pos);
				j.connectedAnchor = connectedBodyRot * (j.connectedAnchor - ji.connectedBodyNode)
					+ ji.connectedBodyNode;
				j.targetPosition = ji.cjm.tgtPos0;

				ji.cjm.setup();
			}
		}

		private bool staticizeOrgInfo()
		{
			if (jm.joint.isOffTree()) {
				Log.trace(jm.desc(), ": skip staticizeOrgInfo(), off tree");
				return false;
			}
			float angle = pos;
			Vector3 nodeAxis = -axis.STd(hostPart, hostPart.vessel.rootPart);
			Quaternion nodeRot = nodeAxis.rotation(angle);
			Vector3 nodePos = node.STp(hostPart, hostPart.vessel.rootPart);
			_propagate(hostPart, nodeRot, nodePos);
			return true;
		}

		private static void _propagate(Part p, Quaternion rot, Vector3 pos)
		{
			p.orgPos = rot * (p.orgPos - pos) + pos;
			p.orgRot = rot * p.orgRot;

			int c = p.children.Count;
			for (int i = 0; i < c; i++)
				_propagate(p.children[i], rot, pos);
		}

		public static implicit operator bool(JointMotionObj r)
		{
			return r != null;
		}
	}
}

