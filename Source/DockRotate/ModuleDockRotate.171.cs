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
			string d = desc();
			if (dockingNode) {
				Log.dbg("{0}: attachJoint: {1}", d, part.attachJoint.desc());
				Log.dbg("{0}: dockedPartUId: {1}", d, dockingNode.dockedPartUId);
				Log.dbg("{0}: dockingNode state: \"{1}\"", d, dockingNode.state);
				Log.dbg("{0}: sameVesselDockingJoint: {1}", d, dockingNode.sameVesselDockJoint.desc());
				Log.dbg("{0}: vesselInfo = {1}", d, dockingNode.vesselInfo.desc());
				Log.dbg("{0}: canRotateDefault() = {1}", d, canRotateDefault());
			} else {
				Log.dbg("{0}: no dockingNode", d);
			}
		}
#endif

	}
}

