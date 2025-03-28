using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class ShapePoolManager : Singleton<ShapePoolManager>
{
    [SerializeField] private GameObject shapePrefab;

    private List<Shape> shapes = new List<Shape>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Shape CreateShape()
    {
        return CreateShape(Vector3.zero, Quaternion.identity);
    }

    public Shape CreateShape(Shape shape)
    {
        Shape newShape = FindFirstShape(false);

        if (newShape == null)
        {
            return InstantiateNewShape(shape);
        }

        newShape.sprite.enabled = true;
        newShape.Initialize();
        newShape.CopyShape(shape);
        newShape.inUse = true;
        return newShape;
    }

    public Shape CreateShape(Vector3 postion, Quaternion rotation)
    {
        Shape newShape = FindFirstShape(false);

        if(newShape == null)
        {
            newShape = InstantiateNewShape(postion, rotation);
            newShape.inUse=true;
            return newShape;
        }
        newShape.sprite.enabled = true;
        newShape.transform.position = postion;
        newShape.transform.rotation = rotation;
        newShape.Initialize();
        newShape.inUse = true;
        return newShape;
    }

    public void RemoveShape(Shape shape)
    {
        shape.sprite.enabled = false;
        shape.inUse = false;
        shape.OnRemoveFromPool();
    }

    public void RemoveAllShapes()
    {
        foreach(Shape shape in shapes)
        {
            RemoveShape(shape);
        }
    }

    public void EjectShapeFromPool(Shape shape)
    {
        int index = FindIndex(shape);
        shapes.RemoveAt(index);
        shape.inUse = true;
        shape.gameObject.layer = CameraManager.Instance.currentStateLayer;
    }

    private Shape FindFirstShape(bool isActive)
    {
        for(int i = 0; i< shapes.Count; i++)
        {
            if (shapes[i].inUse == isActive)
                return shapes[i];
        }
        return null;
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
        Shape newShape = newShapeGameObject.GetComponent<Shape>();
        shapes.Add(newShape);
        return newShape;
    }

    private Shape InstantiateNewShape(Shape shape)
    {
        GameObject newShapeGameObject = Instantiate(shape.gameObject);
        newShapeGameObject.gameObject.layer = CameraManager.Instance.candidateLayer;
        Shape newShape = newShapeGameObject.GetComponent<Shape>();
        shapes.Add(newShape);
        return newShape;
    }

    
}
