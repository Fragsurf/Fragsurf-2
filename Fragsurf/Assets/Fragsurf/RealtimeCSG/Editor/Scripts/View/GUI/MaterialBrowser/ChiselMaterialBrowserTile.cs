/* * * * * * * * * * * * * * * * * * * * * *
Chisel.Editors.ChiselMaterialBrowserTile.cs

License: MIT (https://tldrlegal.com/license/mit-license)
Author: Daniel Cornelius

* * * * * * * * * * * * * * * * * * * * * */

using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using Object = System.Object;

namespace Chisel.Editors
{
    internal class ChiselMaterialBrowserTile : IDisposable
    {
        public readonly string path;
        public readonly string guid;
        public readonly string shaderName;
        public readonly string materialName;
        public readonly string[] labels;
        public readonly int id;

        public float lastClickTime;

        private bool _rendering;
        private Texture2D m_Preview;
        public Texture2D Preview => m_Preview;
        

        /// <inheritdoc />
        public void Dispose()
        {
            m_Preview = null;
        }

        public bool CheckVisible(float yOffset, float thumbnailSize, Vector2 scrollPos, float scrollViewHeight)
        {
            if (scrollPos.y + scrollViewHeight < (yOffset - thumbnailSize)) return false;
            if (yOffset + thumbnailSize < scrollPos.y) return false;
            return true;
        }

        public void RenderThumbnail()
        {
            if (m_Preview
                || materialName.Contains("Font Material")
                || !ChiselMaterialBrowserUtilities.IsValidEntry(this)
                || _rendering)
            {
                return;
            }
            _rendering = true;
            ChiselMaterialThumbnailRenderer.Add(materialName,
                () => m_Preview = AssetPreview.GetAssetPreview(AssetDatabase.LoadAssetAtPath<Material>(path)),
                () => !AssetPreview.IsLoadingAssetPreview(id),
                () => _rendering = false);
        }

        public ChiselMaterialBrowserTile(string instID)
        {
            path = AssetDatabase.GUIDToAssetPath(instID);

            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);

            id = m.GetInstanceID();
            guid = instID;
            labels = AssetDatabase.GetLabels(m);
            shaderName = m.shader.name;
            materialName = m.name;
        }
    }
}
