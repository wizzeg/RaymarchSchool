using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;


public class RayMarchingShapesInjector : MonoBehaviour
{
    public Material RayMarchMaterial;
    public ComputeBuffer ShapesBuffer;
    public int ComputeBufferLength = 1;
    public float SmoothFactor = 0.1f;
    public bool updateShader = true;
    
    [SerializeField]
    public List<ShapeObject> ShapeObjects = new();
    private int oldCount;
    Vector3 oldPosition = Vector3.zero;

    private bool newShape = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (RayMarchMaterial == null)
        { 
            enabled = false;
        }
        RayMarchMaterial.SetFloat("SmoothFactor", SmoothFactor);
        if (ComputeBufferLength > 0)
        {
            ShapesBuffer = new ComputeBuffer(ComputeBufferLength, Marshal.SizeOf<Shape>());
        }
        oldCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (ShapeObjects.Count != oldCount)
        {
            Debug.Log("Count is: " + ShapeObjects.Count);
            AdjustComputeBufferLength(ShapeObjects.Count, ref ShapesBuffer, ref ComputeBufferLength, Marshal.SizeOf<Shape>());
            oldCount = ShapeObjects.Count;
        }

        if (updateShader)
        {
            List<Shape> shapes = new List<Shape>();
            foreach (var shapeObject in ShapeObjects)
            {
                shapes.Add(shapeObject.GetShape());
            }
            ShapesBuffer.SetData(shapes.ToArray());
            RayMarchMaterial.SetFloat("SmoothFactor", SmoothFactor);
            RayMarchMaterial.SetBuffer("ShapesBuffer", ShapesBuffer);
            RayMarchMaterial.SetInt("ShapesCount", shapes.Count);
            shapes.Clear();
        }
    }

    private void AdjustComputeBufferLength(int count, ref ComputeBuffer buffer, ref int length, int size )
    {
        bool changeBuffer = false;
        while (true) // not the nicest, but I don't care atm, I don't wanna deal with multiplier logic...
        {

            if (length >= 1 && count > length)
            {
                changeBuffer = true;
                length = length * 2;
                Debug.Log("Doubling buffer count");
            }
            else if (length > 1 && length / count > 2)
            {
                changeBuffer = true;
                length = length / 2;
                Debug.Log("Halving buffer count");
            }
            else { break; }
        }
        if (changeBuffer)
        {
            buffer.Release(); // This calls dispose, but there's also dispose directly, I don't know, apparently Release is suggested...
            buffer = null;
            buffer = new ComputeBuffer(length, size);
        }

    }

    public bool AddShapeObject(ShapeObject shapeObject)
    {
        bool shapeAdded = false;
        if (shapeObject != null)
        {
            ShapeObjects.Add(shapeObject);
            shapeAdded = true;
        }
        else
        {
            Debug.Log("Shape was null");
            shapeAdded = false;
        }
        return shapeAdded;
    }

    public bool RemoveShapeObject(ShapeObject shapeObject)
    {
        return ShapeObjects.Remove(shapeObject);
    }

    private void OnDisable()
    {
        if (ShapesBuffer != null)
        {
            ShapesBuffer.Release();
            ShapesBuffer = null;
        }
    }

    void OnDestroy()
    {
        foreach (var shapeObject in ShapeObjects)
        {
            shapeObject.NotifyRemoval();
        }
        if (ShapesBuffer != null)
        {
            ShapesBuffer.Release();
            ShapesBuffer = null;
        }

    }
}
