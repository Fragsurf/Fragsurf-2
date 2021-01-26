using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public interface IHasNetProps
    {
        int UniqueId { get; set; }
        bool HasAuthority { get; }
    }
}