using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.GroundTruth.Consumers;
using UnityEngine.Perception.GroundTruth.DataModel;
using UnityEngine.Perception.GroundTruth.LabelManagement;

public class Capture : MonoBehaviour
{
    public Camera cam;
    private PerceptionCamera pc;
    // Start is called before the first frame update
    void Start()
    {
        cam.TryGetComponent<PerceptionCamera>(out pc);
        var watch = System.Diagnostics.Stopwatch.StartNew();
        add_labels();
        watch.Stop();
        Debug.Log("Add labels time: " + watch.ElapsedMilliseconds + " ms");

        PerceptionEndpoint perceptionEndpoint = new PerceptionEndpoint();
        String path = perceptionEndpoint.basePath;
        //write to text
        System.IO.File.WriteAllText("PathToCapturedImages.txt", path);
        System.IO.DirectoryInfo di = new DirectoryInfo(path);
        foreach (FileInfo file in di.EnumerateFiles())
        {
            file.Delete();
        }
        foreach (DirectoryInfo dir in di.EnumerateDirectories())
        {
            dir.Delete(true);
        }

    }

    private void add_labels()
    {
        List<string> labels = new List<string>();
        var all = FindObjectsOfType<GameObject>();
        foreach (GameObject go in all)
        {
            if (go.transform.parent != null && (go.transform.parent.TryGetComponent<MeshRenderer>(out _) || go.transform.parent.TryGetComponent<LODGroup>(out _))) {
                continue;
            }
            if (!go.TryGetComponent<Labeling>(out _) && (go.TryGetComponent<MeshRenderer>(out _) || go.TryGetComponent<LODGroup>(out _)))
            {
                var labeling = go.AddComponent<Labeling>();
                labeling.labels.Add(go.name);
                labels.Add(go.name);
                
            }
        }
        if (!cam.TryGetComponent<PerceptionCamera>(out pc))
        {
            pc = cam.gameObject.AddComponent<PerceptionCamera>();
        }
        pc.showVisualizations = false;
        pc.captureTriggerMode = CaptureTriggerMode.Manual;

        IdLabelConfig Config = ScriptableObject.CreateInstance<IdLabelConfig>();
        int labelCounter = 1;
        List<IdLabelEntry> labelmngs = new List<UnityEngine.Perception.GroundTruth.LabelManagement.IdLabelEntry>();
        foreach (string label in labels)
        {
            labelmngs.Add(new IdLabelEntry { id = labelCounter, label = label });
            labelCounter++;
        }
        Config.Init(labelmngs);
        pc.AddLabeler(new UnityEngine.Perception.GroundTruth.Labelers.InstanceSegmentationLabeler(Config));
    }

    public void capture(string desciption = null)
    {
        pc.RequestCapture();
    }
}

