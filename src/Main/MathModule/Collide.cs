namespace Monod.MathModule;

//this file is vibecoded because collisions are some algorithms written in 500 pages long books i'm too lazy to read. sorry not sorry.
public static class Collide
{
    public static bool CheckAABBToRotatedRectangle(AABB aabb, OrientedRect obb, out Vector2 mtv)
    {
        mtv = Vector2.Zero;
        var obbHalfSize = new Vector2(obb.Width * 0.5f, obb.Height * 0.5f);


        float cos = MathF.Cos(obb.Angle);
        float sin = MathF.Sin(obb.Angle);
        float absCos = MathF.Abs(cos);
        float absSin = MathF.Abs(sin);

        var centerDiff = new Vector2(aabb.CenterX - obb.Center.X, aabb.CenterY - obb.Center.Y);

        float minOverlap = float.MaxValue;
        var smallestAxis = Vector2.Zero;

        // global axis X
        float obbProjX = (obbHalfSize.X * absCos) + (obbHalfSize.Y * absSin);
        float overlapX = aabb.HalfWidth + obbProjX - MathF.Abs(centerDiff.X);
        if (overlapX <= 0) return false;

        minOverlap = overlapX;
        smallestAxis = Vector2.UnitX;

        // global axis Y
        float obbProjY = (obbHalfSize.X * absSin) + (obbHalfSize.Y * absCos);
        float overlapY = aabb.HalfHeight + obbProjY - MathF.Abs(centerDiff.Y);
        if (overlapY <= 0) return false;

        if (overlapY < minOverlap)
        {
            minOverlap = overlapY;
            smallestAxis = Vector2.UnitY;
        }

        // local axes
        var obbAxisX = new Vector2(cos, sin);
        var obbAxisY = new Vector2(-sin, cos);

        // local axis X
        float distObbX = MathF.Abs((centerDiff.X * cos) + (centerDiff.Y * sin));
        float aabbProjObbX = (aabb.HalfWidth * absCos) + (aabb.HalfHeight * absSin);
        float overlapObbX = aabbProjObbX + obbHalfSize.X - distObbX;
        if (overlapObbX <= 0) return false;

        if (overlapObbX < minOverlap)
        {
            minOverlap = overlapObbX;
            smallestAxis = obbAxisX;
        }

        // locla axis Y
        float distObbY = MathF.Abs((centerDiff.X * -sin) + (centerDiff.Y * cos));
        float aabbProjObbY = (aabb.HalfWidth * absSin) + (aabb.HalfHeight * absCos);
        float overlapObbY = aabbProjObbY + obbHalfSize.Y - distObbY;
        if (overlapObbY <= 0) return false;

        if (overlapObbY < minOverlap)
        {
            minOverlap = overlapObbY;
            smallestAxis = obbAxisY;
        }

        // determine mtv direction
        if (Vector2.Dot(centerDiff, smallestAxis) < 0)
            smallestAxis = -smallestAxis;

        mtv = smallestAxis * minOverlap;
        return true;
    }

    /// <summary>
    /// Checks whether two <see cref="AABB"/> intersect and returns minimal traversal vector for <paramref name="a"/>.
    /// </summary>
    /// <param name="a">AABB for which mtv is calculated.</param>
    /// <param name="b">Other AABB.</param>
    /// <param name="mtv">Minimal travelsal vector for <paramref name="a"/>.</param>
    /// <returns>Whether <paramref name="a"/> and <paramref name="b"/> intersect.</returns>
    public static bool CheckAABBToAABB(ref AABB a, ref AABB b, out Vector2 mtv)
    {
        mtv = Vector2.Zero;

        float distX = a.CenterX - b.CenterX;
        float distY = a.CenterY - b.CenterY;

        float overlapX = a.HalfWidth + b.HalfWidth - Math.Abs(distX);
        float overlapY = a.HalfHeight + b.HalfHeight - Math.Abs(distY);

        // SAT
        if (overlapX <= 0 || overlapY <= 0)
            return false;

        if (overlapX < overlapY)
        {
            float sign = Math.Sign(distX);
            if (sign == 0) sign = 1f; //push right if distX is 0, i.e. centers have same X
            mtv = new Vector2(overlapX * sign, 0);
        }
        else
        {
            float sign = Math.Sign(distY);
            if (sign == 0) sign = -1f; //push up if distY is 0, i.e. centers have same Y
            mtv = new Vector2(0, overlapY * sign);
        }

        return true;
    }

