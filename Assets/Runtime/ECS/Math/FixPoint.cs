﻿using System;

namespace Tofunaut.TofuECS.Math
{
    public struct FixPoint : IShape
    {
        public FixVector2 Position;

        public FixPoint(FixVector2 position)
        {
            Position = position;
        }

        public FixAABB BoundingBox => new FixAABB(Position, Position);
        
        public bool Contains(FixVector2 point)
        {
            return point == Position;
        }

        public bool Intersects(IShape other)
        {
            return other switch
            {
                FixCircle otherCircle => otherCircle.Contains(Position),
                FixAABB otherAABB => otherAABB.Contains(Position),
                FixPoint otherPoint => Contains(otherPoint.Position),
                _ => throw new NotImplementedException(
                    "FixPoint.Intersects(IShape other) is not implemented for that IShape implementation")
            };
        }
    }
}