using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Manager : MonoBehaviour
{
    public List<Car>[] cars;
    public Fly fly;
    public Frog frog;

    public static Vector2 laneDimensions; 
    private float speedingFactor;
    public static Vector2 extremePts;

    private bool updateWinRate = true;


    // Start is called before the first frame update
    void Start()
    {

        timeoutCoroutine = StartCoroutine(TimeoutTimer(60f)); // 1-minute timeout

        //PlayerPrefs.SetInt("Losses", 0); // Reset this if want clean stats
        //PlayerPrefs.SetInt("Wins", 0); // Reset this if want clean stats

        Vector3 rightmost = GameObject.FindGameObjectWithTag("R").transform.position;
        Vector3 leftmost = GameObject.FindGameObjectWithTag("L").transform.position;

        extremePts = new Vector2(rightmost.z, leftmost.z); 
        laneDimensions = new Vector2(9.5f, Math.Abs(rightmost.z - leftmost.z));      // height x width, from design 
        speedingFactor = 1.6f;  // x in [0.1,2.0] 

        SpawnAll();

        SetFlyPrimitive();
    }


    // Update is called once per frame
    void Update()
    {
        SetFlyPrimitive();
        
        foreach(List<Car> lane in cars)
        {
            foreach (Car car in lane)
            {
                car.UpdateCar();

                // Lose, restart 
                // Check for collision between the frog and the car
                if (Vector3.Distance(car.transform.position, frog.transform.position) < 1.5f) // threshold should be small enough to not consider inter-lanes (only intra)
                {
                    StartCoroutine(LoseScreen());
                    return; // To prevent duplicate calls
                }
            }
        }

        // Win, restart
        if(frog.state.Row > 9)
        {
            if(updateWinRate)
            {
                // Get saved wins
                PlayerPrefs.SetInt("Wins", PlayerPrefs.GetInt("Wins") + 1);      // Increment and save
                PlayerPrefs.Save();
                updateWinRate = false; 
            }
            

            StartCoroutine(WinScreen());
        }
    }


    

    // Function to destroy fly when gets eaten
    public void DestroyFly()
    {
        float delay = 0.1f;
        fly.transform.position = new Vector3(fly.transform.position.x, fly.transform.position.y - 50f, fly.transform.position.z);
        GameObject.Destroy(fly, delay);
        SpawnFly();
    }

    // Spawn functions
    /// --------------
    
    void SpawnAll()
    {
        SpawnCars();
        SpawnFly();
        SpawnFrog(); 
    }



    void SpawnCars()
    {
        float[] speeds = new float[8];

        cars = new List<Car>[8]; // per lane
        for(int i=0; i<8; i++)
        {
            speeds[i] = laneDimensions.y / (speedingFactor * UnityEngine.Random.Range(1, 10)); 
            cars[i] = new List<Car>();
        }

        int v = UnityEngine.Random.Range(15,35); // 8 - 80 

        GameObject[] laneWaypoints = GameObject.FindGameObjectsWithTag("LaneWaypoint");
        laneWaypoints = laneWaypoints.OrderBy(wp => wp.transform.position.z).ToArray();

        for (int i=0;i<v;i++)
        {

            int randomCarIndex = UnityEngine.Random.Range(1, 11); 
            string carName = $"Car-{randomCarIndex}"; // Construct the name of the random car (see 'Resources')
            Car newcar = Instantiate(Resources.Load(carName, typeof(Car))) as Car;
            newcar.transform.localScale = new Vector3(2,2,2);
            newcar.speed = speeds[i%8];
            newcar.lane = (i % 8);

            newcar.direction = (i % 2 == 0) ? Car.DIRECTION.L : Car.DIRECTION.R;
            
            // Get base waypoint for the current lane
            Vector3 basePosition = laneWaypoints[i % 8].transform.position;
            Vector3 spawnPosition = basePosition;

            // Offset the car along the z-axis, ensuring no overlap
            bool validPosition = false;
            int attempts = 0;

            while (!validPosition && attempts < 100)
            {
                // Propose new spawn position
                spawnPosition = basePosition + new Vector3(0, 0, UnityEngine.Random.Range(15, laneDimensions.y));

                // Check if the new position is far enough from all other cars in the same lane
                validPosition = cars[i % 8].All(existingCar =>
                    Vector3.Distance(existingCar.transform.position, spawnPosition) >= 15.0f);

                attempts++;
            }

            // Assign the validated position
            newcar.transform.position = spawnPosition;

            
            cars[i % 8].Add(newcar);
        }

        
    }


    void SpawnFly()
    {
        fly = Instantiate(Resources.Load("Fly", typeof(Fly))) as Fly;
        Vector3 randPos = fly.RandomPosition();
        fly.transform.position = new Vector3(randPos.x, 8f, randPos.z);
    }


    void SpawnFrog()
    {
        frog = Instantiate(Resources.Load("Froggie", typeof(Frog))) as Frog;
        frog.transform.position = new Vector3(-70f, .2f, 0);
        frog.transform.localScale = new Vector3(3,3,3);
        frog.manager = this;
    }


    // Win / Lose functions
    /// -------------------

    public void Lose()
    {
        // int losses = PlayerPrefs.GetInt("Losses", 0);  // Get saved losses
        PlayerPrefs.SetInt("Losses", PlayerPrefs.GetInt("Losses") + 1);      // Increment and save
        PlayerPrefs.Save();
        StartCoroutine(LoseScreen());
    }

    // Win Coroutine
    private IEnumerator WinScreen()
    {
        // Stop the timeout coroutine
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
        }

        // Show the win screen
        WinScreenController winScreenController = FindObjectOfType<WinScreenController>();
        if (winScreenController != null)
        {
            winScreenController.ShowWinScreen();
        }

        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);

        // Restart(reload scene)
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        updateWinRate = true;
    }

    // Loss Coroutine 
    private IEnumerator LoseScreen(bool isTimeout = false)
    {
        // Show the lose screen with a timeout-specific message
        WinScreenController winScreenController = FindObjectOfType<WinScreenController>();
        if (winScreenController != null)
        {
            if (isTimeout)
            {
                winScreenController.ShowCustomLoseMessage("Frogger took too long!");
            }
            else
            {
                winScreenController.ShowLoseScreen();
            }
        }

        // Freeze the game by pausing time
        Time.timeScale = 0f;

        // Use unscaled time for the delay
        float freezeDuration = 3f;
        float startTime = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup < startTime + freezeDuration)
        {
            yield return null; // Wait for the next frame
        }

        // Unfreeze the game
        Time.timeScale = 1f;

        // Restart the game (reload the active scene)
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void Lose(bool isTimeout = false)
    {
        int losses = PlayerPrefs.GetInt("Losses", 0);  // Get saved losses
        PlayerPrefs.SetInt("Losses", losses + 1);      // Increment and save
        PlayerPrefs.Save();
        StartCoroutine(LoseScreen(isTimeout));
    }




    // TimeOut for taking too long 

    private Coroutine timeoutCoroutine;

    private IEnumerator TimeoutTimer(float duration)
    {
        yield return new WaitForSeconds(duration);

        // If the frog hasn't won yet, trigger a timeout loss
        if (frog.state.Row <= 9) // Check if the frog hasn't reached the winning row
        {
            Debug.Log("Frogger took too long!");
            Lose(isTimeout: true);
        }
    }



    // Other helpers:

    public bool InBounds(Vector3 pos)
    {
        return (pos.z > extremePts.x) && (pos.z < extremePts.y) && (pos.x <= 25);
    }


    public bool IsSafe(Vector3 position, int row, float interval)
    {
        if (row == 4 || row > 8) return true; // sidewalk

        row = GetCarRowIndex(position);
        if (row == -1)
        {
            return true; // sidewalk
        }


        foreach (Car car in cars[row])
        {
            float safeDistance = 14f; //baseSafeDistance * (car.speed * .1f + speedFactor); 

            float predictedCarPositionZ = car.transform.position.z + car.speed * interval * (car.direction == Car.DIRECTION.R ? -1 : 1);

            // Handle wrap-around logic
            if (predictedCarPositionZ > extremePts.y)
            {
                predictedCarPositionZ -= laneDimensions.y;
            }
            else if (predictedCarPositionZ < extremePts.x)
            {
                predictedCarPositionZ += laneDimensions.y;
            }

            float distance = Mathf.Abs(position.z - predictedCarPositionZ);

            if (distance < safeDistance)
            {
                return false;
            }
        }
        return true;
    }

    public int GetCarRowIndex(Vector3 position)
    {
        float threshold = 5f; // threshold for matching the row
        for (int i = 0; i < cars.Length; i++)
        {
            if (cars[i].Count > 0)
            {
                float rowX = cars[i][0].transform.position.x;
                if (Mathf.Abs(position.x - rowX) <= threshold)
                {
                    return i; // Return the index of the matching row
                }
            }
        }

        // Return -1 if no matching row is found
        return -1;
    }


    // Set the primitive for FlyWithinReach
    void SetFlyPrimitive()
    {
        if (FlyWithinReach())
        {
            if (frog.state == null)
            {
                frog.state = new FrogState(0, true, 0);
            }
            frog.state.FlyWithinReach = true;
        }
        else
        {
            if (frog.state == null)
            {
                frog.state = new FrogState(0, false, 0);
            }
            frog.state.FlyWithinReach = false;
        }
    }

    public bool FlyWithinReach()
    {
        bool withinReach = (Distance(fly.transform.position, frog.transform.position) <= 25f);
        return withinReach;
    }

    private float Distance(Vector3 a, Vector3 b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2) + Mathf.Pow(a.z - b.z, 2));
    }


}
