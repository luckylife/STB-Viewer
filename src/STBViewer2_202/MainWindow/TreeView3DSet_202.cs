using OpenTK.GLControl;
using STBViewer2Lib.MainWindow;
using System.Windows.Controls;

namespace STBViewer2_202.MainWindow
{
    public class TreeView3DSet_202
        : AbstractTreeView3DSet
    {
        public TreeView3DSet_202(Grid parentPanel, GLControl _glControl) : base(parentPanel, _glControl)
        {
            // ModelManagerを作成してGLControlを管理
            _modelManager = new ModelManager_202(_glControl);
        }
    }
}
