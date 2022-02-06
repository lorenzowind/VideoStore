using VideoStore.Core.Utils;
using VideoStore.Core.Protocols;

namespace VideoStore.Core.Domain
{
    public class Cpf
    {
        public const int MaxLength = 11;
        public string Number { get; private set; }

        protected Cpf() { }

        public Cpf(string number)
        {
            if (!Validate(number)) throw new DomainException($"Cpf '{number}' is not valid.");
            Number = number;
        }

        public static bool Validate(string cpf)
        {
            cpf = cpf.OnlyNumbers(cpf);

            if (cpf.Length > 11)
                return false;

            while (cpf.Length != 11)
                cpf = '0' + cpf;

            var isEqual = true;
            for (var i = 1; i < 11 && isEqual; i++)
                if (cpf[i] != cpf[0])
                    isEqual = false;

            if (isEqual || cpf == "12345678909")
                return false;

            var numbers = new int[11];

            for (var i = 0; i < 11; i++)
                numbers[i] = int.Parse(cpf[i].ToString());

            var sum = 0;
            for (var i = 0; i < 9; i++)
                sum += (10 - i) * numbers[i];

            var result = sum % 11;

            if (result == 1 || result == 0)
            {
                if (numbers[9] != 0)
                    return false;
            }
            else if (numbers[9] != 11 - result)
                return false;

            sum = 0;
            for (var i = 0; i < 10; i++)
                sum += (11 - i) * numbers[i];

            result = sum % 11;

            if (result == 1 || result == 0)
            {
                if (numbers[10] != 0)
                    return false;
            }
            else if (numbers[10] != 11 - result)
                return false;

            return true;
        }
    }
}
