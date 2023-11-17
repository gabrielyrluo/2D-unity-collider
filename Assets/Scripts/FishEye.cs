using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishEye : MonoBehaviour
{
    public Vector2 velocity;
    public float gravity;
    public float eyeRadius;

    private void Awake()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            eyeRadius = spriteRenderer.bounds.size.x * 0.5f;
        }
    }
    private void Update()
    {
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Update position
        transform.position += (Vector3)velocity * Time.deltaTime;

        HandleEyeCollisions();
        HandleTerrainCollisions();
        HandlePondCollisions();
    }

    private void HandleEyeCollisions()
    {
        FishEye[] allEyes = FindObjectsOfType<FishEye>();
        foreach (FishEye otherEye in allEyes)
        {
            if (otherEye != this && 
                otherEye.GetComponent<SpriteRenderer>().color != this.GetComponent<SpriteRenderer>().color)
            {
                Vector2 difference = (Vector2)transform.position - (Vector2)otherEye.transform.position;
                float distance = difference.magnitude;

                if (distance < 2 * eyeRadius)
                {
                    float penetration = (2 * eyeRadius) - distance;
                    Vector2 normal = difference.normalized;
                    Vector2 relativeVelocity = velocity - otherEye.velocity;
                    
                    float velAlongNormal = Vector2.Dot(relativeVelocity, normal);
                    
                    if (velAlongNormal > 0) // Check if objects are moving apart
                        continue;

                    // Calculate impulse.
                    float impulseMagnitude = -velAlongNormal * 0.5f;
                    Vector2 impulse = impulseMagnitude * normal;

                    velocity += impulse; // Applying impulse for this eye
                    otherEye.velocity -= impulse; // Applying opposite impulse for the other eye

                    // Correct positions to avoid sticking
                    Vector2 correction = (penetration / 2.0f) * normal;
                    transform.position = (Vector2)transform.position + correction;
                    otherEye.transform.position = (Vector2)otherEye.transform.position - correction;
                }
            }
        }
    }

    private void HandleTerrainCollisions()
    {
        foreach (GameObject line in GameObject.FindGameObjectsWithTag("Terrain"))
        {
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            Vector2 A = lineRenderer.GetPosition(0);
            Vector2 B = lineRenderer.GetPosition(1);
            Vector2 C = transform.position;

            Vector2 closestPoint = ClosestPointOnLineSegment(A, B, C);
            
            if (Vector2.Distance(closestPoint, C) < eyeRadius)
            {
                // Calculate the direction of the line segment
                Vector2 direction = (B-A).normalized;

                // Calculate the normal of the direction (which is perpendicular to the line segment)
                Vector2 normal = new Vector2(-direction.y, direction.x);

                // Reflect the eye's velocity based on the normal
                velocity = Vector2.Reflect(velocity, normal) * 0.5f;

                // Adjust position to avoid sticking
                float overlap = eyeRadius - Vector2.Distance(closestPoint, C);
                transform.position += (Vector3)(normal * overlap);
            }
        }
    }

    private void HandlePondCollisions()
    {
        foreach (GameObject line in GameObject.FindGameObjectsWithTag("Pond"))
        {
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            Vector2 A = lineRenderer.GetPosition(0);
            Vector2 B = lineRenderer.GetPosition(1);
            Vector2 C = transform.position;

            Vector2 closestPoint = ClosestPointOnLineSegment(A, B, C);
            
            if (Vector2.Distance(closestPoint, C) < eyeRadius)
            {
                Transform fish = this.transform.parent;
                Destroy(fish.gameObject);

            }
        }
    }

    private Vector2 ClosestPointOnLineSegment(Vector2 A, Vector2 B, Vector2 P)
    {
        Vector2 AP = P - A;
        Vector2 AB = B - A;
        float ab2 = AB.x*AB.x + AB.y*AB.y;
        float ap_ab = AP.x*AB.x + AP.y*AB.y;
        float t = Mathf.Clamp01(ap_ab / ab2);

        return A + AB * t;
    }

    public Vector3 GetEyeMovement()
    {
        return velocity * Time.deltaTime;  
    }

}
