using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Frog : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public HTNPlanner planner;
    public List<Vector3> path;
    public bool coolDown;
    public Manager manager; 

    // World Representation primitives
    public int fliesInBelly;
    public bool flyWithinReach;
    public int row;

    // World representation vector 
    public FrogState state; // < fliesInBelly, flyWithinReach, row > 

    private bool startCoolDown;

    // For digestion
    private float LeapHeight; 
    private float LeapDuration; // Minimum time before landing (in seconds)
    public bool isLeaping;

    // Start is called before the first frame update
    void Start()
    {
        // Initial world rep: < 0, *, 0 >
        state = new FrogState(0, flyWithinReach, 0); 

        planner = ScriptableObject.CreateInstance<HTNPlanner>();
        planner.manager = manager;
        planner.InitializeTasks(this);

        path = new List<Vector3>();
        coolDown = false;
        startCoolDown = true;

        isLeaping = false;
        LeapDuration = 0.5f;
        LeapHeight = 25f;
    }


    public void Update()
    {
        if(startCoolDown)
        {
            StartCoroutine(CoolDown());
            startCoolDown = false;
        }

        if(!isLeaping)
        {
            // Check for flies
            HTNSimpleTask hf = HTNSimpleTask.CreateInstance<HandleFly>();

            if (hf.Precondition(state))  
            {
                Debug.Log($"Executing task: {hf.GetType().Name}");
                hf.Execute(this);
            }

            // Execute HTN tasks
            if (!coolDown)
            {
                planner.ExecuteTasks(this);

                if (coolDown)
                {
                    StartCoroutine(CoolDown());
                }
            }

            // Check for imminent danger
            // If in danger, try to leap or retreat

            if (IsInDanger(out Car threateningCar))
            {
                HTNSimpleTask digestFly = HTNSimpleTask.CreateInstance<DigestFly>();
                if (digestFly.Precondition(state))
                {
                    digestFly.Execute(this);
                    Debug.Log($"Executing task: {digestFly}");
                    
                    return; 
                } else
                {
                    HTNSimpleTask retreat = HTNSimpleTask.CreateInstance<Retreat>();
                    if (retreat.Precondition(state))
                    {
                        Debug.Log($"Executing task: {retreat}");
                        retreat.Execute(this);
                        return; 
                    }
                } 
            }
        }
    }

    public void PerformLeap()
    {
        Vector3 startPosition = transform.position;
        transform.position = startPosition + new Vector3(0, 10f, 0); // animation delay to avoid collisions that shouldn't happen

        StartCoroutine(Leap());
    }

    private IEnumerator Leap()
    {
        isLeaping = true;

        // Initial and target positions
        Vector3 startPosition = transform.position;
        Vector3 leapPosition = startPosition + new Vector3(0, LeapHeight, 0);

        float elapsedTime = 0f;

        // Smoothly move the frog up
        while (elapsedTime < LeapDuration / 2f) // Half the duration for upward leap
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / (LeapDuration / 2f); // Normalized time (0 to 1)
            transform.position = Vector3.Lerp(startPosition, leapPosition, t);
            yield return null;
        }

        transform.position = leapPosition;

        elapsedTime = 0f;

        // Wait until the ground is clear or LeapDuration is reached
        while (elapsedTime < LeapDuration / 2f || !manager.IsSafe(startPosition, manager.GetCarRowIndex(startPosition), 2f))
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        // Smoothly move the frog back down
        while (elapsedTime < LeapDuration / 2f)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / (LeapDuration / 2f); // Normalized time (0 to 1)
            transform.position = Vector3.Lerp(leapPosition, startPosition, t);
            yield return null;
        }

        //transform.position = startPosition;

        isLeaping = false; // Allow other tasks to execute
        state.FliesInBelly--;
        transform.position = transform.position - new Vector3(0, 10f, 0);

    }



    public bool IsInDanger(out Car threateningCar)
    {
        threateningCar = null;

        int row = manager.GetCarRowIndex(transform.position);
        if(row == -1)
        {
            return false; 
        }

        foreach (Car car in manager.cars[row])
            {
                float safeDistance = 7.5f; // Threshold for danger
                float predictedCarPositionZ = car.transform.position.z +
                    car.speed * Time.deltaTime * (car.direction == Car.DIRECTION.R ? -1 : 1);

                if (Vector3.Distance(transform.position, car.transform.position) < safeDistance ||
                    Mathf.Abs(transform.position.z - predictedCarPositionZ) < safeDistance)
                {
                    threateningCar = car;
                    return true;
                }
            }
        

        return false; 
    }


    public void TongueOut()
    {
        // Initialize LineRenderer for the tongue
        if (lineRenderer == null)
        {
            lineRenderer = new GameObject("TongueLine").AddComponent<LineRenderer>();
            lineRenderer.startWidth = 1.8f;
            lineRenderer.endWidth = 1.4f;
            lineRenderer.numCornerVertices = 5;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = new Color(0.3f, 0f, 0f);
        }

        Vector3 frogPosition = transform.position;
        Vector3 flyPredictedPosition = PredictFlyPosition(manager.fly, 0.2f); // Predict position 0.2s ahead

        // Enable LineRenderer and set start point
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, frogPosition); 
        lineRenderer.SetPosition(1, frogPosition); 

        // Animate the tongue 
        StartCoroutine(AnimateTongue(frogPosition, flyPredictedPosition));
    }

    private IEnumerator AnimateTongue(Vector3 frogPosition, Vector3 flyPosition)
    {
        manager.DestroyFly();
        
        float animationDuration = 0.15f; 
        float elapsedTime = 0f;

        // Extend the tongue towards the predicted fly position
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;

            Vector3 currentPosition = Vector3.Lerp(frogPosition, flyPosition, t);
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, currentPosition); // Update tongue end point 

            yield return null; // Wait for next frame
        }

        lineRenderer.SetPosition(1, flyPosition);

        // Animate tongue retracting
        StartCoroutine(RetractTongue(flyPosition, frogPosition));
    }

    private IEnumerator RetractTongue(Vector3 startPosition, Vector3 endPosition)
    {
        float animationDuration = 0.15f; 
        float elapsedTime = 0f;

        // Retract the tongue back to the frog
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;

            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, t);
            lineRenderer.SetPosition(1, currentPosition); // Update tongue end point

            yield return null; // Wait for next frame
        }

        lineRenderer.SetPosition(1, endPosition);
        lineRenderer.enabled = false; 
    }

    private Vector3 PredictFlyPosition(Fly fly, float time)
    {
        // Predict the fly's future position based on its speed and direction
        Vector3 direction = (fly.targetPosition - fly.transform.position).normalized;
        float distanceToTarget = Vector3.Distance(fly.transform.position, fly.targetPosition);

        // Calculate the remaining time to reach the target
        float timeToTarget = distanceToTarget / fly.speed;

        if (timeToTarget > time)
        {
            // If the fly won't reach the target in the given time, predict its position along its current trajectory
            return fly.transform.position + direction * fly.speed * time;
        }
        else
        {
            // Otherwise, oredict its new position after reaching the target
            float remainingTime = time - timeToTarget;
            Vector3 nextTarget = fly.RandomPosition(); // Fly will pick a new random position
            direction = (nextTarget - fly.targetPosition).normalized;

            return fly.targetPosition + direction * fly.speed * remainingTime;
        }
    }



    public void LaunchCoolDown()
    {
        StartCoroutine(CoolDown());
    }

    private IEnumerator CoolDown()
    {

        yield return new WaitForSeconds(.2f); 

        coolDown = false;

    }


    public Vector3 BackwardChoice()
    {
        // Possible moves = 3 downwards (straight, L, R) and 2 laterals 
        Vector3 D = transform.position - new Vector3(Manager.laneDimensions.x, 0, 0); // Down
        Vector3 DL = transform.position - new Vector3(Manager.laneDimensions.x, 0, -Manager.laneDimensions.x / 2); // Down left
        Vector3 DR = transform.position - new Vector3(Manager.laneDimensions.x, 0, Manager.laneDimensions.x / 2); // Down right
        Vector3 L = transform.position - new Vector3(0, 0, Manager.laneDimensions.x / 2); // Left
        Vector3 R = transform.position + new Vector3(0, 0, Manager.laneDimensions.x / 2); // Right

        // Evaluate moves based on safety duration and distance to the fly
        Vector3 flyPosition = manager.fly.transform.position;

        var moves = new List<(Vector3 position, float safetyScore, float flyDistance)>
    {
        (D, EvaluateSafetyDuration(D, state.Row), Vector3.Distance(D, flyPosition)),
        (DL, EvaluateSafetyDuration(DL, state.Row), Vector3.Distance(DL, flyPosition)),
        (DR, EvaluateSafetyDuration(DR, state.Row), Vector3.Distance(DR, flyPosition)),
        (L, EvaluateSafetyDuration(L, state.Row), Vector3.Distance(L, flyPosition)),
        (R, EvaluateSafetyDuration(R, state.Row), Vector3.Distance(R, flyPosition))
    };

        var bestMove = moves
            .Where(move => move.safetyScore > 0) // Only consider safe moves
            .OrderByDescending(move => move.safetyScore) // Prioritize safety
            .ThenBy(move => state.FliesInBelly < 2 ? move.flyDistance : 0) // Consider fly distance only if fliesInBelly < 2
            .FirstOrDefault();

        if (bestMove != default)
        {
            return manager.InBounds(bestMove.position) ? bestMove.position : transform.position;
        }

        return transform.position;
    }



    public Vector3 ForwardChoice()
    {
        // Possible moves = 3 upwards (straight, L, R) 
        Vector3 U = transform.position + new Vector3(Manager.laneDimensions.x, 0, 0); // Up
        Vector3 UL = transform.position + new Vector3(Manager.laneDimensions.x, 0, Manager.laneDimensions.x / 2); // Up left
        Vector3 UR = transform.position + new Vector3(Manager.laneDimensions.x, 0, -Manager.laneDimensions.x / 2); // Up right

        if (state.Row == 9 || state.Row == 4) // Go to sidewalk immediately if possible 
        {
            return U;
        }

        // Evaluate moves based on safety duration and distance to the fly
        Vector3 flyPosition = manager.fly.transform.position;
        
        var moves = new List<(Vector3 position, float safetyScore, float flyDistance)>
    {
           
        (U, EvaluateSafetyDuration(U, state.Row), Vector3.Distance(U, flyPosition)),
        (UL, EvaluateSafetyDuration(UL, state.Row), Vector3.Distance(UL, flyPosition)),
        (UR, EvaluateSafetyDuration(UR, state.Row), Vector3.Distance(UR, flyPosition))
    };

        // Rank moves by safety score, then by proximity to the fly
        var bestMove = moves
            .Where(move => move.safetyScore > 0) // Only consider safe moves
            .OrderByDescending(move => move.safetyScore) // Prioritize safety
            .ThenBy(move => state.FliesInBelly < 2 ? move.flyDistance : 0) // Consider fly distance only if fliesInBelly < 2
            .FirstOrDefault();

        if (bestMove != default)
        {
            return manager.InBounds(bestMove.position) ? bestMove.position : transform.position;
        }

        return transform.position;
    }

    public float EvaluateSafetyDuration(Vector3 position, int row)
    {
        float timeStep = 0.01f; 
        float maxInterval = 1.0f; // Lookahead duration
        float safetyDuration = 0f;

        // If the position corresponds to a diagonal move (UL or UR)
        if (position.z != transform.position.z)
        {
            // Determine intermediate lateral position
            Vector3 lateralMove = new Vector3(transform.position.x, transform.position.y, position.z);
            Vector3 forwardMove = new Vector3(position.x, position.y, position.z);

            // Check safety for the lateral move
            for (float t = 0; t <= maxInterval; t += timeStep)
            {
                bool lateralSafe = manager.IsSafe(lateralMove, row, t);

                if (!lateralSafe)
                {
                    return 0; // If lateral move is unsafe, trajectory is unsafe
                }
            }

            // Check safety for the forward move
            for (float t = 0; t <= maxInterval; t += timeStep)
            {
                bool forwardSafe = manager.IsSafe(forwardMove, row + 1, t);

                if (!forwardSafe)
                {
                    return 0; // If forward move is unsafe, traj is unsafe
                }
                safetyDuration += timeStep;
            }
        }
        else
        {
            // For straight moves, only check the forward position
            for (float t = 0; t <= maxInterval; t += timeStep)
            {
                bool safe = manager.IsSafe(position, row, t);

                if (!safe)
                {
                    break; // unsafe
                }
                safetyDuration += timeStep;
            }
        }

        return safetyDuration;
    }



}
