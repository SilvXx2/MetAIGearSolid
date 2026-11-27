using UnityEngine;

namespace Pathfinding
{
    public readonly struct GridBoundsFitResult
    {
        public readonly Vector3 Origin;
        public readonly int Width;
        public readonly int Height;
        public readonly bool Success;

        public GridBoundsFitResult(Vector3 origin, int width, int height, bool success)
        {
            Origin = origin;
            Width = width;
            Height = height;
            Success = success;
        }
    }

    public interface IGridBoundsFitter
    {
        GridBoundsFitResult ComputeFit(float cellDistance, float nodeHeightY, LayerMask wallMask, float insetPadding);
    }
}
