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

namespace MPD 
{
	using System;
	using System.Collections;
	
	public delegate void CommandCallback(object result);
	public delegate object InternalCallback(string buffer);
	
	internal sealed class AAAStorage // ArtistAndAlbumsStorage
	{
		public CommandCallback Callback;
		public Hashtable Result = new Hashtable();
		public int CurArtist = 0;
		public string[] Artists;
		
		public AAAStorage(CommandCallback callback) {
			Callback = callback;
		}
	}
	
	internal sealed class Command
	{
		public string Cmdline;
		public bool Sent = false;
		public ReadResultType ReadType;
		public InternalCallback Callback;
		public CommandCallback LastCallback;
		
		public Command(string cmdline, InternalCallback callback, 
					   CommandCallback lastCallback) {
			Cmdline = cmdline;
			Callback = callback;
			LastCallback = lastCallback;
		}
	}
	
	internal enum ReadResultType {
		Normal,
		Block,
		Ignore
	}
	
	public enum SearchType {
		Album,
		Artist,
		Title
	}
	
	public enum StateStatus {
		Unknown,
		Play,
		Pause,
		Stop
	}

	public enum IdleStatus {
		Player,
		Mixer,
		Unknown
	}


	
	public sealed class MusicStatus
	{
		public int Song = -1; // position of the currently selected song in playlist
		public int SongID = -1; // Song ID of the current song
		
		// Only if playing
		public int BitRate = -1; // in kbs
		public uint SampleRate = 0;
		public int Bits = -1;
		public int Channels = -1;
		
		public int TotalTime = -1;
		public int ElapsedTime = -1;
		
		public MusicStatus(Hashtable info)
		{
			if (info.ContainsKey("song")) {
				Song = Convert.ToInt32((string)info["song"]); 
				SongID = Convert.ToInt32((string)info["songid"]);
			}
			if (info.ContainsKey("audio")) {
				BitRate = Convert.ToInt32((string)info["bitrate"]);
				string[] temp = ((string)info["audio"]).Split(':');
				SampleRate = Convert.ToUInt32(temp[0]);
				// Sometimes Bits is not return properly.
				try {
					Bits = Convert.ToInt32(temp[1]);
				} catch {
					Bits = 0;
				}
				Channels = Convert.ToInt32(temp[2]);
				
				temp = ((string)info["time"]).Split(':');
				ElapsedTime = Convert.ToInt32(temp[0]);
				TotalTime = Convert.ToInt32(temp[1]);
			}
		}
	}
	
	public sealed class StatusInfo
	{
		public int Volume; // 0-100 or -1 if there is no volume support
		public int Crossfade;
					 
		public bool Repeat;
		public bool Random;
		
		public int PlaylistVersion;
		public int PlaylistLength;
		
		public StateStatus State;
		public MusicStatus Music;
		public bool UpdatingDB = false; // if mpd is updating its database or not
		
		public StatusInfo(Hashtable info) { 
			Volume = Convert.ToInt32((string)info["volume"]);
			Crossfade = Convert.ToInt32((string)info["xfade"]);
			Repeat = ((string)info["repeat"] == "1");
			Random = ((string)info["random"] == "1");
			PlaylistVersion = Convert.ToInt32((string)info["playlist"]);
			PlaylistLength = Convert.ToInt32((string)info["playlistlength"]);
			Music = new MusicStatus(info);
			if (info.ContainsKey("updating_db"))
				UpdatingDB = ((string)info["updating_db"] != "0");
			switch ((string)info["state"]) {
				case "play":
					State = StateStatus.Play;
					break;
				case "pause":
					State = StateStatus.Pause;
					break;
				case "stop":
					State = StateStatus.Stop;
					break;
				default:
					State = StateStatus.Unknown;
					break;
			}
		}
	}
	
	public sealed class StatsInfo
	{
		public TimeSpan DBPlayTime;
		public DateTime DBUpdateDate;
		public int NumberOfArtists;
		public int NumberOfAlbums;
		public int NumberOfSongs;
		public TimeSpan PlayTime;
		public TimeSpan Uptime;
		
		private static DateTime UnixDateStart = new DateTime(1970,1,1,0,0,0);
		
		public StatsInfo(Hashtable info) { 
			DBPlayTime = new 
				TimeSpan(0, 0, Convert.ToInt32((string)info["db_playtime"]));
			double rawdate = Convert.ToDouble((string)info["db_update"]);
			DBUpdateDate = UnixDateStart.AddSeconds(rawdate);
			NumberOfAlbums = Convert.ToInt32((string)info["albums"]); 
			NumberOfArtists = Convert.ToInt32((string)info["artists"]); 
			NumberOfSongs = Convert.ToInt32((string)info["songs"]);
			PlayTime = new TimeSpan(0, 0, Convert.ToInt32((string)info["playtime"])); 
			Uptime = new TimeSpan(0, 0, Convert.ToInt32((string)info["uptime"])); 
		}  
	}
	
	public sealed class MusicInfo
	{
		public string Filename;
		public string Artist = "";
		public string Title = "";
		public string Album = "";
		public string Track = "";
		public string Name = ""; // p.e. the icyName of the stream
		public int Time = -1; // in seconds, -1 if it doesn't have time
		public int ID = -1;
		public int Pos;
		
		public MusicInfo(Hashtable info): this(info, Convert.ToInt32((string)info["Pos"])) {}
		public MusicInfo(Hashtable info, int pos) {
			Filename = (string)info["file"];
			Pos = pos;
			if (info.ContainsKey("Artist"))
				Artist = (string)info["Artist"];
			if (info.ContainsKey("Title"))
				Title = (string)info["Title"];
			if (info.ContainsKey("Album"))
				Album = (string)info["Album"];
			if (info.ContainsKey("Track"))
				Track = (string)info["Track"];
			if (info.ContainsKey("Name"))
				Name = (string)info["Name"];
			if (info.ContainsKey("Time"))
				Time = Convert.ToInt32((string)info["Time"]);
			if (info.ContainsKey("Id"))
				ID = Convert.ToInt32((string)info["Id"]);
		}
	}
	
	public class MusicCollection
	{
		public MusicInfo[] Songs; // keys are the position and values are MusicInfo's
		
		public MusicCollection(ArrayList songs) { 
			int count = songs.Count;
			Songs = new MusicInfo[count];
			for (int i = 0; i < count; i++) {
				Hashtable song = (Hashtable)songs[i];
				Songs[i] = new MusicInfo(song, Convert.ToInt32(song["INDEX"]));
			}
		}
	}

	// Blatently ripping off StatusInfo - only want one field though
	public sealed class IdleInfo
	{
		public IdleStatus ChangedState;

		public IdleInfo(Hashtable info) {
			switch ((string)info["changed"]) {
				case "player":
				ChangedState = IdleStatus.Player;
				break;
				case "mixer":
				ChangedState = IdleStatus.Mixer;
				break;
				default:
				ChangedState = IdleStatus.Unknown;
				break;
			}
		}
	}
}
