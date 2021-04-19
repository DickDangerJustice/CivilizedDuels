using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CivilizedDuels.StatusEffects
{
    class SE_Challenged : StatusEffect
    {
        [Header("SE_Challenged")]
        private Character m_attacker;
		

		public override void Setup(Character character)
        {
            base.Setup(character);
        }

		public override void SetAttacker(Character attacker)
		{
			ZLog.Log("Setting attacker " + attacker.m_name);
			m_attacker = attacker;
			//m_time = 0f;
			m_attacker.Message(MessageHud.MessageType.Center, m_character.m_name + " challenged");
			//GameObject[] startEffectInstances = m_startEffectInstances;
			//foreach (GameObject gameObject in startEffectInstances)
			//{
			//	if ((bool)gameObject)
			//	{
			//		LineConnect component = gameObject.GetComponent<LineConnect>();
			//		if ((bool)component)
			//		{
			//			component.SetPeer(m_attacker.GetComponent<ZNetView>());
			//		}
			//	}
			//}
		}

		public override void UpdateStatusEffect(float dt)
        {
            base.UpdateStatusEffect(dt);
			Debug.Log("Test");
        }

		public override bool IsDone()
		{
			if (base.IsDone())
			{
				return true;
			}
			//if (m_broken)
			//{
			//	return true;
			//}
			//if (!m_attacker)
			//{
			//	return true;
			//}
			//if (m_time > 2f && (m_attacker.IsBlocking() || m_attacker.InAttack()))
			//{
			//	m_attacker.Message(MessageHud.MessageType.Center, m_character.m_name + " released");
			//	return true;
			//}
			return false;
		}
	}
}
