using TagLib;

var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
var outputFile = Path.Combine(desktopPath, "Output.txt");
var unsupportedFile = Path.Combine(desktopPath, "Unsupported.txt");

var result = new Dictionary<string, Tag>();
var unsupported = new List<string>();

RunRecursiveFileAction("\\\\storage\\music\\Music\\", FindDuplicates);

using StreamWriter outputWriter = new(outputFile);
using StreamWriter unsupportedWriter = new(unsupportedFile);

if (result.Any()) {
	foreach (var item in result) {
		outputWriter.WriteLine($"{item.Key}");
	}
}

if (unsupported.Any()) {
	foreach (var file in unsupported) {
		unsupportedWriter.WriteLine(file);
	}
}

outputWriter.Close();
unsupportedWriter.Close();

void RunRecursiveFileAction(string path, Action<string> action) {
	foreach (var dir in Directory.GetDirectories(path)) {
		// Run action on deepest folder
		RunRecursiveFileAction(dir, action);
		action(dir);
	}
}

void FindDuplicates(string path) {
	var songs = new Dictionary<string, Tag>();

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
			else {
				if (!unsupported.Contains(file)) {
					unsupported.Add(file);
				}
			}
		}
		catch (ArgumentException) { }
	}

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