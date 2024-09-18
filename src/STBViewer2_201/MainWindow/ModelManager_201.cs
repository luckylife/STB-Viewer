using OpenTK.GLControl;
using ST_BRIDGE201;
using STBViewer2_201.ST_BRIDGE201;
using STBViewer2Lib;
using STBViewer2Lib.MainWindow;
using STBViewer2Lib.OpenGL;
using STBViewer2Lib.SettingWindow;

namespace STBViewer2_201.MainWindow
{

    public class ModelManager_201 : AbstractModelManager
    {

        public ModelManager_201(GLControl glControl) : base(glControl)
        {
        }

        // ST_BRIDGEデータからStbNodeを読み込むメソッド
        public override void LoadModelFromSTBridge(IST_BRIDGE istbData, CategorySetting categorySetting, CameraSetting cameraSetting, bool isModelB)
        {
            ST_BRIDGE? stbData = istbData as ST_BRIDGE;
            _glControl.MakeCurrent(); // OpenGLコンテキストをアクティブにする
            modelElements.Clear(); // モデル要素をクリア
            if (stbData.StbModel != null)
            {
                if (stbData.StbModel.StbNodes != null)
                {
                    modelElements.AddRange(stbData.StbModel.StbNodes);
                }
                if (stbData.StbModel.StbMembers != null)
                {
                    if (stbData.StbModel.StbMembers.StbColumns != null)
                    {
                        modelElements.AddRange(stbData.StbModel.StbMembers.StbColumns);
                    }
                    if (stbData.StbModel.StbMembers.StbPosts != null)
                    {
                        modelElements.AddRange(stbData.StbModel.StbMembers.StbPosts);
                    }
                    if (stbData.StbModel.StbMembers.StbGirders != null)
                    {
                        modelElements.AddRange(stbData.StbModel.StbMembers.StbGirders);
                    }
                    if (stbData.StbModel.StbMembers.StbBeams != null)
                    {
                        modelElements.AddRange(stbData.StbModel.StbMembers.StbBeams);
                    }
                    if (stbData.StbModel.StbMembers.StbBraces != null)
                    {
                        modelElements.AddRange(stbData.StbModel.StbMembers.StbBraces);
                    }
                    if (stbData.StbModel.StbMembers.StbSlabs != null)
                    {
                        modelElements.AddRange(stbData.StbModel.StbMembers.StbSlabs);
                    }
                    if (stbData.StbModel.StbMembers.StbWalls != null)
                    {
                        modelElements.AddRange(stbData.StbModel.StbMembers.StbWalls);
                    }
                    if (stbData.StbModel.StbMembers.StbFootings != null)
                    {
                        modelElements.AddRange(stbData.StbModel.StbMembers.StbFootings);
                    }
                    if (stbData.StbModel.StbMembers.StbStripFootings != null)
                    {
                        modelElements.AddRange(stbData.StbModel.StbMembers.StbStripFootings);
                    }
                    if (stbData.StbModel.StbMembers.StbPiles != null)
                    {
                        modelElements.AddRange(stbData.StbModel.StbMembers.StbPiles);
                    }
                    if (stbData.StbModel.StbMembers.StbFoundationColumns != null)
                    {
                        modelElements.AddRange(stbData.StbModel.StbMembers.StbFoundationColumns);
                    }
                    if (stbData.StbModel.StbMembers.StbParapets != null)
                    {
                        modelElements.AddRange(stbData.StbModel.StbMembers.StbParapets);
                    }
                }

                foreach (IModelElement_201 element in modelElements)
                {
                    element.InitilizeModel(stbData, shader);
                    element.SetDetails(stbData);
                }


                ApplySetting(categorySetting, cameraSetting);
                if (!isModelB)
                {
                    AdjustCameraForParallelProjection();
                }
            }
            _glControl.Invalidate();
        }


    }
}