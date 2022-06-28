using System;

namespace AllWorlds.GameEngine;

public record QueueComponent : Component
{
    private readonly QueueComponent? _next;

    public QueueComponent(QueueComponent? next) : base(new Component())
    {
        Next = next;
    }

    public QueueComponent? Next
    {
        get => _next;
        init
        {
            if (value is not null)
                try
                {
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    Convert.ChangeType(this, value.GetType());
                }
                catch
                {
                    throw new NotSupportedException(
                        "Setting Next to a QueueComponent of a different type is not supported.");
                }

            _next = value;
        }
    }

    public override QueueComponent HandleDuplicate(Component newComponent)
    {
        if (newComponent.GetType() == GetType() && newComponent is QueueComponent newQueueComponent)
            return MakeQueue(newQueueComponent);

        throw new ArgumentException("Duplicate must be the same type as the QueueComponent.");
    }

    private QueueComponent MakeQueue(QueueComponent newComponent)
    {
        //TODO: Figure out how to cover the last uncovered statement (Unit Testing).
        return Next switch
        {
            null => this with {Next = newComponent},
            _ => this with {Next = Next.MakeQueue(newComponent)}
        };
    }
}