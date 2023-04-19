using TagLib;

#if DEBUG
var musicPath = "\\\\storage\\music\\Music\\";
#else
var musicPath = string.Empty;
#endif

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

var logPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

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

var outputLog = Path.Combine(logPath, "Output.txt");
var unsupportedFilesLog = Path.Combine(logPath, "Unsupported.txt");

var songs = new Dictionary<string, Tag>();
var result = new Dictionary<string, Tag>();
var unsupported = new List<string>();

var rootDirs = Directory.GetDirectories(musicPath);

using StreamWriter outputLogWriter = new(outputLog);
using StreamWriter unsupportedFilesLogWriter = new(unsupportedFilesLog);

try {
	RunRecursiveFileAction(musicPath, FindSongs);
	FindDuplicates();
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

void RunRecursiveFileAction(string path, Action<string> action) {
	foreach (var dir in Directory.GetDirectories(path)) {
		// Run action on deepest folder
		RunRecursiveFileAction(dir, action);
		action(dir);
	}
}

void FindSongs(string path) {
	foreach (var file in Directory.GetFiles(path)) {
		try {
			var extension = Path.GetExtension(file).ToLower();

			var extensions = new[] { ".mp3", ".m4a", ".flac" };
			var ignore = new[] { ".jpg", ".png", ".db", ".pdf" };

			if (extensions.Contains(extension)) {
				try {
					var tagFile = TagLib.File.Create(file);

					if (tagFile.Tag != null) {
						songs.Add(file, tagFile.Tag);
					}
				}
				catch (CorruptFileException) { }
				catch (UnsupportedFormatException) { }
			}
			else if (!ignore.Contains(extension)
				&& !unsupported.Contains(file)) {

				unsupported.Add(file);
			}
		}
		catch (ArgumentException) { }
	}
}

void FindDuplicates() {
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