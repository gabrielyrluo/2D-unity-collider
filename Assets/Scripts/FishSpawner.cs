using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishPrefab;
    public GameObject fishPrefab2;
    public Transform leftPondSpawnPoint;
    public Transform rightPondSpawnPoint;
    private float fishLifetime = 8.0f;
    
    public float minLaunchSpeed = 3.0f;   // Define min launch speed.
    public float maxLaunchSpeed = 10.0f;  // Define max launch speed.
    public float gravity = -2.5f;         // Define gravity.

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            for (int i = 0; i < Random.Range(1, 5); i++){
                SpawnFish(leftPondSpawnPoint.position,1);
            }
            
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            for (int i = 0; i < Random.Range(1, 5); i++){
                SpawnFish(rightPondSpawnPoint.position,2);
            }
        }
    }

    private void SpawnFish(Vector3 spawnPosition,int dir)
    {
        GameObject newFish;
        if (dir == 2)
        {
            newFish = Instantiate(fishPrefab2, spawnPosition, Quaternion.identity);
            newFish.transform.Rotate(0, 180f, 0);  // Rotate around Y-axis by 180 degrees.
        }
        else{
            newFish = Instantiate(fishPrefab, spawnPosition, Quaternion.identity);
        }
        ApplyInitialVelocity(newFish,dir);
        StartCoroutine(DestroyFishAfterTime(newFish, fishLifetime));
        
    }

    private void ApplyInitialVelocity(GameObject fish, int dir)
    {
        float launchAngle = Random.Range(30, 70); 
        float launchSpeed = Random.Range(minLaunchSpeed, maxLaunchSpeed);

        float launchVx = launchSpeed * Mathf.Cos(launchAngle * Mathf.Deg2Rad);
        float launchVy = launchSpeed * Mathf.Sin(launchAngle * Mathf.Deg2Rad);

        if (dir == 2)
        {
            launchVx = -launchVx;
        }

        Transform fishEye = fish.transform.Find("Eye"); 

        if (fishEye)
        {
            FishEye fishEyeScript = fishEye.GetComponent<FishEye>();
            
            if (fishEyeScript)
            {
                fishEyeScript.velocity = new Vector2(launchVx, launchVy);
                fishEyeScript.gravity = gravity;
            }
        }
    }

    private IEnumerator DestroyFishAfterTime(GameObject fish, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(fish);
    }
}
