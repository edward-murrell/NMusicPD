/*  Copyright (C) 2004 Felipe Almeida Lessa
    
    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Collections;
using MPD;

namespace UnitTest
{
	class UnitTest
	{
		public static int Main(string[] args) {
			UnitTest program = new UnitTest();
			
			string host; 
			int port;
			
			if (args.Length == 0) {
				host = "localhost"; 
				port = 6600;
				Console.WriteLine("Warning: no host specified, using localhost.");
				Console.WriteLine("Warning: no port specified, using 6600.");
			}
			else if (args.Length == 1) {
				host = args[0]; 
				port = 6600;
				Console.WriteLine("Warning: no port specified, using 6600.");
			}
			else if (args.Length == 2) {
				host = args[0]; 
				port = Convert.ToInt32(args[1]);
			}
			else
			{
				Console.WriteLine("Too many arguments, sorry.");
				return 1;
			}
			return program.Run(host, port);
		}
		
		public string FormatTime(int time) {
			if (time == -1)
				return "not avaiable";
			else
				return MPD.Format.TimeInSeconds(time);
		}
		
		public string FormatBoolean(bool b) {
			return (b ? "yes" : "no");
		}		

		public void PrintResult(object result) {
			if (result is string[]) {
				string[] response = (string[])result;
				if (response.Length == 0)
					Console.WriteLine("empty list.");
				else {
					Array.Sort(response);
					Console.WriteLine("from \"{0}\" to \"{1}\".", response[0], 
									  response[response.Length-1]);
				}
			}
			else if (result == null)
				Console.WriteLine("got blank response.");
			else if (result is bool)
				Console.WriteLine(FormatBoolean((bool)result));
			else if (result is StatsInfo) {
				StatsInfo stats = (StatsInfo)result;
				Console.WriteLine("\n\tNumber of Artists: {0}" + 
					"\n\tNumber of Albums: {1}\n\tNumber of Songs: {2}" + 
					"\n\tDatabase Play Time: {3}\n\tDatabase Update Date: {4} UTC" + 
					"\n\tPlaylist Play Time: {5}\n\tServer Uptime: {6}",
					Convert.ToString(stats.NumberOfArtists),
					Convert.ToString(stats.NumberOfAlbums),
					Convert.ToString(stats.NumberOfSongs),
					Convert.ToString(stats.DBPlayTime),
					Convert.ToString(stats.DBUpdateDate),
					Convert.ToString(stats.PlayTime),
					Convert.ToString(stats.Uptime));
			}
			else if (result is MusicCollection) {
				MusicInfo[] Songs = ((MusicCollection)result).Songs;
				MPD.Format format = new MPD.Format();
				Console.Write("\n");
				for (int i = 0; i < Songs.Length; i++) {
					MusicInfo info = Songs[i];
					Console.WriteLine("\t{0}) {1} ({2})", i+1, format.MusicInfo(info), FormatTime(info.Time)); 
				}
			}
			else if (result is StatusInfo) {
				StatusInfo status = (StatusInfo)result;
				Console.WriteLine("\n\tVolume: {0}%\n\tCrossfade: {1} seconds" + 
					"\n\tRepeat: {2}\n\tRandom: {3}\n\tState: {4}" + 
					"\n\tUpdating Database: {5}",
					Convert.ToString(status.Volume),
					Convert.ToString(status.Crossfade),
					FormatBoolean(status.Repeat),
					FormatBoolean(status.Random),
					Enum.GetName(typeof(StateStatus), status.State).ToLower(),
					FormatBoolean(status.UpdatingDB));
			}
			else if (result is MusicInfo) {
				MusicInfo info = (MusicInfo)result;
				Console.WriteLine("\n\tFilename: {0}\n\tArtist: {1}\n\tTitle: {2}" +
					"\n\tAlbum: {3}\n\tTrack: {4}\n\tName: {5}\n\tLength: {6}" + 
					"\n\tID: {7}",
					info.Filename,
					info.Artist,
					info.Title,
					info.Album,
					info.Track,
					info.Name,
					FormatTime(info.Time),
					Convert.ToString(info.ID));
			}
			else
				Console.WriteLine("got \"{0}\".", Convert.ToString(result));			
			Console.Write("\n");
		}
		
		public void Read(MPDClient conn, bool printSomething) {
			while (conn.ReadResponse())
				{ }
				
			if (printSomething)
				PrintResult(null);
		}
		
		public int Run(string host, int port) 
		{
			// Vars
			MPDClient conn;
			CommandCallback callback = new CommandCallback(PrintResult);
			bool boolean;
		
			// Connect
			conn = new MPDClient(host, port);
			Console.WriteLine("Connected to MPD version {0}.", conn.MPDVersion);
			
			// Tests ...
			Console.WriteLine("Starting tests (not everything is tested/shown)\n");
			
			/* Our model:
			Console.Write("Testing command \"\"... ");
			conn (callback);
			Wait(conn);	
			*/	
			
			// Can't test Add()
			
			Console.Write("Testing command \"Albums\"... ");
			conn.Albums(callback);
			Read(conn, false);
			
			Console.Write("Testing command \"Artists\"... ");
			conn.Artists(callback);
			Read(conn, false);
			
			// TODO: Test ArtistsAndAlbums()
			
			// Can't test Clear() and Crossfade()
			
			Console.Write("Testing command \"CurrentSong\"... ");
			conn.CurrentSong(callback);
			Read(conn, false);
			
			// TODO: Test ListAll()
			
			Console.Write("Testing command \"ListPlaylists\"... ");
			conn.ListPlaylists(callback);
			Read(conn, false);

			// Can't test LoadPlaylist(), Move(), Next(), Password() and Paused()
			
			Console.Write("Testing command \"Ping\"... ");
			conn.Ping();
			Read(conn, true);
			
			// Can't test Play()
			
			Console.Write("Testing command \"Playlist\"... ");
			conn.Playlist(callback);
			Read(conn, false);
			
			// TODO: Test PlaylistChanges(), PlaylistSongInfo()
			
			// Can't test Prev(), Random(), RemovePlaylist(), RemoveSong(), Repeat(), SavePlaylist()
			
			// TODO: Test Search()
			
			// Can't test SeekTo(), Shuffle(), Shutdown()
			
			Console.Write("Testing command \"Statistics\"... ");
			conn.Statistics(callback);
			Read(conn, false);
			
			Console.Write("Testing command \"Status\"... ");
			conn.Status(callback);
			Read(conn, false);

			// Can't test Stop(), Swap(), Volume()
			
			Console.WriteLine("Trying to send a block with 5 \"Status\" commands...");
			for (int i = 0; i < 5; i++)
				conn.Status(callback);
			Read(conn, false);

			Console.Write("Trying to send a block with 300 \"Ping\" commands... ");
			for (int i = 0; i < 300; i++)
				conn.Ping();
			Read(conn, true);
			
			Console.WriteLine("Testing MPD.Format.MusicInfo() function...");
			FormatTest.TestIt();
			Console.Write("\n");
			
			// ... done, bye 
			Console.Write("Disconnecting from server... ");
			conn.Disconnect();
			Console.WriteLine("bye!");	
			return 0;
			
		}
	}
}
