using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server.Wedding
{
    internal sealed class StoreUIDComponent : Component
    {
        [ViewVariables]
        public EntityUid? UID = null;
    }
}
