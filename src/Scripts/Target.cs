using Godot;
using System;

public class Target : Area2D
{

    AnimationPlayer ap;
    [Signal]
    public delegate void TargetCreated(Area2D area);
    public override void _Ready()
    {
        ap = (AnimationPlayer)GetNode("AnimationPlayer");
        Connect("body_entered", this, "_onBodyEntered");
        GetParent().GetNode("CharacterArriveTo").Connect("StartedMoving", this, "_OnTargetCreated");
        Visible = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("click"))
        {
            ap.Play("FadeIn");
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Input.IsActionPressed("click"))
        {
            GlobalPosition = GetGlobalMousePosition();
            EmitSignal(nameof(TargetCreated), this);
        }
    }

    private void _onBodyEntered(PhysicsBody2D body)
   {
       ap.Play("FadeOut");
   }
    async public void _OnTargetCreated()
    {
        if (GetOverlappingBodies().Count > 0)
        {
            await ToSignal(ap, "animation_finished");
            ap.Play("FadeOut"); 
        }
    }
}
