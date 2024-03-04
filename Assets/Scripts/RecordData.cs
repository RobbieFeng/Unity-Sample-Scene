using System.IO;
using UnityEngine;
using System;
using static Result;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine.Rendering.HighDefinition;

public class RecordData : MonoBehaviour
{
    public GameObject Area;
    public Camera Cam;
    [HideInInspector] public GameObject Lookat; //Assign randomely by other script
    public Terrain terrain;
    public float playerHight = 1f;
    public float Camera_distance_max = 10f; //Max distance from camera to target object
    public float Camera_distance_min = 10f;
    public bool automation = false; //If true, will automatically generate data
    public int dataPerTarget = 10;
    public int targetCount = 10;
    [Range(1,10)]public int maxTargetRepetition = 1;
    public bool checkBound = true;
    [HideInInspector] private int dataPerTargetMax;

    Result result = new Result();
    string now,json,filename = "init";
    private const float minArea = 0.006724f + 0.004f;
    private int abort_count = 0;
    private int SkipFrameOnStart = 10;
    private bool successSample = false;
    private Stack<FiringDelegate> queue = new Stack<FiringDelegate>();//A queue of functions to take multiple screenshots
    private delegate void FiringDelegate();
    private bool skip;
    private int step = 0;
    void Start()
    {
        Initialize();
        if (Area == null)
        {
            Area = GameObject.FindGameObjectWithTag("GameArea");
        }
        if (Cam == null)
        {
            Cam = GameObject.Find("My Camera").GetComponent<Camera>();
        }
        dataPerTargetMax = dataPerTarget;
        //this.gameObject.GetComponent<AssignVariables>().Find_Victom_and_Set();
        this.gameObject.GetComponent<AssignVariables>().Select_Victim(); ;
    }

    // Update is called once per frame
    void Update()
    {
        if (SkipFrameOnStart > 0) 
        {
            SkipFrameOnStart += -1;
            return;
        }
        if (skip) //This will give enough time for every task
        {
            skip = false;
            return;
        } else
        {
            skip = true;
        }

        if (queue.Count > 0)
        {
            queue.Pop()();
            return;
        }
        if (automation && targetCount > 0)
        {
            if (dataPerTarget > 0)
            {
                dataPerTarget += -1;
            }
            else
            {
                dataPerTarget = dataPerTargetMax;
                targetCount += -1;
                abort_count = 0;
                //this.gameObject.GetComponent<AssignVariables>().Find_Victom_and_Set();
                this.gameObject.GetComponent<AssignVariables>().Select_Victim(successSample);
                //this.gameObject.GetComponent<AssignVariables>().bugController.GetComponent<BugCreator>().Create_bug();
                return;
            }
            placeBlock();
            successSample = check_frustum();
            return;
        }


    }


    //Freeze time and Remove static

    private void Initialize()
    {
        Time.timeScale = 0f;
    }

