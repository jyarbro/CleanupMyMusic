namespace CleanMusicFolder;
public class RecursiveFolderActor {
	protected static void RunRecursiveFolderAction(string path, Action<string> action) {
		foreach (var dir in Directory.GetDirectories(path)) {
			// Run action on deepest folder before shallowest folder.
			RunRecursiveFolderAction(dir, action);
			action(dir);
		}
	}
}
