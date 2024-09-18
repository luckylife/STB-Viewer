using OpenTK.GLControl;
using STBViewer2Lib.MainWindow;
using System.Windows.Controls;

namespace STBViewer2_201.MainWindow
{
    public class TreeView3DSet_201
        : AbstractTreeView3DSet
    {
        public TreeView3DSet_201(Grid parentPanel, GLControl _glControl) : base(parentPanel, _glControl)
        {
            // ModelManagerを作成してGLControlを管理
            _modelManager = new ModelManager_201(_glControl);
        }
    }
}
