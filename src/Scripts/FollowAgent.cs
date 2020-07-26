using Godot;
using System;

public class FollowAgent : KinematicBody2D
{
    Sprite Triangle;
    [Export]
    public float MaxSpeed = 500.0f;
    [Export]
    public NodePath TargetPath;
    [Export]
    public float FollowOffset = 100f;
    [Export]
    public float ArriveRadius = 200f;
    [Export]
    public float Mass = 5.0f;

    public const float DISTANCE_TRESHOLD = 3.0f;
    private Vector2 _velocity = Vector2.Zero;
    private Vector2 _targetGlobalPosition;
    [Signal]
    public delegate void StartedMoving();
    public bool startedMoving = false;
    private Node2D _target;

    public override void _Ready()
    {
        Triangle = (Sprite)GetNode("TriangleSprite");
        SetPhysicsProcess(false);
        _target = (Node2D)GetNode(TargetPath);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("click"))
        {
            SetPhysicsProcess(true);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        _targetGlobalPosition = _target.GlobalPosition;
        float toTarget = GlobalPosition.DistanceTo(_targetGlobalPosition);

        if (startedMoving)
        {
            // Emit signal one frame after agent starts moving. This is because
            // Target uses GetOverlappingBodies(), which is updated only once per frame,
            // to detect overlapping bodies.
            EmitSignal(nameof(StartedMoving));
            startedMoving = false;
        }

        if (toTarget < DISTANCE_TRESHOLD)
        {
            return;
        }

        // OffsetTargetPosition is a position slightly behind the object that the agent follows.
        // We get it by calculating the direction to the target (subtract agent's position from target's
        // position and normalize the result), multiplying the direction with the follow offset,
        // and subtracting that from the target's global position.
        // We only use it if the distance to target is greater than the follow offset. Otherwise, 
        // the offsetTargetPosition is set to the current position.
        Vector2 offsetTargetPosition = toTarget > FollowOffset 
                                        ? _targetGlobalPosition - (_targetGlobalPosition - GlobalPosition).Normalized() * FollowOffset 
                                        : GlobalPosition;

        _velocity = Steering.ArriveTo(_velocity,
                                        GlobalPosition, 
                                        offsetTargetPosition, 
                                        MaxSpeed, 
                                        ArriveRadius,
                                        Mass);
        _velocity = MoveAndSlide(_velocity);
        Triangle.Rotation = _velocity.Angle();
    }
}
