namespace LowlandTech.Redux.Abstractions;

public interface IAction
{
    IDictionary<string, object> Metadata { get; }
}