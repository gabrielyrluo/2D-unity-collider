using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishManager : MonoBehaviour
{
    public GameObject linePrefab;
    public FishEye fishEyeScript; 

    private List<LineRenderer> lines = new List<LineRenderer>();
    private List<Vector3> previousPositions = new List<Vector3>();
    private List<Vector3> initialEyeDistances = new List<Vector3>();

    private Transform fishBody;

    private void Start()
    {
        fishBody = transform.GetChild(0); 

        // Initialize previous positions and initial distances from the eye
        foreach (Transform child in fishBody)
        {
            previousPositions.Add(child.position);
            initialEyeDistances.Add(child.position - fishEyeScript.transform.position);
        }

        for (int i = 0; i < fishBody.childCount - 1; i++)
        {
            Transform vertexA = fishBody.GetChild(i);
            Transform vertexB = fishBody.GetChild(i + 1);
            lines.Add(ConnectPointsWithLine(vertexA, vertexB));
        }

        // Special connection
        Transform sp = fishBody.GetChild(0);
        Transform ep = fishBody.GetChild(8);
        lines.Add(ConnectPointsWithLine(sp, ep));
    }

    private void Update()
    {
        // Verlet Integration
        for (int i = 0; i < fishBody.childCount; i++)
        {
            Transform vertex = fishBody.GetChild(i);
            Vector3 currentPosition = vertex.position;
            Vector3 previousPosition = previousPositions[i];
            
            Vector3 newPosition = currentPosition + (currentPosition - previousPosition);
            previousPositions[i] = currentPosition;
            vertex.position = newPosition;
        }

        SolveConstraints();
        
        // Update Line Renderers
        UpdateLines();
    }

    private void SolveConstraints()
    {
        //First solve eye constraints to make sure the fish body follows the eye
        for (int i = 0; i < fishBody.childCount; i++)
        {
            Transform vertex = fishBody.GetChild(i);
            Vector3 desiredPosition = fishEyeScript.transform.position + initialEyeDistances[i];
            Vector3 direction = (desiredPosition - vertex.position).normalized;

            float error = Vector3.Distance(vertex.position, desiredPosition);
            previousPositions[i] = direction * error;
            vertex.position += direction * error;
        }

        // Then solve terrain collisions
        for (int i = 0; i < fishBody.childCount; i++)
        {
            Transform vertex = fishBody.GetChild(i);
            HandleTerrainCollision(vertex);
        }
    }

    private void UpdateLines()
    {
        for (int i = 0; i < fishBody.childCount - 1; i++)
        {
            Transform startPoint = fishBody.GetChild(i);
            Transform endPoint = fishBody.GetChild(i + 1);
            LineRenderer lr = lines[i];
            lr.SetPosition(0, startPoint.position);
            lr.SetPosition(1, endPoint.position);
        }

        // Special connection update
        Transform sp = fishBody.GetChild(0);
        Transform ep = fishBody.GetChild(8);
        LineRenderer lrr = lines[8];
        lrr.SetPosition(0, sp.position);
        lrr.SetPosition(1, ep.position);
    }

    private LineRenderer ConnectPointsWithLine(Transform start, Transform end)
    {
        GameObject newLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        newLine.transform.SetParent(this.transform, true);
        LineRenderer lr = newLine.GetComponent<LineRenderer>();
        lr.SetPosition(0, start.position);
        lr.SetPosition(1, end.position);
        return lr;
    }

    private void HandleTerrainCollision(Transform vertex)
    {
        GameObject[] terrains = GameObject.FindGameObjectsWithTag("Terrain");

        foreach (GameObject terrain in terrains)
        {
            LineRenderer lineRenderer = terrain.GetComponent<LineRenderer>();
            Vector2 A = lineRenderer.GetPosition(0);
            Vector2 B = lineRenderer.GetPosition(1);
            Vector2 P = vertex.position;

            if (iscoliide(A,B,P))
            {
                vertex.position = getcollidepoint(A,B,P);
            }
        }
    }

    private bool iscoliide(Vector2 A, Vector2 B, Vector2 P){
        float m = (B.y - A.y) / (B.x - A.x); // Slope of the line
        if(B.y - A.y == 0){
            m = 0f;
        }
        float c = A.y - m * A.x;             // y-intercept
        
        float yOnLine = m * P.x + c;     // y-coordinate on the line for the given x-coordinate of the point

        return P.y <= yOnLine && A.x < P.x && P.x < B.x;
    }

    private Vector2 getcollidepoint(Vector2 A, Vector2 B, Vector2 P){
        float m = (B.y - A.y) / (B.x - A.x); 
        float c = A.y - m * A.x;            
        
        float yOnLine = m * P.x + c;    

        return new Vector2(P.x,yOnLine);
    }
}
