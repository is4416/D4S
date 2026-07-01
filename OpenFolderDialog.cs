using System;
using System.Windows.Forms;

class Program
{
	/**
	 * args[0]: description
	 * args[1]: selectedPath
	 */
	[STAThread]
	static void Main(string[] args)
	{
		using (var dialog = new FolderBrowserDialog())
		{
			if (args.Length > 0)
			{
				dialog.Description = args[0];
			}

			if (args.Length > 1)
			{
				dialog.SelectedPath = args[1];
			}

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				Console.Write(dialog.SelectedPath);
				return;
			}
		}
	}
}
