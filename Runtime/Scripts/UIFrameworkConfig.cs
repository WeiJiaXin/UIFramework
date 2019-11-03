using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UIFrameworkConfig",menuName = "Lowy/UIFrameworkConfig")]
[Serializable]
public class UIFrameworkConfig : ScriptableObject
{
    public LayerMask UILayer;
    public LayerMask OutUILayer;
    public LayerMask UIParticleLayer;
}
