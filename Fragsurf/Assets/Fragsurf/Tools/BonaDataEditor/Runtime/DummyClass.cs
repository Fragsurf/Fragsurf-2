using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fyrvall.DataEditor
{
    // This class is deliberetaly left empty and does not do anything. It cannot be instanciated or inherited from.
    // Its only purpose is to end up in the unity game assembly so reflection can be used with this as an entry point so
    // the assembly can be found and some reflection magic can be used on it.
    // DO NOT REMOVE
    public sealed class DummyClass{
        private DummyClass(){}
    }
}