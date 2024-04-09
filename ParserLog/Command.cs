
namespace ParserLog;

public abstract class Command
{
    public virtual string Name { get; set; } = string.Empty;
    public virtual string Value { get; set; } =string.Empty;
    public virtual bool IsRequired { get; set; }
    public virtual int Priority { get; set; } = 0;

    public abstract IEnumerator<Log> Move(IEnumerator<Log> enumerator);

}