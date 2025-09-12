using UnityEngine;
using UnityEngine.UIElements;

public enum ShapeType
{
    Sphere,
    Cube,
    Donut
}

[System.Serializable]
public struct Shape
{
    public Vector3 Position;
    public Vector3 Scale;
    public Vector4 Color;
    public ShapeType Type;
}