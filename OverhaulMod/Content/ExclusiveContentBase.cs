using System;

namespace OverhaulMod.Content
{
    public class ExclusiveContentBase
    {
        public bool ForceUnlock;

        [NonSerialized]
        public ExclusiveContentInfo InfoReference;
    }
}
