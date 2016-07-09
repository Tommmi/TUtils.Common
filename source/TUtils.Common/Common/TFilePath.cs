using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TUtils.Common.Extensions;
using TUtils.Common.Logging;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace TUtils.Common.Common
{
	/// <summary>
	/// Represents a file path.
	/// Splits a file path into 
	/// <![CDATA[
	/// <ParentDirectory>\<FileBaseName>[<FileSuffix>][.FileExtension1][.FileExtension2]
	/// ]]>
	/// Provides some static helper methods for file and folder manipulation
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public class TFilePath
	{
		#region private

		#region private static

		private static T CatchAllExceptions<T>(bool throwException, Func<T> func, T resultOnException)
		{
			if (throwException)
			{
				return func();
			}
			else
			{
				try
				{
					return func();
				}
				catch
				{
					return resultOnException;
				}
			}
		}

		private static bool CatchAllException(bool throwException, Func<bool> func)
		{
			return CatchAllExceptions(throwException, func, false);
		}

		private static void DeleteDirectoryInternal(string dirPath)
		{
			MakeWritable(false, dirPath);
			Do4EachFileAndDirectory(dirPath, MakeWritable, false);
			Directory.Delete(dirPath, true);
		}

		#endregion

		#endregion

		#region public

		// ReSharper disable once ClassNeverInstantiated.Global
		public class Context
		{
			public ITLog Logger { get; }

			public Context(
				ITLog logger)
			{
				Logger = logger;
			}
		}

		#region public static

		/// <summary>
		/// ensures that given path ends with "\"
		/// </summary>
		/// <param name="dirPath"></param>
		/// <returns></returns>
		public static string AppendSeperator(string dirPath)
		{
			if (!dirPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				dirPath += Path.DirectorySeparatorChar;
			}
			return dirPath;
		}

		/// <summary>
		/// Copies the file "sourcePath" to destination file "sdestPath".
		/// If "overWrite" == true, overwrites destination, if it exists allready.
		/// Overwrites destination even if it is write-protected !
		/// </summary>
		/// <param name="sourcePath">path to source file</param>
		/// <param name="destPath">
		/// 
		/// </param>
		/// <param name="overWrite"></param>
		/// <param name="ignoreWriteProtection"></param>
		public static void CopyFile(string sourcePath, string destPath, bool overWrite, bool ignoreWriteProtection)
		{
			// ReSharper disable once IntroduceOptionalParameters.Global
			CopyFile(sourcePath, destPath, overWrite, ignoreWriteProtection, ignoreMissingSource:false);
		}

		/// <summary>
		/// Copies the file "sourcePath" to destination file "sdestPath".
		/// If "overWrite" == true, overwrites destination, if it exists allready.
		/// Overwrites destination even if it is write-protected !
		/// </summary>
		/// <param name="sourcePath">path to source file</param>
		/// <param name="destPath">
		/// 
		/// </param>
		/// <param name="overWrite"></param>
		/// <param name="ignoreWriteProtection"></param>
		/// <param name="ignoreMissingSource"></param>
		// ReSharper disable once MemberCanBePrivate.Global
		public static void CopyFile(string sourcePath, string destPath, bool overWrite, bool ignoreWriteProtection, bool ignoreMissingSource)
		{
			try
			{

				if (!File.Exists(sourcePath))
				{
					// Source directory is missing...
					if (ignoreMissingSource)
					{
						// leave and ignore...
						return;
					}
					else
					{
						// throw exception...
						throw new FileNotFoundException($"Source file not found: {sourcePath}");
					}
				}

				if (File.Exists(destPath) && overWrite)
					DeleteFile(destPath);

				File.Copy(sourcePath, destPath);

			}
			catch(Exception e)
			{
				if (ignoreWriteProtection)
				{
					var fileInfo = new FileInfo(destPath);
					// delete read only flag
					fileInfo.Attributes = fileInfo.Attributes & ~FileAttributes.ReadOnly;
					CopyFile(sourcePath, destPath, overWrite, false, ignoreMissingSource);
				}
				else
				{
					throw new ApplicationException(e.Message, e);
				}
			}
		}

		/// <summary>
		/// Copies the content of the folder sourceDir to the folder destDir.
		/// Ensures that destination folder exists.
		/// </summary>
		/// <param name="sourceDir">path to source folder (with or without terminationg "\"</param>
		/// <param name="destDir">
		/// This directory will be created, if it doesn't exist
		/// </param>
		/// <param name="overWrite"></param>
		/// <param name="ignoreWriteProtection"></param>
		public static void CopyDirectory(string sourceDir, string destDir, bool overWrite, bool ignoreWriteProtection)
		{
			// ReSharper disable once IntroduceOptionalParameters.Global
			CopyDirectory(sourceDir, destDir, overWrite, ignoreWriteProtection, ignoreMissingSource:false);
		}

		/// <summary>
		/// Copies the content of the folder sourceDir to the folder destDir.
		/// Ensures that destination folder exists.
		/// </summary>
		/// <param name="sourceDir">path to source folder (with or without terminationg "\"</param>
		/// <param name="destDir">
		/// This directory will be created, if it doesn't exist
		/// </param>
		/// <param name="overWrite"></param>
		/// <param name="ignoreWriteProtection"></param>
		/// <param name="ignoreMissingSource"></param>
		public static void CopyDirectory(string sourceDir, string destDir, bool overWrite, bool ignoreWriteProtection, bool ignoreMissingSource)
		{
			if (!Directory.Exists(destDir))
				Directory.CreateDirectory(destDir);
			RecursiveCopyFiles(sourceDir, destDir, true, overWrite, ignoreWriteProtection, ignoreMissingSource);
		}

		public static string Combine(string path1, string path2)
		{
			return path1.Combine(path2);
		}

		/// <summary>
		/// Compares to normalized paths, containing wildcards.
		/// </summary>
		/// <remarks>
		/// \section1\section2.extension (allowed)
		/// \section1\section2 (allowed)
		/// \section1\*.extension (not allowed)
		/// \section1\* (allowed)
		/// \*\section1\section2\ (allowed)
		/// *\section1\section2\ (allowed)
		/// </remarks>
		/// <param name="path1"></param>
		/// <param name="path2"></param>
		/// <returns></returns>
		public static PathCompareResult ComparePaths(string path1, string path2)
		{
			bool path1MustBeSubset = false;
			bool path2MustBeSubset = false;

			var splitsPath1 = path1.Split('\\').Where(split => !split.IsNullOrEmpty()).ToList();
			var splitsPath2 = path2.Split('\\').Where(split => !split.IsNullOrEmpty()).ToList();

			int count1 = splitsPath1.Count;
			int count2 = splitsPath2.Count;

			if ( count1 > count2 )
				path1MustBeSubset = true;
			else if (count1 < count2)
				path2MustBeSubset = true;

			for (int i = 0; i < count1 && i < count2; i++)
			{
				if (splitsPath1[i] != splitsPath2[i])
				{
					if (splitsPath1[i] == "*")
					{
						path2MustBeSubset = true;
					}
					else if (splitsPath2[i] == "*")
					{
						path1MustBeSubset = true;
					}
					else
					{
						// paths are different
						return PathCompareResult.Else;
					}
				}
			}

			if (path1MustBeSubset && path2MustBeSubset)
				return PathCompareResult.Else;

			if (path1MustBeSubset)
				return PathCompareResult.Path1IsSubset;
			else if (path2MustBeSubset)
				return PathCompareResult.Path2IsSubset;
			else
				return PathCompareResult.Equal;
		}

		/// <summary>
		/// Deletes given file
		/// Removes write protection from file, if neccessary.
		/// </summary>
		public static void DeleteFile(string filePath)
		{
			try
			{
				File.Delete(filePath);
			}
			catch(UnauthorizedAccessException)
			{
				MakeWritable(true, filePath);
				File.Delete(filePath);
			}
		}

		/// <summary>
		/// calls given delegate method for each subfile und subfolder.
		/// </summary>
		/// <param name="dirPath">path to parent folder</param>
		/// <param name="handler"></param>
		/// <param name="topLevelFilesFirst">
		/// false: begin with the leafs of the folder tree first
		/// </param>
		/// <returns>false, if iteration was canceled when handler method returned false</returns>
		public static bool Do4EachFileAndDirectory(
			string dirPath,
			OnFileOrDirectoryMethod handler,
			bool topLevelFilesFirst
			)
		{
			for (var step = 0; step < 2; step++)
			{
				if (topLevelFilesFirst && step == 0 || !topLevelFilesFirst && step == 1)
				{
					string[] filePaths = Directory.GetFiles(dirPath);
					if (filePaths.Any(filePath => !handler(true, filePath)))
					{
						return false;
					}
				}
				else
				{
					string[] subDirs = Directory.GetDirectories(dirPath);
					foreach (string subDir in subDirs)
					{
						if (topLevelFilesFirst)
						{
							if (!handler(false, AppendSeperator(subDir)))
								return false;
							if (!Do4EachFileAndDirectory(subDir, handler, topLevelFilesFirst:true))
								return false;
						}
						else
						{
							if (!Do4EachFileAndDirectory(subDir, handler, topLevelFilesFirst:false))
								return false;
							if (!handler(false, AppendSeperator(subDir)))
								return false;
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Deletes given folder recursively.
		/// Removes write protection from files, if neccessary.
		/// </summary>
		/// <param name="dirPath"></param>
		public static void DeleteDirectory(string dirPath)
		{
			try
			{
				Directory.Delete(dirPath, true);
			}
			catch (UnauthorizedAccessException)
			{
				DeleteDirectoryInternal(dirPath);
			}
			catch (IOException)
			{
				DeleteDirectoryInternal(dirPath);
			}
		}

		/// <summary>
		/// ensures that given folder path exists
		/// </summary>
		/// <param name="folderPath"></param>
		/// <param name="throwException">false, if all exceptions should be catched</param>
		/// <returns>true, if succeeded</returns>
		public static bool EnsureFolder(string folderPath, bool throwException)
		{
			return CatchAllException(throwException, () =>
				                                         {
					                                         Directory.CreateDirectory(folderPath);
					                                         return true;
				                                         });
		}

		public static void EmptyFolder(string folderPath)
		{
			Do4EachFileAndDirectory(
				folderPath,
				(isFile, path) =>
					{
						MakeWritable(isFile, path);
					
						if (isFile)
							DeleteFile(path);
						else
							DeleteDirectory(path);

						return true;
					},
				false);

		}

		/// <summary>
		/// looks for a given file path (e.g. "\a\b\c\zzz.rrr")
		/// a file which doesn't exist yet ("\a\b\c\zzz1.rrr" or "\a\b\c\zzz2.rrr", ...).
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static string FindFreeFileName(string filePath)
		{
			TFilePath tFilePath = new TFilePath(filePath);
			int suffix = 1;
			while (File.Exists(tFilePath.FilePath))
			{
				tFilePath.FileSuffix = suffix.ToString();
				suffix++;
			}
			return tFilePath.FilePath;
		}

		/// <summary>
		/// returns all subfiles, which meet given condition
		/// </summary>
		/// <param name="folderPath"></param>
		/// <param name="condition">
		/// bool condition(string filePath)
		/// </param>
		/// <returns></returns>
		public static IEnumerable<string> FindFilesInFolder(string folderPath, Func<string,bool> condition)
		{
			if ( !Directory.Exists(folderPath))
				yield break;
			foreach (var filepath in Directory.GetFiles(folderPath).ToList())
				if (condition(filepath))
					yield return filepath;
		}

		/// <summary>
		/// looks for a given folder path (e.g. "\a\b\c\zzz")
		/// a folder name which doesn't exist yet ("\a\b\c\zzz1" or "\a\b\c\zzz2", ...).
		/// </summary>
		/// <param name="folderPath"></param>
		/// <returns></returns>
		public static string FindFreeFolderName(string folderPath)
		{
			TFilePath tFilePath = new TFilePath(folderPath);
			int suffix = new Random().Next(1000);
			while (Directory.Exists(tFilePath.FilePath))
			{
				tFilePath.FileSuffix = suffix.ToString();
				suffix++;
			}
			return tFilePath.FilePath;
		}


		/// <summary>
		/// converts file path to a normalized comparable full path.<br/>
		/// "\A\b\C.txt" ==> "\a\b\c.txt"<br/>
		/// "\A\b\..\B\c.txt" ==> "\a\b\c.txt"<br/>
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static string NormalizeFilePath(string filePath)
		{
			return Path.GetFullPath(filePath).ToLower();
		}

		/// <summary>
		/// converts folder path to a normalized comparable full path.<br/>
		/// "\A\b\C"      ==> "\a\b\c"<br/>
		/// "\A\b\C\"     ==> "\a\b\c"<br/>
		/// "\A\b\..\B\c" ==> "\a\b\c"<br/>
		/// </summary>
		/// <param name="folderPath"></param>
		/// <returns></returns>
		public static string NormalizeFolderPath(string folderPath)
		{
			folderPath = NormalizeFilePath(folderPath);
			int len = folderPath.Length;
			if (folderPath.Substring(len - 1, 1) == "\\")
				folderPath = folderPath.Substring(0, len - 1);
			return folderPath;
		}

		/// <summary>
		/// makes given folder or file writable.
		/// </summary>
		/// <param name="isFile"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool MakeWritable(bool isFile, string path)
		{
			if (isFile)
			{
				if (File.Exists(path))
				{
					// ReSharper disable once UseObjectOrCollectionInitializer
					var fileInfo = new FileInfo(path);
					fileInfo.Attributes = FileAttributes.Normal;
				}
			}
			else
			{
				if (Directory.Exists(path))
				{
					// ReSharper disable once UseObjectOrCollectionInitializer
					var info = new DirectoryInfo(path);
					info.Attributes = FileAttributes.Normal;
				}
			}
			return true;
		}

		/// <summary>
		/// Copies the content of the given folder sourceDir to the given folder destDir.
		/// </summary>
		/// <param name="sourceDir"></param>
		/// <param name="destDir"></param>
		/// <param name="fRecursive"></param>
		/// <param name="overWrite"></param>
		/// <param name="ignoreWriteProtection"></param>
		/// <param name="ignoreMissingSource"></param>
		// ReSharper disable once MemberCanBePrivate.Global
		public static void RecursiveCopyFiles(string sourceDir, string destDir, bool fRecursive, bool overWrite, bool ignoreWriteProtection, bool ignoreMissingSource)
		{
			int i;
			int posSep;

			/*Add trailing separators to the supplied paths if they don't exist.*/
			if (!sourceDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				sourceDir += Path.DirectorySeparatorChar;
			}

			if (!destDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				destDir += Path.DirectorySeparatorChar;
			}

			if (!Directory.Exists(sourceDir))
			{
				// Source directory is missing...
				if (ignoreMissingSource)
				{
					// leave and ignore...
					return;
				}
				else
				{
					// throw exception...
					throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
				}
			}

			/*Recursive switch to continue drilling down into dir structure.*/
			if (fRecursive)
			{
				/*Get a list of directories from the current parent.*/
				var aDirs = Directory.GetDirectories(sourceDir);

				for (i = 0; i <= aDirs.GetUpperBound(0); i++)
				{
					/*Get the position of the last separator in the current path.*/
					posSep = aDirs[i].LastIndexOf(Path.DirectorySeparatorChar);

					/*Get the path of the source directory.*/
					var sDir = aDirs[i].Substring((posSep + 1), aDirs[i].Length - (posSep + 1));

					/*Create the new directory in the destination directory.*/
					if (!Directory.Exists(destDir + sDir))
						Directory.CreateDirectory(destDir + sDir);

					/*Since we are in recursive mode, copy the children also*/
					RecursiveCopyFiles(aDirs[i], (destDir + sDir), fRecursive:true, overWrite: overWrite, ignoreWriteProtection: ignoreWriteProtection, ignoreMissingSource: ignoreMissingSource);
				}
			}

			/*Get the files from the current parent.*/
			var aFiles = Directory.GetFiles(sourceDir);

			/*Copy all files.*/
			for (i = 0; i <= aFiles.GetUpperBound(0); i++)
			{
				/*Get the position of the trailing separator.*/
				posSep = aFiles[i].LastIndexOf(Path.DirectorySeparatorChar);

				/*Get the full path of the source file.*/
				var sFile = aFiles[i].Substring((posSep + 1), aFiles[i].Length - (posSep + 1));

				/*Copy the file.*/
				CopyFile(aFiles[i], destDir + sFile, overWrite, ignoreWriteProtection, ignoreMissingSource);
			}

		}

		public static bool TryReadAllText(Context context, string filePath, Encoding encoding, out string content)
		{
			try
			{
				using (var streamReader = new StreamReader(filePath, encoding))
				{
					content = streamReader.ReadToEnd();
				}

				return true;
			}
			catch (Exception e)
			{
				context.Logger.LogException(e);
			}

			content = null;
			return false;
		}

		public static bool WriteFile(Context context, string filePath, string fileContent, bool throwException)
		{
			try
			{
				if (filePath.IsNullOrEmpty())
					return false;
				var folderPath = Path.GetDirectoryName(filePath);
				if (!EnsureFolder(folderPath, throwException))
					return false;
				if (File.Exists(filePath))
					MakeWritable(true, filePath);

				using(var file = File.CreateText(filePath))
				{
					file.Write(fileContent);
				}
				return true;
			}
			catch (Exception e)
			{
				context.Logger.LogException(e);
				if ( throwException)
					throw new ApplicationException("6q3dhcwe",e);
				return false;
			}
		}

		#endregion

		public enum PathCompareResult
		{
			Equal,
			Path1IsSubset,
			Path2IsSubset,
			Else
		}

		public TFilePath()
		{
			FileSuffix = String.Empty;
			ParentDirectory = String.Empty;
			FileBaseName = String.Empty;
			FileExtension1 = String.Empty;
			FileExtension2 = String.Empty;
		}

		/// <summary>
		/// Analyzes the passed file path
		/// </summary>
		/// <param name="filePath"></param>
		public TFilePath(string filePath) : this()
		{
			ParentDirectory = Path.GetDirectoryName(filePath);
			FileBaseName = Path.GetFileName(filePath);
			if (FileBaseName != null)
			{
				var parts = FileBaseName.Split('.');
				if ( parts.Length < 2 )
				{
					// do nothing
				}
				else if ( parts.Length == 2)
				{
					FileBaseName = parts[0];
					FileExtension1 = parts[1];
				}
				else 
				{
					FileBaseName = parts[0];
					FileExtension1 = parts[parts.Length-2];
					FileExtension2 = parts[parts.Length-1];
				}
			}
		}

		/// <summary>
		/// <![CDATA[
		/// <ParentDirectory>\<FileBaseName>[<FileSuffix>][.FileExtension1][.FileExtension2]
		/// ]]>
		/// </summary>
		public string FileBaseName { get; set; }
		/// <summary>
		/// <![CDATA[
		/// <ParentDirectory>\<FileBaseName>[<FileSuffix>][.FileExtension1][.FileExtension2]
		/// ]]>
		/// </summary>
		public string FileSuffix { get; set; }
		/// <summary>
		/// <![CDATA[
		/// <ParentDirectory>\<FileBaseName>[<FileSuffix>][.FileExtension1][.FileExtension2]
		/// ]]>
		/// </summary>
		public string FileExtension1 { get; set; }
		/// <summary>
		/// <![CDATA[
		/// <ParentDirectory>\<FileBaseName>[<FileSuffix>][.FileExtension1][.FileExtension2]
		/// ]]>
		/// </summary>
		public string FileExtension2 { get; set; }

		/// <summary>
		/// <![CDATA[
		/// <ParentDirectory>\<FileBaseName>[<FileSuffix>][.FileExtension1][.FileExtension2]
		/// ]]>
		/// </summary>
		public string FilePath => ParentDirectory.Combine(FileName);

		/// <summary>
		/// <![CDATA[
		/// <FileBaseName>[<FileSuffix>][.FileExtension1][.FileExtension2]
		/// ]]>
		/// </summary>
		public string FileName
		{
			get
			{
				string fileName = FileBaseName + FileSuffix;
				if (!string.IsNullOrEmpty(FileExtension1))
					fileName = fileName + "." + FileExtension1;
				if (!string.IsNullOrEmpty(FileExtension2))
					fileName = fileName + "." + FileExtension2;

				return fileName;
			}
		}

		/// <summary>
		/// <![CDATA[
		/// <ParentDirectory>\<FileBaseName>[<FileSuffix>][.FileExtension1][.FileExtension2]
		/// ]]>
		/// </summary>
		public string ParentDirectory { get; }

		/// <summary>
		/// Clones this instance
		/// </summary>
		/// <returns></returns>
		public TFilePath Copy()
		{
			var res = new TFilePath
			{
				FileBaseName = FileBaseName,
				FileExtension1 = FileExtension1,
				FileExtension2 = FileExtension2,
				FileSuffix = FileSuffix
			};
			return res;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="isFile"></param>
		/// <param name="path"></param>
		/// <returns>false, if iteration must be canceled</returns>
		public delegate bool OnFileOrDirectoryMethod(bool isFile, string path);


		/// <summary>
		/// <![CDATA[
		/// <ParentDirectory>\<FileBaseName>[<FileSuffix>][.FileExtension1][.FileExtension2]
		/// ]]>
		/// </summary>
		public override string ToString()
		{
			return FilePath;
		}

		#endregion
	}
}
