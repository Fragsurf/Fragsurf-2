using Fragsurf.Maps;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_Loading : UGuiModal
    {

        private void Update()
        {
            if(Map.Loading && !IsOpen)
            {
                Open();
            }
            else if(!Map.Loading && IsOpen)
            {
                Close();
            }
        }

    }
}

