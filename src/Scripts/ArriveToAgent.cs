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
    private Vector2 _targetGlobalPosition;
    [Signal]
    public delegate void StartedMoving();
    public bool startedMoving = false;

    public override void _Ready()
    {
        Triangle = (Sprite)GetNode("TriangleSprite");
        SetPhysicsProcess(false);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("click"))
        {
            _targetGlobalPosition = GetGlobalMousePosition();
            SetPhysicsProcess(true);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (startedMoving)
        {
            EmitSignal(nameof(StartedMoving));
            startedMoving = false;
        }

        if (Input.IsActionPressed("click"))
        {
            _targetGlobalPosition = GetGlobalMousePosition();
            startedMoving = true;
            
        }
        if (GlobalPosition.DistanceTo(_targetGlobalPosition) < DISTANCE_TRESHOLD)
        {
            return;
        }
        _velocity = Steering.ArriveTo(_velocity,
                                    GlobalPosition,
                                    _targetGlobalPosition,
                                    maxSpeed: MaxSpeed,
                                    SlowdownRadius);
        _velocity = MoveAndSlide(_velocity);
        Triangle.Rotation = _velocity.Angle();
    }
}
