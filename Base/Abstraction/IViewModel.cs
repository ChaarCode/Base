using System;
using System.Collections.Generic;
using System.Text;

namespace CharCode.Base.Abstraction
{
    public interface IViewModel<TKey>
    {
        TKey Id { get; set; }
    }

    public interface IViewModel : IViewModel<long>
    {
    }
}
