using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
    public interface ISerializer
    {
        void Serialize(BeetleX.Buffers.IBinaryWriter writer);

        void Deserialize(BeetleX.Buffers.IBinaryReader reader);
    }
}
