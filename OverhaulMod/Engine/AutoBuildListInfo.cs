using System.Collections.Generic;

namespace OverhaulMod.Engine
{
    public class AutoBuildListInfo
    {
        public List<AutoBuildInfo> Builds;

        public void FixValues()
        {
            if (Builds == null)
                Builds = new List<AutoBuildInfo>();
        }
    }
}
