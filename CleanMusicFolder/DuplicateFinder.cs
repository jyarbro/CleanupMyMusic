using TagLib;

namespace CleanMusicFolder;

public class DuplicateFinder : RecursiveFolderActor {
	List<string> Unsupported => new List<string>();
	List<string> Duplicates => new List<string>();

	string MusicFilesRootPath { get; }
	string OutputLogPath { get; }
	string UnsupportedFilesLogPath { get; }

    public DuplicateFinder(
		string musicPath, 
		string logPath
	) {
		MusicFilesRootPath = musicPath;
		OutputLogPath = Path.Combine(logPath, "Output.txt");
		UnsupportedFilesLogPath = Path.Combine(logPath, "Unsupported.txt");
	}

	public void Run() {
		Console.Write("Completed: ");

		var rootDirs = Directory.GetDirectories(MusicFilesRootPath);

		using StreamWriter outputLogWriter = new(OutputLogPath);
		using StreamWriter unsupportedFilesLogWriter = new(UnsupportedFilesLogPath);

		try {
			RunRecursiveFolderAction(MusicFilesRootPath, FindSongs);
		}
		catch (Exception e) {
			outputLogWriter.WriteLine($"ERROR ERROR ERROR\n{e.Message}");
		}

		if (Duplicates.Any()) {
			foreach (var filePath in Duplicates) {
				outputLogWriter.WriteLine($"{filePath}");
			}
		}

		if (Unsupported.Any()) {
			foreach (var file in Unsupported) {
				unsupportedFilesLogWriter.WriteLine(file);
			}
		}

		outputLogWriter.Close();
		unsupportedFilesLogWriter.Close();
	}

	void FindSongs(string dirPath) {
		var songs = new Dictionary<string, TagLib.File>();

		foreach (var filePath in Directory.GetFiles(dirPath)) {
			try {
				var extension = Path.GetExtension(filePath).ToLower();

				var extensions = new[] { ".mp3", ".m4a", ".flac" };
				var ignore = new[] { ".jpg", ".png", ".db", ".pdf" };

				if (extensions.Contains(extension)) {
					CheckDuplicates(filePath);
				}
				else if (!ignore.Contains(extension)
					&& !Unsupported.Contains(filePath)) {

					Unsupported.Add(filePath);
				}
			}
			catch (ArgumentException) { }
		}

		// I can do stupid stuff like nesting functions in functions because.
		// I think this is all nested in an implied Main function too.
		// It's turtles all the way down.
		void CheckDuplicates(string filePath) {
			try {
				var songFile = TagLib.File.Create(filePath);

				if (songFile.Tag is not null) {
					var duplicates = songs.Where(
						o => o.Key != filePath
						&& o.Value.Tag.Album == songFile.Tag.Album
						&& o.Value.Tag.Title == songFile.Tag.Title
						&& o.Value.Tag.Length == songFile.Tag.Length);

					// TODO replace this conditional with a recommendation to delete the one with worse bitrate.
					if (duplicates.Any()) {
						if (!Duplicates.Contains(filePath)) {
							Duplicates.Add(filePath);
						}

						foreach (var duplicate in duplicates) {
							if (!Duplicates.Contains(duplicate.Key)) {
								Duplicates.Add(duplicate.Key);
							}
						}
					}
				}
			}
			catch (CorruptFileException) { }
			catch (UnsupportedFormatException) { }
		}
	}
}
