using UnityEngine;

public class DebugDraw
{
    /// <summary>
    /// Draws a 2D line as a gizmo (scene view)
    /// </summary>
    public static void GizmoLine2D(Vector2 origin, Vector2 direction, float length, Color colour)
    {
        Gizmos.color = colour;
        Gizmos.DrawLine(origin, origin + (direction * length));
    }

    /// <summary>
    /// Draws a 2D arrow as a gizmo (scene view)
    /// </summary>
    public static void GizmosArrow2D(Vector2 origin, Vector2 direction, float length, Color colour, float headSize)
    {
        Gizmos.color = colour;

        float angle  = Mathf.Atan2(direction.y, direction.x);
        float ahead1 = angle + (Mathf.Deg2Rad * 30f);
        float ahead2 = angle - (Mathf.Deg2Rad * 30f);

        Vector2 target = origin + (direction * length);
        Vector2 arrow1 = target - new Vector2(headSize * Mathf.Cos(ahead1), headSize * Mathf.Sin(ahead1));
        Vector2 arrow2 = target - new Vector2(headSize * Mathf.Cos(ahead2), headSize * Mathf.Sin(ahead2));

        Gizmos.DrawLine(origin, target);
        Gizmos.DrawLine(target, arrow1);
        Gizmos.DrawLine(target, arrow2);
    }

    public static void GizmosCircle2D(Vector2 origin, float radius, Color colour, int segments = 16)
    {
        Gizmos.color = colour;

        // The angle of a segment in radians
        float segmentSize = (Mathf.PI * 2F) / segments;

        for (int i = 0; i < segments; ++i)
        {
            // We need to calculate the line points for this segment
            Vector2 P1 = origin + new Vector2(radius * Mathf.Cos(segmentSize * (i + 0)), radius * Mathf.Sin(segmentSize * (i + 0)));
            Vector2 P2 = origin + new Vector2(radius * Mathf.Cos(segmentSize * (i + 1)), radius * Mathf.Sin(segmentSize * (i + 1)));

            // Draw a line between the two points
            Gizmos.DrawLine(P1, P2);
        }
    }

    public static void GizmosRectangle2D(Vector2 origin, Vector2 halfSize, Color colour)
    {
        // Calculate Coords
        Vector2 TL = origin + new Vector2(-halfSize.x, -halfSize.y);
        Vector2 TR = origin + new Vector2(+halfSize.x, -halfSize.y);
        Vector2 BL = origin + new Vector2(-halfSize.x, +halfSize.y);
        Vector2 BR = origin + new Vector2(+halfSize.x, +halfSize.y);

        Gizmos.color = colour;

        Gizmos.DrawLine(TL, TR);
        Gizmos.DrawLine(TR, BR);
        Gizmos.DrawLine(BR, BL);
        Gizmos.DrawLine(BL, TL);
    }

    public static void GizmosRectangle2D(Vector3 origin, Vector2 halfSize, Color colour, Quaternion rotation)
    {
        // Calculate Coords
        Vector3 TL = origin + (rotation * new Vector2(-halfSize.x, -halfSize.y));
        Vector3 TR = origin + (rotation * new Vector2(+halfSize.x, -halfSize.y));
        Vector3 BL = origin + (rotation * new Vector2(-halfSize.x, +halfSize.y));
        Vector3 BR = origin + (rotation * new Vector2(+halfSize.x, +halfSize.y));

        Gizmos.color = colour;

        Gizmos.DrawLine(TL, TR);
        Gizmos.DrawLine(TR, BR);
        Gizmos.DrawLine(BR, BL);
        Gizmos.DrawLine(BL, TL);
    }
}
