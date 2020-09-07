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
    const float FACTOR = 1.0f / 8.0f;
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
            _addRoom(rng, data, rooms, room);
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
    /// dictionary.</summary>
    public void _addRoom(RandomNumberGenerator rng, Dictionary<Vector2, int> data, List<Rect2> rooms, Rect2 room)
    {
        rooms.Add(room);

        // 50% chance to create a rectangular room.
        if (rng.RandiRange(0, 1) == 0)
        {
            for (int x = (int)room.Position.x; x < (int)room.End.x; x++)
            {
                for (int y = (int)room.Position.y; y < (int)room.End.y; y++)
                {
                    data[new Vector2(x, y)] = 0;
                }
            }
        }
        else
        {
            var unit = FACTOR * room.Size;
            Rect2[] order = new Rect2[] {
                // Create a rectangle at the top part of the original room rectangle.
                // This rectangle is two units narrower than the original room Rect2
                // with one unit taken from each side, and one unit high.
                room.GrowIndividual(-unit.x, 0, -unit.x, unit.y - room.Size.y),
                // This one is on the right side of the original room rectangle. It's
                // width is one unit and its height is two units smaller than the originals.
                room.GrowIndividual(unit.x - room.Size.x, -unit.y, 0, -unit.y),
                // This one is at the bottom. Two units narrower and one unit high.
                room.GrowIndividual(-unit.x, unit.y - room.Size.y, -unit.x, 0),
                // This one's at the left side. Two units shorter and one unit wide.
                room.GrowIndividual(0, -unit.y, unit.x - room.Size.x, -unit.y),
            };

            // Stores the Vector2's that make up the organic polygon shape
            List<Vector2> poly = new List<Vector2>(); 
            
            // Loop through the rectangles one by one
            for (int index = 0; index < order.Length ; index++)
            {
                Rect2 rect = order[index];
                // Top and bottom are even, sides are odd
                bool isEven = index % 2 == 0;
                List<Vector2> poly_partial = new List<Vector2>();

                // Create one or two points inside each rectangle
                foreach (int r in Enumerable.Range(0, rng.RandiRange(1, 2)))
                {
                    // Pick a random point inside the rectangle
                    poly_partial.Add(new Vector2(
                        rng.RandfRange(rect.Position.x, rect.End.x),
                        rng.RandfRange(rect.Position.y, rect.End.y)
                    ));
                }
                // Sort the points that get created using LINQ
                if (isEven)
                {
                    poly_partial.Sort((vec1, vec2) => vec1.x.CompareTo(vec2.x));
                }
                else
                {
                    poly_partial.Sort((vec1, vec2) => vec1.y.CompareTo(vec2.y));
                }
                // If two points are created, reverse the order so that the points
                // end up being in clockwise order.
                if (index > 1)
                {
                    poly_partial.Reverse();
                }
                // Add points that get created to the complete polygon
                poly.AddRange(poly_partial);
            }

            // Loop through the room's points
            for (float x = room.Position.x; x < room.End.x; x++)
            {
                for (float y = room.Position.y; y < room.End.y; y++)
                {
                    Vector2 point = new Vector2(x, y);
                    // Check to see if the current point is inside the polygon 
                    // we just created. Store the point in data only if the point
                    // is within the polygon.
                    if (Geometry.IsPointInPolygon(point, poly.ToArray())) {
                        data[point] = 0;
                    }
                }
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