    //PLace camera in a appropriate position
    void placeBlock()
    {
        int count = 0;
        while (true)
        {
            if (count++ > 1000)
            {
                Debug.Log("Cannot find a place to put the block. Skip: " + Lookat.name);
                //this.gameObject.GetComponent<AssignVariables>().Find_Victom_and_Set();
                this.gameObject.GetComponent<AssignVariables>().Select_Victim(false);
                count = 0;
            }
            Lookat = GameObject.Find("ControlBlock").GetComponent<AssignVariables>().victim;
            Vector3 position = Lookat.transform.position;
            position.x = UnityEngine.Random.Range(position.x - Camera_distance_max, position.x + Camera_distance_max);
            position.z = UnityEngine.Random.Range(position.z - Camera_distance_max, position.z + Camera_distance_max);
            RaycastHit hit2;
            if (Physics.Raycast(position, Vector3.down, out hit2, 100f))
            {
                RaycastHit hit3;
                if (Physics.Raycast(position, Vector3.up, out hit3, 100f)) {
                    if (hit3.point.y - hit2.point.y < (1.2 * playerHight))
                    {
                        continue;
                    }
                }
                
                position.y = hit2.point.y + playerHight;
                //Debug.Log("Find ground");
            } else
            {
                //Debug.Log("Find no ground");
            }

            transform.position = position;
            Cam.transform.position = transform.position;
            Cam.transform.LookAt(Lookat.transform);
            
            if (!Area.GetComponent<Renderer>().bounds.Contains(Cam.transform.position)) {
                //Debug.Log("GameArea Fail");
                continue;
            }
            if (!terrain_check())
            {
                //Debug.Log("Terrain Fail");
                continue;
            }
            if (check_visual_with_corners(Cam.gameObject, Lookat, out RaycastHit hit, out int corners, out bool see_center) && see_center && corners > 5)
            {
                
                if (Vector3.Distance(Cam.transform.position, hit.point) > Camera_distance_max || Vector3.Distance(Cam.transform.position, hit.point) < Camera_distance_min)
                {
                    //Debug.Log("Distance Fail");
                    continue;
                } 
                
            } else 
            {
                //Debug.Log("Visual Fail");
                continue;
            }
            if (!check_bound(Cam.gameObject))
            {
                Debug.Log("Collision Fail");
                continue;
            }
            turn_camera();
            return;
        }
        

    }
    private bool check_bound(GameObject go)
    {
        //Check if the object is in the bound of any other object
        Collider[] hitColliders = Physics.OverlapSphere(go.transform.position, 0.5f);
        return hitColliders.Length == 0 || !checkBound;
    }
    private void turn_camera()
    {
        Quaternion q = Cam.transform.rotation;
        int count = 0;
        while (count++ < 100)
        {
            //Randomly turn camera by an angle
            float xangle = UnityEngine.Random.Range(-30f, 30f);
            float yangle = UnityEngine.Random.Range(-30f, 30f);
            Cam.transform.eulerAngles = Cam.transform.eulerAngles + new Vector3(xangle, yangle, 0);


            check_visual_with_corners(Cam.gameObject, Lookat,out RaycastHit _ray,out int num, out bool see_center);
            bool rs = this.gameObject.GetComponent<AssignVariables>().bugController.GetComponent<BugCreator>().camera_placement_requiement(Cam, Lookat);

            if (turn_helper(Cam, Lookat) && (num > 3 && see_center))
            {
                return;
            }
            Cam.transform.rotation = q;
        }
        return;

    }
    //Check if corners of the object is within the camera view
    public static bool turn_helper(Camera camera, GameObject target)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        Bounds bound;
        int cornerCount = 0;
        if (target.TryGetComponent<LODGroup>(out var lod))
        {
            bound = lod.GetLODs()[0].renderers[0].bounds;

        
        } else
        {
            bound = target.GetComponent<Renderer>().bounds;
        }
        //Create 8 bounds on each corner of the object
        Vector3[] corners = new Vector3[8];
        corners[0] = bound.center + new Vector3(bound.extents.x, bound.extents.y, bound.extents.z);
        corners[1] = bound.center + new Vector3(bound.extents.x, bound.extents.y, -bound.extents.z);
        corners[2] = bound.center + new Vector3(bound.extents.x, -bound.extents.y, bound.extents.z);
        corners[3] = bound.center + new Vector3(bound.extents.x, -bound.extents.y, -bound.extents.z);
        corners[4] = bound.center + new Vector3(-bound.extents.x, bound.extents.y, bound.extents.z);
        corners[5] = bound.center + new Vector3(-bound.extents.x, bound.extents.y, -bound.extents.z);
        corners[6] = bound.center + new Vector3(-bound.extents.x, -bound.extents.y, bound.extents.z);
        corners[7] = bound.center + new Vector3(-bound.extents.x, -bound.extents.y, -bound.extents.z);
        Vector3 center = bound.center;
        //create a block on each corner and check if it is in the frustum
        foreach (Vector3 corner in corners)
        {
            //Instantiate(block, corner, Quaternion.identity);
            if (GeometryUtility.TestPlanesAABB(planes, new Bounds(corner, new Vector3(0.1f, 0.1f, 0.1f))))
            {
                cornerCount++;
            }
        }
        if (GeometryUtility.TestPlanesAABB(planes, new Bounds(center, new Vector3(0.1f, 0.1f, 0.1f))))
        {
            return true;
        }

