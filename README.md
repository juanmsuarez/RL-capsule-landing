# Landing a space capsule using RL
## Introduction
This project consists of a simulation environment built in Unity, where a space capsule can be flown firing any of its engines (inspired by the [SpaceX Dragon 2](https://en.wikipedia.org/wiki/SpaceX_Dragon_2)). 

A reinforcement learning agent is trained to solve the task of landing the capsule in the target zone (specifically, using the PPO algorithm implemented by the [ML-Agents library](https://unity.com/products/machine-learning-agents)). Note that this is a difficult task for humans, since the force applied by the engines can make the capsule unstable really easily.


![testing-2](https://user-images.githubusercontent.com/7390500/159096074-69e926d7-fb7b-44d9-bd0c-480cf45571d6.gif)

## Agent
### Observations
The agent observes the current state of the capsule, which is defined by its position, rotation, velocity and angular velocity.
### Actions
The agent can fire any of the four engines (four branches with two possible actions each).
### Rewards
The main reward is given when the capsule lands or crashes; since it's a sparse reward, it's defined as a continuous function to speed up training. This function depends on:
- Landing angle: high positive reward when landing with a nose-up orientation.
- Landing speed: high positive reward when landing slowly.
- Distance to landing zone: high positive when landing close to the target. 

These factors contribute a positive reward to the function when they are close to zero, and contribute a negative reward when they exceed a certain threshold.

## Training process
The agent was trained for 40M steps, the learning process is shown below:
### ~50k steps
The agent can't control the capsule.
![training-1](https://user-images.githubusercontent.com/7390500/159094569-e836671c-2027-47b4-83aa-6d8cf71d98a9.gif)
### ~5M steps
The agent flies the capsule towards the landing zone, but it can't land.
![training-2](https://user-images.githubusercontent.com/7390500/159094674-3df1969f-f2d6-4c4d-9596-bbf7fa03c3f7.gif)
### ~20M steps
The agent can land the capsule, but only when the initial conditions are favorable.
![training-3](https://user-images.githubusercontent.com/7390500/159094957-2576db9e-0aad-4e3e-8ead-c362b6a16a58.gif)
### ~30M steps
The agent lands the capsule in most cases, but it can still crash and it's somewhat inefficient.
![training-4](https://user-images.githubusercontent.com/7390500/159095115-2d98a344-ad2b-4d14-8e8e-b1ee820dd086.gif)
### ~40M steps
The agent learned to land the capsule in all the situation tested, minimizing the number of steps required.
![training-5](https://user-images.githubusercontent.com/7390500/159095446-1aa13d56-e404-4773-a2fe-99aaf58bfe02.gif)
### Cumulative reward
![image](https://user-images.githubusercontent.com/7390500/159065012-fb1c8466-59b6-4138-a403-8443fd9fe2aa.png)

## Conclusion
As mentioned in the paper [“It’s Unwieldy and It Takes a Lot of Time” — Challenges and Opportunities for Creating Agents in Commercial Games](https://www.aaai.org/ojs/index.php/AIIDE/article/view/7415), training an RL agent involves a wide variety of challenges, including lack of designer control, training and evaluation costs, among others. These issues are also present in this task, which makes it an interesting case to study.
