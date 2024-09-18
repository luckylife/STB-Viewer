using OpenTK.Mathematics;
using STBViewer2Lib;
using STBViewer2Lib.MainWindow;

namespace ST_BRIDGE210
{
    public partial class ST_BRIDGE : IST_BRIDGE
    {
        public bool HasStbNodes()
        {
            return (StbModel?.StbNodes) != null;
        }

        public (Vector3, Vector3) GetMinMax()
        {
            IEnumerable<StbNode> nodes = StbModel.StbNodes;

            // X, Y, Zの最小値と最大値を計算
            double minX = nodes.Min(node => node.X) * AbstractModelManager.ScaleFactor;
            double maxX = nodes.Max(node => node.X) * AbstractModelManager.ScaleFactor;
            double minY = nodes.Min(node => node.Y) * AbstractModelManager.ScaleFactor;
            double maxY = nodes.Max(node => node.Y) * AbstractModelManager.ScaleFactor;
            double minZ = nodes.Min(node => node.Z) * AbstractModelManager.ScaleFactor;
            double maxZ = nodes.Max(node => node.Z) * AbstractModelManager.ScaleFactor;

            return (new Vector3((float)minX, (float)minY, (float)minZ),
            new Vector3((float)maxX, (float)maxY, (float)maxZ));
        }

        public List<FloorGrid> MakeStories(Vector3 min, Vector3 max)
        {
            List<FloorGrid> stories = [];
            if (StbModel?.StbStories != null)
            {
                foreach (StbStory story in StbModel.StbStories)
                {
                    stories.Add(new FloorGrid(story.name, story.height, min, max));
                }
            }
            return stories;
        }

        public List<FloorGrid> MakeParallelAxes(Vector3 min, Vector3 max)
        {
            List<FloorGrid> axes = [];
            if (StbModel?.StbAxes?.StbParallelAxes != null)
            {
                foreach (StbParallelAxes? ax in StbModel.StbAxes.StbParallelAxes)
                {
                    foreach (StbParallelAxis? axis in ax.StbParallelAxis)
                    {
                        axes.Add(new FloorGrid(new Vector2((float)ax.X, (float)ax.Y), ax.angle, axis.name, axis.distance, min, max));
                    }
                }
            }
            return axes;
        }

        public List<FloorGrid> MakeRadialAxes(Vector3 min, Vector3 max)
        {
            List<FloorGrid> axes = [];
            if (StbModel?.StbAxes?.StbRadialAxes != null)
            {
                foreach (StbRadialAxes? ax in StbModel.StbAxes.StbRadialAxes)
                {
                    foreach (StbRadialAxis? axis in ax.StbRadialAxis)
                    {
                        axes.Add(new FloorGrid(new Vector2((float)ax.X, (float)ax.Y), axis.name, axis.angle, min, max));
                    }
                }
            }
            return axes;
        }
    }
}