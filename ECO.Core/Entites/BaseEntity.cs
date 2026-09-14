using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.Core.Entites
{
    public class BaseEntity<T>
    {
        public T Id { get; set; }
    }
}
