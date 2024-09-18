using OpenTK.GLControl;
using STBViewer2Lib.MainWindow;
using System.Windows.Controls;

namespace STBViewer2_210.MainWindow
{
    public class TreeView3DSet_210
        : AbstractTreeView3DSet
    {
        public TreeView3DSet_210(Grid parentPanel, GLControl _glControl) : base(parentPanel, _glControl)
        {
            // ModelManagerを作成してGLControlを管理
            _modelManager = new ModelManager_210(_glControl);
        }
    }
}
