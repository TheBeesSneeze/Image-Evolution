using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;

public class ShapePoolManager : Singleton<ShapePoolManager>
{
    [SerializeField] private GameObject shapePrefab;

    public List<Shape> shapes = new List<Shape>();
    public List<Shape> oldShapes = new List<Shape>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private Shape GetNextShape()
    {
        return shapes.FirstOrDefault(s => s.settingsApplied == false);
    }

    public Shape CreateShape()
    {
        return CreateShape(Vector3.zero, Quaternion.identity);
    }

    public Shape CreateShape(Shape shape)
    {
        Shape newShape = GetNextShape();

        if (newShape == null)
        {
            return InstantiateNewShape(shape);
        }

        newShape.CopyShape(shape);
        newShape.spriteRenderer.enabled = false;
        return newShape;
    }

    public Shape CreateShape(Vector3 postion, Quaternion rotation)
    {
        Shape newShape = GetNextShape();

        if(newShape == null)
        {
            newShape = InstantiateNewShape(postion, rotation);
            return newShape;
        }

        newShape.Reset(ShapeManager.Instance.randomColorMode, true);
        return newShape;
    }

    public void RemoveShape(Shape shape)
    {
        
    }

    public void ResetAllShapesForNextTime()
    {
        foreach(Shape shape in shapes)
        {
            shape.Reset(0, false);
        }
    }

    public void EjectShapeGameObjectFromPool(Shape shape)
    {
        oldShapes.Add(shape);
        shapes.Remove(shape);

        shape.spriteRenderer.enabled = true;
        /*
        int index = FindIndex(shape);
        shapes.RemoveAt(index);
        shape.inUse = true;
        shape.gameObject.layer = CameraManager.Instance.currentStateLayer;
        */
    }

    // i need to do something better than this :~[
    private int FindIndex(Shape shape)
    {
        for(int i=0; i<shapes.Count; i++)
        {
            if (shapes[i]==shape)
                return i;
        }
        return -1;
    }

    private void Swap(int idx1, int idx2)
    {
        Shape temp = shapes[idx1];
        shapes[idx1] = shapes[idx2];
        shapes[idx2] = temp;
    }

    private Shape InstantiateNewShape(Vector3 postion, Quaternion rotation)
    {
        GameObject newShapeGameObject = Instantiate(shapePrefab, postion, rotation);
        newShapeGameObject.gameObject.layer = CameraManager.Instance.candidateLayer;
        Shape newShape = new Shape(newShapeGameObject.transform, ShapeManager.Instance.randomColorMode);

        shapes.Add(newShape);
        return newShape;
    }

    private Shape InstantiateNewShape(Shape shape)
    {
        GameObject newShapeGameObject = Instantiate(shape.gameObject);
        newShapeGameObject.gameObject.layer = CameraManager.Instance.candidateLayer;
        Shape newShape = new Shape(newShapeGameObject.transform, ShapeManager.Instance.randomColorMode);
        shapes.Add(newShape);
        return newShape;
    }

    
}
