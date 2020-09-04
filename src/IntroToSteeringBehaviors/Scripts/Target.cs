using Godot;
using System;

public class Target : Area2D
{

    AnimationPlayer ap;

    public override void _Ready()
    {
        ap = (AnimationPlayer)GetNode("AnimationPlayer");
        Connect("body_entered", this, "_onBodyEntered");
        GetParent().GetNode("CharacterArriveTo").Connect("StartedMoving", this, "_OnAgentStartedMoving");
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
        }
    }

    private void _onBodyEntered(PhysicsBody2D body)
    {
        ap.Play("FadeOut");
    }
    
    async public void _OnAgentStartedMoving()
    {
        if (GetOverlappingBodies().Count > 0)
        {
            await ToSignal(ap, "animation_finished");
            ap.Play("FadeOut");
        }
    }
}
