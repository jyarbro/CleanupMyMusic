// This is written more like a shell script than a .NET app. What on earth am I doing?

using TagLib;

#if DEBUG
var musicPath = "\\\\storage\\music\\Music\\";
var logPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
#else
var musicPath = string.Empty;
var logPath = string.Empty;
#endif

try {
	LoadArgs();
}
catch {
	return;
}

var outputLog = Path.Combine(logPath, "Output.txt");
var unsupportedFilesLog = Path.Combine(logPath, "Unsupported.txt");

var result = new Dictionary<string, Tag>();
var unsupported = new List<string>();

var rootDirs = Directory.GetDirectories(musicPath);

using StreamWriter outputLogWriter = new(outputLog);
using StreamWriter unsupportedFilesLogWriter = new(unsupportedFilesLog);

try {
	RunRecursiveFolderAction(musicPath, FindSongs);
}
catch (Exception e) {
	outputLogWriter.WriteLine($"ERROR ERROR ERROR\n{e.Message}");
}

if (result.Any()) {
	foreach (var item in result) {
		outputLogWriter.WriteLine($"{item.Key}");
	}
}

if (unsupported.Any()) {
	foreach (var file in unsupported) {
		unsupportedFilesLogWriter.WriteLine(file);
	}
}

outputLogWriter.Close();
unsupportedFilesLogWriter.Close();

void LoadArgs() {
	if (args.Length > 0 && !string.IsNullOrEmpty(args[0])) {
		musicPath = args[0];
	}

	try {
		musicPath = Path.GetFullPath(musicPath);
	}
	catch (Exception e) {
		Console.WriteLine($"Music path error: {e.Message}");
		return;
	}

	if (args.Length > 1 && !string.IsNullOrEmpty(args[1])) {
		logPath = args[1];
	}

	try {
		logPath = Path.GetFullPath(logPath);
	}
	catch (Exception e) {
		Console.WriteLine($"Log path error: {e.Message}");
		return;
	}
}

void RunRecursiveFolderAction(string path, Action<string> action) {
	foreach (var dir in Directory.GetDirectories(path)) {
		// Run action on deepest folder before shallowest folder.
		RunRecursiveFolderAction(dir, action);
		action(dir);
	}
}

void FindSongs(string dirPath) {
	var songs = new Dictionary<string, Tag>();
	
	foreach (var filePath in Directory.GetFiles(dirPath)) {
		try {
			var extension = Path.GetExtension(filePath).ToLower();

			var extensions = new[] { ".mp3", ".m4a", ".flac" };
			var ignore = new[] { ".jpg", ".png", ".db", ".pdf" };

			if (extensions.Contains(extension)) {
				try {
					var taggedSongFile = TagLib.File.Create(filePath);

					if (taggedSongFile.Tag != null) {
						songs.Add(filePath, taggedSongFile.Tag);
					}
				}
				catch (CorruptFileException) { }
				catch (UnsupportedFormatException) { }
			}
			else if (!ignore.Contains(extension)
				&& !unsupported.Contains(filePath)) {

				unsupported.Add(filePath);
			}
		}
		catch (ArgumentException) { }
	}

	FindDuplicateSongs(songs);
}

void FindDuplicateSongs(Dictionary<string, Tag> songs) {
	var result = new Dictionary<string, Tag>();

	foreach (var kvp in songs) {
		var duplicates = songs.Where(o => o.Key != kvp.Key && o.Value.Album == kvp.Value.Album && o.Value.Title == kvp.Value.Title);

		if (duplicates.Any()) {
			try {
				if (!result.ContainsKey(kvp.Key)) {
					result.Add(kvp.Key, kvp.Value);
				}

				foreach (var duplicate in duplicates) {
					if (!result.ContainsKey(duplicate.Key)) {
						result.Add(duplicate.Key, duplicate.Value);
					}
				}
			}
			catch { }
		}
	}
}