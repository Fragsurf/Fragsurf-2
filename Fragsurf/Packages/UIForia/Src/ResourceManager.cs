using System.Collections.Generic;
using TMPro;
using UIForia.Util;
using UnityEngine;
using UnityEngine.U2D;

namespace UIForia {

    public class ResourceManager {

        private struct AssetEntry<T> where T : Object {

            public T asset;
            public int id;
            public int linkedId;

        }

        // todo -- add cursors / animations / maybe style sheets
        private readonly IntMap<AssetEntry<Texture2D>> s_TextureMap;
        private readonly IntMap<AssetEntry<SpriteAtlas>> s_SpriteAtlasMap;
        private readonly Dictionary<string, FontAsset> s_FontMap;
        private readonly IntMap<AssetEntry<AudioClip>> s_AudioMap;

        public ResourceManager() {
            s_TextureMap = new IntMap<AssetEntry<Texture2D>>();
            s_SpriteAtlasMap = new IntMap<AssetEntry<SpriteAtlas>>();
            s_FontMap = new Dictionary<string, FontAsset>();
            s_AudioMap = new IntMap<AssetEntry<AudioClip>>();
        }

        public void Reset() {
            s_TextureMap.Clear();
            s_SpriteAtlasMap.Clear();
            s_FontMap.Clear();
            s_AudioMap.Clear();
        }

        public Texture2D AddTexture(string path, Texture2D texture) {
            return AddResource(path, texture, s_TextureMap);
        }

        public Texture2D AddTexture(Texture2D texture) {
            return AddResource(texture, s_TextureMap);
        }

//        public FontAsset AddFont(TMP_FontAsset font) {
//            return AddResource(font, s_FontMap);
//        }

        public FontAsset AddFont(string path, TMP_FontAsset font) {
            if (font == null || path == null) {
                return null;
            }

            FontAsset asset = new FontAsset(font);
            s_FontMap.Add(path, asset);
            return asset;
        }

        public AudioClip AddAudioClip(AudioClip clip) {
            return AddResource(clip, s_AudioMap);
        }

        public Texture2D GetTexture(string path) {
            return GetResource(path, s_TextureMap);
        }


        public FontAsset GetFont(string path, bool tryReloading = false) {
            // will be present & null if loaded but not resolved
            if (s_FontMap.TryGetValue(path, out FontAsset fontAsset)) {
                if (fontAsset != null) {
                    return fontAsset;
                }

                if (!tryReloading) {
                    return null;
                }
            }

            TMP_FontAsset tmpFontAsset = Resources.Load<TMP_FontAsset>(path);

            if (tmpFontAsset == null) {
                s_FontMap.Add(path, null);
                return null;
            }

            //if (tmpFontAsset. != TMP_FontAsset.FontAssetTypes.SDF) {
            //    throw new Exception($"UIForia currently supports only SDF Fonts. {path} is not an SDF font, please reference another");
            //}

            FontAsset retn = new FontAsset(tmpFontAsset);
            s_FontMap.Add(path, retn);

            return retn;
        }

        public AudioClip GetAudioClip(string path) {
            return GetResource(path, s_AudioMap);
        }

        private T AddResource<T>(string path, T resource, IntMap<AssetEntry<T>> map) where T : Object {
            if (resource == null || path == null) {
                return null;
            }

            int pathId = path.GetHashCode();
            int id = resource.GetHashCode();

            AssetEntry<T> pathEntry;
            AssetEntry<T> idEntry;

            if (map.TryGetValue(pathId, out pathEntry)) {
                return resource;
            }

            idEntry.id = id;
            idEntry.linkedId = pathId;
            idEntry.asset = resource;
            pathEntry.id = id;
            pathEntry.linkedId = id;
            pathEntry.asset = resource;
            map.Add(pathId, pathEntry);
            map.Add(id, idEntry);
            return resource;
        }

        private T AddResource<T>(T resource, IntMap<AssetEntry<T>> map) where T : Object {
            int id = resource.GetHashCode();
            AssetEntry<T> entry;
            if (map.TryGetValue(id, out entry)) {
                return resource;
            }

            entry.id = id;
            entry.linkedId = -1;
            entry.asset = resource;
            map.Add(id, entry);
            return resource;
        }

        private T GetResource<T>(int id, IntMap<AssetEntry<T>> map) where T : Object {
            AssetEntry<T> entry;
            map.TryGetValue(id, out entry);
            return entry.asset;
        }

        private T GetResource<T>(string path, IntMap<AssetEntry<T>> map) where T : Object {
            T resource;
            if (path == null) {
                return null;
            }

            AssetEntry<T> pathEntry;
            int pathId = path.GetHashCode();
            if (map.TryGetValue(pathId, out pathEntry)) {
                return pathEntry.asset;
            }
            else {
                // this might be null, but we want to mark the map to show that we tried to load it
                // during the lifecycle of an application we can expect Resources not to be updated
                resource = Resources.Load<T>(path);
                pathEntry.id = pathId;
                pathEntry.asset = resource;
                pathEntry.linkedId = -1;
                if (resource != null) {
                    // see if we already have it loaded by id and update linkedId accordingly
                    int resourceId = resource.GetHashCode();
                    AssetEntry<T> idEntry;
                    idEntry.id = resourceId;
                    idEntry.linkedId = pathId;
                    idEntry.asset = resource;
                    map[idEntry.id] = idEntry;

                    pathEntry.linkedId = idEntry.id;
                }

                map.Add(pathId, pathEntry);
            }

            return resource;
        }

        private void RemoveResource<T>(T resource, IntMap<AssetEntry<T>> map) where T : Object {
            if (resource == null) return;
            int id = resource.GetHashCode();
            AssetEntry<T> entry;
            if (map.TryGetValue(id, out entry)) {
                map.Remove(entry.linkedId);
                map.Remove(id);
            }
        }

        private void RemoveResource<T>(string path, IntMap<AssetEntry<T>> map) where T : Object {
            if (string.IsNullOrEmpty(path)) {
                return;
            }

            int pathId = path.GetHashCode();
            AssetEntry<T> entry;
            if (map.TryGetValue(pathId, out entry)) {
                map.Remove(entry.linkedId);
                map.Remove(pathId);
            }
        }


    }

}