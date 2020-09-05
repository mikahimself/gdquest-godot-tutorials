using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        
        // Save info here for Level node
        var data = new Dictionary<Vector2, int>();
        // Store generated rooms (used for checking intersections)
        List<Rect2> rooms = new List<Rect2>();

        foreach (int r in Enumerable.Range(0, RoomsMax))
        {
            var room = _getRandomRoom(rng);
            // Break if generated room intersects with others
            if (_intersects(rooms, room))
            {
                continue;
            }
            _addRoom(data, rooms, room);
            if (rooms.Count > 1)
            {
                Rect2 roomPrevious = rooms[rooms.Count - 2];
                _addConnection(rng, data, roomPrevious, room);
            }
        }
        return data.Keys.ToArray();
    }

    /// <summary>The <c>_getRandomRoom</c> generates a random room as
    /// a <c>Rect2</c>, which stores the room's position and size.</summary>
    public Rect2 _getRandomRoom(RandomNumberGenerator rng)
    {
        // RoomSize.x defines the min size, RoomSize.y the max.
        var width = rng.RandiRange((int)RoomsSize.x, (int)RoomsSize.y);
        var height = rng.RandiRange((int)RoomsSize.x, (int)RoomsSize.y);
        // Rooms are defined as Rect2
        var x = rng.RandiRange(0, (int)LevelSize.x - width - 1);
        var y = rng.RandiRange(0, (int)LevelSize.y - height - 1);
        return new Rect2(x, y, width, height);
    }

    /// <summary>The <c>_addRoom</c> method adds randomly generated rooms in the
    /// <c>rooms</c> list, and each Vector2 point of the room in the <c>data</c>
    /// dictionary</summary>
    public void _addRoom(Dictionary<Vector2, int> data, List<Rect2> rooms, Rect2 room)
    {
        rooms.Add(room);
        for (int x = (int)room.Position.x; x < (int)room.End.x; x++)
        {
          for (int y = (int)room.Position.y; y < (int)room.End.y; y++)
            {
                data[new Vector2(x, y)] = 0;
            }
        }
    }
    
    /// <summary>The <c>_addConnection</c> method adds a connection from <c>room1</c> to <c>room2</c>.
    /// The connection starts from the middle point of the room on either the top/bottom or left/right
    /// side of the room.</summary>
    public void _addConnection(RandomNumberGenerator rng, Dictionary<Vector2, int> data, Rect2 room1, Rect2 room2)
    {
        var roomCenter1 = (room1.Position + room1.End) / 2;
        var roomCenter2 = (room2.Position + room2.End) / 2;

        if (rng.RandiRange(0, 1) == 0)
        {   
            // 1. Draw line from the first room's center X position to second room's center X position
            // horizontally along the middle point of the first room.
            // 2. Draw line from the first room's center Y position to second room's center Y position
            // vertically along the middle point of the second room.
            // That is, start from left/right side, finish up at top or bottom.
            _addCorridor(data, (int)roomCenter1.x, (int)roomCenter2.x, (int)roomCenter1.y, Vector2.Axis.X);
            _addCorridor(data, (int)roomCenter1.y, (int)roomCenter2.y, (int)roomCenter2.x, Vector2.Axis.Y);
        }
        else
        {
            // Start from top/bottm, finish up at left/right side.
            _addCorridor(data, (int)roomCenter1.y, (int)roomCenter2.y, (int)roomCenter1.x, Vector2.Axis.Y);
            _addCorridor(data, (int)roomCenter1.x, (int)roomCenter2.x, (int)roomCenter2.y, Vector2.Axis.X);
        }
    }

    /// <summary>The <c>_addCorridor</c> method adds corridor points along either the X axis or the Y axis
    /// starting from the <c>start</c> position to the <c>end</c> position.</summary>

    public void _addCorridor(Dictionary<Vector2, int> data, int start, int end, int constant, Vector2.Axis axis)
    {
        for (int t = Math.Min(start, end); t <= Math.Max(start, end); t++)
        {
            var point = Vector2.Zero;

            switch (axis)
            {
                case Vector2.Axis.X:
                    point = new Vector2(t, constant);
                    break;
                case Vector2.Axis.Y:
                    point = new Vector2(constant, t);
                    break;
            }
            data[point] = 0;
        }
    }

    /// <summary>The <c>_intersect</c> method tests whether the room passed as the second parameter
    /// intersects with any of the rooms in the <c>rooms</c> list.</summary>
    public bool _intersects(List<Rect2> rooms, Rect2 room)
    {
        var isIntersecting = false;
        foreach (Rect2 roomOther in rooms)
        {
            if (room.Intersects(roomOther))
            {
                isIntersecting = true;
                break;
            }
        }
        return isIntersecting;
    }
}
