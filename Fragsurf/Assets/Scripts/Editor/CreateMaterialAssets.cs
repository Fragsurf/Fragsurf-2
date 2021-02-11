using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreateMaterialAssets 
{

    [MenuItem("Fragsurf Dev/Make Materials")]
    public static void DoIt()
    {
        var template = AssetDatabase.LoadAssetAtPath<Material>("Assets\\Default Material Pack\\Template.mat");
        var d = "Assets\\Default Material Pack";
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var dir in Directory.GetDirectories(d))
            {
                MakeMaterials(template, dir);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    private static void MakeMaterials(Material template, string dir)
    {
        foreach(var file in Directory.GetFiles(dir))
        {
            if(Path.GetExtension(file) != ".jpg")
            {
                continue;
            }
            var fileName = Path.GetFileNameWithoutExtension(file);
            var materialPath = Path.Combine(dir, fileName + ".mat");
            var exists = File.Exists(materialPath);
            var material = exists
                ? AssetDatabase.LoadAssetAtPath<Material>(materialPath)
                : new Material(template.shader);
            material.shader = template.shader;
            material.CopyPropertiesFromMaterial(template);
            material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture>(file);
            if(!exists) AssetDatabase.CreateAsset(material, materialPath);
        }
        foreach(var dir2 in Directory.GetDirectories(dir))
        {
            MakeMaterials(template, dir2);
        }
    }

}
