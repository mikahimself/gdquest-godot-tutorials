using Godot;
using System;

public class ArriveToAgent : KinematicBody2D
{
    Sprite Triangle;
    [Export]
    public float SlowdownRadius = 300.0f;
    [Export]
    public float MaxSpeed = 500.0f;
    public const float DISTANCE_TRESHOLD = 3.0f;
    private Vector2 _velocity = Vector2.Zero;


    public override void _Ready()
    {
        Triangle = (Sprite)GetNode("TriangleSprite");
    }

    public override void _PhysicsProcess(float delta)
    {
        Vector2 targetGlobalPosition = GetGlobalMousePosition();
        if (GlobalPosition.DistanceTo(targetGlobalPosition) < DISTANCE_TRESHOLD)
        {
            return;
        }
        _velocity = Steering.ArriveTo(_velocity,
                                    GlobalPosition,
                                    targetGlobalPosition,
                                    maxSpeed: MaxSpeed,
                                    SlowdownRadius);
        _velocity = MoveAndSlide(_velocity);
        Triangle.Rotation = _velocity.Angle();
    }
}
