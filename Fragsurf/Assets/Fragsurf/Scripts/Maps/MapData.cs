using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Maps
{
    public class MapData 
    {

        public string Name;
        public string Author;
        public Texture2D Cover;

        public IFragsurfMap GetFragsurfMap()
        {
            return null;
        }

    }
}

