using System;
using System.Collections.Generic;
using System.Text;

namespace CharCode.Base.Exceptions
{
    public class PasswordIncorrectException : Exception
    {
        public PasswordIncorrectException() : base("رمزعبور وارده معتبر نیست!")
        {
        }

        public PasswordIncorrectException(string message) : base(message)
        {
        }
    }
}
