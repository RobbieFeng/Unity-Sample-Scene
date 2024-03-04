using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;


public class TextureLost : BugCreator
{
    private List<Tuple<Material, string, Texture2D>> textureOld = new List<Tuple<Material, string, Texture2D>>();
    public bool randomTexture = true;
    
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
        string[] texturePropertyNames1 = targetMaterial.GetTexturePropertyNames();
        string[] texturePropertyNames = {"_MainTex", "BaseMap"};
        string[] properties = { "unity_LightmapsInd", "unity_Lightmaps", "unity_ShadowMasks" };

        // Iterate through each texture property
        foreach (string propertyName in texturePropertyNames1)
        {
            if (properties.Contains(propertyName))
            {
                continue;
            }
            
            Texture2D currentTexture = (Texture2D)targetMaterial.GetTexture(propertyName);
            String path = AssetDatabase.GetAssetPath(currentTexture);
            string[] files; string file;
            if (randomTexture)
            {
                files = Directory.GetFiles(Application.dataPath + "/Resources/PlaceHolder", "*.png"); 
                file = files[UnityEngine.Random.Range(0, files.Length)];
            } else
            {
                files = Directory.GetFiles(Application.dataPath + "/Resources/1PlaceHolder", "*.png"); ;
                file = files[0];
            }
            
            file = "PlaceHolder/" + Path.GetFileName(file);
            file = file.Remove(file.Length - 4);
            Texture2D tex = Resources.Load<Texture2D>(file);

            Tuple<Material, string, Texture2D> record = new Tuple<Material, string, Texture2D>(targetMaterial, propertyName, currentTexture);
            try {
                targetMaterial.SetTexture(propertyName, tex);
                textureOld.Add(record);
            } catch (Exception e)
            {
                
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
        return "TextureLost";
    }

    protected override void initialize()
    {
        Bug_Desription = "TextureLost";
        return;
    }

    public override Result get_discription(Result result, GameObject go)
    {
        result.desription = "The texture on " + go.name + " is lost, causing wrong patterns on this object";
        return result;
    }
}
