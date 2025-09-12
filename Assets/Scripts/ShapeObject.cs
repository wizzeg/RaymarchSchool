using UnityEngine;

public class ShapeObject : MonoBehaviour
{
    [Header("Initial settings for shape")]
    [SerializeField] private bool isNew = true;
    [SerializeField] private Shape shape;
    [SerializeField] private RayMarchingShapesInjector shapeInjector;

    [Header("Movement type")]
    [Tooltip("if false, moves sideways")]
    [SerializeField] private bool moveUpDown = true;

    private void Start()
    {
        shape.Position = transform.position;
        shape.Scale = transform.localScale;
        if (shape.Type == null) shape.Type = ShapeType.Sphere;
        if (shape.Color == null) shape.Color = Color.magenta;
    }

    private void Update()
    {
        if (isNew && shapeInjector != null)
        {
            isNew = !shapeInjector.AddShapeObject(this);
        }
        transform.position = MoveShape();
        shape.Position = transform.position;
        shape.Scale = transform.localScale;
    }

    public Shape GetShape()
    {
        return shape;
    }

    public void NotifyRemoval()
    {
        isNew = true;
    }

    private void OnDestroy()
    {
        if (!isNew && shapeInjector != null)
        {
            shapeInjector.RemoveShapeObject(this);
        }
    }

    private Vector3 MoveShape()
    {
        if (moveUpDown)
        {
            return new Vector3(2.7f * Mathf.Sin(2.7183f * Time.time * 0.5f), 0.0f, 0.0f);
        }
        else
        {
            return new Vector3(0.0f, 3.5f * Mathf.Sin(3.1416f * Time.time * 0.5f), 0.0f);
        }
    }
}
