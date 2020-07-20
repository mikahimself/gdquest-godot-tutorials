using Godot;
using System;

public class FollowAgent : KinematicBody2D
{
    Sprite Triangle;
    [Export]
    public float MaxSpeed = 500.0f;
    public const float DISTANCE_TRESHOLD = 3.0f;
    private Vector2 _velocity = Vector2.Zero;


    public override void _Ready()
    {
        Triangle = (Sprite)GetNode("TriangleSprite");
    }

//  public override void _Process(float delta)
//  {
//      
//  }

    public override void _PhysicsProcess(float delta)
    {
        Vector2 targetGlobalPosition = GetGlobalMousePosition();
        if (GlobalPosition.DistanceTo(targetGlobalPosition) < DISTANCE_TRESHOLD)
        {
            return;
        }
        GD.Print("test");
        _velocity = Steering.Follow(_velocity, GlobalPosition, targetGlobalPosition, MaxSpeed);
        _velocity = MoveAndSlide(_velocity);
        Triangle.Rotation = _velocity.Angle();
    }
}
