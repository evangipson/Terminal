using Godot;

using Terminal.Constants;

namespace Terminal.Navigators
{
    /// <summary>
    /// A global singleton that is responsible for switching between Godot screens.
    /// </summary>
    public partial class ScreenNavigator : Node
    {
        private Node _rootScene;
        private ColorRect _monitorShader;
        private CanvasLayer _currentScene;

        public override void _Ready()
        {
            _rootScene = GetTree().Root?.GetChild(GetTree().Root.GetChildCount() - 1);
            _monitorShader = _rootScene?.GetNode<ColorRect>("Monitor");
            _currentScene = _monitorShader?.GetNode<CanvasLayer>(ScenePathConstants.IntroScenePath);
        }

        /// <summary>
        /// Navigates to the scene with the provided <paramref name="scenePath"/> as it's path.
        /// </summary>
        /// <param name="scenePath">
        /// The path of the new scene to navigate to.
        /// </param>
        public void GotoScene(string scenePath)
        {
            CallDeferred(MethodName.DeferredGotoScene, scenePath);
        }

        private void DeferredGotoScene(string scenePath)
        {
            _currentScene.Free();
            _currentScene = _monitorShader.GetNode<CanvasLayer>(scenePath);
            _currentScene.Visible = true;
        }
    }
}