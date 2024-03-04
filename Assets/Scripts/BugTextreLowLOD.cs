using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.AutoLOD;

public class BugTextreLowLOD : BugCreator
{
    public int minimumSize = 50;
    private List<Tuple<Material,string,Texture2D>> textureOld = new List<Tuple<Material,string, Texture2D>>();
    /*public override bool victim_requiement(GameObject go)
    {
        string[] names = { "bridge", "house", "barn", "sauser","tower", "cube","Plane","temple" };
        int i = 0;
        foreach (string name in names)
        {
            if (go.name.ToLower().Contains(name.ToLower()))
            {
                i++;
            }
        } 
        if (i<1)
        {
            return false;
        }
        if (go.TryGetComponent<Renderer>(out var renderer))
        {
           return go.GetComponent<Renderer>().bounds.size.magnitude > minimumSize;
        } else
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            Bounds bounds = new Bounds();
            foreach (Renderer r in renderers)
            {
                bounds.Encapsulate(r.bounds);
                if (bounds.size.magnitude > minimumSize)
                {
                    return true;
                }
            }
            return false;
            return bounds.size.magnitude > minimumSize;
        }
    }*/
    public override bool victim_requiement(GameObject go)
    {
        if (go.TryGetComponent<MeshFilter>(out MeshFilter filter))
        {

            return filter.sharedMesh.GetTriangleCount() > 300;

        }
        return false;
    }
    public override void Create_bug()
    {
        fix_bug();
        victim = get_victim();
        Renderer[] renderers = victim.GetComponentsInChildren<Renderer>();
        foreach (Renderer ren in renderers)
        {
            Material[] mats = ren.materials;
            foreach (Material mat in mats)
            {
                create_bug_on_material(mat);
            }
        }


        if (textureOld.Count < 1)
        {
            throw new System.Exception("No texture changed for " + victim.name);
        }
    }
    private void create_bug_on_material(Material targetMaterial)
    {
        string[] texturePropertyNames = targetMaterial.GetTexturePropertyNames();
        string[] properties = { "_OcclusionMap", "_DetailNormalMap", "_DetailAlbedoMap", "_MetallicGlossMap" };
       
        // Iterate through each texture property
        foreach (string propertyName in texturePropertyNames)
        {
            if (properties.Contains(propertyName))
            {
                //continue;
            }
            // Get the current texture assigned to the property
            Texture2D currentTexture = (Texture2D)targetMaterial.GetTexture(propertyName);
            String path = AssetDatabase.GetAssetPath(currentTexture);
            if (path.Length > 8)
            {
                string newPath = "Texture/" + path.Remove(0, 7);
                string newPathFull = Application.dataPath+"/Resources/" + newPath;
                newPath = newPath.Replace(".png", "");
                newPath = newPath.Replace(".tif", "");
                newPath = newPath.Replace(".tiff", "");
                newPath = newPath.Replace(".TGA", "");
                Texture2D newTexture;
                if (File.Exists(newPathFull))
                {
                    newTexture = Resources.Load<Texture2D>(newPath);
                    Tuple<Material,string, Texture2D> t = new Tuple<Material,string, Texture2D>(targetMaterial,propertyName, currentTexture);
                    textureOld.Add(t);
                    targetMaterial.SetTexture(propertyName, newTexture);
                }
            }
        }
    }



    public override void fix_bug()
    {
        foreach (Tuple<Material, string, Texture2D> t in textureOld)
        {
            t.Item1.SetTexture(t.Item2, t.Item3);
        }
        textureOld.Clear();
    }

    public override string get_tag()
    {
        return "TextureLowLOD";
    }

    protected override void initialize()
    {
        Bug_Desription = "PlaceHolder";
        return;
    }

    public override Result get_discription(Result result, GameObject go)
    {
        result.desription = "The texture of " + go.name + " has a low LOD, causing the patterns on the surface to be blurred. This may happen becuase the texture file is corrupted or not correctly loaded";
        return result;
    }
}
