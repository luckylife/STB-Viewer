using OpenTK.Mathematics;

namespace STBViewer2Lib
{
    public interface IST_BRIDGE
    {
        public bool HasStbNodes();
        public (Vector3, Vector3) GetMinMax();
        public List<FloorGrid> MakeStories(Vector3 min, Vector3 max);
        public List<FloorGrid> MakeParallelAxes(Vector3 min, Vector3 max);
        public List<FloorGrid> MakeRadialAxes(Vector3 min, Vector3 max);
    }
}
