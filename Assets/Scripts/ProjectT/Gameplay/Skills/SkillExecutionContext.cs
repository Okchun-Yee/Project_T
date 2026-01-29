using UnityEngine;

namespace ProjectT.Gameplay.Skills.Runtime
{
    public readonly struct SkillExecutionContext
    {
        public readonly Transform owner;
        public readonly Transform spinHubRoot;

        public SkillExecutionContext(
            Transform owner,
            Transform spinHubRoot)
        {
            this.owner = owner;
            this.spinHubRoot = spinHubRoot;
        }
    }
}
