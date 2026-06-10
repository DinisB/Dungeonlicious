namespace Dungeonlicious.Assets.Script
{
    using System.Collections.Generic;
    using UnityEngine;

    public struct Room
        {
            public RectInt Rect;
            public Vector3 centerTile;
            public Transform root;

            public Room(RectInt rect, Vector3 center, Transform root)
            {
                this.Rect = rect;
                this.centerTile = center;
                this.root = root;
            }
        }
}