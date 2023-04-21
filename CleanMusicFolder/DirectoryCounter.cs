namespace CleanMusicFolder;
public class DirectoryCounter : RecursiveFolderActor {
	public static async void Run(string path) {
		Console.Write("Total Directories: ");

		var dirCount = 0;
		var updateDirectoryCount = true;

		var updateTask = UpdateDirCountText();
		RunRecursiveFolderAction(path, (_) => { dirCount++; });

		updateDirectoryCount = false;

		updateTask.Wait(); // Ensures the final update has occurred so the number doesn't output after the newline.
		
		Console.Write("\n");

		async Task UpdateDirCountText() {
			while (updateDirectoryCount) {
				await Task.Delay(TimeSpan.FromMilliseconds(500));

				Console.SetCursorPosition(19, Console.CursorTop);
				Console.Write(dirCount);
			}
		}
	}
}
