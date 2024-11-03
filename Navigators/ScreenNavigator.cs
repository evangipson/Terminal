using Godot;

using Terminal.Constants;

namespace Terminal.Navigators
{
	public partial class ScreenNavigator : Node
	{
		private Node _rootScene;
		private Node _monitorShader;
		private CanvasLayer _currentScene;

		public override void _Ready()
		{
			_rootScene = GetTree().Root?.GetChild(GetTree().Root.GetChildCount() - 1);
			_monitorShader = _rootScene?.GetChild(_rootScene.GetChildCount() - 1);
			_currentScene = _monitorShader?.GetNode<CanvasLayer>(ScreenConstants.IntroScenePath);
		}

		public void GotoScene(string scenePath)
		{
			CallDeferred(MethodName.DeferredGotoScene, scenePath);
		}

		public void DeferredGotoScene(string scenePath)
		{
			_currentScene.Free();
			_currentScene = _monitorShader.GetNode<CanvasLayer>(scenePath);
			_currentScene.Visible = true;
		}
	}
}