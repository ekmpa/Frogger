# Frogger
A Frogger-inspired game in which a frog AI Agent (using a Hierarchical Task Network) navigates a busy road.

## Demos

Short demo:

 https://github.com/user-attachments/assets/b8c82f67-987a-44de-835d-6e212eadd587

[Full map traversal demo here](https://github.com/user-attachments/files/18275125/Screen.Recording.2024-12-30.at.2.51.10.PM.zip)

## Details

### Synopsis
The frog crosses the highway upwards, totaling 8 lanes with a sidewalk (safe zone) in the middle. It can move in cardinal movement, and avoids the cars (which are spawned randomly, both in terms of number and speed). A fly flies around the scene, and, if within radius, can be eaten by the frog. In that case, the frog can use that fly in its belly at any point to leap over the cars for 500ms, in case of risk due to a car incoming. However, the frog can not have more than 2 flies in its belly.

Using a Hierarchical Task Network, the frog plans a course of actions, as well as some instantaneous decision-making concerning the fly, to attain its goal.   


### World State Representation
The world state is represented with 3 primitives, `fliesInBelly`, `flyWithinReach` and `row`, giving a `FrogState state` field for the frog.

### The Hierarchical Task Network (HTN)
The frog agent is implemented with an HTN with composite tasks for crossing the highway (`CrossHighay -> CrossSection -> CrossLane`) as well as simple tasks for individual actions (`HandleFly`, `DigestFly`, `Retreat`, `MoveForward` and `CoolDown`), each with their pre- and post-conditions. 

<img width="488" alt="Screenshot 2024-12-30 at 3 12 04 PM" src="https://github.com/user-attachments/assets/395b4174-dcca-4105-949e-553dde73ba19" />

**Transitions:**
- $m_1$ = minimum 1 section left to traverse (i.e. 4 lanes)
- $m_2$ = minimum 1 lane to traverse in a section to attain a sidewalk.
- $m_3 = \texttt{pre(HandleFly)}$
- $m_4$ = frog is in danger & $\texttt{pre(DigestFly)}$
- $m_5$ = frog is in danger, can't leap, and $\texttt{pre(Retreat)}$
- $m_6 = \texttt{!}m_{3,4,5}$ & $\texttt{pre(MoveForward)}$
- $m_7$ = just executed a move other than digesting a fly (i.e., leaping) 

**Decision-making:**
The decision making process makes the frog prioritize its goal of getting to the goal, while also accounting for a second goal: getting within radius of the fly to eat it. This was decided as, depending on the parameters (specifically the range of values within which the cars' speed and number are spawned in), some levels are impossible without leaping - it offers a great advantage to the frog and thus having the frog consider this goal as well generally increase the success rate.
