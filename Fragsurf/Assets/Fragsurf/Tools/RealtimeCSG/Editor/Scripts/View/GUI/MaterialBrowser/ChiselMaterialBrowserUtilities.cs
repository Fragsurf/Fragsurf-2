/* * * * * * * * * * * * * * * * * * * * * *
Chisel.Editors.ChiselMaterialBrowserCache.cs

License: MIT (https://tldrlegal.com/license/mit-license)
Author: Daniel Cornelius

* * * * * * * * * * * * * * * * * * * * * */

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RealtimeCSG.Components;
using SurfaceConfigurator;
using UnityEditor;
using UnityEngine;

namespace Chisel.Editors
{
    internal static class ChiselMaterialBrowserUtilities
    {
        // checks a path and returns true/false if a material is ignored or not
        public static bool IsValidEntry( ChiselMaterialBrowserTile tile )
        {
            // these are here to clean things up a little bit and make it easier to read

            bool PathContains( string path )
            {
                return tile.path.ToLower().Contains( path );
            }

            // checks for any shaders we want to exclude
            bool HasInvalidShader()
            {
                string shader = tile.shaderName.ToLower();

                string[] excludedShaders = new string[]
                {
                        "skybox/"
                };

                return shader.Contains( excludedShaders[0] );
            }

            string chiselPath = "packages/com.chisel.components/package resources/";

            string[] ignoredEntries = new string[]
            {
                    "packages/com.unity.searcher/",    // 0, we ignore this to get rid of the built-in font materials
                    "packages/com.unity.entities/",    // 1, we ignore this to get rid of the entities materials
                    $"{chiselPath}preview materials/", // 2, these are tool textures, so we are ignoring them
            };

            // if the path contains any of the ignored paths, then this will return false
            bool valid = !PathContains( ignoredEntries[0] )
                         && !PathContains( ignoredEntries[1] )
                         && !PathContains( ignoredEntries[2] )
                         && !HasInvalidShader(); // also check the shader

            return valid;
        }

        // step val by powers of two
        public static int GetPow2( int val )
        {
            val--;
            val |= val >> 1;
            val |= val >> 2;
            val |= val >> 4;
            val |= val >> 8;
            val |= val >> 16;
            val++;

            return val;
        }

        private static SurfaceDatabase CreateSurfaceDatabase()
        {
            SurfaceDatabase asset = ScriptableObject.CreateInstance<SurfaceDatabase>();

            AssetDatabase.CreateAsset(asset, "Assets/Surface Database.asset");
            AssetDatabase.SaveAssets();

            return asset as SurfaceDatabase;
        }


        // gets all materials and the labels on them in the project, compares them against a filter,
        // and then adds them to the list of materials to be used in this window
        public static void GetMaterials( ref List<ChiselMaterialBrowserTile> materials,
                                         ref List<ChiselMaterialBrowserTile> usedMaterials,
                                         ref List<string>                    labels,
                                         ref List<CSGModel>                  models,
                                         bool                                usingLabel,
                                         string                              searchLabel = "",
                                         string                              searchText  = "" )
        {
            if( usingLabel && searchLabel == string.Empty)
            {
                Debug.LogError($"usingLabel set to true, but no search term was given. This may give undesired results.");
            }

            materials.ForEach( e =>
            {
                e.Dispose();
                e = null;
            } );

            materials.Clear();

            // exclude the label search tag if we arent searching for a specific label right now
            string search = usingLabel ? $"l:{searchLabel} {searchText}" : $"{searchText}";

            string[] guids = AssetDatabase.FindAssets( $"t:Material {search}" );

            var dbs = FindAssetsByType<SurfaceDatabase>();
            var db = dbs.Count > 0 ? dbs[0] : CreateSurfaceDatabase();

            // assemble preview tiles
            foreach( string id in guids )
            {
                var path = AssetDatabase.GUIDToAssetPath(id);
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                var browserTile = new ChiselMaterialBrowserTile(m, id); //, ref cache );
                browserTile.surfaceConfig = db.FindOrCreateSurfaceConfig(m);
                browserTile.surfaceDb = db;

                if ( labels != null )
                {
                    // add any used labels we arent currently storing
                    foreach( string label in browserTile.labels )
                    {
                        if( !labels.Contains( label ) )
                            labels.Add( label );
                    }
                }

                // check each entry against a filter to exclude certain entries
                if( IsValidEntry( browserTile ) )
                {
                    // if we have the material already, skip, else add it
                    materials.Add( browserTile );
                }
            }

            AssetPreview.SetPreviewTextureCacheSize( materials.Count + 2 );

            models = Object.FindObjectsOfType<CSGModel>().ToList();

            PopulateUsedMaterials( ref usedMaterials, ref models, searchLabel, searchText );
        }

        private static void PopulateUsedMaterials( ref List<ChiselMaterialBrowserTile> tiles, ref List<CSGModel> models, string searchLabel, string searchText )
        {
            tiles.ForEach( e =>
            {
                e.Dispose();
                e = null;
            } );

            tiles.Clear();

            foreach( CSGModel m in models )
            {
                if (!m.generatedMeshes)
                {
                    continue;
                }

                var renderMeshes = m.generatedMeshes.GetComponentsInChildren<MeshRenderer>();

                if(renderMeshes.Length == 0)
                {
                    return;
                }

                var dbs = FindAssetsByType<SurfaceDatabase>();
                var db = dbs.Count > 0 ? dbs[0] : CreateSurfaceDatabase();

                foreach ( MeshRenderer mesh in renderMeshes )
                {
                    foreach( Material mat in mesh.sharedMaterials )
                    {
                        if(mat == null
                            || (!mat.name.Contains(searchText)
                            && !MaterialContainsLabel(searchLabel, mat)))
                        {
                            continue;
                        }
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier( mat, out string guid, out long id );
                        var tile = new ChiselMaterialBrowserTile(mat, guid);
                        tile.surfaceConfig = db.FindOrCreateSurfaceConfig(mat);
                        tile.surfaceDb = db;
                        tiles.Add(tile);
                    }
                }
            }
        }

        private static bool MaterialContainsLabel( string label, Material material )
        {
            string[] labels = AssetDatabase.GetLabels( material );

            if( labels.Length > 0 )
                foreach( string l in labels )
                {
                    if( l.Contains( label ) ) return true;
                }

            return false;
        }

        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }

        public static void SelectMaterialInScene( string name )
        {
            MeshRenderer[] objects = Object.FindObjectsOfType<MeshRenderer>();

            if( objects.Length > 0 )
                foreach( MeshRenderer r in objects )
                {
                    if( r.sharedMaterials.Length > 0 )
                        foreach( Material m in r.sharedMaterials )
                        {
                            if( m != null )
                                if( m.name.Contains( name ) )
                                {
                                    EditorGUIUtility.PingObject( r.gameObject );
                                    Selection.activeObject = r.gameObject;
                                }
                        }
                }
        }

        private static MethodInfo m_GetAssetPreviewMethod;

        public static Texture2D GetAssetPreviewFromGUID( string guid )
        {
            m_GetAssetPreviewMethod ??= typeof( AssetPreview ).GetMethod(
                    "GetAssetPreviewFromGUID",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                    null,
                    new[] { typeof( string ) },
                    null
            );

            return m_GetAssetPreviewMethod.Invoke( null, new object[] { guid } ) as Texture2D;
        }
    }
}
