using System.Collections;
using UnityEngine;

public class Fly : MonoBehaviour
{
    public float speed = 10f;                  // Speed of the fly
    public float changeDirectionInterval = 2f; // How often the fly changes direction

    public Vector3 targetPosition;

    void Start()
    {
        // Initialize with random target position 
        SetNewTargetPosition();
        StartCoroutine(ChangeDirectionRoutine());
    }

    void Update()
    {
        // Move towards target position
        MoveFly();
    }

    private void MoveFly()
    {
        // Move towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        
        // Reach the target -> pick a new random target
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            SetNewTargetPosition();
        }
    }

    private void SetNewTargetPosition()
    {
        // Randomize a new position
        targetPosition = RandomPosition(); 
    }

    public Vector3 RandomPosition()
    {
        // Randomize a new position
        // (the fly bounds are a bit smaller than scene to increase eating, heuristic choice)
        float randomX = Random.Range(-65, 20);
        float randomZ = Random.Range(-70, 70);
        return new Vector3(randomX, 15.5f, randomZ);
    }

    private IEnumerator ChangeDirectionRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(4);
            SetNewTargetPosition();
        }
    }
}