        if (cornerCount >= 6)
        {
            return true;
        } else
        {
            return false;
        }
    }

    private bool terrain_check()
    {
        if (terrain == null)
        {
            return true;
        }
        RaycastHit[] hits = Physics.RaycastAll(Cam.transform.position, Vector3.down, 99999, 1);
        foreach  (RaycastHit hit in hits)
        {
            
            if (hit.collider.gameObject.TryGetComponent<Terrain>(out var _))
            {
                return true;
            }
        }
        return false;
    }   
    //Check if no other objects are blocking the view between two objects
    public static bool check_visual(GameObject o1, GameObject o2, out RaycastHit hit)
    {
        return check_visual_by_coord(o1.transform.position, o2.transform.position, o1, o2, out hit);
    }
    //This function also check if o1 can see corners of o2
    public static bool check_visual_with_corners(GameObject o1, GameObject o2,  out RaycastHit hit, out int conerCount, out bool see_center)
    {
        see_center = false;
        conerCount = 0;
        if (check_visual(o1, o2, out hit))
        {
            conerCount++;
            see_center = true;
        }
        Collider[] colliders = o2.GetComponentsInChildren<Collider>();

        Bounds[] boundss;
        if (colliders.Length > 0)
        {
            boundss = new Bounds[colliders.Length];
            foreach (Collider collider1 in colliders)
            {
                Bounds bounds = collider1.bounds;
                boundss[Array.IndexOf(colliders, collider1)] = bounds;
            }
        } else
        {
            Renderer[] renderer = o2.GetComponentsInChildren<Renderer>();
            boundss = new Bounds[renderer.Length];
            foreach (Renderer renderer1 in renderer)
            {
                Bounds bounds = renderer1.bounds;
                boundss[Array.IndexOf(renderer, renderer1)] = renderer1.bounds;
            }
        }
        
        foreach (Bounds bounds in boundss)
        {
            Vector3[] corners = new Vector3[9];
            corners[0] = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z);
            corners[1] = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z);
            corners[2] = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z);
            corners[3] = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z);
            corners[4] = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z);
            corners[5] = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z);
            corners[6] = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z);
            corners[7] = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z);
            corners[8] = bounds.center;
            foreach (Vector3 corner in corners)
            {
                if (check_visual_by_coord(o1.transform.position, corner, o1, o2, out var hit2))
                {
                    hit = hit2;
                    conerCount++;
                }
            }
        }
        
        return (conerCount > 2) || see_center;
    }
    //Check if no other objects are blocking the view between two coordinates, and also check if the hit is within frustum
    public static bool check_visual_by_coord(Vector3 v1, Vector3 v2, GameObject o1, GameObject o2, out RaycastHit hit)
    {
        if (Physics.Raycast(v1, (v2 - v1), out hit))
        {
            if (hit.transform.gameObject == o2 && hit_validate(hit, o1.GetComponent<Camera>()))
            {
                return true;
            }
            else if (hit.transform.parent != null)
            {
                if (hit.transform.parent.gameObject == o2 && hit_validate(hit, o1.GetComponent<Camera>()))
                {
                    return true;
                }
            }
        }
        return false;
    }
    //A point is actually visible only if it is within the frustum and not blocked by other objects. This function checks if a hit is in the frustum
    public static bool hit_validate(RaycastHit hit, Camera cam)
    {
        var vec3 = cam.WorldToViewportPoint(hit.point);
        if (vec3.x > 0 && vec3.x < 1 && vec3.y > 0 && vec3.y < 1 && vec3.z > 0)
        {
            return true;
        }
        return false;
    }

    //Check what objects are visible from the camera, and write info to file
    //return true if the file is written, false if this result is aborted
    public bool check_frustum()
    {
        

        this.gameObject.GetComponent<AssignVariables>().bugController.GetComponent<BugCreator>().fix_bug();
        result = new Result();
        /*
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Cam);
        GameObject[] objs2 = GameObject.FindObjectsOfType<GameObject>();
        GameObject tempObject = this.gameObject.GetComponent<AssignVariables>().bugController.GetComponent<BugCreator>().predict_create(Lookat, out bool RequireDestroy);
        //Append the predicted object to the list of objects
        GameObject[] objs = new GameObject[objs2.Length + 1];
        for (int i = 0; i < objs2.Length; i++)
        {
            objs[i] = objs2[i];
        }
        objs[objs.Length - 1] = tempObject;
        
        

        for (int i = 0; i < objs.Length; i++)
        {
            if (((!GameObject.Find("ControlBlock").GetComponent<AssignVariables>().ExcludedTags.Contains(objs[i].tag)) 
                & boolHelper(objs[i]))|| objs[i] == Lookat || i == objs.Length - 1)
            {
                if ((check_visual_with_corners(Cam.gameObject, objs[i], out RaycastHit hit, out int _int, out bool see_center) && can_see(planes, objs[i]))|| objs[i] == Lookat || i == objs.Length - 1)
                {
                   
                    Result_Object obj = new Result_Object();
                    obj.name = objs[i].name;
                    obj.position = objs[i].transform.position;
                    obj.screenPosition = get_screen_position(objs[i]);
                    obj.color = GetComponent<AssignShaderProperty>().Calculate_Color_255(objs[i]);
                    if (i < objs.Length - 1)
                    {
                        result.objects.Add(obj);
                    } else
                    {
                        obj.name = GetComponent<AssignVariables>().victim.name;
                        result.victim = obj;
                    }
                    if (objs[i] == GetComponent<AssignVariables>().victim)
                    {
                        result.victimOrigin = obj;
                    
                                   
                        //Check target size on screen
                        if (obj.screenPosition.size.x * obj.screenPosition.size.y / (Cam.pixelWidth*Cam.pixelHeight) < minArea)
                        {
                            this.gameObject.GetComponent<AssignVariables>().bugController.GetComponent<BugCreator>().fix_bug();
                            abort_count++;
                            //Set a limit so program do not stuck on a tiny object
                            if (abort_count < 20)
                            {
                                dataPerTarget++;
                            }
                            if (RequireDestroy)
                            {
                                Destroy(tempObject);
                            }
                            Debug.Log("Target too small");
                            return false;
                        }
                    }
                }
            }
        }
        if (result.victimOrigin == null)
        {
            throw new Exception("Cannot see victim");
        }
        if (result.victim == null)
        {
            abort_count++;
            if (abort_count < 20)
            {
                dataPerTarget++;
            }
            if (RequireDestroy)
            {
                Destroy(tempObject);//Only destroy if it is instantiated. otherwise it is the victim itself
            }

            Debug.Log("Cannot see second");
            return false;
        }
        if (RequireDestroy)
        {
            Destroy(tempObject);//Only destroy if it is instantiated. otherwise it is the victim itself
        }*/

        queue.Push(screenshot_nobug);//A screenshot without bug
        queue.Push(screenshot_bug);//A screenshot with bug

        filename =  "raw-" + DateTime.Now.ToString("MMdd-HH.mm.ss.ff");
        result.victim = Lookat.name;
        result.step = step;
        step+=2;
        result.frame = Time.frameCount + 1;
        result.pixelHeight = Cam.pixelHeight;
        result.pixelWidth = Cam.pixelWidth;
        result.tag = this.gameObject.GetComponent<AssignVariables>().bugController.GetComponent<BugCreator>().get_tag();
        result = this.gameObject.GetComponent<AssignVariables>().bugController.GetComponent<BugCreator>().get_discription(result, GetComponent<AssignVariables>().victim);
        save_json();
        return true;
    }
    //Some prefabs contain multiple items. We don not want the result to be that the cameara can see whole "level", "Rocks" etc.
    //Since a general parent object do not have renderer, this logic can find the specific object, like a single rock.
    public static bool boolHelper(GameObject go)
    {
        
        if (go.transform.parent == null)
        {
            if (go.TryGetComponent<Renderer>(out Renderer _rend))
            {
                return true;
            }
            if (go.TryGetComponent<LODGroup>(out var _c)) //Deal with LODGroup
            {
                return true;
            }
        }else if (!go.transform.parent.TryGetComponent<Renderer>(out Renderer _) && !go.transform.parent.TryGetComponent<LODGroup>(out var _) && go.TryGetComponent<Renderer>(out Renderer _))
        {
            return true;
        }
        return false;
    }
    //Check if an object is in the frustum
    private bool can_see(Plane[] planes, GameObject go)
    {
        //Both mesh and collider bounds are used, beacuse some objects do not have colliders.
        //And the bounds of the mesh is way bigger than the bounds of the collider :(.
        //So, Only if the object has no colliders, the bounds of the mesh will be used.
        //Collider[] colliders = go.GetComponentsInChildren<Collider>();
        Collider[] colliders = new Collider[0];//use only renderer
        Renderer[] meshFilters = go.GetComponentsInChildren<Renderer>();
        if (colliders.Length > 0)
        {
            foreach (Collider collider in colliders)
            {
                Bounds bound = collider.bounds;
                if (GeometryUtility.TestPlanesAABB(planes, bound))
                {
                    return true;
                }
            }
        }
        else
        {
            foreach (Renderer meshFilter in meshFilters)
            {
                Bounds bound = meshFilter.bounds;
                //bound.center = meshFilter.transform.TransformPoint(bound.center);
                if (GeometryUtility.TestPlanesAABB(planes, bound))
                {
                    return true;
                }
            }

        }
        return false;
    }




    //Get screen postion of bounding box
    private Rect get_screen_position(GameObject go)
    {
        return GUIRectWithObject(go);
    }
    public Rect GUIRectWithObject(GameObject go)
    {
        //Deal with LODGroup, since it has no renderer, but its children do
        if (go.TryGetComponent<LODGroup>(out var _c))
        {
            go = go.transform.GetChild(0).gameObject;
        }

        Vector3 cen = go.GetComponent<Renderer>().bounds.center;
        Vector3 ext = go.GetComponent<Renderer>().bounds.extents;
        Vector2[] extentPoints = new Vector2[8]
         {
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
         };
        Vector2 min = extentPoints[0];
        Vector2 max = extentPoints[0];
        foreach (Vector2 v in extentPoints)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    public Vector2 WorldToGUIPoint(Vector3 world)
    {
        Vector2 screenPoint = Cam.WorldToScreenPoint(world);
        screenPoint.y = (float)Screen.height - screenPoint.y;
        return screenPoint;
    }
    //Take a screenshot, and before that, add some post-processing effects
    //pathScreenshot has to be set before calling this function
    //tail: addtion to the filename
    private string screenshot(string tail = "")
    {
        string path = filename.Remove(filename.Length - 4) + tail + ".png";
        ScreenCapture.CaptureScreenshot(path);
        //CaptureScreenshotFromCamera(Cam, path);
        return path;
    }



    void screenshot_bug()
    {
        this.gameObject.GetComponent<AssignVariables>().bugController.GetComponent<BugCreator>().Create_bug();
        queue.Push(this.gameObject.GetComponent<AssignVariables>().bugController.GetComponent<BugCreator>().fix_bug);
        //return screenshot("(bug)");

        GetComponent<Capture>().capture(filename);
        return;
    }
    //Take a screenshot with bug
    void screenshot_nobug()
    {
        GetComponent<Capture>().capture(filename);
        return;
    }   

    
    /*
    //Capture a screenshot from a camera
    public void CaptureScreenshotFromCamera(Camera targetCamera, string screenshotFilename)
    {
        // Create a RenderTexture with the same dimensions as the camera's viewport
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);

        // Set the camera's target texture to the created RenderTexture
        targetCamera.targetTexture = renderTexture;

        // Force the camera to render
        targetCamera.Render();

        // Create a Texture2D and read the pixels from the RenderTexture
        Texture2D screenshotTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        screenshotTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshotTexture.Apply();

        // Reset the camera's target texture
        targetCamera.targetTexture = null;
        RenderTexture.active = null;

        // Convert the Texture2D to a PNG byte array
        byte[] screenshotBytes = screenshotTexture.EncodeToPNG();

        // Save the PNG byte array as a file
        System.IO.File.WriteAllBytes(screenshotFilename, screenshotBytes);

        Debug.Log("Camera screenshot captured: " + screenshotFilename);
    }
    */
    //Check if the screenshot is captured
    /*
    private bool check_capture(string path)
    {
        return File.Exists(path) || path == "init";
    }*/
    //Json file is named 
    private void save_json()
    {
        json = JsonUtility.ToJson(result);
        //string now = DateTime.Now.ToString("MMdd-HH.mm.ss.ff");
        string now = filename;
        File.WriteAllText("out/" + now + ".json", json);

        //Debug.Log(result.object_count);
        result = new Result();
    }



}
