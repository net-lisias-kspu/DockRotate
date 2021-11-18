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

namespace DockRotate
{
	public class JointLockStateProxy: PartModule, IJointLockState
	{
		public bool verboseEvents = true;

		private List<IJointLockState> tgt = null;

		public static void register(Part p, IJointLockState jls)
		{
			JointLockStateProxy jlsp = get(p);
			if (jlsp)
				jlsp.add(jls);
		}

		private static JointLockStateProxy get(Part p)
		{
			if (!p)
				return null;

			PartModule pm_jlsp = p.gameObject.GetComponent<JointLockStateProxy>();
			if (!pm_jlsp) {
				pm_jlsp = p.AddModule(nameof(JointLockStateProxy));
				log(nameof(JointLockStateProxy), ".get(" + p.desc() + ") created " + pm_jlsp);
			}
			JointLockStateProxy jlsp = pm_jlsp as JointLockStateProxy;
			jlsp.enabled = false;
			return jlsp;
		}

		private void add(IJointLockState jls)
		{
			if (tgt == null)
				tgt = new List<IJointLockState>();
			if (tgt.Contains(jls)) {
				log(desc(), ".add(): skip adding duplicate");
				return;
			}
			tgt.Add(jls);
		}

		public void OnDestroy()
		{
			log(desc(), ".OnDestroy()");
		}

		public bool IsJointUnlocked()
		{
			bool ret = false;
			if (tgt != null)
				for (int i = 0; i < tgt.Count && !ret; i++)
					if (tgt[i] != null && tgt[i].IsJointUnlocked())
						ret = true;
			if (verboseEvents || ret)
				log(desc(), ".IsJointUnLocked() is " + ret);
			return ret;
		}

		public string desc(bool bare = false)
		{
			return (bare ? "" : "JLSP:") + (tgt == null ? 0 : tgt.Count) + ":" + part.desc(true);
		}

		protected static bool log(string msg1, string msg2 = "")
		{
			return Extensions.log(msg1, msg2);
		}
	}
}

