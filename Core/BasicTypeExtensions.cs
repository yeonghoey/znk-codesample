using System.Collections.Generic;
using UnityEngine;

public static class BasicTypeExtensions
{
    public static readonly Quaternion Ccw90 = Quaternion.AngleAxis(-90f, Vector3.up);

    public static Vector3 WithY(this Vector3 v, float y)
    {
        return new Vector3(v.x, y, v.z);
    }

    public static Vector3 ToWorldDir(this Vector2 moveInput)
    {
        Vector3 right = Camera.main.transform.right;
        right.y = 0f;
        right.Normalize();
        Vector3 forward = Ccw90 * right;
        return (forward * moveInput.y + right * moveInput.x).normalized;
    }

    public static Vector3 ToWorldDir(this Vector3 v)
    {
        Vector3 x = v;
        x.y = 0f;
        x.Normalize();
        return x;
    }

    public static Quaternion WorldDirToRotation(this Vector3 worldDir, Quaternion defaultRotation)
    {
        return worldDir != Vector3.zero ?
                Quaternion.LookRotation(worldDir, Vector3.up) :
                defaultRotation;
    }

    public static bool IsClockwise(this Quaternion a, Quaternion target)
    {
        Quaternion b = Quaternion.RotateTowards(a, target, 1f);
        Vector3 va = a * Vector3.forward;
        Vector3 vb = b * Vector3.forward;
        return va.x * vb.z - va.z * vb.x < 0f;
    }

    public static bool IsSameOrientation(this Quaternion a, Quaternion b)
    {
        return Mathf.Approximately(Mathf.Abs(Quaternion.Dot(a, b)), 1.0f);
    }

    public static Color WithAlpha(this Color c, float alpha)
    {
        return new Color(c.r, c.g, c.b, alpha);
    }

    public static Color WithValue(this Color c, float value)
    {
        float h, s;
        Color.RGBToHSV(c, out h, out s, out _);
        return Color.HSVToRGB(h, s, value);
    }

    public static Color WithBrightness(this Color c, float brightness)
    {
        float currentBrightness = c.maxColorComponent;
        if (currentBrightness == 0f)
        {
            return new Color(brightness, brightness, brightness, 1f);
        }
        float f = brightness / currentBrightness;
        return new Color(f * c.r, f * c.g, f * c.b, c.a);
    }

    public static void AssertLayer(this GameObject gameObject, string expected)
    {
        if (gameObject.layer != LayerMask.NameToLayer(expected))
        {
            Debug.LogWarning($"Expected: {expected}, current: {LayerMask.LayerToName(gameObject.layer)}", gameObject);
        }
    }

    public static int AddMaterial(this Renderer renderer, Material material)
    {
        var ms = renderer.sharedMaterials;
        int n = ms.Length;
        for (int i = 0; i < n; i++)
        {
            var m = ms[i];
            if (m == material)
            {
                return i;
            }
        }
        var newMs = new Material[n + 1];
        ms.CopyTo(newMs, 0);
        newMs[n] = material;
        renderer.sharedMaterials = newMs;
        return n;
    }

    public static bool RemoveMaterial(this Renderer renderer, Material material)
    {
        var ms = renderer.sharedMaterials;
        int n = ms.Length;
        var newMs = new List<Material>(capacity: n);
        for (int i = 0; i < n; i++)
        {
            var m = ms[i];
            if (m != material)
            {
                newMs.Add(m);
            }
        }
        if (newMs.Count == n)
        {
            return false;
        }
        renderer.sharedMaterials = newMs.ToArray();
        return true;
    }

    public static bool RemoveMaterial(this Renderer renderer, int materialIndex)
    {
        var ms = renderer.sharedMaterials;
        int n = ms.Length;
        var newMs = new List<Material>(capacity: n);
        for (int i = 0; i < n; i++)
        {
            var m = ms[i];
            if (i != materialIndex)
            {
                newMs.Add(m);
            }
        }
        if (newMs.Count == n)
        {
            return false;
        }
        renderer.sharedMaterials = newMs.ToArray();
        return true;
    }

    public static bool SetMaterial(this Renderer renderer, Material material, int materialIndex)
    {
        var sharedMaterials = renderer.sharedMaterials;
        if (sharedMaterials.Length > materialIndex)
        {
            sharedMaterials[materialIndex] = material;
            renderer.sharedMaterials = sharedMaterials;
            return true;
        }
        else
        {
            return false;
        }
    }
}
