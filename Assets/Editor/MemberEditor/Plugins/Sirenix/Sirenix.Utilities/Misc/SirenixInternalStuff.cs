#if SIRENIX_INTERNAL
using System;

/// <summary>
/// Internal...
/// </summary>
[System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public sealed class TodoAttribute : Attribute
{
    /// <summary>
    /// Internal...
    /// </summary>
    public readonly string Assignee;

    /// <summary>
    /// Internal...
    /// </summary>
    public readonly string TodoMessage;

    /// <summary>
    /// Internal...
    /// </summary>
    public TodoAttribute(string todoMessage)
    {
        this.Assignee = "-";
        this.TodoMessage = todoMessage;
    }

    /// <summary>
    /// Internal...
    /// </summary>
    public TodoAttribute(string todoMessage, string assignee)
    {
        this.TodoMessage = todoMessage;
        this.Assignee = assignee;
    }
}
#endif