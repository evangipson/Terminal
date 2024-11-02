using Godot;

namespace Terminal.Navigators
{
	public partial class ScreenNavigator : Node
	{
		private Window _root;
		private Node _rootScene;
		private Node _monitorShader;
		private Node _currentScene;

		public override void _Ready()
		{
			_root = GetTree().Root;
			_rootScene = _root.GetChild(_root.GetChildCount() - 1);
			_monitorShader = _rootScene.GetChild(_rootScene.GetChildCount() - 1);
			_currentScene = _monitorShader.GetChild(_monitorShader.GetChildCount() - 1);
		}

		public void GotoScene(string scenePath)
		{
			CallDeferred(MethodName.DeferredGotoScene, scenePath);
		}

		public void DeferredGotoScene(string scenePath)
		{
			_currentScene.Free();
			_currentScene = GD.Load<PackedScene>(scenePath).Instantiate();
			_rootScene.AddChild(_currentScene);
			GetTree().CurrentScene = _currentScene;
		}
	}
}