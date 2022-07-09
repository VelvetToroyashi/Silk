using System;
using System.Collections.Generic;
using System.Text;

namespace Silk.Utilities;

public class LineBuilder
{
    private readonly StringBuilder _builder      = new();
    private readonly List<string>  _pendingLines = new();
    
    /// <summary>
    /// Appends a line to the builder.
    /// </summary>
    /// <returns>The current builder to chain calls with.</returns>
    public LineBuilder AppendLine()
    {
        _pendingLines.Add(string.Empty);
        return this;
    }
    
    /// <summary>
    /// Appends a new line to the builder. This line can be removed so long as it's not committed.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <returns>The current builder to chain calls with.</returns>
    public LineBuilder AppendLine(string value)
    {
        _pendingLines.Add(value);
        return this;
    }

    /// <summary>
    /// Removes the last written line from the builder.
    /// </summary>
    /// <returns>The current builder to chain calls with.</returns>
    public LineBuilder RemoveLine() => RemoveLine(_pendingLines.Count - 1);
    
    /// <summary>
    /// Removes an uncommitted line at the given index from the builder.
    /// </summary>
    /// <param name="index">The line index to remove from.</param>
    /// <returns>The current builder to chain calls with.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the builder does not contain uncommited lines.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is negative, or exceeds the uncommited line count.</exception>
    public LineBuilder RemoveLine(int index)
    {
        if (_pendingLines.Count <= 0)
        {
            throw new InvalidOperationException("The current LineBuilder does not have pending lines to remove.");
        }
        
        if (index < 0 || index >= _pendingLines.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, $"The index must be within the bounds of the pending lines. ({_pendingLines} pending lines).");
        }
        
        _pendingLines.RemoveAt(index);
        
        return this;
    }
    
    /// <summary>
    /// Flushes uncommited lines to the backing buffer.
    /// </summary>
    /// <returns>The builder to chain calls with.</returns>
    public LineBuilder Commit()
    {
        _builder.AppendLine(string.Join(Environment.NewLine, _pendingLines));
        _pendingLines.Clear();
        return this;
    }
    
    /// <summary>
    /// Returns a string representation of the current builder, including uncommited lines.
    /// </summary>
    public override string ToString()
    {
        var builder = new StringBuilder(_builder.ToString());
        
        foreach (var line in _pendingLines)
        {
            builder.AppendLine(line);
        }
        
        return builder.ToString();
    }
    
    public static implicit operator string(LineBuilder builder) => builder.ToString();
}