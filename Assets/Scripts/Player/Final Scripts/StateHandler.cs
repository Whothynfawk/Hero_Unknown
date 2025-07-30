using UnityEngine;
public enum MovementStates
{
    groundMovement,
    sprinting,
    wallRun,
    vault,
    cliffMovement,
    sliding,
    unlimited,
    restricted,
}

public enum GameFeelStates
{
    explore,
    view,
    speedrun,
}
public class StateHandler : MonoBehaviour
{
    public MovementStates state;
    public GameFeelStates feel;
}
