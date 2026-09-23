using System;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    //public static void StartSplashDamageView(float splashRadius, Vector3 position)
    //{
    //    var viewPrefab = Resources.Load<SplashDamageView>("Prefabs/SplashDamageView");
    //    var viewInstance = GameObject.Instantiate(viewPrefab, position, Quaternion.identity);
    //    viewInstance.StartView(splashRadius);
    //}

    public static string ConvertSecondsToTimerFormat(float secondsElapsed)
    {
        int seconds = Mathf.FloorToInt(secondsElapsed % 60);
        int minutes = Mathf.FloorToInt(seconds / 60);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public static string GetLayerNamesFromMask(LayerMask mask)
    {
        string str = "";

        for (int i = 0; i < 32; i++)
            if ((mask.value & (1 << i)) != 0)
                str += $"{LayerMask.LayerToName(i)} | ";

        return str;
    }

    public static T GetInstancedCopyOf<T>(T target) where T : class
    {
        if (target == null)
            return null;

        string json = JsonUtility.ToJson(target);
        T clone = JsonUtility.FromJson(json, target.GetType()) as T;

        return clone;
    }

    public static void SpawnTemporarySphere(Vector3 position, float scale, float destroyTime = 1)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        if (sphere.TryGetComponent(out Collider col))
            GameObject.Destroy(col);

        sphere.GetComponent<Renderer>().material.color = Color.red;
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * scale;

        GameObject.Destroy(sphere, destroyTime);
    }

    /// <summary>
    /// Creates a copy of the skeleton in its current pose.
    /// Returns the cloned root bone and a bone mapping.
    /// </summary>
    public static Transform CloneSkeleton(SkinnedMeshRenderer source, Transform parent, out Dictionary<Transform, Transform> boneMap)
    {
        boneMap = new Dictionary<Transform, Transform>();

        static Transform CloneRecursive(Transform src, Transform dstParent, Dictionary<Transform, Transform> bones)
        {
            var go = new GameObject(src.name);
            var dst = go.transform;

            dst.SetParent(dstParent, false);

            // Copy current pose
            dst.SetLocalPositionAndRotation(src.localPosition, src.localRotation);
            dst.localScale = src.localScale;

            bones[src] = dst;

            foreach (Transform child in src)
                CloneRecursive(child, dst, bones);

            return dst;
        }

        Transform clonedRoot = CloneRecursive(source.rootBone, parent, boneMap);

        return clonedRoot;
    }

    /// <summary>
    /// Creates a fully functional clone of a SkinnedMeshRenderer.
    /// </summary>
    public static SkinnedMeshRenderer CloneMeshRenderer(SkinnedMeshRenderer source, Transform parent)
    {
        CloneSkeleton(source, parent, out Dictionary<Transform, Transform> boneMap);

        GameObject go = new GameObject(source.name);
        go.transform.SetParent(parent, false);

        go.transform.SetLocalPositionAndRotation(source.transform.localPosition, source.transform.localRotation);
        go.transform.localScale = source.transform.localScale;

        var dst = go.AddComponent<SkinnedMeshRenderer>();

        dst.sharedMesh = source.sharedMesh;
        dst.sharedMaterials = source.sharedMaterials;

        dst.rootBone = boneMap[source.rootBone];

        Transform[] bones = new Transform[source.bones.Length];

        for (int i = 0; i < bones.Length; i++)
            bones[i] = boneMap[source.bones[i]];

        dst.bones = bones;

        dst.updateWhenOffscreen = source.updateWhenOffscreen;
        dst.shadowCastingMode = source.shadowCastingMode;
        dst.receiveShadows = source.receiveShadows;
        dst.lightProbeUsage = source.lightProbeUsage;
        dst.reflectionProbeUsage = source.reflectionProbeUsage;

        return dst;
    }
}
