using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public delegate void UpdateStackEvent();

public class ObservableStack<T> : Stack<T>
{
    public event UpdateStackEvent OnPush;
    public event UpdateStackEvent OnPop;
    public event UpdateStackEvent OnClear;

    // constructors to make sure you can create an observaleStack with some items in it from the start
    public ObservableStack(ObservableStack<T> items) : base(items)
    {

    }

    // and one where you can instantiate an observable stack without any items like when we make a slot
    public ObservableStack()
    {

    }

    public new void Push(T item)
    {
        base.Push(item);

        if (OnPush != null)
        {
            OnPush();
        }
    }

    public new T Pop()
    {
        T item = base.Pop();

        if (OnPop != null)
        {
            OnPop();
        }

        return item;
    }

    public new void Clear()
    {
        base.Clear();

        if (OnClear != null)
        {
            OnClear();
        }
    }

}
