/**
 * GameObjectEx.cs
 * 
 * @author mosframe / https://github.com/mosframe
 * 
 */

namespace Mosframe
{
    using UnityEngine;

    /// <summary>
    /// GameObject Extention
    /// </summary>
    public static class GameObjectEx
    {
        /// <summary>
        /// set layer
        /// </summary>
        public static void setLayer( this GameObject self, int layer, bool includeChildren = true )
        {
            self.layer = layer;
            if( includeChildren )
            {
                var children = self.transform.GetComponentsInChildren<Transform>(true);
                for( var c=0; c<children.Length; ++c ) {
                    children[c].gameObject.layer = layer;
                }
            }
        }
    }
}