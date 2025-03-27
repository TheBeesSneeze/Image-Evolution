using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DrawVector2
{
    public static void Point(float x, float y)
    {
        Point(new Vector3(x,y,0), Color.white);
    }
    public static void Point(float x, float y, Color color)
    {
        Point(new Vector3(x, y, 0), color);
    }
    public static void Point(float x, float y, float duration)
    {
        Point(new Vector3(x, y, 0), Color.white, duration);
    }

    public static void Point(float x, float y, Color color, float duration)
    {
        Point(new Vector3(x, y, 0), color, duration);
    }

    public static void Point(Vector3 target)
    {
        Point(target, Color.white);
    }

    public static void Point(Vector3 target, Color color)
    {
        Debug.DrawLine(target + (Vector3.down / 2), target + (Vector3.up / 2), color);
        Debug.DrawLine(target + (Vector3.left / 2), target + (Vector3.right / 2), color);
    }

    public static void Point(Vector3 target, float duration)
    {
        Point(target, Color.white, duration);
    }

    public static void Point(Vector3 target, Color color, float duration)
    {
        Debug.DrawLine(target + (Vector3.down / 2), target + (Vector3.up / 2), color, duration);
        Debug.DrawLine(target + (Vector3.left / 2), target + (Vector3.right / 2), color, duration);
    }

    public static void Velocity(Rigidbody2D rb)
    {
        Velocity(rb, Color.white);
    }
    public static void Velocity(Rigidbody2D rb, Color color)
    {
        Debug.DrawLine(rb.position, rb.position + rb.velocity, color);
    }

    #region Direction
    /// <summary>
    /// Draws a ray from point to point + direction
    /// ex: draw a rigidbody position + velocity to see where it will be in one second
    /// </summary>
    /// <param name="point"></param>
    /// <param name="direction"></param>
    public static void Direction(Vector2 point, Vector2 direction)
    {
        Direction(point, direction, Color.white, Time.deltaTime);
    }
    public static void Direction(Vector2 point, Vector2 direction, float duration)
    {
        Direction(point, direction, Color.white, duration);
    }

    public static void Direction(Vector2 point, Vector2 direction, Color color)
    {
        Direction(point, direction, color, Time.deltaTime);
    }

    public static void Direction(Vector2 point, Vector2 direction, Color color, float duration)
    {
        Point(point, color);
        Debug.DrawLine(point, point + direction, color, duration);
    }
    #endregion
}

public static class DrawVector2Gizmos
{
    public static void Point(Vector3 target)
    {
        Point(target, Color.white);
    }

    public static void Point(Vector3 target, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(target + (Vector3.down / 2), target + (Vector3.up / 2));
        Debug.DrawLine(target + (Vector3.left / 2), target + (Vector3.right / 2));
    }

    #region Direction
    /// <summary>
    /// Draws a ray from point to point + direction
    /// ex: draw a rigidbody position + velocity to see where it will be in one second
    /// </summary>
    /// <param name="point"></param>
    /// <param name="direction"></param>
    public static void Direction(Vector2 point, Vector2 direction)
    {
        Direction(point, direction, Color.white);
    }

    public static void Direction(Vector2 point, Vector2 direction, Color color)
    {
        Point(point, color);
        // Gizmos.color = color; not needed because color is set in point function
        Gizmos.DrawLine(point, point + direction);
    }
    #endregion
}
