using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoneCombiner
{
    private readonly Dictionary<string, Transform> _rootBoneDictionary = new Dictionary<string, Transform>();
    private readonly Transform _transform;

    public BoneCombiner(GameObject rootObj)
    {
        _transform = rootObj.transform;

        SkinnedMeshRenderer rootRenderer = rootObj.GetComponentInChildren<SkinnedMeshRenderer>();
        if (rootRenderer != null)
        {
            TraverseBones(rootRenderer.bones);
        }
        else
        {
            TraverseHierarchy(_transform);
        }
    }

    private void TraverseHierarchy(Transform root)
    {
        if (root == null) return;

        string boneKey = root.name;
        if (!_rootBoneDictionary.ContainsKey(boneKey))
        {
            _rootBoneDictionary[boneKey] = root;
        }

        foreach (Transform child in root)
        {
            TraverseHierarchy(child);
        }
    }

    private void TraverseBones(Transform[] bones)
    {
        foreach (Transform bone in bones)
        {
            if (bone != null)
            {
                string boneKey = bone.name;
                if (!_rootBoneDictionary.ContainsKey(boneKey))
                {
                    _rootBoneDictionary[boneKey] = bone;
                }
                TraverseHierarchy(bone);
            }
        }

        var sampleKeys = _rootBoneDictionary.Keys.Take(5).ToArray();
    }
}