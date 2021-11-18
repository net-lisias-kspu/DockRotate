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
namespace DockRotate
{
	public partial class ModuleBaseRotate
	{
		[KSPField(
			guiName = "#DCKROT_angle",
			guiActive = true,
			guiActiveEditor = true
		)]
		public string angleInfo;

		[UI_Toggle]
		[KSPField(
			guiName = "#DCKROT_rotation",
			guiActive = true,
			guiActiveEditor = true,
			isPersistant = true
		)]
		public bool rotationEnabled = false;

		[UI_FloatEdit(
			incrementSlide = 0.5f, incrementSmall = 5f, incrementLarge = 30f,
			sigFigs = 1, unit = "\u00b0",
			minValue = 0f, maxValue = 360f
		)]
		[KSPField(
			guiActive = true,
			guiActiveEditor = true,
			isPersistant = true,
			guiName = "#DCKROT_rotation_step",
			guiUnits = "\u00b0"
		)]
		public float rotationStep = 15f;

		[UI_FloatEdit(
			incrementSlide = 1f, incrementSmall = 15f, incrementLarge = 180f,
			sigFigs = 0, unit = "\u00b0/s",
			minValue = 1, maxValue = 8f * 360f
		)]
		[KSPField(
			guiActive = true,
			guiActiveEditor = true,
			isPersistant = true,
			guiName = "#DCKROT_rotation_speed",
			guiUnits = "\u00b0/s"
		)]
		public float rotationSpeed = 5f;

		[UI_Toggle(affectSymCounterparts = UI_Scene.None)]
		[KSPField(
			guiActive = true,
			guiActiveEditor = true,
			isPersistant = true,
			advancedTweakable = true,
			guiName = "#DCKROT_reverse_rotation"
		)]
		public bool reverseRotation = false;

		[UI_Toggle]
		[KSPField(
			guiActive = true,
			guiActiveEditor = true,
			isPersistant = true,
			advancedTweakable = true,
			guiName = "#DCKROT_flip_flop_mode"
		)]
		public bool flipFlopMode = false;

		[UI_Toggle]
		[KSPField(
			guiActive = DEBUGMODE,
			guiActiveEditor = DEBUGMODE,
			isPersistant = true,
			advancedTweakable = false,
			guiName = "#DCKROT_smart_autostruts"
		)]
		public bool smartAutoStruts = true;

		[KSPField(
			guiActive = DEBUGMODE
		)]
		public float anglePosition;
		[KSPField(
			guiActive = DEBUGMODE
		)]
		public float angleVelocity;

		[KSPField(
			guiActive = DEBUGMODE
		)]
		public bool angleIsMoving;

#if DEBUG
		[KSPField(
			guiName = "#DCKROT_status",
			guiActive = true,
			guiActiveEditor = false
		)]
		public string nodeStatus = "";
#endif

#if DEBUG
		[KSPField(
			guiName = "AxisField",
			guiActive = true,
			guiActiveEditor = true,
			isPersistant = true,
			guiFormat = "F3"
		)]
		[UI_FloatRange]
		public float axisField = 0f;
#endif

		[KSPEvent(
			guiName = "#DCKROT_rotate_clockwise",
			guiActive = false,
			guiActiveEditor = false,
			requireFullControl = true
		)]
		public void RotateClockwise()
		{
			doRotateClockwise();
		}

		[KSPEvent(
			guiName = "#DCKROT_rotate_counterclockwise",
			guiActive = false,
			guiActiveEditor = false,
			requireFullControl = true
		)]
		public void RotateCounterclockwise()
		{
			doRotateCounterclockwise();
		}

		[KSPEvent(
			guiName = "#DCKROT_rotate_to_snap",
			guiActive = false,
			guiActiveEditor = false,
			requireFullControl = true
		)]
		public void RotateToSnap()
		{
			doRotateToSnap();
		}

		[KSPEvent(
			guiName = "#DCKROT_stop_rotation",
			guiActive = false,
			guiActiveEditor = false,
			requireFullControl = true
		)]
		public void StopRotation()
		{
			doStopRotation();
		}

#if DEBUG
		[UI_Toggle]
#endif
		[KSPField(
			guiName = "autoSnap",
			isPersistant = true,
			guiActive = DEBUGMODE,
			guiActiveEditor = DEBUGMODE
		)]
		public bool autoSnap = false;

#if DEBUG
		[UI_Toggle]
#endif
		[KSPField(
			guiName = "hideCommands",
			isPersistant = true,
			guiActive = DEBUGMODE,
			guiActiveEditor = DEBUGMODE
		)]
		public bool hideCommands = false;


#if DEBUG
		[KSPEvent(
			guiName = "Toggle Autostrut Display",
			guiActive = true,
			guiActiveEditor = true
		)]
		public void ToggleAutoStrutDisplay()
		{
			this.doToggleAutoStrutDisplay();
		}

		[KSPEvent(
			guiActive = true
		)]
		public void DumpToLog()
		{
			this.doDumpToLog();
		}

		[KSPEvent(
			guiName = "Cycle Autostruts",
			guiActive = true,
			guiActiveEditor = true
		)]
		public void CycleAutoStruts()
		{
			if (vessel)
				vessel.CycleAllAutoStrut();
		}

		private BaseEvent ToggleTraceEventsEvent;
		[KSPEvent(
			guiName = "Toggle Trace Events",
			guiActive = true,
			guiActiveEditor = true
		)]
		public void ToggleTraceEvents()
		{
			GameEvents.debugEvents = !GameEvents.debugEvents;
		}
#endif

	}
}