    /// <summary>
    /// Check whether two <see cref="AABB"/> intersect.
    /// </summary>
    /// <returns>Whether two <see cref="AABB"/> intersect.</returns>
    public static bool QuickCheckAABBToAABB(ref AABB a, ref AABB b)
    {
        return (a.HalfWidth + b.HalfWidth) >= Math.Abs(a.CenterX - b.CenterX) && (a.HalfHeight + b.HalfHeight) >= Math.Abs(a.CenterY - b.CenterY);
    }

    /// <summary>
    /// Compute the exact time of intersection and surface normal between a moving AABB and a static AABB using the Swept AABB algorithm.
    /// </summary>
    /// <param name="a">The moving AABB.</param>
    /// <param name="b">The static AABB acting as the obstacle.</param>
    /// <param name="movement">The displacement vector (movement) of AABB 'a' for the current frame.</param>
    /// <param name="time">Outputs the normalized time of collision [0.0, 1.0] if a hit occurs; otherwise, 1.0f.</param>
    /// <param name="normal">Outputs the collision normal vector pointing outward from AABB 'b'.</param>
    /// <returns>True if a collision occurs within the current frame's time step; otherwise, false.</returns>
    public static bool SweptCheck(ref AABB a, ref AABB b, ref Vector2 movement, out float time, out Vector2 normal)
    {
        float invEntryX, invEntryY;
        float invExitX, invExitY;

        // 1. Find the distance between the objects on the near and far sides for both axes.
        if (movement.X > 0.0f)
        {
            invEntryX = b.Left - a.Right;
            invExitX = b.Right - a.Left;
        }
        else
        {
            invEntryX = b.Right - a.Left;
            invExitX = b.Left - a.Right;
        }

        if (movement.Y > 0.0f)
        {
            invEntryY = b.Top - a.Bottom;
            invExitY = b.Bottom - a.Top;
        }
        else
        {
            invEntryY = b.Bottom - a.Top;
            invExitY = b.Top - a.Bottom;
        }

        float xEntry, yEntry;
        float xExit, yExit;

        // 2. Calculate time of collision and time of leaving for each axis.
        if (Math.Abs(movement.X) < MathM.Epsilon)
        {
            // Minkowski difference: if there is no velocity on this axis, they must already be overlapping to collide.
            if (a.Right <= b.Left || a.Left >= b.Right)
            {
                xEntry = float.PositiveInfinity;
                xExit = float.PositiveInfinity;
            }
            else
            {
                xEntry = float.NegativeInfinity;
                xExit = float.PositiveInfinity;
            }
        }
        else
        {
            xEntry = invEntryX / movement.X;
            xExit = invExitX / movement.X;
        }

        if (Math.Abs(movement.Y) < MathM.Epsilon)
        {
            if (a.Bottom <= b.Top || a.Top >= b.Bottom)
            {
                yEntry = float.PositiveInfinity;
                yExit = float.PositiveInfinity;
            }
            else
            {
                yEntry = float.NegativeInfinity;
                yExit = float.PositiveInfinity;
            }
        }
        else
        {
            yEntry = invEntryY / movement.Y;
            yExit = invExitY / movement.Y;
        }

        // find the earliest/latest times of collision across all axes.
        float entryTime = Math.Max(xEntry, yEntry);
        float exitTime = Math.Min(xExit, yExit);

        // a collision occurs if the entry time is before the exit time,
        // and the entry time is within the bounds of the current frame [0.0, 1.0].
        // entryTime < 0.0f ignores objects that are already intersecting.
        if (MathF.Abs(entryTime - exitTime) < MathM.Epsilon || entryTime < 0f || entryTime > 1.0f)
        {
            //return 1 because that means object can fully move (no collision prevents movement).
            time = 1.0f;
            normal = Vector2.Zero;
            return false;
        }

        // determine collision normal based on which axis collided first.
        if (xEntry > yEntry)
            normal = movement.X > 0.0f ? new Vector2(-1.0f, 0.0f) : new Vector2(1.0f, 0.0f);
        else
            normal = movement.Y > 0.0f ? new Vector2(0.0f, -1.0f) : new Vector2(0.0f, 1.0f);

        time = entryTime;
        return true;
    }

    public static bool IsPointInsideRect(Vector2 point, OrientedRect rect) => IsPointInsideRect(point, rect.Center, rect.Angle, rect.Width, rect.Height);

    public static bool IsPointInsideRect(Vector2 point, Vector2 center, float rotation, float width, float height)
    {
        // Transform the point into the rectangle's local space.
        var local = point - center;

        float cos = MathF.Cos(-rotation);
        float sin = MathF.Sin(-rotation);

        Vector2 rotated = new(
            (local.X * cos) - (local.Y * sin),
            (local.X * sin) + (local.Y * cos)
        );

        return MathF.Abs(rotated.X) <= width * 0.5f &&
               MathF.Abs(rotated.Y) <= height * 0.5f;
    }
}
