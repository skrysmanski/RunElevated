using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace RunElevatedApp;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            IReadOnlyList<string> actualArgs;
            bool waitForExit;

            if (args.Length > 0)
            {
                switch (args[0].ToLowerInvariant())
                {
                    case "/wait":
                    case "--wait":
                        waitForExit = true;
                        actualArgs = args.Skip(1).ToArray();
                        break;

                    default:
                        waitForExit = false;
                        actualArgs = args;
                        break;
                }
            }
            else
            {
                waitForExit = false;
                actualArgs = args;
            }

            if (actualArgs.Count == 0)
            {
                ShowError("No arguments specified.");
                return -1;
            }

            var psi = new ProcessStartInfo(actualArgs[0])
            {
                // NOTE: "CreateNoWindow" must be "false" or newly started command prompts won't be visible.
                CreateNoWindow = false,
                // NOTE: "UseShellExecute" should be "true" so that you can use commands like on cmd (e.g. "RunElevated cmd").
                UseShellExecute = true,
                // NOTE: We use "Verb" to make the app run elevated - instead of making this "RunElevated" application run elevated
                //   because for the latter we would need to sign our application with an actual trusted certificate, and it would always
                //   say "RunElevated" wants to run elevated - whereas with "Verb" it says "Target application wants to run elevated"
                //   which is much better UX.
                Verb = "runas",
            };

            if (actualArgs.Count > 1)
            {
                psi.Arguments = ConvertArgsToString(actualArgs.Skip(1));
            }

            using var process = Process.Start(psi);
            if (process is null)
            {
                ShowError($"Process '{actualArgs[0]}' couldn't be started.");
                return -1;
            }

            if (waitForExit)
            {
                process.WaitForExit();
                return process.ExitCode;
            }

            return 0;

        }
        catch (Exception ex)
        {
            ShowError(ex.ToString());
            return -1;
        }
    }

    private static string ConvertArgsToString(IEnumerable<string> args)
    {
        var result = new StringBuilder();

        foreach (var arg in args)
        {
            // NOTE: cmd is a little bit squishy when it comes to quotes (e.g. it recognizes pushd but not "pushd"). So,
            //   we'll only surround arguments with quotes that contain spaces.
            if (arg.Contains(" "))
            {
                result.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\" ", arg);
            }
            else
            {
                result.Append(arg).Append(' ');
            }
        }

        return result.ToString().Trim();
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(
            message,
            caption: "Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}
