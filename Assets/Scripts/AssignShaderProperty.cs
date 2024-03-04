
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RecordData;

using System.IO;
using static UnityEditor.PlayerSettings;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor;
using UnityEngine.TerrainTools;

public class AssignShaderProperty : MonoBehaviour
{
    public GameObject GameArea;
    public Terrain terrain;
    private float x_min;
    private float x_max;
    private float y_min;
    private float y_max;
    private float z_min;
    private float z_max;
    private Dictionary<Material, string> previousShader;
    private Texture2D terrainTextureOld;
    private float metallicOld;
    void Start()
    {
        x_max = GameArea.transform.position.x+ GameArea.transform.localScale.x/2;
        x_min = GameArea.transform.position.x- GameArea.transform.localScale.x/2;
        y_max = GameArea.transform.position.y+ GameArea.transform.localScale.y/2;
        y_min = GameArea.transform.position.y- GameArea.transform.localScale.y/2;
        z_max = GameArea.transform.position.z+ GameArea.transform.localScale.z/2;
        z_min = GameArea.transform.position.z- GameArea.transform.localScale.z/2;
        RecordSceneShader();

    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space)) 
        {
            ChangeShaders();
        } else if (Input.GetKey(KeyCode.C))
        {
            ScreenCapture.CaptureScreenshot("F:\\Documents\\Unity\\Viking\\out\\text.png");
        }
    }
    //Apply segmentation shader to all objects
    public void ChangeShaders()
    {
       
        var gos = FindObjectsOfType<MeshRenderer>();
        foreach (MeshRenderer mr in gos)
        {
            foreach (var mat in mr.materials)
            {
                mat.shader = Shader.Find("Custom/SegmentationHDRP");
            }
        }
        AssignAll();
        change_terrain(terrain);
    }
    //Assign color to segmentation shader
    void AssignAll()
    {
        var mrs = FindObjectsOfType<MeshRenderer>();
        foreach (var mr in mrs)
        {
            if (!boolHelper(mr.gameObject))
            {
                
                continue;
            }
            var mats = mr.materials;
            foreach (var mat in mats)
            {
                if (mat.shader == Shader.Find("Custom/SegmentationHDRP"))
                {
                    change_shader(mat, mr.gameObject);
                }
                
            }
            //Also assign same color to its children 
            var mrs2 = mr.GetComponentsInChildren<MeshRenderer>();
            foreach(var mr2 in mrs2)
            {
                var mats2 = mr2.materials;
                foreach (var mat2 in mats2)
                {
                    if (mat2.shader == Shader.Find("Custom/SegmentationHDRP"))
                    {
                        change_shader(mat2, mr.gameObject);
                    }
                }
            }

        }
    }

    void change_shader(Material mat, GameObject go)
    {
        mat.SetColor("_Color", Calculate_Color(go));
    }

    public Color Calculate_Color(GameObject go)
    {
        float xNorm = (go.transform.position.x - x_min) / (x_max - x_min);
        float yNorm = (go.transform.position.y - y_min) / (y_max - y_min);
        float zNorm = (go.transform.position.z - z_min) / (z_max - z_min);
        int sum = 0;
        foreach (char c in go.name)
        {
            sum += c;
        }
        float r = (xNorm * sum) % 1;
        float g = (yNorm * sum) % 1;
        float b = (zNorm * sum) % 1;
        return new Color(r, g, b, 1);
    }
    
    public Color32 Calculate_Color_255(GameObject go)
    {
        Color color = Calculate_Color(go);
        Color32 color32 = Calculate_Color(go);
        //Color32 color32 = new Color32((byte)(color.r*255), (byte)(color.g*255), (byte)(color.b*255), 255);
        return color32;
    }
    void RecordSceneShader()
    {
        previousShader = new Dictionary<Material, string>();
        var gos = FindObjectsOfType<MeshRenderer>();
        foreach (MeshRenderer mr in gos)
        {
            foreach (var mat in mr.materials)
            {
                if(!previousShader.ContainsKey(mat))
                {
                    previousShader.Add(mat, mat.shader.name);
                }
            }
        }
    }

    public string RestoreShaders()
    {
        foreach (var mat in previousShader.Keys)
        {
            mat.shader = Shader.Find(previousShader[mat]);
        }
        GameObject.Find("ControlBlock").GetComponent<Volume>().enabled = false;
        GetComponent<RecordData>().Cam.clearFlags = CameraClearFlags.Skybox;
        restore_terrain(terrain);
        return "";
    }

    private void change_terrain(Terrain terrain)
    {
        terrainTextureOld = terrain.terrainData.terrainLayers[0].diffuseTexture;
        metallicOld = terrain.terrainData.terrainLayers[0].metallic;
        terrain.terrainData.terrainLayers[0].diffuseTexture = Resources.Load<Texture2D>("Black");
        terrain.terrainData.terrainLayers[0].metallic = 1;


    }

    private void restore_terrain(Terrain terrain)
    {
        if (terrainTextureOld != null)
        {
            terrain.terrainData.terrainLayers[0].diffuseTexture = terrainTextureOld;
            terrain.terrainData.terrainLayers[0].metallic = metallicOld;
        }
    }

    private void OnDestroy()
    {
        RestoreShaders();
    }
}
