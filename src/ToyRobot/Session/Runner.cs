using ToyRobot.Commands;
using ToyRobot.Domain;
using ToyRobot.Parsing;

namespace ToyRobot.Session;

/// <summary>
/// Runs one session: reads lines from the input, shows the prompt or the echo on the console messages,
/// passes each line to the CommandParser, and sends command objects to the RobotOperator
/// (RN-01; FR-08, FR-11 to FR-16; DEC-20, DEC-21, DEC-23, DEC-24, DEC-25, DEC-26, DEC-29, DEC-30).
/// It uses only the reader and writers it is given, never <see cref="Console"/>.
/// </summary>
public sealed class Runner
{
    private const char ByteOrderMark = '\uFEFF';

    // The Windows console gives Ctrl+Z then Enter as a line holding only this character, not as the end of input (DEC-10).
    private const string CtrlZ = "\u001A";

    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly TextWriter _error;
    private readonly RobotOperator _robotOperator;
    private readonly bool _interactive;
    private readonly CommandParser _parser = new();

    /// <param name="input">Where lines are read from.</param>
    /// <param name="output">Console output (stdout): receives only REPORT results.</param>
    /// <param name="error">Console messages (stderr): receives the prompt, the echo and warnings.</param>
    /// <param name="robotOperator">Runs the command objects.</param>
    /// <param name="interactive">True when a person is typing: shows the prompt instead of the echo.</param>
    public Runner(TextReader input, TextWriter output, TextWriter error, RobotOperator robotOperator, bool interactive)
    {
        _input = input;
        _output = output;
        _error = error;
        _robotOperator = robotOperator;
        _interactive = interactive;
    }

    /// <summary>Runs the session. Returns at EXIT, at the end of the input, or at a line that is only Ctrl+Z (FR-16; DEC-10).</summary>
    public void Run()
    {
        var isFirstLine = true;

        while (true)
        {
            if (_interactive)
            {
                _error.Write("> ");
            }

            var line = _input.ReadLine();
            if (line is null || line == CtrlZ)
            {
                return;
            }

            // Removes an invisible "byte order mark" (BOM) from the start of the first line.
            //
            // What is it? Some editors, like Notepad, put a hidden character (\uFEFF) at the very
            // start of a file to say "this file is UTF-8". You can't see it, but it's there.
            //
            // Why remove it? Without this, the first line would be "\uFEFFPLACE 0,0,NORTH".
            // Trim() does not remove this character, so the command word would be "\uFEFFPLACE",
            // which is not PLACE. The robot would never be placed, and the warning would look
            // like a normal PLACE line, because the character is invisible.
            //
            // isFirstLine     - a BOM can only be at the start of a file, so only check the first line.
            // line.Length > 0 - an empty line has no line[0]; reading it would crash the app.
            // line[0]         - the first character of the line.
            // Substring(1)    - keeps everything after the first character, so the BOM is dropped.
            if (isFirstLine && line.Length > 0 && line[0] == ByteOrderMark)
            {
                line = line.Substring(1);
            }

            isFirstLine = false;

            if (!_interactive)
            {
                _error.WriteLine("> " + line.Trim());
            }

            switch (_parser.Parse(line))
            {
                case ParseResult.Failure failure:
                    _error.WriteLine("Warning: " + failure.Reason);
                    break;
                case ParseResult.Exit:
                    return;
                case ParseResult.CommandObject commandObject:
                    Show(_robotOperator.Run(commandObject.Command));
                    break;
            }
        }
    }

    private void Show(Outcome outcome)
    {
        switch (outcome)
        {
            case Outcome.PositionText position:
                _output.WriteLine("Output:");
                _output.WriteLine(position.Text);
                break;
            case Outcome.Warning warning:
                _error.WriteLine("Warning: " + warning.Text);
                break;
        }
    }
}
