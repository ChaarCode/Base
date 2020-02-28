using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CharCode.Base.Abstraction
{
    public interface IModel<TKey>
    {
        [Key]
        TKey Id { get; set; }
    }

    public interface IModel : IModel<long>
    {

    }
}
