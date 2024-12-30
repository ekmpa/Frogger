using System.Collections.Generic;
using UnityEngine;
using System.Text; 

public class HTNPlanner : ScriptableObject
{
    public Stack<(HTNTask task, int depth)> tasks;
    private int currentIndex;
    public List<HTNSimpleTask> plan;
    public Manager manager; 

    public void InitializeTasks(Frog frog)
    {
        currentIndex = 0;

        MakePlan(frog); 
    }



    public void MakePlan(Frog frog)
    {
        tasks = new Stack<(HTNTask task, int depth)>(); // tasks with their depth levels (for logging the plan) 

        if (frog.row == 0)
        {
            frog.LaunchCoolDown();
        }

        plan = new List<HTNSimpleTask>();
        FrogState state = frog.state;

        tasks.Push((HTNCompositeTask.CreateInstance<CrossHighway>(), 0)); // Start with depth 0

        Debug.Log("Starting HTN planning...");
        StringBuilder compositeTaskLog = new StringBuilder();

        while (tasks.Count > 0)
        {
            var (task, depth) = tasks.Pop();

            if (task is HTNCompositeTask ct)
            {
                // Log composite task with indentation based on depth
                compositeTaskLog.AppendLine($"{new string(' ', depth * 2)}{ct.GetType().Name}");

                List<HTNTask> subtasks = ct.PlanSubtasks(frog, state);
                if (subtasks == null) break;

                // Push subtasks onto stack with increased depth
                // Reverse the list to maintain the correct order when popping from the stack
                for (int i = subtasks.Count - 1; i >= 0; i--)
                {
                    tasks.Push((subtasks[i], depth + 1));
                }
            }
            else if (task is HTNSimpleTask st)
            {
                if (st.Precondition(state))
                {
                    plan.Add(st);
                }
            }
        }

    // Output the task hierarchy
    Debug.Log("Planned composite tasks:\n" + compositeTaskLog.ToString());
    }


    public bool ExecuteTasks(Frog frog) 
    {

        if ((plan.Count == 0 || currentIndex >= plan.Count) && !(frog.state.Row > 9)) 
        {
            currentIndex = 0; 
            MakePlan(frog);
        }

        Debug.Log($"Executing task: {plan[currentIndex].GetType().Name}");

        if (frog.state.Row > 9) 
        {
            return true;
        }

        if (currentIndex < plan.Count)
        {
            bool taskComplete = plan[currentIndex].Execute(frog);

            if (plan[currentIndex].GetType() != typeof(DigestFly))
            {
                frog.coolDown = true;
            }

            if (taskComplete)
            {
                currentIndex++;
                return true;

            }
            else
            {
                if (plan[currentIndex] is MoveForward)
                {
                    frog.LaunchCoolDown();
                    if(frog.row < 10)
                    {
                        HTNSimpleTask mf = HTNSimpleTask.CreateInstance<MoveForward>();
                        mf.Execute(frog);
                    }
                    
                }
            }
        }
        return false;
    }
}
