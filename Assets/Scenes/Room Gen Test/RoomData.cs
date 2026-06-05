namespace Dungeonlicious.Assets.Scripts
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public struct RoomData
        {
            public RectInt rect;
            public Vector3 center;
            public Transform root;

            public RoomData(RectInt rect, Vector3 center, Transform root)
            {
                this.rect = rect;
                this.center = center;
                this.root = root;
            }
        }
}