using UnityEngine;
using System.Collections.Generic;

public class DrawPolygon : MonoBehaviour
{
  public int numVertices = 0;
  public float radius = 5f;
  public float width = 1f;


  private LineRenderer lineRenderer;
  private PolygonCollider2D polygonCollider;

  void Start()
  {
    lineRenderer = GetComponent<LineRenderer>();
    polygonCollider = GetComponent<PolygonCollider2D>();
  }

  void FixedUpdate()
  {
    DrawPolygonLine(numVertices, radius, transform.position, width, width);

  }

  void DrawPolygonLine(int vertexNumber, float radius, Vector3 centerPos, float startWidth, float endWidth)
  {
    lineRenderer.startWidth = startWidth;
    lineRenderer.endWidth = endWidth;
    lineRenderer.loop = true;
    float angle = 2 * Mathf.PI / vertexNumber;
    lineRenderer.positionCount = vertexNumber;
    List<Vector2> positions = new List<Vector2>();

    for (int i = 0; i < vertexNumber; i++)
    {
      Matrix4x4 rotationMatrix = new Matrix4x4(new Vector4(Mathf.Cos(angle * i), Mathf.Sin(angle * i), 0, 0),
                                               new Vector4(-1 * Mathf.Sin(angle * i), Mathf.Cos(angle * i), 0, 0),
                                 new Vector4(0, 0, 1, 0),
                                 new Vector4(0, 0, 0, 1));
      Vector3 initialRelativePosition = new Vector3(0, radius, 0);
      Vector3 rotatedRelativePosition = rotationMatrix.MultiplyPoint(initialRelativePosition);
      Vector3 rotatedPosition = centerPos + rotatedRelativePosition;

      lineRenderer.SetPosition(i, rotatedPosition);
      positions.Add(new Vector2(rotatedRelativePosition.x, rotatedRelativePosition.y));
    }

    if (polygonCollider != null)
    {
      polygonCollider.enabled = false;
      polygonCollider.pathCount = 1;
      polygonCollider.SetPath(0, positions.ToArray());
      polygonCollider.enabled = true;
    }
  }
}