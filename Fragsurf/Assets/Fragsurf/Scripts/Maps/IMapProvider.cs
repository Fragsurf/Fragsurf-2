using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fragsurf.Maps
{
    public interface IMapProvider
    {

        Task<List<BaseMap>> GetMapsAsync();

    }
}

