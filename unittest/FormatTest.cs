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
	internal class FormatTest {		
		public static void TestIt() {
			Hashtable tests = FormatTest.Tests();
			int number = 0;
			
			MPD.Format format = new MPD.Format(@"[%name%: &[%artist% - ]%title%]|%name%|[%artist% - ]%title%|%file%");
			foreach (object key in tests.Keys)
				TestThisOne(format, (MusicInfo)key, ((string[])tests[key])[0], ref number);

			format.MusicInfoFormat = @"[%name:1%: &[%artist:1% - ]%title:1%]|%name:1%|[%artist:1% - ]%title:1%|%file:1%";
			foreach (object key in tests.Keys)
				TestThisOne(format, (MusicInfo)key, ((string[])tests[key])[1], ref number);
		}

		private static void TestThisOne(MPD.Format format, MusicInfo musicinfo, string expected, ref int number) {
			number++;
			string result = format.MusicInfo(musicinfo);
			if (result != expected)
				Console.WriteLine("\tTest {0} failed: expected \"{1}\", got \"{2}\".",
					number, expected, result);
			else
				Console.WriteLine("\tTest {0} passed succesfully.", number);

		}
		
		private static Hashtable Tests() {
			Hashtable result = new Hashtable();
			Hashtable temp = new Hashtable();
			
			temp["file"] = "Filename";
			temp["Artist"] = "Artist";
			temp["Title"] = "Title";
			temp["Album"] = "Album";
			temp["Track"] = "Track";
			temp["Name"] = "Name";
			temp["Time"] = "100"; // 1:40
			temp["ID"] = "0"; // wont be used
			
			// First, with all the possibilites
			result[new MusicInfo(temp, 0)] = new string[] {"Name: Artist - Title", "N...: A... - T..."};
			
			// Second, without artist
			temp.Remove("Artist");
			result[new MusicInfo(temp, 0)] = new string[] {"Name: Title", "N...: T..."};
			
			// Third, with artist but without name
			temp["Artist"] = "Artist";
			temp.Remove("Name");
			result[new MusicInfo(temp, 0)] = new string[] {"Artist - Title", "A... - T..."};
			
			// Fourth, only with the name
			temp = new Hashtable();
			temp["file"] = "Filename";
			temp["Name"] = "Name";
			result[new MusicInfo(temp, 0)] = new string[] {"Name", "N..."};
			
			// And Finally, without anything
			temp.Remove("Name");
			result[new MusicInfo(temp, 0)] = new string[] {"Filename", "F..."};

			return result;
		}
	}
}





