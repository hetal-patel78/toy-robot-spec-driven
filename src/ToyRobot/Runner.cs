namespace ToyRobot;

/// <summary>
/// Runs one session: reads lines from the input, shows the prompt or the echo on the console messages,
/// passes each line to the CommandParser, and sends command objects to the RobotOperator
/// (RN-01; FR-08, FR-11 to FR-16; DEC-20, DEC-21, DEC-23, DEC-24, DEC-25, DEC-26, DEC-29, DEC-30).
/// It uses only the reader and writers it is given, never <see cref="Console"/>.
/// </summary>
public sealed class Runner
{
    private const char ByteOrderMark = '\uFEFF';

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

    /// <summary>Runs the session. Returns at EXIT or at the end of the input.</summary>
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
            if (line is null)
            {
                return;
            }

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
