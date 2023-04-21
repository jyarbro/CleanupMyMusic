// This is written more like a shell script than a .NET app. What on earth am I doing?

using CleanMusicFolder;
using TagLib;

#if DEBUG
var musicPath = "\\\\storage\\music\\Music\\";
var logPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
#else
var musicPath = string.Empty;
var logPath = string.Empty;
#endif

LoadArgs();

DirectoryCounter.Run(musicPath);

var duplicateFinder = new DuplicateFinder(musicPath, logPath);
duplicateFinder.Run();

void LoadArgs() {
	if (args.Length > 0 && !string.IsNullOrEmpty(args[0])) {
		musicPath = args[0];
	}

	try {
		musicPath = Path.GetFullPath(musicPath);

		if (string.IsNullOrEmpty(musicPath)) {
			throw new ArgumentNullException(musicPath);
		}
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

		if (string.IsNullOrEmpty(logPath)) {
			throw new ArgumentNullException(logPath);
		}
	}
	catch (Exception e) {
		Console.WriteLine($"Log path error: {e.Message}");
		return;
	}
}
