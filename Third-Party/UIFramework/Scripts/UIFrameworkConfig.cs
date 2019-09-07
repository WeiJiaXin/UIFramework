using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu]
[Serializable]
public class UIFrameworkConfig : ScriptableObject
{
    public LayerMask UILayer;
    public LayerMask OutUILayer;
    public LayerMask UIParticleLayer;
}
