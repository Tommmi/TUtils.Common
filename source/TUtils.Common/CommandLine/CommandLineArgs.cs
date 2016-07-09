using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TUtils.Common.CommandLine.Common;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace TUtils.Common.CommandLine
{
	public class CommandLineArgs : IEnumerable<ICommandLineArg>
	{
		#region private

		private string[] _commandLineArgs;
		private IEnumerable<CommandLineArgDefinitionBase> _commandArgDefs;

		#endregion

		#region public

		public IEnumerable<ICommandLineArg> Args { get; private set; }
		public IEnumerable<CommandLineArgDefinitionBase> CommandArgDefs => _commandArgDefs;

		public IEnumerable<string> ArgsRaw => _commandLineArgs;

		public ICommandLineArg this[string key]
		{
			get { return Args.FirstOrDefault(a => a.Key == key); }
		}

		public ICommandLineArg this[CommandLineArgDefinitionBase argDef] => this[argDef.Key];

		public string UsageHint
		{
			get
			{
				var text = new StringBuilder();
				text.Append("correct usage: ");
				text.Append(Path.GetFileName(Assembly.GetEntryAssembly().CodeBase));
				text.Append(" ");

				foreach (var commandArgDef in _commandArgDefs)
				{
					text.AppendLine(commandArgDef.UsageHint + " ");
				}

				return text.ToString();
			}
		}

		public static bool TryCreate(
			string[] commandLineArgsRaw,
			out CommandLineArgs commandLineArgs,
			out CommandLineParseResult result,
			params CommandLineArgDefinitionBase[] commandArgDefs)
		{
			commandLineArgs = new CommandLineArgs();
			result = commandLineArgs.Init(commandLineArgsRaw, commandArgDefs);
			return result.Succeeded;
		}

		public CommandLineParseResult Init(string[] commandLineArgs, params CommandLineArgDefinitionBase[] commandArgDefs)
		{
			_commandArgDefs = commandArgDefs;
			_commandLineArgs = commandLineArgs;

			var argObjs = new List<ICommandLineArg>();

			foreach (var commandArgDef in commandArgDefs)
			{
				foreach (var commandLineArg in commandLineArgs)
				{
					ICommandLineArg argObj;
					if (commandArgDef.TryGetCommandLineArg(commandLineArg, out argObj))
					{
						argObjs.Add(argObj);
						break;
					}
				}
			}

			Args = argObjs;

			return new CommandLineParseResult().Init(
				succeeded: argObjs.Count == commandArgDefs.Length,
				failedCommandArgs: commandArgDefs
					.Where(def=>argObjs.All(argObj=>argObj.Key!=def.Key))
					.Select(def=>def.Key));
		}

		IEnumerator<ICommandLineArg> IEnumerable<ICommandLineArg>.GetEnumerator()
		{
			return Args.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Args.GetEnumerator();
		}

		#endregion

	}
}