	using System.Collections.Generic;

namespace FlashlightGame {
	public static class Preferences {
		public abstract class Game {
			public static int TargetFrameRate = 60;
		}
		
		public abstract class DebugHandler {
			public static DebugLevel   DbgLevel        = DebugLevel.Error;
			public static List<string> LogFilter       = new List<string>();
		}
	}
}