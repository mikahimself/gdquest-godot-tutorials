using Godot;
using System;

public class Steering : Node
{
    public const float DEFAULT_MASS = 2.0f;
    public const float DEFAULT_MAX_SPEED = 400.0f;
    public const float DEFAULT_SLOWDOWN_RADIUS = 200.0f;



    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    public static Vector2 Follow(Vector2 velocity,
                                 Vector2 globalPosition,
                                 Vector2 targetPosition,
                                 float max_speed = DEFAULT_MAX_SPEED,
                                 float mass = DEFAULT_MASS)
    {
        // Vector pointing from agent's current position to it's target at max speed.
        Vector2 desiredVelocity = (targetPosition - globalPosition).Normalized() * max_speed;
        // Vector pointing from the agent's current velocity to the tip of the desired velocity.
        // That is, the difference between the desired velocity and the current velocity divided by mass.
        Vector2 steering = (desiredVelocity - velocity) / mass;
        // Use steering to slowly turn the velocity towards the desired velocity.
        return velocity + steering;
    }

    public static Vector2 ArriveTo(Vector2 velocity,
                                 Vector2 globalPosition,
                                 Vector2 targetPosition,
                                 float maxSpeed = DEFAULT_MAX_SPEED,
                                 float slowdownRadius = DEFAULT_SLOWDOWN_RADIUS,
                                 float mass = DEFAULT_MASS)
    {
        // Vector pointing from agent's current position to it's target at max speed.
        float toTarget = globalPosition.DistanceTo(targetPosition);
        Vector2 desiredVelocity = (targetPosition - globalPosition).Normalized() * maxSpeed;
        if (toTarget < slowdownRadius)
        {
            desiredVelocity *= (toTarget / slowdownRadius) * 0.8f + 0.2f;
        }

        // Vector pointing from the agent's current velocity to the tip of the desired velocity.
        // That is, the difference between the desired velocity and the current velocity divided by mass.
        Vector2 steering = (desiredVelocity - velocity) / mass;
        // Use steering to slowly turn the velocity towards the desired velocity.
        return velocity + steering;
    }
}
