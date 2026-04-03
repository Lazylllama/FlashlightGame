using System.Collections.Generic;

namespace FlashlightGame {
	public static class Preferences {
		public abstract class Mixer {
			public static float MasterVolume   = 1f;
			public static float MusicVolume    = 1f;
			public static float SfxVolume      = 1f;
			public static float AmbienceVolume = 1f;
			public static float UIVolume       = 1f;
		}

		public abstract class Game {
			public static int TargetFrameRate = 60;
		}

		public abstract class DebugHandler {
			public static DebugLevel   DbgLevel  = DebugLevel.Error;
			public static List<string> LogFilter = new List<string>();
		}
	}
}