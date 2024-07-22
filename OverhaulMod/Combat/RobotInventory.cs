using UnityEngine;

namespace OverhaulMod.Combat
{
    public class RobotInventory : MonoBehaviour
    {
        public bool IsNotAbleToDoubleJump;

        private FirstPersonMover m_owner;
        public FirstPersonMover owner
        {
            get
            {
                if (!m_owner)
                {
                    m_owner = base.GetComponent<FirstPersonMover>();
                }
                return m_owner;
            }
        }

        public bool hasDoubleJumpAbility
        {
            get;
            private set;
        }

        public RobotSprintMethod sprintMethod
        {
            get;
            private set;
        }

        private void Start()
        {
            OnUpgradesRefreshed(owner._upgradeCollection);
        }

        public void OnUpgradesRefreshed(UpgradeCollection upgrades)
        {
            hasDoubleJumpAbility = upgrades.HasUpgrade(ModUpgradesManager.DOUBLE_JUMP_UPGRADE);
        }
    }
}
