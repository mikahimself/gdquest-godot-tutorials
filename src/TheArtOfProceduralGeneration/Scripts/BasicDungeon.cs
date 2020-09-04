using Godot;
using System;
using System.Collections.Generic;

public class BasicDungeon : Node2D
{
    [Export]
    public Vector2 LevelSize = new Vector2(100, 80);
    [Export]
    public Vector2 RoomsSize = new Vector2(10, 14);
    [Export]
    public int RoomsMax = 15;

    TileMap level;
    Camera2D camera;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        level = (TileMap)GetNode("Level");
        camera = (Camera2D)GetNode("Camera2D");

        _setupCamera();
        _generate();
    }
    
    public void _setupCamera()
    {
        camera.Position = level.MapToWorld(LevelSize / 2);
        var zoom = Math.Max(LevelSize.x, LevelSize.y) / 8;
        camera.Zoom = new Vector2(zoom, zoom);
    }

    public void _generate()
    {
        level.Clear();
        foreach (Vector2 vector in _generateData())
        {
            level.SetCellv(vector, 0);
        }
    }

    public Vector2[] _generateData()
    {
        var rng = new GD.RandomNumberGenerator();
        rng.Randomize();

        var data = new Dictionary<Vector2, int>();
        rect2[] rooms;

        foreach (int r in Enumerable.Range(RoomsMax))
        {
            var room = _getRandomRoom(rng);
            if (_intersects(rooms, room))
            {
                continue;
            }
            _addRoom(data, rooms, room);
            if (rooms.Count > 1)
            {
                var roomPrevious = rooms[-2];
                _addConnection(rng, data, roomPrevious, room);
            }
        }
        return data.Keys();
    }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
