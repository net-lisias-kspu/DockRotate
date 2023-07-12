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
using System.Diagnostics;

namespace DockRotate
{
	public partial class ModuleDockRotate: ModuleBaseRotate
	{

		[KSPEvent(
			guiActive = true,
			// put right label and group according to DEBUG
#if DEBUG
			groupName = DEBUGGROUP,
			groupDisplayName = DEBUGGROUP,
			groupStartCollapsed = true,
			guiActiveUncommand = true
#else
			guiName = "#DCKROT_log_state"
#endif
		)]
		public void CheckDockingState()
		{
			this.doCheckDockingState();
		}

#if DEBUG
		public override void dumpExtra()
		{
			if (dockingNode) {
				Log.dbg(this, ": attachJoint: {0}", part.attachJoint.desc());
				Log.dbg(this, ": dockedPartUId: {0}", dockingNode.dockedPartUId);
				Log.dbg(this, ": dockingNode state: \"{0}\"", dockingNode.state);
				Log.dbg(this, ": sameVesselDockingJoint: {0}", dockingNode.sameVesselDockJoint.desc());
				Log.dbg(this, ": vesselInfo = {0}", dockingNode.vesselInfo.desc());
				Log.dbg(this, ": canRotateDefault() = {0}", canRotateDefault());
			} else {
				Log.dbg("{0}: no dockingNode");
			}
		}
#endif

		[ConditionalAttribute("DEBUG")]
		private void LogDockingNode()
		{
			if (dockingNode) {
				Log.detail(this, "Part: {0}-{1} FSM Start State {2} in ModuleDockRotate.doSetup({3})"
						, part.name, part.persistentId, dockingNode.state, part.flightID
					);
				Util.setDebug(dockingNode);
			}
		}
	}
}

