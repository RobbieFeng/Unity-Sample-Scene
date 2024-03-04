using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable()]
public class Result
{
    public string victim;//info for the bugged object. It is also contained in objects
    public int step;
    public int frame;
    public string desription; 
    public int pixelWidth;//screenshot width
    public int pixelHeight;//screenshot hidth
    public string tag;
}


